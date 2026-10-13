import JSZip from 'jszip';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, expect, it, vi } from 'vitest';
import { http, HttpResponse } from 'msw';
import { API, server } from '../../test/mocks/server';
import { ReportPreview } from './ReportPreview';

beforeEach(() => {
    HTMLDialogElement.prototype.showModal = function () { this.setAttribute('open', ''); };
});

it('fetches a PDF with authorization only on demand and releases it on close', async () => {
    let requests = 0;
    let authorization: string | null = null;
    localStorage.setItem('token', 'preview-token');
    server.use(http.get(`${API}/preview`, ({ request }) => {
        requests++;
        authorization = request.headers.get('Authorization');
        return new HttpResponse('%PDF-1.4', { headers: { 'Content-Type': 'application/octet-stream' } });
    }));
    const create = vi.spyOn(URL, 'createObjectURL').mockReturnValue('blob:preview');
    const revoke = vi.spyOn(URL, 'revokeObjectURL').mockImplementation(() => { });
    render(<ReportPreview url="/preview" filename="Отчёт.PDF" />);
    expect(requests).toBe(0);
    const user = userEvent.setup();
    await user.click(screen.getByRole('button', { name: 'Предпросмотр' }));
    expect(await screen.findByTitle('Предпросмотр: Отчёт.PDF')).toHaveAttribute('src', 'blob:preview');
    expect(authorization).toBe('Bearer preview-token');
    expect(create.mock.calls[0][0]).toHaveProperty('type', 'application/pdf');
    await user.click(screen.getByRole('button', { name: 'Закрыть' }));
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    expect(revoke).toHaveBeenCalledWith('blob:preview');
});

it('reports an error and allows another attempt after reopening', async () => {
    let requests = 0;
    server.use(http.get(`${API}/preview`, () => { requests++; return new HttpResponse(null, { status: 500 }); }));
    render(<ReportPreview url="/preview" filename="report.pdf" />);
    const user = userEvent.setup();
    await user.click(screen.getByRole('button', { name: 'Предпросмотр' }));
    expect(await screen.findByRole('alert')).toHaveTextContent('Не удалось открыть документ');
    await user.click(screen.getByRole('button', { name: 'Закрыть' }));
    await user.click(screen.getByRole('button', { name: 'Предпросмотр' }));
    await waitFor(() => expect(requests).toBe(2));
});

it('explains unsupported DOC files without fetching them', async () => {
    render(<ReportPreview url="/must-not-fetch" filename="report.doc" />);
    await userEvent.setup().click(screen.getByRole('button', { name: 'Предпросмотр' }));
    expect(screen.getByRole('status')).toHaveTextContent('Предпросмотр этого формата недоступен');
});

it('renders DOCX text in an isolated frame without active document content', async () => {
    const zip = new JSZip();
    zip.file('[Content_Types].xml', '<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types"><Override PartName="/word/document.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"/></Types>');
    zip.file('_rels/.rels', '<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="word/document.xml"/></Relationships>');
    zip.file('word/document.xml', '<w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"><w:body><w:p><w:r><w:t>Выводы лабораторной</w:t></w:r></w:p></w:body></w:document>');
    const data = await zip.generateAsync({ type: 'arraybuffer' });
    server.use(http.get(`${API}/preview`, () => new HttpResponse(data)));
    render(<ReportPreview url="/preview" filename="report.docx" />);
    await userEvent.setup().click(screen.getByRole('button', { name: 'Предпросмотр' }));
    const frame = await screen.findByTitle('Предпросмотр: report.docx');
    expect(frame).toHaveAttribute('sandbox', '');
    expect(frame.getAttribute('srcdoc')).toContain('Выводы лабораторной');
    expect(frame.getAttribute('srcdoc')).toContain("default-src 'none'");
});
