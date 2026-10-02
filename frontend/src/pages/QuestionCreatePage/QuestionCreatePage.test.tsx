import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { it, expect } from 'vitest';
import { http, HttpResponse } from 'msw';
import QuestionCreatePage from './QuestionCreatePage';
import { API, server } from '../../test/mocks/server';
import { renderPage } from '../../test/render';
it.each([false, true])('creates question with optional lab=%s after retry', async linked => {
    let body: unknown; let calls = 0;
    server.use(http.get(`${API}/public/api/v1/laboratories`, () => HttpResponse.json({ items: [{ id: 'l1', title: 'Сети' }] })),
        http.post(`${API}/public/api/v1/questions`, async ({ request }) => { body = await request.json(); return ++calls === 1 ? HttpResponse.error() : HttpResponse.json({ id: 'q1' }); }));
    renderPage(<QuestionCreatePage />, '/questions/new', linked ? '/questions/new?lab=сети' : '/questions/new');
    const user = userEvent.setup();
    await screen.findByText(linked ? 'Сети' : '— Не привязывать к лабораторной —');
    await user.type(screen.getByPlaceholderText('Опишите ваш вопрос подробно…'), '   ');
    await user.click(screen.getByRole('button', { name: 'Отправить' }));
    expect(screen.getByText('Введите описание вопроса')).toBeInTheDocument(); expect(calls).toBe(0);
    await user.type(screen.getByPlaceholderText('Опишите ваш вопрос подробно…'), 'Как начать? ');
    await user.click(screen.getByRole('button', { name: 'Отправить' }));
    expect(await screen.findByText('Не удалось создать вопрос. Попробуйте снова.')).toBeInTheDocument();
    await user.click(screen.getByRole('button', { name: 'Отправить' }));
    expect(await screen.findByLabelText('Current route')).toHaveTextContent('/questions/q1');
    expect(body).toEqual({ laboratoryTitle: linked ? 'Сети' : null, description: 'Как начать?' });
});
