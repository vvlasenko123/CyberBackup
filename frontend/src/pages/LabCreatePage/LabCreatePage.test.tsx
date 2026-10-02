import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { it, expect } from 'vitest';
import { http, HttpResponse } from 'msw';
import LabCreatePage from './LabCreatePage';
import { API, server } from '../../test/mocks/server';
import { renderPage } from '../../test/render';

it.each([true, false])('creates a laboratory with publication=%s and normalized payload', async publish => {
    let body: unknown;
    server.use(http.get(`${API}/public/api/v1/teacher/laboratories/blocks`, () => HttpResponse.json(['Сети'])),
        http.post(`${API}/public/api/v1/teacher/laboratories`, async ({ request }) => {
            body = await request.json(); return HttpResponse.json({ id: 'new' });
        }));
    renderPage(<LabCreatePage />);
    const user = userEvent.setup();
    expect(screen.getByRole('button', { name: 'Опубликовать' })).toBeDisabled();
    await user.type(screen.getByPlaceholderText('SQL Workshop'), '  Новая работа  ');
    await user.type(screen.getByPlaceholderText('Блок 1: Веб-безопасность'), 'Сети');
    await user.type(screen.getByPlaceholderText('150'), '10');
    await user.type(screen.getByPlaceholderText('Подробное описание для студентов...'), ' Задание ');
    await user.click(screen.getByRole('button', { name: publish ? 'Опубликовать' : 'Сохранить как черновик' }));
    expect(await screen.findByLabelText('Current route')).toHaveTextContent('/labs/new');
    expect(body).toMatchObject({
        title: 'Новая работа', description: 'Задание', block: 'Сети', maxPoints: 10,
        isPublished: publish, environmentUrl: null, credentials: null, deadlineAtUtc: null
    });
});

it('retains the form after a server failure and allows retry', async () => {
    let calls = 0;
    server.use(http.get(`${API}/public/api/v1/teacher/laboratories/blocks`, () => HttpResponse.error()),
        http.post(`${API}/public/api/v1/teacher/laboratories`, () => ++calls === 1
            ? HttpResponse.json({ message: 'Название занято' }, { status: 409 }) : HttpResponse.json({ id: 'new' })));
    renderPage(<LabCreatePage />);
    const user = userEvent.setup();
    for (const [placeholder, value] of [['SQL Workshop', 'Работа'], ['Блок 1: Веб-безопасность', 'Сети'], ['150', '10'], ['Подробное описание для студентов...', 'Описание']])
        await user.type(screen.getByPlaceholderText(placeholder), value);
    await user.click(screen.getByRole('button', { name: 'Опубликовать' }));
    expect(await screen.findByText('Название занято')).toBeInTheDocument();
    expect(screen.getByPlaceholderText('SQL Workshop')).toHaveValue('Работа');
    await user.click(screen.getByRole('button', { name: 'Опубликовать' }));
    expect(await screen.findByLabelText('Current route')).toHaveTextContent('/labs/new');
    expect(calls).toBe(2);
});
