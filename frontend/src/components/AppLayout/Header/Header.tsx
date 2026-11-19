import React from 'react';
import { useLocation } from 'react-router-dom';
import './Header.css';
import type { UserRole } from '../../../utils/types';
import { navigationByRole } from '../Sidebar/navigation';

type Props = {
    role: UserRole;
    onRoleChange: (role: UserRole) => void;
    notificationBell?: React.ReactNode;
    onMenuClick?: () => void;
};

export const Header: React.FC<Props> = ({ role, notificationBell, onMenuClick }) => {
    const location = useLocation();

    const currentNavItem = navigationByRole[role].find(
        (item) => location.pathname === item.path || location.pathname.startsWith(item.path + '/')
    );
    const pageTitle = currentNavItem?.label || 'NeoLab space';

    return (
        <header className="header">
            <div className="header__lead">
                <button className="header__burger" onClick={onMenuClick} aria-label="Открыть меню">
                    <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" aria-hidden="true">
                        <line x1="4" y1="7" x2="20" y2="7" />
                        <line x1="4" y1="12" x2="20" y2="12" />
                        <line x1="4" y1="17" x2="20" y2="17" />
                    </svg>
                </button>
                <h1 className="header__title">{pageTitle}</h1>
            </div>

            <div className="header__actions">
                {notificationBell}
            </div>
        </header>
    );
};
