import { describe, expect, it } from 'vitest';
import { http, HttpResponse } from 'msw';
import { loginRequest } from './auth';
import { API, server } from '../../test/mocks/server';
import { tokenFor } from '../../test/helpers/session';

describe('loginRequest', () => {
    it.each([
        ['student', 'student'], ['teacher', 'teacher'], ['admin', 'admin'], ['superadmin', 'admin'],
    ])('stores the session for %s', async (role, storedRole) => {
        const response = { accessToken: tokenFor(role), expiresAt: '2099-01-01T00:00:00Z', mustChangePassword: true };
        let body: unknown;
        let authorization: string | null = null;
        server.use(
            http.post(`${API}/public/auth/login`, async ({ request }) => {
                body = await request.json();
                return HttpResponse.json(response);
            }),
            http.get(`${API}/public/api/v1/user/get/user-1`, ({ request }) => {
                authorization = request.headers.get('Authorization');
                return HttpResponse.json({ fullName: 'Test User' });
            }),
        );
        await expect(loginRequest('user@example.com', 'secret')).resolves.toEqual(response);
        expect(body).toEqual({ email: 'user@example.com', password: 'secret' });
        expect(authorization).toBe(`Bearer ${response.accessToken}`);
        expect(localStorage.getItem('token')).toBe(response.accessToken);
        expect(localStorage.getItem('expiresAt')).toBe(response.expiresAt);
        expect(localStorage.getItem('user_id')).toBe('user-1');
        expect(localStorage.getItem('user_role')).toBe(storedRole);
        expect(localStorage.getItem('user_name')).toBe('Test User');
        expect(localStorage.getItem('must_change_password')).toBe('true');
    });
    it.each(['server', 'network'])('falls back to email on a profile %s error', async kind => {
        server.use(
            http.post(`${API}/public/auth/login`, () => HttpResponse.json({ accessToken: tokenFor(), expiresAt: '2099-01-01' })),
            http.get(`${API}/public/api/v1/user/get/user-1`, () => kind === 'network' ? HttpResponse.error() : new HttpResponse(null, { status: 500 })),
        );
        await loginRequest('user@example.com', 'secret');
        expect(localStorage.getItem('user_name')).toBe('user@example.com');
        expect(localStorage.getItem('must_change_password')).toBe('false');
    });
    it.each([true, false])('reports a login error (JSON: %s) without creating a session', async json => {
        server.use(http.post(`${API}/public/auth/login`, () => json
            ? HttpResponse.json({ message: 'Доступ запрещён' }, { status: 403 })
            : new HttpResponse('Unavailable', { status: 500 })));
        await expect(loginRequest('user@example.com', 'wrong')).rejects.toThrow(json ? 'Доступ запрещён' : 'Неверный email или пароль');
        expect(localStorage.length).toBe(0);
    });
});
