import { useNavigate } from 'react-router-dom';
import { fireEvent, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { http, HttpResponse } from 'msw';
import LabReportPage from './LabReportPage';
import { API, server } from '../../test/mocks/server';
import { renderPage } from '../../test/render';

const endpoint = `${API}/public/api/v1/laboratories/lab-1`;
function setupReport() {
    server.use(
        http.get(endpoint, () => HttpResponse.json({ title: 'Основы сетей' })),
        http.get(`${endpoint}/reports/my`, () => new HttpResponse(null, { status: 404 })),
    );
}
function renderReport() {
    return renderPage(<LabReportPage />, '/labs/:labId/report', '/labs/lab-1/report');
}

describe('LabReportPage', () => {
    it('disables submission until a file is selected', async () => {
        setupReport();
        renderReport();
        expect(await screen.findByRole('button', { name: 'Отправить отчет' })).toBeDisabled();
    });
    it.each([true, false])('uploads a file (success: %s)', async success => {
        let fileName = '';
        let fileContent = '';
        let reads = 0;
        setupReport();
        server.use(
            http.get(`${endpoint}/reports/my`, () => {
                reads++;
                return HttpResponse.json({ allowResubmit: true, versions: [] });
            }),
            http.post(`${endpoint}/reports`, async ({ request }) => {
                const form = await request.formData();
                const file = form.get('file') as File;
                fileName = file.name;
                fileContent = await file.text();
                return new HttpResponse(null, { status: success ? 201 : 500 });
            }),
        );
        const { container } = renderReport();
        await screen.findByRole('button', { name: 'Отправить отчет' });
        const input = container.querySelector<HTMLInputElement>('input[type="file"]')!;
        const user = userEvent.setup();
        await user.upload(input, new File(['report contents'], 'report.pdf', { type: 'application/pdf' }));
        await user.click(screen.getByRole('button', { name: 'Отправить отчет' }));
        if (success) {
            expect(await screen.findByText('Отчёты ещё не загружались')).toBeInTheDocument();
            expect(reads).toBe(2);
        } else {
            expect(await screen.findByText('Не удалось загрузить отчёт. Попробуйте ещё раз.')).toBeInTheDocument();
            expect(screen.getByRole('button', { name: 'Отправить отчет' })).toBeEnabled();
            expect(screen.getByText('report.pdf')).toBeInTheDocument();
        }
        expect(fileName).toBe('report.pdf');
        expect(fileContent).toBe('report contents');
    });
});

function version(number: number, status = 1) {
    return {
        versionId: `v${number}`, versionNumber: number, status,
        originalFileName: 'report.pdf', fileSizeBytes: 10, points: null,
        teacherComment: null as string | null, checkedByTeacherFullName: null,
        createDateUtc: '2026-01-10T12:00:00Z', fileDownloadUrl: null as string | null,
    };
}
function existingReport(versions: ReturnType<typeof version>[], allowResubmit = true) {
    setupReport();
    server.use(http.get(`${endpoint}/reports/my`, () => HttpResponse.json({
        reportId: 'r1', status: 1, points: null, teacherComment: null, allowResubmit, versions,
    })));
}

describe('report history and resubmission', () => {
    it('shows versions newest first with their statuses and download availability', async () => {
        existingReport([version(1, 3), { ...version(3, 4), fileDownloadUrl: `${API}/download` }, version(2, 2)]);
        renderReport();
        const user = userEvent.setup();
        await user.click(await screen.findByRole('button', { name: 'История версий' }));
        const rows = screen.getAllByRole('row').slice(1);
        expect(rows.map(row => within(row).getAllByRole('cell')[0].textContent)).toEqual(['v3', 'v2', 'v1']);
        expect(rows[0]).toHaveTextContent('Принята');
        expect(rows[1]).toHaveTextContent('На проверке');
        expect(rows[2]).toHaveTextContent('Нужны правки');
        expect(within(rows[0]).getByRole('button', { name: 'Скачать' })).toBeInTheDocument();
        expect(within(rows[1]).queryByRole('button')).not.toBeInTheDocument();
    });
    it('shows the comment from the latest revision request', async () => {
        existingReport([
            { ...version(1, 3), teacherComment: 'Старый комментарий' },
            { ...version(2, 3), teacherComment: 'Добавьте выводы' },
            version(3, 1),
        ]);
        renderReport();
        expect(await screen.findByText('Добавьте выводы')).toBeInTheDocument();
        expect(screen.getByText('Версия 2 · Нужны правки')).toBeInTheDocument();
        expect(screen.queryByText('Старый комментарий')).not.toBeInTheDocument();
        expect(screen.getByRole('button', { name: 'Отправить отчет' })).toBeDisabled();
    });
    it('hides upload controls when resubmission is forbidden but retains history', async () => {
        existingReport([version(1, 4)], false);
        const { container } = renderReport();
        expect(await screen.findByText('Повторная загрузка отчёта сейчас недоступна')).toBeInTheDocument();
        expect(screen.queryByRole('button', { name: 'Отправить отчет' })).not.toBeInTheDocument();
        expect(container.querySelector('input[type="file"]')).toBeNull();
        await userEvent.setup().click(screen.getByRole('button', { name: 'История версий' }));
        expect(screen.getByText('v1')).toBeInTheDocument();
    });
    it('accepts a dropped file and ignores an empty drop', async () => {
        setupReport();
        const { container } = renderReport();
        await screen.findByRole('button', { name: 'Отправить отчет' });
        const zone = container.querySelector('input[type="file"]')!.parentElement!;
        fireEvent.drop(zone, { dataTransfer: { files: [] } });
        expect(screen.getByRole('button', { name: 'Отправить отчет' })).toBeDisabled();
        fireEvent.drop(zone, { dataTransfer: { files: [new File(['test'], 'dropped.pdf', { type: 'application/pdf' })] } });
        expect(screen.getByText('dropped.pdf')).toBeInTheDocument();
        expect(screen.getByRole('button', { name: 'Отправить отчет' })).toBeEnabled();
    });
    it('retries the selected file after failure and refreshes history on success', async () => {
        setupReport();
        let attempts = 0;
        const names: string[] = [];
        server.use(
            http.post(`${endpoint}/reports`, async ({ request }) => {
                names.push(((await request.formData()).get('file') as File).name);
                attempts++;
                return new HttpResponse(null, { status: attempts === 1 ? 500 : 201 });
            }),
            http.get(`${endpoint}/reports/my`, () => HttpResponse.json({ allowResubmit: true, versions: attempts >= 2 ? [version(1)] : [] })),
        );
        const { container } = renderReport();
        const user = userEvent.setup();
        await screen.findByRole('button', { name: 'Отправить отчет' });
        await user.upload(container.querySelector<HTMLInputElement>('input[type="file"]')!, new File(['test'], 'retry.pdf', { type: 'application/pdf' }));
        await user.click(screen.getByRole('button', { name: 'Отправить отчет' }));
        expect(await screen.findByText('Не удалось загрузить отчёт. Попробуйте ещё раз.')).toBeInTheDocument();
        await user.click(screen.getByRole('button', { name: 'Отправить отчет' }));
        expect(await screen.findByText('v1')).toBeInTheDocument();
        expect(names).toEqual(['retry.pdf', 'retry.pdf']);
        expect(screen.queryByText('Не удалось загрузить отчёт. Попробуйте ещё раз.')).not.toBeInTheDocument();
        await user.click(screen.getByRole('button', { name: 'Загрузить' }));
        expect(screen.getByText('Отчёт успешно отправлен!')).toBeInTheDocument();
        expect(screen.getByRole('button', { name: 'Отправить отчет' })).toBeDisabled();
    });
    it.each([
        ["attachment; filename*=UTF-8''%D0%BE%D1%82%D1%87%D1%91%D1%82.pdf", 'отчёт.pdf'],
        [null, 'report.pdf'],
    ])('downloads using the server filename or fallback: %s', async (disposition, expectedName) => {
        existingReport([{ ...version(1), fileDownloadUrl: `${API}/download` }]);
        server.use(http.get(`${API}/download`, () => new HttpResponse('PDF contents', {
            headers: {
                'Content-Type': 'application/pdf', ...(disposition ? { 'Content-Disposition': disposition } : {}),
            }
        })));
        const create = vi.fn(() => 'blob:test-report');
        const revoke = vi.fn();
        vi.spyOn(URL, 'createObjectURL').mockImplementation(create);
        vi.spyOn(URL, 'revokeObjectURL').mockImplementation(revoke);
        let downloadedName = '';
        let downloadedHref = '';
        vi.spyOn(HTMLAnchorElement.prototype, 'click').mockImplementation(function (this: HTMLAnchorElement) {
            downloadedName = this.download; downloadedHref = this.href;
        });
        renderReport();
        const user = userEvent.setup();
        await user.click(await screen.findByRole('button', { name: 'История версий' }));
        await user.click(screen.getByRole('button', { name: 'Скачать' }));
        await waitFor(() => expect(revoke).toHaveBeenCalledWith('blob:test-report'));
        expect(downloadedName).toBe(expectedName);
        expect(downloadedHref).toBe('blob:test-report');
        expect(create).toHaveBeenCalledOnce();
        expect(document.querySelector('a[download]')).toBeNull();
    });
    it.each([['Лабораторные', '/labs'], ['Основы сетей', '/labs/lab-1']])('navigates via breadcrumb %s', async (name, path) => {
        setupReport();
        renderReport();
        await userEvent.setup().click(await screen.findByRole('button', { name }));
        expect(screen.getByLabelText('Current route')).toHaveTextContent(path);
    });
});

it('prevents duplicate submission while the upload is pending', async () => {
    setupReport();
    let release!: () => void;
    const pending = new Promise<void>(resolve => { release = resolve; });
    let requests = 0;
    server.use(http.post(`${endpoint}/reports`, async () => {
        requests++;
        await pending;
        return new HttpResponse(null, { status: 201 });
    }));
    const { container } = renderReport();
    const user = userEvent.setup();
    await screen.findByRole('button', { name: 'Отправить отчет' });
    await user.upload(container.querySelector<HTMLInputElement>('input[type="file"]')!, new File(['test'], 'pending.pdf', { type: 'application/pdf' }));
    try {
        await user.click(screen.getByRole('button', { name: 'Отправить отчет' }));
        const button = screen.getByRole('button', { name: 'Отправка...' });
        expect(button).toBeDisabled();
        await user.click(button);
        await waitFor(() => expect(requests).toBe(1));
    } finally { release(); }
    expect(await screen.findByText('Отчёты ещё не загружались')).toBeInTheDocument();
    expect(requests).toBe(1);
});

it('shows download failure and permits retry', async () => {
    existingReport([{ ...version(1), fileDownloadUrl: `${API}/download` }]);
    let calls = 0; server.use(http.get(`${API}/download`, () => { calls++; return HttpResponse.error(); }));
    renderReport(); const user = userEvent.setup();
    await user.click(await screen.findByRole('button', { name: 'История версий' }));
    await user.click(screen.getByRole('button', { name: 'Скачать' }));
    expect(await screen.findByText('Не удалось скачать отчёт. Попробуйте ещё раз.')).toBeInTheDocument();
    await user.click(screen.getByRole('button', { name: 'Скачать' }));
    await waitFor(() => expect(calls).toBe(2));
});
it('reports history refresh failure separately after a successful upload', async () => {
    existingReport([version(1)]); let uploaded = false;
    server.use(http.post(`${endpoint}/reports`, () => { uploaded = true; return new HttpResponse(null, { status: 201 }); }),
        http.get(`${endpoint}/reports/my`, () => uploaded ? HttpResponse.error() : HttpResponse.json({ allowResubmit: true, versions: [version(1)] })));
    const { container } = renderReport(); const user = userEvent.setup(); await screen.findByRole('button', { name: 'Отправить отчет' });
    await user.upload(container.querySelector<HTMLInputElement>('input[type=file]')!, new File(['test'], 'report.pdf', { type: 'application/pdf' }));
    await user.click(screen.getByRole('button', { name: 'Отправить отчет' }));
    expect(await screen.findByText('Не удалось обновить историю отчётов. Попробуйте обновить страницу.')).toBeInTheDocument();
    expect(screen.getByText('v1')).toBeInTheDocument();
    await user.click(screen.getByRole('button', { name: 'Загрузить' }));
    expect(screen.getByText('Отчёт успешно отправлен!')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Отправить отчет' })).toBeDisabled();
});
it('clears the previous title, report and selected file when labId changes without unmounting', async () => {
    existingReport([version(1)]);
    server.use(http.get(`${API}/public/api/v1/laboratories/lab-2`, () => HttpResponse.json({ title: 'Другая работа' })),
        http.get(`${API}/public/api/v1/laboratories/lab-2/reports/my`, () => new HttpResponse(null, { status: 404 })));
    function Page() { const navigate = useNavigate(); return <><button onClick={() => navigate('/labs/lab-2/report')}>Другая лабораторная</button><LabReportPage /></>; }
    const { container } = renderPage(<Page />, '/labs/:labId/report', '/labs/lab-1/report'); const user = userEvent.setup();
    await screen.findByRole('button', { name: 'Отправить отчет' });
    await user.upload(container.querySelector<HTMLInputElement>('input[type=file]')!, new File(['old'], 'old.pdf', { type: 'application/pdf' }));
    await user.click(screen.getByRole('button', { name: 'Другая лабораторная' }));
    expect(await screen.findByRole('heading', { name: 'Отчёт: Другая работа' })).toBeInTheDocument();
    expect(screen.queryByText('old.pdf')).not.toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Отправить отчет' })).toBeDisabled();
    await user.click(screen.getByRole('button', { name: 'История версий' }));
    expect(screen.getByText('Отчёты ещё не загружались')).toBeInTheDocument(); expect(screen.queryByText('v1')).not.toBeInTheDocument();
});
