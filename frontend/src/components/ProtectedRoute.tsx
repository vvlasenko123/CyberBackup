import { Navigate } from 'react-router-dom';
import type { ReactNode } from 'react';

function isTokenExpired(expiresAt: string) {
    return new Date(expiresAt) < new Date();
}

function ProtectedRoute({ children, allowedRoles }: { children: ReactNode; allowedRoles: string[] }) {
    const token = localStorage.getItem('token');
    const expiresAt = localStorage.getItem('expiresAt');

    if (!token || !expiresAt) {
        return <Navigate to="/login" />;
    }

    if (isTokenExpired(expiresAt)) {
        localStorage.removeItem('token');
        localStorage.removeItem('expiresAt');
        localStorage.removeItem('user_role');
        localStorage.removeItem('user_name');
        return <Navigate to="/login" />;
    }

    const userRole = localStorage.getItem('user_role');
    if (allowedRoles && (!userRole || !allowedRoles.includes(userRole))) {
        return <Navigate to="/unauthorized" />;
    }

    return <>{children}</>;
}

export default ProtectedRoute;
