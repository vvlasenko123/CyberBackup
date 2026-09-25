import React from 'react';
import { useLocation } from 'react-router-dom';
import './Header.css';
import type { UserRole } from '../../types';
import { navigationByRole } from '../Sidebar/navigation';

type Props = {
    role: UserRole;
    onRoleChange: (role: UserRole) => void;
    notificationBell?: React.ReactNode;
};

export const Header: React.FC<Props> = ({ role, notificationBell }) => {
    const location = useLocation();

    const currentNavItem = navigationByRole[role].find(
        (item) => location.pathname === item.path || location.pathname.startsWith(item.path + '/')
    );
    const pageTitle = currentNavItem?.label || 'NeoLab space';

    return (
        <header className="header">
            <div>
                <h1 className="header__title">{pageTitle}</h1>
            </div>

            <div className="header__actions">
                {notificationBell}
            </div>
        </header>
    );
};
