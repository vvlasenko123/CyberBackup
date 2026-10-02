import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { it, expect } from 'vitest';
import { http, HttpResponse } from 'msw';
import LabEditPage from './LabEditPage';
import { API, server } from '../../test/mocks/server';
import { renderPage } from '../../test/render';
import { labDetail } from '../../test/fixtures';

it('loads and saves edited fields even when block suggestions fail', async () => {
    let body: unknown;
    server.use(http.get(`${API}/public/api/v1/teacher/laboratories/lab-1`, () => HttpResponse.json(labDetail)),
        http.get(`${API}/public/api/v1/teacher/laboratories/blocks`, () => HttpResponse.error()),
        http.put(`${API}/public/api/v1/teacher/laboratories/lab-1`, async ({ request }) => { body = await request.json(); return new HttpResponse(null, { status: 204 }); }));
    renderPage(<LabEditPage />, '/labs/:labId/edit', '/labs/lab-1/edit');
    const user = userEvent.setup();
    const title = await screen.findByDisplayValue(labDetail.title);
    await user.clear(title); await user.type(title, ' Обновлено ');
    await user.click(screen.getByRole('button', { name: /черновик/i }));
    expect(await screen.findByLabelText('Current route')).toHaveTextContent('/labs/lab-1');
    expect(body).toMatchObject({ title: 'Обновлено', isPublished: false, maxPoints: 10, sortOrder: 2 });
});
it('shows a load error', async () => {
    server.use(http.get(`${API}/public/api/v1/teacher/laboratories/lab-1`, () => new HttpResponse(null, { status: 404 })),
        http.get(`${API}/public/api/v1/teacher/laboratories/blocks`, () => HttpResponse.json([])));
    renderPage(<LabEditPage />, '/labs/:labId/edit', '/labs/lab-1/edit');
    expect(await screen.findByText('Не удалось загрузить лабораторную работу')).toBeInTheDocument();
});
it('preserves values on save failure and publishes on retry with an unchanged deadline', async () => {
    let attempts = 0; let body: unknown;
    server.use(http.get(`${API}/public/api/v1/teacher/laboratories/lab-1`, () => HttpResponse.json({ ...labDetail, deadlineAtUtc: '2027-03-10T12:30:00.000Z' })),
        http.get(`${API}/public/api/v1/teacher/laboratories/blocks`, () => HttpResponse.json(['Сети'])),
        http.put(`${API}/public/api/v1/teacher/laboratories/lab-1`, async ({ request }) => { body = await request.json(); return ++attempts === 1 ? HttpResponse.json({ message: 'Ошибка сохранения' }, { status: 500 }) : new HttpResponse(null, { status: 204 }); }));
    renderPage(<LabEditPage />, '/labs/:labId/edit', '/labs/lab-1/edit'); const user = userEvent.setup();
    await user.click(await screen.findByRole('button', { name: /опубликовать/i }));
    expect(await screen.findByText('Ошибка сохранения')).toBeInTheDocument(); expect(screen.getByDisplayValue(labDetail.title)).toBeInTheDocument();
    await user.click(screen.getByRole('button', { name: /опубликовать/i }));
    expect(await screen.findByLabelText('Current route')).toHaveTextContent('/labs/lab-1');
    expect(body).toMatchObject({ isPublished: true, deadlineAtUtc: '2027-03-10T12:30:00.000Z' });
});
