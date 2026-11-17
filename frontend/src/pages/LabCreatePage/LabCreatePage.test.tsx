import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, it, expect } from 'vitest';
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

beforeEach(() => {
    server.use(http.get(`${API}/public/api/v1/teacher/groups`, () => HttpResponse.json([
        { id: 'group-a', name: 'Группа А' }, { id: 'group-b', name: 'Группа Б' }
    ])));
});

it('defaults to all groups and sends multiple selected groups from one dropdown', async () => {
    let body: unknown;
    server.use(http.get(`${API}/public/api/v1/teacher/laboratories/blocks`, () => HttpResponse.json([])),
        http.post(`${API}/public/api/v1/teacher/laboratories`, async ({ request }) => {
            body = await request.json(); return HttpResponse.json({ id: 'new' });
        }));
    renderPage(<LabCreatePage />);
    const user = userEvent.setup();
    for (const [placeholder, value] of [['SQL Workshop', 'Работа'], ['Блок 1: Веб-безопасность', 'Сети'], ['150', '10'], ['Подробное описание для студентов...', 'Описание']])
        await user.type(screen.getByPlaceholderText(placeholder), value);
    await user.click(screen.getByText('Все группы', { selector: 'summary span' }));
    expect(screen.getByRole('checkbox', { name: 'Все группы' })).toBeChecked();
    await user.click(await screen.findByRole('checkbox', { name: 'Группа А' }));
    await user.click(screen.getByRole('checkbox', { name: 'Группа Б' }));
    await user.click(screen.getByRole('button', { name: 'Опубликовать' }));
    expect(await screen.findByLabelText('Current route')).toHaveTextContent('/labs/new');
    expect(body).toMatchObject({ groupIds: ['group-a', 'group-b'], isPublished: true });
});

it('retries loading groups while preserving the default selection', async () => {
    let attempts = 0;
    server.use(http.get(`${API}/public/api/v1/teacher/laboratories/blocks`, () => HttpResponse.json([])),
        http.get(`${API}/public/api/v1/teacher/groups`, () => ++attempts === 1
            ? HttpResponse.error() : HttpResponse.json([{ id: 'group-a', name: 'Группа А' }])));
    renderPage(<LabCreatePage />);
    const user = userEvent.setup();
    await user.click(screen.getByText('Все группы', { selector: 'summary span' }));
    await user.click(await screen.findByRole('button', { name: 'Повторить' }));
    expect(await screen.findByRole('checkbox', { name: 'Группа А' })).not.toBeChecked();
    expect(screen.getByRole('checkbox', { name: 'Все группы' })).toBeChecked();
});
