import { describe, expect, it, vi } from 'vitest';
import { screen } from '@testing-library/react';
import ProtectedRoute from './ProtectedRoute';
import { renderPage } from '../test/render';
import { setSession } from '../test/helpers/session';

function renderProtected(roles = ['student'], path = '/private') {
    renderPage(<ProtectedRoute allowedRoles={roles}><h1>Private content</h1></ProtectedRoute>, path);
}

describe('ProtectedRoute', () => {
    it.each(['token', 'expiresAt'])('redirects when %s is missing', key => {
        setSession();
        localStorage.removeItem(key);
        renderProtected();
        expect(screen.getByLabelText('Current route')).toHaveTextContent('/login');
        expect(screen.queryByText('Private content')).not.toBeInTheDocument();
    });
    it('clears an expired session and redirects to login', () => {
        setSession();
        localStorage.setItem('expiresAt', '2000-01-01T00:00:00Z');
        renderProtected();
        expect(screen.getByLabelText('Current route')).toHaveTextContent('/login');
        for (const key of ['token', 'expiresAt', 'user_role', 'user_name']) {
            expect(localStorage.getItem(key)).toBeNull();
        }
    });
    it.each(['teacher', 'admin', null])('rejects an unauthorized role: %s', role => {
        setSession();
        if (role) localStorage.setItem('user_role', role);
        else localStorage.removeItem('user_role');
        renderProtected();
        expect(screen.getByLabelText('Current route')).toHaveTextContent('/unauthorized');
    });
    it.each(['student', 'teacher', 'admin'])('allows the permitted role: %s', role => {
        setSession(role);
        renderProtected([role]);
        expect(screen.getByRole('heading', { name: 'Private content' })).toBeInTheDocument();
    });
    it('requires a password change', () => {
        setSession('student', true);
        renderProtected();
        expect(screen.getByLabelText('Current route')).toHaveTextContent('/change-password');
    });
    it('allows access to the password change route', () => {
        setSession('student', true);
        renderProtected(['student'], '/change-password');
        expect(screen.getByRole('heading')).toHaveTextContent('Private content');
    });
});

it.each(['not-a-date', '2026-01-01T12:00:00.000Z'])('rejects invalid or exactly expired expiry: %s', expiresAt => {
    vi.spyOn(Date, 'now').mockReturnValue(Date.parse('2026-01-01T12:00:00.000Z'));
    setSession(); localStorage.setItem('expiresAt', expiresAt);
    renderProtected();
    expect(screen.getByLabelText('Current route')).toHaveTextContent('/login');
    expect(localStorage.getItem('token')).toBeNull();
});
it('allows the last millisecond before expiration', () => {
    vi.spyOn(Date, 'now').mockReturnValue(Date.parse('2026-01-01T12:00:00.000Z') - 1);
    setSession(); localStorage.setItem('expiresAt', '2026-01-01T12:00:00.000Z');
    renderProtected(); expect(screen.getByText('Private content')).toBeInTheDocument();
});
