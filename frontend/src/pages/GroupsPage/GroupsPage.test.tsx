import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { it, expect, vi } from 'vitest';
import { http, HttpResponse } from 'msw';
import GroupsPage from './GroupsPage';
import { API, server } from '../../test/mocks/server';
import { renderPage } from '../../test/render';
const endpoint = `${API}/public/api/v1/admin/groups`;
it('creates, renames and deletes a group, respecting cancellation', async () => {
    let groups: object[] = []; const bodies: unknown[] = []; let deletes = 0;
    server.use(http.get(endpoint, () => HttpResponse.json(groups)),
        http.post(endpoint, async ({ request }) => { bodies.push(await request.json()); groups = [{ id: 'g1', name: 'РИ-1', studentCount: 0, teacherCount: 0 }]; return new HttpResponse(null, { status: 201 }); }),
        http.put(`${endpoint}/g1`, async ({ request }) => { bodies.push(await request.json()); return new HttpResponse(null, { status: 204 }); }),
        http.delete(`${endpoint}/g1`, () => { deletes++; return new HttpResponse(null, { status: 204 }); }));
    renderPage(<GroupsPage />);
    const user = userEvent.setup();
    await user.click(await screen.findByRole('button', { name: '+ Создать группу' }));
    expect(screen.getByRole('button', { name: 'Создать' })).toBeDisabled();
    await user.type(screen.getByPlaceholderText('Например: РИ-111111'), ' РИ-1 ');
    await user.click(screen.getByRole('button', { name: 'Создать' }));
    await user.click(await screen.findByRole('button', { name: 'Переименовать' }));
    await user.clear(screen.getByRole('textbox')); await user.type(screen.getByRole('textbox'), 'РИ-2{Enter}');
    expect(await screen.findByText('РИ-2')).toBeInTheDocument();
    const confirm = vi.spyOn(window, 'confirm').mockReturnValue(false);
    await user.click(screen.getByRole('button', { name: 'Удалить' })); expect(deletes).toBe(0);
    confirm.mockReturnValue(true);
    await user.click(screen.getByRole('button', { name: 'Удалить' }));
    expect(await screen.findByText('Групп ещё нет. Создайте первую группу.')).toBeInTheDocument();
    expect(bodies).toEqual([{ name: 'РИ-1' }, { name: 'РИ-2' }]); expect(deletes).toBe(1);
});
it('shows creation failure without losing entered name', async () => {
    server.use(http.get(endpoint, () => HttpResponse.json([])), http.post(endpoint, () => HttpResponse.error()));
    renderPage(<GroupsPage />); const user = userEvent.setup();
    await user.click(await screen.findByRole('button', { name: '+ Создать группу' }));
    await user.type(screen.getByRole('textbox'), 'РИ-1{Enter}');
    expect(await screen.findByText('Не удалось создать группу')).toBeInTheDocument();
    await waitFor(() => expect(screen.getByRole('button', { name: 'Создать' })).toBeEnabled());
    expect(screen.getByRole('textbox')).toHaveValue('РИ-1');
});
