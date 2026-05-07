import React from 'react';
import './Header.css';
import type { User, UserRole } from '../../types';
import { Icon } from '../../shared/Icon';

type Props = {
    role: UserRole;
    user: User;
    onRoleChange: (role: UserRole) => void;
};

export const Header: React.FC<Props> = ({
    role,
    onRoleChange,
}) => {
    return (
        <header className="header">
            <div>
                <h1 className="header__title">
                    Новости и объявления
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
                    <option value="instructor">Instructor</option>
                    <option value="admin">Admin</option>
                </select>
            </div>
        </header>
    );
};