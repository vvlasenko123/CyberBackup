import { screen, within, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { it, expect, vi } from 'vitest';
import { http, HttpResponse } from 'msw';
import UsersPage from './UsersPage';
import { API, server } from '../../test/mocks/server';
import { renderPage } from '../../test/render';
import { student } from '../../test/fixtures';
const endpoint = `${API}/public/api/v1/user`;
function setup() {
    server.use(http.get(`${endpoint}/get-all`, () => HttpResponse.json([student])),
        http.get(`${API}/public/api/v1/admin/groups`, () => HttpResponse.json([{ id: 'g1', name: 'РИ-1' }])));
}
it('creates a user with selected role and validates required fields', async () => {
    setup(); let body: unknown;
    server.use(http.post(`${endpoint}/create`, async ({ request }) => { body = await request.json(); return new HttpResponse(null, { status: 201 }); }));
    const { container } = renderPage(<UsersPage />); const user = userEvent.setup();
    await screen.findByText(student.email);
    await user.click(screen.getByRole('button', { name: '+ Создать пользователя' }));
    await user.click(screen.getByRole('button', { name: 'Создать' })); expect(body).toBeUndefined();
    const inputs = container.querySelectorAll<HTMLInputElement>('.users-modal input');
    await user.type(inputs[0], ' Анна '); await user.type(inputs[1], 'anna@example.com'); await user.type(inputs[2], 'secret');
    await user.selectOptions(within(container.querySelector('.users-modal') as HTMLElement).getByRole('combobox'), '1');
    await user.click(screen.getByRole('button', { name: 'Создать' }));
    await waitFor(() => expect(container.querySelector('.users-modal')).toBeNull());
    expect(body).toEqual({ fullName: 'Анна', email: 'anna@example.com', password: 'secret', role: 1 });
});
it.each([false, true])('edits a user with password reset=%s', async reset => {
    setup(); let body: unknown;
    server.use(http.get(`${endpoint}/get/user-1`, () => HttpResponse.json(student)),
        http.put(`${endpoint}/update/user-1`, async ({ request }) => { body = await request.json(); return new HttpResponse(null, { status: 204 }); }));
    renderPage(<UsersPage />); const user = userEvent.setup();
    await user.click(await screen.findByRole('button', { name: 'Редактировать' }));
    const name = await screen.findByDisplayValue(student.fullName);
    await user.clear(name); await user.type(name, 'Новое имя');
    if (reset) await user.type(screen.getByPlaceholderText('Оставьте пустым, чтобы не менять'), 'new-secret');
    await user.click(screen.getByRole('button', { name: 'Сохранить' }));
    await waitFor(() => expect(body).toMatchObject({ fullName: 'Новое имя', password: reset ? 'new-secret' : null, mustChangePassword: reset }));
    await waitFor(() => expect(screen.queryByDisplayValue('Новое имя')).not.toBeInTheDocument());
});
it('requires confirmation before deleting and preserves the row on failure', async () => {
    setup(); let calls = 0;
    server.use(http.delete(`${endpoint}/delete/user-1`, () => new HttpResponse(null, { status: ++calls === 1 ? 500 : 204 })));
    const confirm = vi.spyOn(window, 'confirm').mockReturnValue(false); const alert = vi.spyOn(window, 'alert').mockImplementation(() => { });
    renderPage(<UsersPage />); const user = userEvent.setup();
    await user.click(await screen.findByRole('button', { name: 'Удалить' })); expect(calls).toBe(0);
    confirm.mockReturnValue(true); await user.click(screen.getByRole('button', { name: 'Удалить' }));
    await waitFor(() => expect(alert).toHaveBeenCalled()); expect(screen.getByText(student.email)).toBeInTheDocument();
    await user.click(screen.getByRole('button', { name: 'Удалить' }));
    await waitFor(() => expect(screen.queryByText(student.email)).not.toBeInTheDocument());
});
it('imports the actual RTF file and displays partial success and errors', async () => {
    setup(); let filename = ''; let contents = '';
    server.use(http.post(`${endpoint}/bulk-import`, async ({ request }) => {
        const data = await request.formData(); const file = data.get('file') as File;
        filename = file.name; contents = await file.text();
        return HttpResponse.json({
            imported: [
                { fullName: 'Анна', email: 'anna@example.com', success: true, password: 'generated' },
                { fullName: 'Ошибка', email: 'bad@example.com', success: false, error: 'Email занят' },
            ]
        });
    }));
    const { container } = renderPage(<UsersPage />); await screen.findByText(student.email);
    await userEvent.setup().upload(container.querySelector<HTMLInputElement>('input[type=file]')!, new File(['{rtf data}'], 'students.rtf', { type: 'application/rtf' }));
    expect(await screen.findByText(/Email занят/)).toBeInTheDocument(); expect(screen.getByText(/Анна — anna@example.com/)).toBeInTheDocument();
    expect(filename).toBe('students.rtf'); expect(contents).toBe('{rtf data}');
    expect(container.querySelector('input[type=file]')).toHaveValue('');
});
it('shows import failure and permits selecting the same file again', async () => {
    setup(); server.use(http.post(`${endpoint}/bulk-import`, () => HttpResponse.json({ message: 'Неверный RTF' }, { status: 400 })));
    const { container } = renderPage(<UsersPage />); await screen.findByText(student.email);
    const input = container.querySelector<HTMLInputElement>('input[type=file]')!;
    await userEvent.setup().upload(input, new File(['bad'], 'bad.rtf', { type: 'application/rtf' }));
    expect(await screen.findByText('Неверный RTF')).toBeInTheDocument(); expect(input).toBeEnabled(); expect(input).toHaveValue('');
});
