import { screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { it, expect, vi } from 'vitest';
import { Sidebar } from './Sidebar';
import DesktopOnlyScreen from '../../../pages/DesktopOnly/DesktopOnlyScreen';
import { renderPage } from '../../../test/render';
import { setSession } from '../../../test/helpers/session';

const user = { name: 'Иван Петров' };

it('marks desktop-only sections on touch devices and keeps mobile-ready ones plain', () => {
    renderPage(<Sidebar role="student" user={user} onLogout={vi.fn()} touchDevice />);
    expect(within(screen.getByRole('button', { name: /Лабораторные/ })).getByText('ПК')).toBeInTheDocument();
    expect(within(screen.getByRole('button', { name: /Главная/ })).queryByText('ПК')).not.toBeInTheDocument();
    expect(within(screen.getByRole('button', { name: /^Профиль/ })).queryByText('ПК')).not.toBeInTheDocument();
});

it('shows no badges on desktop', () => {
    renderPage(<Sidebar role="student" user={user} onLogout={vi.fn()} />);
    expect(screen.queryByText('ПК')).not.toBeInTheDocument();
});

it('closes the mobile menu after navigation', async () => {
    const onClose = vi.fn();
    renderPage(<Sidebar role="student" user={user} onLogout={vi.fn()} open onClose={onClose} touchDevice />);
    await userEvent.setup().click(screen.getByRole('button', { name: /^Профиль/ }));
    expect(onClose).toHaveBeenCalled();
    expect(screen.getByLabelText('Current route')).toHaveTextContent('/profile');
});

it('offers a way back to the dashboard from the desktop-only stub', async () => {
    setSession('student');
    renderPage(<DesktopOnlyScreen />);
    await userEvent.setup().click(screen.getByRole('button', { name: 'Вернуться на главную' }));
    expect(screen.getByLabelText('Current route')).toHaveTextContent('/dashboard');
});

it('hides the back button without a session', () => {
    renderPage(<DesktopOnlyScreen />);
    expect(screen.queryByRole('button', { name: 'Вернуться на главную' })).not.toBeInTheDocument();
});
