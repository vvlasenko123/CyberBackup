import { fireEvent, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { it, expect } from 'vitest';
import { http, HttpResponse } from 'msw';
import ChangePasswordPage from './ChangePasswordPage';
import { API, server } from '../../test/mocks/server';
import { renderPage } from '../../test/render';
import { setSession } from '../../test/helpers/session';
it('validates confirmation, retains the mandatory flag on failure and clears it after retry', async () => {
    setSession('student', true);
    let body: unknown; let calls = 0;
    server.use(http.post(`${API}/public/user/password/change`, async ({ request }) => {
        body = await request.json(); return ++calls === 1 ? HttpResponse.json({ message: 'Неверный текущий пароль' }, { status: 400 }) : new HttpResponse(null, { status: 204 });
    }));
    renderPage(<ChangePasswordPage />);
    const user = userEvent.setup();
    const inputs = screen.getAllByPlaceholderText('••••••••');
    expect(screen.getByRole('button', { name: 'Сохранить' })).toBeDisabled();
    await user.type(inputs[0], 'old'); await user.type(inputs[1], 'new'); await user.type(inputs[2], 'wrong');
    expect(screen.getByText('Пароли не совпадают')).toBeInTheDocument();
    fireEvent.keyDown(inputs[2], { key: 'Enter' }); expect(calls).toBe(0);
    await user.clear(inputs[2]); await user.type(inputs[2], 'new');
    await user.click(screen.getByRole('button', { name: 'Сохранить' }));
    expect(await screen.findByText('Неверный текущий пароль')).toBeInTheDocument();
    expect(localStorage.getItem('must_change_password')).toBe('true');
    await user.keyboard('{Enter}');
    expect(await screen.findByLabelText('Current route')).toHaveTextContent('/dashboard');
    expect(localStorage.getItem('must_change_password')).toBe('false');
    expect(body).toEqual({ currentPassword: 'old', newPassword: 'new' });
});
it('ignores Enter while a password change is pending', async () => {
    let release!: () => void; const pending = new Promise<void>(r => { release = r; }); let calls = 0;
    server.use(http.post(`${API}/public/user/password/change`, async () => { calls++; await pending; return new HttpResponse(null, { status: 204 }); }));
    renderPage(<ChangePasswordPage />); const user = userEvent.setup();
    const inputs = screen.getAllByPlaceholderText('••••••••');
    await user.type(inputs[0], 'old'); await user.type(inputs[1], 'new'); await user.type(inputs[2], 'new');
    try {
        await user.click(screen.getByRole('button', { name: 'Сохранить' }));
        expect(screen.getByRole('button', { name: 'Сохранение...' })).toBeDisabled();
        fireEvent.keyDown(inputs[2], { key: 'Enter' });
        await waitFor(() => expect(calls).toBe(1));
    } finally { release(); }
    expect(await screen.findByLabelText('Current route')).toHaveTextContent('/dashboard'); expect(calls).toBe(1);
});
