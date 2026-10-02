import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { it, expect } from 'vitest';
import { http, HttpResponse } from 'msw';
import QuestionsPage from './QuestionsPage';
import { API, server } from '../../test/mocks/server';
import { renderPage } from '../../test/render';
import { setSession } from '../../test/helpers/session';
const question = { id: 'q1', description: 'Как начать?', laboratoryTitle: 'Сети', status: 0, createdAtUtc: '2026-01-01', studentFullName: 'Иван', groupName: 'РИ-1' };
it('opens a student question', async () => {
    setSession(); server.use(http.get(`${API}/public/api/v1/questions/my`, () => HttpResponse.json([question])));
    renderPage(<QuestionsPage />); await userEvent.setup().click(await screen.findByText('Как начать?'));
    expect(screen.getByLabelText('Current route')).toHaveTextContent('/questions/q1');
});
it.each([false, true])('shows student empty/error state: error=%s', async error => {
    setSession(); server.use(http.get(`${API}/public/api/v1/questions/my`, () => error ? HttpResponse.error() : HttpResponse.json([])));
    renderPage(<QuestionsPage />);
    expect(await screen.findByText(error ? 'Не удалось загрузить вопросы' : 'У вас пока нет вопросов')).toBeInTheDocument();
});
it('sends teacher status, laboratory and search filters to the API', async () => {
    setSession('teacher'); let params = new URLSearchParams();
    server.use(http.get(`${API}/public/api/v1/teacher/questions/lab-titles`, () => HttpResponse.json(['Сети'])),
        http.get(`${API}/public/api/v1/teacher/questions`, ({ request }) => { params = new URL(request.url).searchParams; return HttpResponse.json({ items: [question], totalCount: 1 }); }));
    renderPage(<QuestionsPage />); const user = userEvent.setup(); await screen.findByText('Как начать?');
    await user.selectOptions(screen.getAllByRole('combobox')[0], '2');
    await user.selectOptions(screen.getAllByRole('combobox')[1], 'Сети');
    await user.type(screen.getByRole('textbox'), 'Иван');
    await waitFor(() => expect(Object.fromEntries(params)).toMatchObject({ status: '2', laboratoryTitle: 'Сети', search: 'Иван' }));
    await screen.findByText('Как начать?');
});
