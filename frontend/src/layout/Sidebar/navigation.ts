import type { IconName } from '../../shared/Icon';
import type { UserRole } from '../../types';

export type NavItem = {
    id: string;
    label: string;
    icon: IconName;
};

export const navigationByRole: Record<UserRole, NavItem[]> = {
    student: [
        { id: 'dashboard', label: 'Главная', icon: 'home' },
        { id: 'labs', label: 'Лабораторные', icon: 'lightbulb' },
        { id: 'progress', label: 'Мой прогресс', icon: 'trending' },
        { id: 'statement', label: 'Ведомость', icon: 'table' },
        { id: 'questions', label: 'Вопросы', icon: 'messageSquare' },
        { id: 'calendar', label: 'Календарь', icon: 'calendar' },
    ],

    teacher: [
        { id: 'dashboard', label: 'Главная', icon: 'home' },
        { id: 'labs', label: 'Отчеты студентов', icon: 'lightbulb' },
        { id: 'questions', label: 'Вопросы студентов', icon: 'messageSquare' },
        { id: 'calendar', label: 'Календарь', icon: 'calendar' },
        { id: 'statement', label: 'Ведомость', icon: 'table' },
    ],

    admin: [
        { id: 'dashboard', label: 'Главная', icon: 'home' },
        { id: 'users', label: 'Пользователи', icon: 'users' }
    ],
};