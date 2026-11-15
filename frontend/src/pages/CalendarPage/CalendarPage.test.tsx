import { screen, waitFor, fireEvent } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { it, expect, vi } from 'vitest';
import { http, HttpResponse } from 'msw';
import CalendarPage from './CalendarPage';
import { API, server } from '../../test/mocks/server';
import { renderPage } from '../../test/render';
import { setSession } from '../../test/helpers/session';

function usePhone() {
    vi.stubGlobal('matchMedia', vi.fn(() => ({
        matches: true,
        addEventListener: vi.fn(),
        removeEventListener: vi.fn(),
    })));
}

it('phone shows stacked months and opens the selected day with events in time order', async () => {
    usePhone();
    setSession('student');
    const date = new Date();
    date.setHours(10, 30, 0, 0);
    const later = new Date(date);
    later.setHours(14, 0);
    server.use(http.get(`${API}/public/calendar/events`, () => HttpResponse.json([
        { id: 'later', title: 'Вебинар', eventType: 1, userId: 'other', startsAtUtc: later.toISOString(), endsAtUtc: later.toISOString() },
        { id: 'first', title: 'Лекция по сетям', eventType: 0, userId: 'other', startsAtUtc: date.toISOString(), endsAtUtc: later.toISOString() },
    ])));
    const { container } = renderPage(<CalendarPage />);
    await waitFor(() => expect(container.querySelector('.mobile-calendar__scheduled')).toBeInTheDocument());
    expect(container.querySelectorAll('.rdp-month')).toHaveLength(3);
    expect(container.querySelector('.cal-card')).not.toBeInTheDocument();
    const user = userEvent.setup();
    await user.click(screen.getByRole('button', { name: /есть события/ }));
    expect(screen.getByRole('list', { name: 'Расписание по часам' })).toBeInTheDocument();
    expect(screen.getAllByRole('article').map(element => element.textContent)).toEqual([
        expect.stringContaining('Лекция по сетям'), expect.stringContaining('Вебинар'),
    ]);
    expect(screen.queryByRole('button', { name: /Создать событие в/ })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /Удалить/ })).not.toBeInTheDocument();
    await user.click(screen.getByRole('button', { name: 'Следующий день' }));
    expect(screen.getByText('На этот день событий нет')).toBeInTheDocument();
    await user.click(screen.getByRole('button', { name: /К месяцам/ }));
    await user.click(screen.getByRole('button', { name: 'Показать следующие месяцы' }));
    expect(container.querySelectorAll('.rdp-month')).toHaveLength(6);
});

it('phone teacher creates an event for the selected hour through the existing API', async () => {
    usePhone();
    setSession('teacher');
    let body: unknown;
    let events: object[] = [];
    let deleted = 0;
    server.use(
        http.get(`${API}/public/calendar/events`, () => HttpResponse.json(events)),
        http.post(`${API}/public/calendar/event`, async ({ request }) => {
            body = await request.json();
            events = [{ ...(body as object), id: 'mobile-event', userId: 'user-1' }];
            return HttpResponse.json(events[0]);
        }),
        http.delete(`${API}/public/calendar/event/mobile-event`, () => {
            deleted++;
            return new HttpResponse(null, { status: 204 });
        }),
    );
    const { container } = renderPage(<CalendarPage />);
    await waitFor(() => expect(screen.queryByRole('status')).not.toBeInTheDocument());
    const user = userEvent.setup();
    // Choose a date in the next month, rather than today's default date.
    const target = new Date(new Date().getFullYear(), new Date().getMonth() + 1, 12);
    const label = target.toLocaleDateString('ru-RU', { day: 'numeric', month: 'long', year: 'numeric' });
    await user.click(screen.getByRole('button', { name: label }));
    await user.click(screen.getByRole('button', { name: 'Создать событие в 14:00' }));
    const inputs = container.querySelectorAll<HTMLInputElement>('input[type=datetime-local]');
    target.setHours(14, 0, 0, 0);
    expect(new Date(inputs[0].value).toISOString()).toBe(target.toISOString());
    await user.type(screen.getByPlaceholderText('Введите название'), 'Мобильное событие');
    await user.click(screen.getByRole('button', { name: 'Создать' }));
    expect(await screen.findByText('Мобильное событие')).toBeInTheDocument();
    expect(body).toMatchObject({ title: 'Мобильное событие', startsAtUtc: target.toISOString() });
    vi.spyOn(window, 'confirm').mockReturnValue(true);
    await user.click(screen.getByRole('button', { name: 'Удалить Мобильное событие' }));
    await waitFor(() => expect(screen.queryByText('Мобильное событие')).not.toBeInTheDocument());
    expect(deleted).toBe(1);
});
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
