import React, { useMemo, useState } from 'react';
import './AppLayout.css';
import type { User, UserRole } from '../types';
import { DashboardPage } from '../pages/DashboardPage';
import { LabsPage } from '../pages/LabsPage';
import { ProgressPage } from '../pages/ProgressPage';
import { StatementPage } from '../pages/StatementPage';
import { QuestionsPage } from '../pages/QuestionsPage';
import { CalendarPage } from '../pages/CalendarPage';
import { UsersPage } from '../pages/UsersPage';
import { Sidebar } from './Sidebar/Sidebar';
import { Header } from './Header/Header';

type Props = {
    role: UserRole;
    user: User;
    onLogout: () => void;
    onRoleChange: (role: UserRole) => void;
};

export const AppLayout: React.FC<Props> = ({
    role,
    user,
    onLogout,
    onRoleChange,
}) => {
    const [activePage, setActivePage] = useState('dashboard');

    const renderPage = useMemo(() => {
        switch (activePage) {
            case 'dashboard':
                return <DashboardPage />;

            case 'labs':
                return <LabsPage />;

            case 'progress':
                return <ProgressPage />;

            case 'statement':
                return <StatementPage />;

            case 'questions':
                return <QuestionsPage />;

            case 'calendar':
                return <CalendarPage />;

            case 'users':
                return <UsersPage />;

            default:
                return <DashboardPage />;
        }
    }, [activePage]);

    return (
        <div className="layout">
            <Sidebar
                role={role}
                activePage={activePage}
                onNavigate={setActivePage}
                user={user}
                onLogout={onLogout}
            />

            <div className="layout__content">
                <Header
                    role={role}
                    user={user}
                    onRoleChange={onRoleChange}
                    activePage={activePage}
                />

                <main className="layout__page">
                    {renderPage}
                </main>
            </div>
        </div>
    );
};