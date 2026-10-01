import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { it, expect, vi } from 'vitest';
import { http, HttpResponse } from 'msw';
import DashboardPage from './DashboardPage';
import { API, server } from '../../test/mocks/server';
import { renderPage } from '../../test/render';
import { setSession } from '../../test/helpers/session';
import { progress, leaderboard } from '../../test/fixtures';
const post = { id: 'p1', title: 'Новости курса', content: 'Начинаем', authorFullName: 'Преподаватель', category: 2, createdAtUtc: '2026-01-01' };
it.each([false, true])('shows news and filters categories despite sidebar failures=%s', async fails => {
    setSession(); let category: string | null = null;
    server.use(http.get(`${API}/public/api/v1/posts`, ({ request }) => { category = new URL(request.url).searchParams.get('category'); return HttpResponse.json({ items: [post] }); }),
        http.get(`${API}/public/api/v1/laboratories/progress/my`, () => fails ? HttpResponse.error() : HttpResponse.json(progress)),
        http.get(`${API}/public/api/v1/laboratories/progress/leaderboard`, () => fails ? HttpResponse.error() : HttpResponse.json(leaderboard)));
    renderPage(<DashboardPage />); expect(await screen.findByText('Новости курса')).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: '+ Объявление' })).not.toBeInTheDocument();
    await userEvent.setup().click(screen.getByRole('button', { name: 'События' }));
    await waitFor(() => expect(category).toBe('0')); await screen.findByText('Новости курса');
});
it('teacher publishes and deletes announcements', async () => {
    setSession('teacher'); let posts: typeof post[] = []; let body: unknown;
    server.use(http.get(`${API}/public/api/v1/teacher/gradebook`, () => HttpResponse.json({ items: [] })),
        http.get(`${API}/public/api/v1/posts`, () => HttpResponse.json({ items: posts })),
        http.post(`${API}/public/api/v1/posts`, async ({ request }) => { body = await request.json(); posts = [post]; return new HttpResponse(null, { status: 201 }); }),
        http.delete(`${API}/public/api/v1/posts/p1`, () => new HttpResponse(null, { status: 204 })));
    renderPage(<DashboardPage />); const user = userEvent.setup();
    await user.click(screen.getByRole('button', { name: '+ Объявление' }));
    expect(screen.getByRole('button', { name: 'Опубликовать' })).toBeDisabled();
    await user.type(screen.getByPlaceholderText('Заголовок'), ' Новости курса '); await user.type(screen.getByPlaceholderText('Текст объявления...'), 'Начинаем');
    await user.click(screen.getByRole('button', { name: 'Опубликовать' })); await screen.findByText('Новости курса');
    expect(body).toEqual({ title: 'Новости курса', content: 'Начинаем', category: 2 });
    vi.spyOn(window, 'confirm').mockReturnValue(true); await user.click(screen.getByTitle('Удалить'));
    await waitFor(() => expect(screen.queryByText('Новости курса')).not.toBeInTheDocument());
});
