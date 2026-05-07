import React from 'react';
import './Sidebar.css'
import type { User, UserRole } from '../../types';
import { navigationByRole } from './navigation';
import { Icon } from '../../shared/Icon';

type Props = {
    role: UserRole;
    user: User;
    activePage: string;
    onNavigate: (page: string) => void;
    onLogout: () => void;
};

export const Sidebar: React.FC<Props> = ({
    role,
    user,
    activePage,
    onNavigate,
    onLogout,
}) => {
    const nav = navigationByRole[role];

    return (
        <aside className="sidebar">
            <div className="sidebar__logo">
                Neo<span>Lab</span>
            </div>

            <nav className="sidebar__nav">
                {nav.map((item) => (
                    <button
                        key={item.id}
                        className={`sidebar__item ${activePage === item.id ? 'sidebar__item--active' : ''
                            }`}
                        onClick={() => onNavigate(item.id)}
                    >
                        <Icon
                            name={item.icon}
                            size={18}
                            color={activePage === item.id ? '#ffffff' : '#8A94A6'}
                        />

                        <span>{item.label}</span>
                    </button>
                ))}
            </nav>

            <div className="sidebar__footer">
                <div className="sidebar__user">
                    <div className="sidebar__avatar">
                        {user.name[0]}
                    </div>

                    <div>
                        <div className="sidebar__name">
                            {user.name}
                        </div>

                        <div className="sidebar__role">
                            {role}
                        </div>
                    </div>
                </div>

                <button
                    className="sidebar__logout"
                    onClick={onLogout}
                >
                    <Icon name="logout" size={18} />
                </button>
            </div>
        </aside>
    );
};