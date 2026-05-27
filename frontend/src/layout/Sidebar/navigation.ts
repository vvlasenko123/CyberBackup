import type { IconName } from '../../shared/Icon';
import type { UserRole } from '../../types';

export type NavItem = {
    id: string;
    label: string;
    icon: IconName;
    path: string;
};

export const navigationByRole: Record<UserRole, NavItem[]> = {
    student: [
        { id: 'dashboard', label: 'Главная', icon: 'home', path: '/dashboard' },
        { id: 'labs', label: 'Лабораторные', icon: 'lightbulb', path: '/labs' },
        { id: 'progress', label: 'Мой прогресс', icon: 'trending', path: '/progress' },
        { id: 'statement', label: 'Ведомость', icon: 'table', path: '/statement' },
        { id: 'questions', label: 'Вопросы', icon: 'messageSquare', path: '/questions' },
        { id: 'calendar', label: 'Календарь', icon: 'calendar', path: '/calendar' },
    ],

    teacher: [
        { id: 'dashboard', label: 'Главная', icon: 'home', path: '/dashboard' },
        { id: 'labs', label: 'Отчеты студентов', icon: 'lightbulb', path: '/labs' },
        { id: 'questions', label: 'Вопросы студентов', icon: 'messageSquare', path: '/questions' },
        { id: 'calendar', label: 'Календарь', icon: 'calendar', path: '/calendar' },
        { id: 'statement', label: 'Ведомость', icon: 'table', path: '/statement' },
    ],

    admin: [
        { id: 'dashboard', label: 'Главная', icon: 'home', path: '/dashboard' },
        { id: 'users', label: 'Пользователи', icon: 'users', path: '/users' },
        { id: 'groups', label: 'Группы', icon: 'users', path: '/groups' },
        { id: 'calendar', label: 'Календарь', icon: 'calendar', path: '/calendar' },
    ],
};
