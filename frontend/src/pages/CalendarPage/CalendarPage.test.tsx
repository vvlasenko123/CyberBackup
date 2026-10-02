import { screen, waitFor, fireEvent } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { it, expect, vi } from 'vitest';
import { http, HttpResponse } from 'msw';
import CalendarPage from './CalendarPage';
import { API, server } from '../../test/mocks/server';
import { renderPage } from '../../test/render';
import { setSession } from '../../test/helpers/session';
it('student can view events but cannot create or delete them', async () => {
    setSession(); const now = new Date().toISOString();
    server.use(http.get(`${API}/public/calendar/events`, () => HttpResponse.json([{ id: 'e1', userId: 'other', title: 'Экзамен по сетям', eventType: 2, startsAtUtc: now, endsAtUtc: now }])));
    renderPage(<CalendarPage />); expect(await screen.findByText(/Экзамен по сетям/)).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /Добавить/ })).not.toBeInTheDocument(); expect(screen.queryByTitle('Удалить')).not.toBeInTheDocument();
});
it('teacher creates an event with UTC dates and deletes their own event', async () => {
    setSession('teacher'); let body: unknown; let events: object[] = []; let deleted = 0;
    server.use(http.get(`${API}/public/calendar/events`, () => HttpResponse.json(events)),
        http.post(`${API}/public/calendar/event`, async ({ request }) => {
            body = await request.json(); events = [{ ...(body as object), id: 'e1', userId: 'user-1' }]; return new HttpResponse(null, { status: 201 });
        }), http.delete(`${API}/public/calendar/event/e1`, () => { deleted++; return new HttpResponse(null, { status: 204 }); }));
    const { container } = renderPage(<CalendarPage />); const user = userEvent.setup();
    await user.click(screen.getByRole('button', { name: /Добавить/ }));
    await user.type(screen.getByPlaceholderText('Введите название'), 'Практика');
    const date = new Date(); const local = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
    const inputs = container.querySelectorAll('input[type=datetime-local]');
    fireEvent.change(inputs[0], { target: { value: `${local}T10:00` } }); fireEvent.change(inputs[1], { target: { value: `${local}T11:00` } });
    await user.click(screen.getByRole('button', { name: 'Создать' }));
    expect(await screen.findByText(/Практика/)).toBeInTheDocument();
    expect(body).toMatchObject({ title: 'Практика', startsAtUtc: new Date(`${local}T10:00`).toISOString(), endsAtUtc: new Date(`${local}T11:00`).toISOString() });
    vi.spyOn(window, 'confirm').mockReturnValue(true); await user.click(screen.getByTitle('Удалить'));
    await waitFor(() => expect(screen.queryByText(/Практика/)).not.toBeInTheDocument()); expect(deleted).toBe(1);
});
