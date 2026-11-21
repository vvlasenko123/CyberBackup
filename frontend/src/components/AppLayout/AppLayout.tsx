import React, { useState } from 'react';
import { Outlet, useNavigate } from 'react-router-dom';
import './AppLayout.css';
import type { User, UserRole } from '../../utils/types';
import { Sidebar } from './Sidebar/Sidebar';
import { Header } from './Header/Header';
import { useNotifications } from '../../features/notifications/useNotifications';
import { NotificationBell } from '../../features/notifications/NotificationBell';
import { NotificationToast } from '../../features/notifications/NotificationToast';
import { useIsDesktop } from '../../hooks/useIsDesktop';

export const AppLayout: React.FC = () => {
    const navigate = useNavigate();

    const isDesktop = useIsDesktop();
    const [menuOpen, setMenuOpen] = useState(false);

    const [role, setRole] = useState<UserRole>(
        () => (localStorage.getItem('user_role') as UserRole) || 'student'
    );

    const user: User = {
        name: localStorage.getItem('user_name') || '',
    };

    const { notifications, loading: notificationsLoading, unreadCount, markAllRead, latestToast, dismissToast } =
        useNotifications();

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
            <Sidebar
                role={role}
                user={user}
                onLogout={handleLogout}
                open={menuOpen}
                onClose={() => setMenuOpen(false)}
                touchDevice={!isDesktop}
            />

            {menuOpen && <div className="layout__overlay" onClick={() => setMenuOpen(false)} />}

            <div className="layout__content">
                <Header
                    role={role}
                    onRoleChange={handleRoleChange}
                    onMenuClick={() => setMenuOpen(true)}
                    notificationBell={
                        <NotificationBell
                            notifications={notifications}
                            loading={notificationsLoading}
                            unreadCount={unreadCount}
                            onMarkAllRead={markAllRead}
                        />
                    }
                />

                <main className="layout__page">
                    <Outlet />
                </main>
            </div>

            {/* Toast — вне layout чтобы не обрезался */}
            <NotificationToast toast={latestToast} onDismiss={dismissToast} />
        </div>
    );
};
