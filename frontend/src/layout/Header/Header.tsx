import React from 'react';
import './Header.css';
import type { User, UserRole } from '../../types';
import { Icon } from '../../shared/Icon';
import { navigationByRole } from '../Sidebar/navigation';

type Props = {
    role: UserRole;
    user: User;
    onRoleChange: (role: UserRole) => void;
    activePage: string;
};

export const Header: React.FC<Props> = ({
    role,
    onRoleChange,
    activePage
}) => {

    const currentNavItem = navigationByRole[role].find(
        (item) => item.id === activePage
    );

    const pageTitle = currentNavItem?.label || 'NeoLab space';
    return (
        <header className="header">
            <div>
                <h1 className="header__title">
                    {pageTitle}
                </h1>
            </div>

            <div className="header__actions">
                <button className="header__icon">
                    <Icon name="bell" size={18} />
                </button>

                <select
                    value={role}
                    onChange={(e) =>
                        onRoleChange(e.target.value as UserRole)
                    }
                    className="header__select"
                >
                    <option value="student">Student</option>
                    <option value="teacher">Teacher</option>
                    <option value="admin">Admin</option>
                </select>
            </div>
        </header>
    );
};