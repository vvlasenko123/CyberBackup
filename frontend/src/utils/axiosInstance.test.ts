import { describe, expect, it, vi } from 'vitest';
import { http, HttpResponse } from 'msw';
import axiosInstance from './axiosInstance';
import { API, server } from '../test/mocks/server';
import { setSession, tokenFor } from '../test/helpers/session';

describe('axiosInstance', () => {
    it.each([false, true])('sets authorization only for an active session: %s', async authenticated => {
        if (authenticated) setSession();
        let authorization: string | null = null;
        server.use(http.get(`${API}/test-resource`, ({ request }) => {
            authorization = request.headers.get('Authorization');
            return HttpResponse.json({ value: 42 });
        }));
        const response = await axiosInstance.get('/test-resource');
        expect(response.data).toEqual({ value: 42 });
        expect(authorization).toBe(authenticated ? `Bearer ${tokenFor()}` : null);
    });
    it('propagates a server error without clearing the session', async () => {
        setSession();
        server.use(http.get(`${API}/test-resource`, () => new HttpResponse(null, { status: 500 })));
        await expect(axiosInstance.get('/test-resource')).rejects.toMatchObject({ response: { status: 500 } });
        expect(localStorage.getItem('token')).toBe(tokenFor());
        expect(localStorage.getItem('user_role')).toBe('student');
    });
});

describe('session lifecycle', () => {
    it('uses the current token on every request, including after removal', async () => {
        const headers: (string | null)[] = [];
        server.use(http.get(`${API}/test-resource`, ({ request }) => {
            headers.push(request.headers.get('Authorization'));
            return HttpResponse.json({});
        }));
        setSession();
        await axiosInstance.get('/test-resource');
        localStorage.setItem('token', tokenFor('teacher'));
        await axiosInstance.get('/test-resource');
        localStorage.removeItem('token');
        await axiosInstance.get('/test-resource');
        expect(headers).toEqual([`Bearer ${tokenFor()}`, `Bearer ${tokenFor('teacher')}`, null]);
    });
    it('clears authentication on 401 and requests a login redirect', async () => {
        setSession();
        localStorage.setItem('theme', 'dark');
        const location = { href: '/private' };
        const originalWindow = window;
        vi.stubGlobal('window', new Proxy(originalWindow, {
            get(target, key) { return key === 'location' ? location : Reflect.get(target, key); },
        }));
        server.use(http.get(`${API}/test-resource`, () => new HttpResponse(null, { status: 401 })));
        await expect(axiosInstance.get('/test-resource')).rejects.toMatchObject({ response: { status: 401 } });
        expect(location.href).toBe('/login');
        for (const key of ['token', 'expiresAt', 'user_role', 'user_name']) {
            expect(localStorage.getItem(key)).toBeNull();
        }
        expect(localStorage.getItem('theme')).toBe('dark');
    });
    it.each([403, 503])('preserves the session on HTTP %s', async status => {
        setSession();
        server.use(http.get(`${API}/test-resource`, () => new HttpResponse(null, { status })));
        await expect(axiosInstance.get('/test-resource')).rejects.toMatchObject({ response: { status } });
        expect(localStorage.getItem('token')).toBe(tokenFor());
        expect(window.location.pathname).toBe('/');
    });
    it('preserves the session on a network failure', async () => {
        setSession();
        server.use(http.get(`${API}/test-resource`, () => HttpResponse.error()));
        await expect(axiosInstance.get('/test-resource')).rejects.toThrow();
        expect(localStorage.getItem('token')).toBe(tokenFor());
    });
});
