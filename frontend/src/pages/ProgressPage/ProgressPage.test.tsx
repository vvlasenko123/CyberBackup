import { screen } from '@testing-library/react';
import { it, expect } from 'vitest';
import { http, HttpResponse } from 'msw';
import ProgressPage from './ProgressPage';
import { API, server } from '../../test/mocks/server';
import { renderPage } from '../../test/render';
import { progress, leaderboard } from '../../test/fixtures';
it.each([false, true])('renders progress even if leaderboard fails=%s', async fails => {
    server.use(http.get(`${API}/public/api/v1/laboratories/progress/my`, () => HttpResponse.json(progress)),
        http.get(`${API}/public/api/v1/laboratories/progress/leaderboard`, () => fails ? HttpResponse.error() : HttpResponse.json(leaderboard)));
    renderPage(<ProgressPage />);
    expect(await screen.findByText('50%')).toBeInTheDocument(); expect(screen.getByText('Сети')).toBeInTheDocument();
    if (fails) expect(await screen.findByText('Рейтинг недоступен')).toBeInTheDocument();
    else expect((await screen.findAllByText('#1')).length).toBeGreaterThan(0);
});
it('shows progress API errors', async () => {
    server.use(http.get(`${API}/public/api/v1/laboratories/progress/my`, () => HttpResponse.json({ message: 'Нет данных' }, { status: 500 })),
        http.get(`${API}/public/api/v1/laboratories/progress/leaderboard`, () => HttpResponse.json(leaderboard)));
    renderPage(<ProgressPage />); expect(await screen.findByText('Нет данных')).toBeInTheDocument();
});
it('handles zero maximum points without invalid progress widths', async () => {
    server.use(http.get(`${API}/public/api/v1/laboratories/progress/my`, () => HttpResponse.json({ ...progress, laboratories: [
        { laboratoryId: 'a', title: 'Без баллов', status: 3, maxPoints: 0, earnedPoints: 0 },
        { laboratoryId: 'b', title: 'Не начата', status: 0, maxPoints: 0, earnedPoints: 0 },
    ] })), http.get(`${API}/public/api/v1/laboratories/progress/leaderboard`, () => HttpResponse.json({ items: [], currentUserRank: 0 })));
    const { container } = renderPage(<ProgressPage />); await screen.findByText('Без баллов');
    expect([...container.querySelectorAll<HTMLElement>('.prog-lab-bar-track .prog-bar-fill')].map(e => e.style.width)).toEqual(['100%', '0%']);
});
