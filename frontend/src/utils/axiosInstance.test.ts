import { describe, expect, it } from 'vitest';
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
