import { screen, waitFor } from '@testing-library/react';
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
