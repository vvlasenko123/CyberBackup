import { act, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { expect, it } from 'vitest';
import { http, HttpResponse } from 'msw';
import CalendarPage from '../../pages/CalendarPage/CalendarPage';
import ProgressPage from '../../pages/ProgressPage/ProgressPage';
import UsersPage from '../../pages/UsersPage/UsersPage';
import { API, server } from '../../test/mocks/server';
import { renderPage } from '../../test/render';
import { leaderboard, progress, student } from '../../test/fixtures';
import { setSession } from '../../test/helpers/session';

function pendingRequest() {
    let release!: () => void;
    const promise = new Promise<void>(resolve => { release = resolve; });
    return { promise, release };
}

it('keeps all calendar cells mounted while events are loading', async () => {
    setSession();
    const request = pendingRequest();
    server.use(http.get(`${API}/public/calendar/events`, async () => {
        await request.promise;
        return HttpResponse.json([]);
    }));
    const { container } = renderPage(<CalendarPage />);
    const cells = container.querySelectorAll('.cal-day').length;
    expect(cells).toBeGreaterThanOrEqual(28);
    expect(screen.getByRole('status')).toHaveAttribute('aria-busy', 'true');
    await act(async () => request.release());
    await waitFor(() => expect(screen.queryByRole('status')).not.toBeInTheDocument());
    expect(container.querySelectorAll('.cal-day')).toHaveLength(cells);
});

it('keeps the leaderboard skeleton until its own request settles, even after progress is ready', async () => {
    const request = pendingRequest();
    server.use(
        http.get(`${API}/public/api/v1/laboratories/progress/my`, () => HttpResponse.json(progress)),
        http.get(`${API}/public/api/v1/laboratories/progress/leaderboard`, async () => {
            await request.promise;
            return HttpResponse.json(leaderboard);
        }),
    );
    const { container } = renderPage(<ProgressPage />);
    await screen.findByText('50%');
    expect(container.querySelector('.prog-leaderboard-card [aria-busy="true"]')).toBeInTheDocument();
    expect(screen.queryByText('Вы ещё не добавлены в группу')).not.toBeInTheDocument();
    await act(async () => request.release());
    await screen.findAllByText('#1');
    expect(screen.queryByRole('status')).not.toBeInTheDocument();
});

it('shows a pending user form immediately and does not reopen it after cancellation', async () => {
    const request = pendingRequest();
    let started = false;
    server.use(
        http.get(`${API}/public/api/v1/user/get-all`, () => HttpResponse.json([student])),
        http.get(`${API}/public/api/v1/admin/groups`, () => HttpResponse.json([])),
        http.get(`${API}/public/api/v1/user/get/${student.id}`, async () => {
            started = true;
            await request.promise;
            return HttpResponse.json(student);
        }),
    );
    const { container } = renderPage(<UsersPage />);
    const user = userEvent.setup();
    await user.click(await screen.findByRole('button', { name: 'Редактировать' }));
    await waitFor(() => expect(started).toBe(true));
    expect(screen.getByText('Редактировать пользователя')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Сохранить' })).toBeDisabled();
    expect(container.querySelector('.users-modal [aria-busy="true"]')).toBeInTheDocument();
    await user.click(screen.getByRole('button', { name: 'Отмена' }));
    await act(async () => request.release());
    await waitFor(() => expect(screen.queryByText('Редактировать пользователя')).not.toBeInTheDocument());
});
