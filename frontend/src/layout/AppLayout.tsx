import React, { useState } from 'react';
import { Outlet, useNavigate } from 'react-router-dom';
import './AppLayout.css';
import type { User, UserRole } from '../types';
import { Sidebar } from './Sidebar/Sidebar';
import { Header } from './Header/Header';

export const AppLayout: React.FC = () => {
    const navigate = useNavigate();

    const [role, setRole] = useState<UserRole>(
        () => (localStorage.getItem('user_role') as UserRole) || 'student'
    );

    const user: User = {
        name: localStorage.getItem('user_name') || '',
    };

    const handleRoleChange = (newRole: UserRole) => {
        localStorage.setItem('user_role', newRole);
        setRole(newRole);
    };

    const handleLogout = () => {
        localStorage.removeItem('token');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('expiresAt');
        localStorage.removeItem('refreshTokenExpiresAt');
        localStorage.removeItem('user_id');
        localStorage.removeItem('user_role');
        localStorage.removeItem('user_name');
        navigate('/login');
    };

    return (
        <div className="layout">
            <Sidebar role={role} user={user} onLogout={handleLogout} />

            <div className="layout__content">
                <Header role={role} onRoleChange={handleRoleChange} />

                <main className="layout__page">
                    <Outlet />
                </main>
            </div>
        </div>
    );
};
