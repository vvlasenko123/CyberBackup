import { fireEvent, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { it, expect, vi, beforeEach, afterEach } from 'vitest';
import { http, HttpResponse } from 'msw';
import QuestionDetailPage from './QuestionDetailPage';
import { API, server } from '../../test/mocks/server';
import { renderPage } from '../../test/render';
import { setSession } from '../../test/helpers/session';
const originalScroll = Object.getOwnPropertyDescriptor(HTMLElement.prototype, 'scrollIntoView');
beforeEach(() => Object.defineProperty(HTMLElement.prototype, 'scrollIntoView', { configurable: true, value: vi.fn() }));
afterEach(() => {
    if (originalScroll) Object.defineProperty(HTMLElement.prototype, 'scrollIntoView', originalScroll);
    else Reflect.deleteProperty(HTMLElement.prototype, 'scrollIntoView');
});
it.each(['student', 'teacher'])('sends, retries and closes a question as %s', async role => {
    setSession(role);
    const endpoint = `${API}/public/api/v1/${role === 'teacher' ? 'teacher/' : ''}questions/q1`;
    const question = { id: 'q1', description: 'Как начать?', studentFullName: 'Иван', studentGroupName: 'РИ-1', laboratoryTitle: null, status: 0, createdAtUtc: '2026-01-01', messages: [] as object[] };
    let calls = 0; let body: unknown; let closed = 0;
    server.use(http.get(endpoint, () => HttpResponse.json(question)),
        http.post(`${endpoint}/${role === 'teacher' ? 'reply' : 'message'}`, async ({ request }) => {
            body = await request.json(); if (++calls === 1) return HttpResponse.error();
            question.messages = [{ id: 'm1', senderFullName: 'Иван', isFromTeacher: role === 'teacher', content: 'Ответ', createdAtUtc: '2026-01-01' }];
            return new HttpResponse(null, { status: 204 });
        }), http.post(`${endpoint}/close`, () => { closed++; question.status = 2; return new HttpResponse(null, { status: 204 }); }));
    renderPage(<QuestionDetailPage />, '/questions/:questionId', '/questions/q1'); const user = userEvent.setup();
    const input = await screen.findByRole('textbox'); await user.type(input, ' Ответ ');
    await user.click(screen.getByRole('button', { name: /Отправить/ }));
    expect(await screen.findByText('Не удалось отправить сообщение')).toBeInTheDocument(); expect(input).toHaveValue(' Ответ ');
    await user.click(screen.getByRole('button', { name: /Отправить/ }));
    expect(await screen.findByText('Ответ')).toBeInTheDocument(); expect(body).toEqual({ content: 'Ответ' });
    const confirm = vi.spyOn(window, 'confirm').mockReturnValue(false);
    await user.click(screen.getByRole('button', { name: 'Закрыть' })); expect(closed).toBe(0);
    confirm.mockReturnValue(true); await user.click(screen.getByRole('button', { name: 'Закрыть' }));
    await waitFor(() => expect(screen.queryByRole('textbox')).not.toBeInTheDocument()); expect(closed).toBe(1);
});

it.each(['student', 'teacher'])('sends files without text and preserves them for retry as %s', async role => {
    setSession(role);
    const endpoint = `${API}/public/api/v1/${role === 'teacher' ? 'teacher/' : ''}questions/q1`;
    const question = { id: 'q1', description: 'Вопрос', status: 0, createdAtUtc: '2026-01-01', messages: [] as object[] };
    let calls = 0;
    let names: string[] = [];
    server.use(http.get(endpoint, () => HttpResponse.json(question)),
        http.post(`${endpoint}/${role === 'teacher' ? 'reply' : 'message'}/attachments`, async ({ request }) => {
            const body = await request.formData();
            expect(body.get('content')).toBe('');
            names = body.getAll('files').map(file => (file as File).name);
            if (++calls === 1) return new HttpResponse(null, { status: 500 });
            question.messages = [{ id: 'm1', senderFullName: 'Иван', isFromTeacher: role === 'teacher', content: '', createdAtUtc: '2026-01-01',
                attachments: [{ id: 'a1', fileName: 'report.pdf', contentType: 'application/pdf', sizeBytes: 10 }] }];
            return new HttpResponse(null, { status: 204 });
        }));
    renderPage(<QuestionDetailPage />, '/questions/:questionId', '/questions/q1');
    const user = userEvent.setup();
    await user.upload(await screen.findByLabelText('Вложения'), [new File(['pdf'], 'report.pdf'), new File(['docx'], 'draft.docx')]);
    await user.click(screen.getByRole('button', { name: 'Удалить draft.docx' }));
    await user.click(screen.getByRole('button', { name: 'Отправить' }));
    expect(await screen.findByText('Не удалось отправить сообщение')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Удалить report.pdf' })).toBeInTheDocument();
    await user.click(screen.getByRole('button', { name: 'Отправить' }));
    expect(await screen.findByRole('button', { name: 'Скачать' })).toBeInTheDocument();
    expect(names).toEqual(['report.pdf']);
    expect(screen.queryByRole('button', { name: 'Удалить report.pdf' })).not.toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Отправить' })).toBeDisabled();
});

it('rejects invalid files and more than five attachments', async () => {
    setSession('student');
    server.use(http.get(`${API}/public/api/v1/questions/q1`, () => HttpResponse.json({
        id: 'q1', description: 'Вопрос', status: 0, createdAtUtc: '2026-01-01', messages: [],
    })));
    renderPage(<QuestionDetailPage />, '/questions/:questionId', '/questions/q1');
    const user = userEvent.setup({ applyAccept: false });
    const input = await screen.findByLabelText('Вложения');
    for (const file of [new File(['bad'], 'script.svg'), new File(['RIFF'], 'image.webp'), new File([], 'empty.pdf'), new File([new Uint8Array(10 * 1024 * 1024 + 1)], 'large.pdf')]) {
        await user.upload(input, file);
        expect(screen.getByText(/Выберите непустые/)).toBeInTheDocument();
        expect(screen.getByRole('button', { name: 'Отправить' })).toBeDisabled();
    }
    await user.upload(input, Array.from({ length: 6 }, (_, i) => new File(['pdf'], `${i}.pdf`)));
    expect(screen.getByText('Можно прикрепить до 5 файлов')).toBeInTheDocument();
});

it('previews an attachment from a closed chat through the authenticated endpoint', async () => {
    setSession('student');
    const originalShowModal = Object.getOwnPropertyDescriptor(HTMLDialogElement.prototype, 'showModal');
    Object.defineProperty(HTMLDialogElement.prototype, 'showModal', { configurable: true, value: function (this: HTMLDialogElement) { this.setAttribute('open', ''); } });
    const originalCreate = Object.getOwnPropertyDescriptor(URL, 'createObjectURL');
    const originalRevoke = Object.getOwnPropertyDescriptor(URL, 'revokeObjectURL');
    const revoke = vi.fn();
    Object.defineProperty(URL, 'createObjectURL', { configurable: true, value: vi.fn(() => 'blob:preview') });
    Object.defineProperty(URL, 'revokeObjectURL', { configurable: true, value: revoke });
    let downloaded = false;
    server.use(http.get(`${API}/public/api/v1/questions/q1`, () => HttpResponse.json({
        id: 'q1', description: 'Вопрос', status: 2, createdAtUtc: '2026-01-01', messages: [
            { id: 'm1', senderFullName: 'Иван', content: '', isFromTeacher: false, createdAtUtc: '2026-01-01',
                attachments: [{ id: 'a1', fileName: 'image.png', contentType: 'image/png', sizeBytes: 12 }] },
        ],
    })), http.get(`${API}/public/api/v1/questions/q1/attachments/a1`, ({ request }) => {
        expect(request.headers.get('Authorization')).toMatch(/^Bearer /);
        downloaded = true;
        return new HttpResponse(new Uint8Array([137, 80, 78, 71]), { headers: { 'Content-Type': 'image/png' } });
    }));
    const { unmount } = renderPage(<QuestionDetailPage />, '/questions/:questionId', '/questions/q1');
    try {
        const user = userEvent.setup();
        await user.click(await screen.findByRole('button', { name: 'Предпросмотр' }));
        expect(await screen.findByRole('img', { name: 'image.png' })).toHaveAttribute('src', 'blob:preview');
        expect(screen.getByRole('status')).toHaveTextContent('Загрузка изображения');
        fireEvent.load(screen.getByRole('img', { name: 'image.png' }));
        expect(screen.queryByRole('status')).not.toBeInTheDocument();
        expect(screen.getByRole('dialog', { name: 'image.png' })).toBeInTheDocument();
        expect(downloaded).toBe(true);
        expect(screen.queryByLabelText('Вложения')).not.toBeInTheDocument();
        await user.click(screen.getByRole('button', { name: 'Закрыть' }));
        expect(revoke).toHaveBeenCalledWith('blob:preview');
    } finally {
        unmount();
        if (originalShowModal) Object.defineProperty(HTMLDialogElement.prototype, 'showModal', originalShowModal);
        else Reflect.deleteProperty(HTMLDialogElement.prototype, 'showModal');
        if (originalCreate) Object.defineProperty(URL, 'createObjectURL', originalCreate);
        else Reflect.deleteProperty(URL, 'createObjectURL');
        if (originalRevoke) Object.defineProperty(URL, 'revokeObjectURL', originalRevoke);
        else Reflect.deleteProperty(URL, 'revokeObjectURL');
    }
});
