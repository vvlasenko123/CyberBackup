import { describe, expect, it } from 'vitest';
import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import LoginPage from './LoginPage';
import { renderPage } from '../../test/render';
import { API, server } from '../../test/mocks/server';
import { tokenFor } from '../../test/helpers/session';

async function fillCredentials() {
    const user = userEvent.setup();
    await user.type(screen.getByPlaceholderText('student@urfu.ru'), 'user@example.com');
    await user.type(screen.getByPlaceholderText('••••••••'), 'secret');
    return user;
}

describe('LoginPage', () => {
    it('validates empty fields without sending a request', async () => {
        let calls = 0;
        server.use(http.post(`${API}/public/auth/login`, () => { calls++; return HttpResponse.json({}); }));
        renderPage(<LoginPage />, '/login');
        await userEvent.setup().click(screen.getByRole('button', { name: 'Войти' }));
        expect(screen.getByText('Заполните все поля')).toBeInTheDocument();
        expect(calls).toBe(0);
    });
    it.each([false, true])('logs in and redirects (password change: %s)', async mustChangePassword => {
        let body: unknown;
        server.use(
            http.post(`${API}/public/auth/login`, async ({ request }) => {
                body = await request.json();
                return HttpResponse.json({ accessToken: tokenFor(), expiresAt: '2099-01-01', mustChangePassword });
            }),
            http.get(`${API}/public/api/v1/user/get/user-1`, () => HttpResponse.json({ fullName: 'Test User' })),
        );
        renderPage(<LoginPage />, '/login');
        const user = await fillCredentials();
        await user.keyboard('{Enter}');
        expect(await screen.findByLabelText('Current route')).toHaveTextContent(mustChangePassword ? '/change-password' : '/dashboard');
        expect(body).toEqual({ email: 'user@example.com', password: 'secret' });
        expect(localStorage.getItem('token')).toBe(tokenFor());
    });
    it('disables submission while pending and allows retry after failure', async () => {
        let release!: () => void;
        const pending = new Promise<void>(resolve => { release = resolve; });
        server.use(http.post(`${API}/public/auth/login`, async () => {
            await pending;
            return HttpResponse.json({ message: 'Неверный пароль' }, { status: 401 });
        }));
        renderPage(<LoginPage />, '/login');
        const user = await fillCredentials();
        await user.click(screen.getByRole('button', { name: 'Войти' }));
        try {
            expect(screen.getByRole('button', { name: 'Вход...' })).toBeDisabled();
        } finally { release(); }
        expect(await screen.findByText('Неверный пароль')).toBeInTheDocument();
        expect(screen.getByRole('button', { name: 'Войти' })).toBeEnabled();
    });
    it('shows a connection error', async () => {
        server.use(http.post(`${API}/public/auth/login`, () => HttpResponse.error()));
        renderPage(<LoginPage />, '/login');
        const user = await fillCredentials();
        await user.click(screen.getByRole('button', { name: 'Войти' }));
        expect(await screen.findByText('Ошибка соединения')).toBeInTheDocument();
    });
});
