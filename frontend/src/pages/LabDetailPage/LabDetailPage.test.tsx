import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { it, expect } from 'vitest';
import { http, HttpResponse } from 'msw';
import LabDetailPage from './LabDetailPage';
import { API, server } from '../../test/mocks/server';
import { renderPage } from '../../test/render';
import { setSession } from '../../test/helpers/session';
import { labDetail } from '../../test/fixtures';
it.each(['student', 'teacher'])('loads role-specific detail and navigates as %s', async role => {
    setSession(role); server.use(http.get(`${API}/public/api/v1/${role === 'teacher' ? 'teacher/' : ''}laboratories/lab-1`, () => HttpResponse.json(labDetail)));
    renderPage(<LabDetailPage />, '/labs/:labId', '/labs/lab-1');
    expect(await screen.findByText('Изучите сеть')).toBeInTheDocument();
    await userEvent.setup().click(screen.getByRole('button', { name: role === 'teacher' ? /Редактировать/ : 'Загрузить отчет' }));
    expect(screen.getByLabelText('Current route')).toHaveTextContent(role === 'teacher' ? '/labs/lab-1/edit' : '/labs/lab-1/report');
});
it('blocks upload after deadline', async () => {
    setSession(); server.use(http.get(`${API}/public/api/v1/laboratories/lab-1`, () => HttpResponse.json({ ...labDetail, deadlineAtUtc: '2000-01-01' })));
    renderPage(<LabDetailPage />, '/labs/:labId', '/labs/lab-1');
    expect(await screen.findByRole('button', { name: 'Загрузить отчет' })).toBeDisabled(); expect(screen.getByText('Срок сдачи истёк')).toBeInTheDocument();
});
it('shows load failure', async () => {
    setSession(); server.use(http.get(`${API}/public/api/v1/laboratories/lab-1`, () => HttpResponse.error()));
    renderPage(<LabDetailPage />, '/labs/:labId', '/labs/lab-1'); expect(await screen.findByText('Не удалось загрузить лабораторную работу')).toBeInTheDocument();
});

it('opens hints and retries an incorrect flag without losing the laboratory', async () => {
    setSession(); let body: unknown; let calls = 0;
    server.use(http.get(`${API}/public/api/v1/laboratories/lab-1`, () => HttpResponse.json({
        ...labDetail, hasFlag: true,
        hints: [{ id: 'h1', orderNumber: 1, title: 'Подсказка', penaltyPoints: 2, isOpened: false, text: null }]
    })),
        http.post(`${API}/public/api/v1/laboratories/lab-1/hints/h1/open`, () => HttpResponse.json({ text: 'Проверьте порты', penaltyPoints: 2 })),
        http.post(`${API}/public/api/v1/laboratories/lab-1/flag`, async ({ request }) => { body = await request.json(); return HttpResponse.json({ isCorrect: ++calls > 1, message: 'Неверный флаг', earnedPoints: 8 }); }));
    renderPage(<LabDetailPage />, '/labs/:labId', '/labs/lab-1'); const user = userEvent.setup();
    await user.click(await screen.findByRole('button', { name: 'Открыть подсказку #1' }));
    expect(await screen.findByText('Проверьте порты')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Сдать флаг' })).toBeDisabled();
    await user.type(screen.getByPlaceholderText('Введите флаг...'), ' wrong ');
    await user.click(screen.getByRole('button', { name: 'Сдать флаг' })); expect(await screen.findByText('Неверный флаг')).toBeInTheDocument();
    await user.clear(screen.getByPlaceholderText('Введите флаг...')); await user.type(screen.getByPlaceholderText('Введите флаг...'), ' correct{Enter}');
    expect(await screen.findByText('Флаг уже сдан')).toBeInTheDocument(); expect(body).toEqual({ flag: 'correct' });
});
