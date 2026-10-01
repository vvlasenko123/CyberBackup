import { act, fireEvent, render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { it, expect, vi } from 'vitest';
import { NotificationToast } from './NotificationToast';
import { NotificationBell } from './NotificationBell';
const notification = { id: 'n1', title: 'Отчёт проверен', message: 'Принят', read: false, createdAtUtc: '2026-01-01' };
it('dismisses toast after five seconds and cancels its timer on unmount', () => {
    vi.useFakeTimers(); const dismiss = vi.fn();
    const { unmount } = render(<NotificationToast toast={notification} onDismiss={dismiss} />);
    act(() => vi.advanceTimersByTime(4999)); expect(dismiss).not.toHaveBeenCalled();
    act(() => vi.advanceTimersByTime(1)); expect(dismiss).toHaveBeenCalledOnce(); unmount();
    const next = render(<NotificationToast toast={notification} onDismiss={dismiss} />); next.unmount();
    act(() => vi.advanceTimersByTime(5000)); expect(dismiss).toHaveBeenCalledOnce();
});
it('opens notification history, marks read and closes on outside click', async () => {
    const markRead = vi.fn(); render(<NotificationBell notifications={[notification]} unreadCount={100} onMarkAllRead={markRead} />);
    const user = userEvent.setup(); expect(screen.getByText('99+')).toBeInTheDocument();
    await user.click(screen.getByTitle('Уведомления')); expect(screen.getByText(notification.title)).toBeInTheDocument(); expect(markRead).toHaveBeenCalledOnce();
    await user.click(screen.getByRole('button', { name: 'Отметить все прочитанными' })); expect(markRead).toHaveBeenCalledTimes(2);
    fireEvent.mouseDown(document.body); expect(screen.queryByText(notification.title)).not.toBeInTheDocument();
});
