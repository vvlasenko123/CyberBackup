import { screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { it, expect } from 'vitest';
import { http, HttpResponse } from 'msw';
import GroupDetailPage from './GroupDetailPage';
import { API, server } from '../../test/mocks/server';
import { renderPage } from '../../test/render';
const endpoint = `${API}/public/api/v1/admin/groups`;
it.each(['students', 'teachers'] as const)('adds and removes %s through the real picker UI', async kind => {
    const member = { userId: 'u1', fullName: 'Анна', email: 'anna@example.com' };
    const group = { id: 'g1', name: 'РИ-1', students: [] as typeof member[], teachers: [] as typeof member[] };
    let added = 0; let deleted = 0;
    server.use(http.get(`${endpoint}/g1`, () => HttpResponse.json(group)),
        http.get(`${endpoint}/ungrouped-students`, () => HttpResponse.json(group.students.length ? [] : [member])),
        http.get(`${API}/public/api/v1/user/get-all`, () => HttpResponse.json([{ id: 'u1', fullName: member.fullName, email: member.email, role: 1 }])),
        http.post(`${endpoint}/g1/${kind}/u1`, () => { added++; group[kind] = [member]; return new HttpResponse(null, { status: 204 }); }),
        http.delete(`${endpoint}/g1/${kind}/u1`, () => { deleted++; group[kind] = []; return new HttpResponse(null, { status: 204 }); }));
    const { container } = renderPage(<GroupDetailPage />, '/groups/:groupId', '/groups/g1');
    const user = userEvent.setup(); await screen.findByRole('heading', { name: 'РИ-1' });
    const section = within(container.querySelectorAll('section')[kind === 'students' ? 0 : 1]);
    await user.type(section.getByRole('textbox'), 'АННА');
    await user.click(section.getByRole('button', { name: '+ Добавить' }));
    await user.click(await section.findByRole('button', { name: '×' }));
    await waitFor(() => expect(section.queryByText('Анна')).not.toBeInTheDocument());
    expect(added).toBe(1); expect(deleted).toBe(1);
});
it.each(['students', 'teachers'] as const)('bulk adds selected %s and refreshes membership', async kind => {
    const member = { userId: 'u1', fullName: 'Анна', email: 'anna@example.com' };
    const group = { id: 'g1', name: 'РИ-1', students: [] as typeof member[], teachers: [] as typeof member[] }; let body: unknown;
    server.use(http.get(`${endpoint}/g1`, () => HttpResponse.json(group)),
        http.get(`${endpoint}/ungrouped-students`, () => HttpResponse.json(group.students.length ? [] : [member])),
        http.get(`${API}/public/api/v1/user/get-all`, () => HttpResponse.json([{ ...member, id: 'u1', role: 1 }])),
        http.post(`${endpoint}/g1/${kind}/bulk`, async ({ request }) => { body = await request.json(); group[kind] = [member]; return new HttpResponse(null, { status: 204 }); }));
    renderPage(<GroupDetailPage />, '/groups/:groupId', '/groups/g1'); const user = userEvent.setup();
    await user.click(await screen.findByRole('button', { name: kind === 'students' ? 'Выбрать студентов из списка' : 'Выбрать преподавателей из списка' }));
    await user.click(screen.getByRole('button', { name: 'Выбрать всех (1)' }));
    await user.click(screen.getByRole('button', { name: 'Добавить выбранных (1)' }));
    expect(await screen.findByRole('button', { name: '×' })).toBeInTheDocument();
    expect(body).toEqual({ userIds: ['u1'] });
});
