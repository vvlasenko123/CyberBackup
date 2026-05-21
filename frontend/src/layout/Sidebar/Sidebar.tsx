import React from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import './Sidebar.css';
import type { User, UserRole } from '../../types';
import { navigationByRole } from './navigation';
import { Icon } from '../../shared/Icon';

type Props = {
    role: UserRole;
    user: User;
    onLogout: () => void;
};

export const Sidebar: React.FC<Props> = ({ role, user, onLogout }) => {
    const navigate = useNavigate();
    const location = useLocation();
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
                        className={`sidebar__item ${location.pathname === item.path ? 'sidebar__item--active' : ''}`}
                        onClick={() => navigate(item.path)}
                    >
                        <Icon
                            name={item.icon}
                            size={18}
                            color={location.pathname === item.path ? '#ffffff' : '#8A94A6'}
                        />
                        <span>{item.label}</span>
                    </button>
                ))}
            </nav>

            <div className="sidebar__footer">
                <div className="sidebar__user">
                    <div className="sidebar__avatar">
                        {user.name ? user.name[0].toUpperCase() : '?'}
                    </div>
                    <div>
                        <div className="sidebar__name">{user.name || 'Пользователь'}</div>
                        <div className="sidebar__role">{role}</div>
                    </div>
                </div>

                <button className="sidebar__logout" onClick={onLogout}>
                    <Icon name="logout" size={18} />
                </button>
            </div>
        </aside>
    );
};
