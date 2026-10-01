import { describe, expect, it } from 'vitest';
import { screen } from '@testing-library/react';
import LogOut from './LogOut';
import ProtectedRoute from '../components/ProtectedRoute';
import { renderPage } from '../test/render';
import { setSession } from '../test/helpers/session';

describe('LogOut', () => {
    it('removes session credentials and prevents access to a protected page', () => {
        setSession();
        for (const key of ['refreshToken', 'refreshTokenExpiresAt', 'user_id']) localStorage.setItem(key, 'test');
        localStorage.setItem('theme', 'dark');
        const { unmount } = renderPage(<LogOut />, '/logout');
        expect(screen.getByLabelText('Current route')).toHaveTextContent('/login');
        for (const key of ['token', 'expiresAt', 'refreshToken', 'refreshTokenExpiresAt', 'user_id', 'user_role', 'user_name']) {
            expect(localStorage.getItem(key)).toBeNull();
        }
        expect(localStorage.getItem('theme')).toBe('dark');
        unmount();
        renderPage(<ProtectedRoute allowedRoles={['student']}><h1>Private</h1></ProtectedRoute>, '/private');
        expect(screen.getByLabelText('Current route')).toHaveTextContent('/login');
        expect(screen.queryByText('Private')).not.toBeInTheDocument();
    });
    it('allows logout when the session is already empty', () => {
        renderPage(<LogOut />, '/logout');
        expect(screen.getByLabelText('Current route')).toHaveTextContent('/login');
    });
});
