import React, { useRef, useEffect, useState } from 'react';
import { Icon } from '../shared/Icon';
import type { AppNotification } from './useNotifications';
import './Notifications.css';

function formatTime(iso: string): string {
    const d = new Date(iso);
    const now = new Date();
    const diffMs = now.getTime() - d.getTime();
    const diffMin = Math.floor(diffMs / 60000);

    if (diffMin < 1)  return 'только что';
    if (diffMin < 60) return `${diffMin} мин. назад`;
    const diffH = Math.floor(diffMin / 60);
    if (diffH < 24)   return `${diffH} ч. назад`;
    return d.toLocaleDateString('ru-RU', { day: '2-digit', month: '2-digit' });
}

interface Props {
    notifications: AppNotification[];
    unreadCount: number;
    onMarkAllRead: () => void;
}

export function NotificationBell({ notifications, unreadCount, onMarkAllRead }: Props) {
    const [open, setOpen] = useState(false);
    const wrapRef = useRef<HTMLDivElement>(null);

    // Закрываем панель по клику снаружи
    useEffect(() => {
        function handleClick(e: MouseEvent) {
            if (wrapRef.current && !wrapRef.current.contains(e.target as Node)) {
                setOpen(false);
            }
        }
        if (open) document.addEventListener('mousedown', handleClick);
        return () => document.removeEventListener('mousedown', handleClick);
    }, [open]);

    function handleToggle() {
        if (!open && unreadCount > 0) onMarkAllRead();
        setOpen(v => !v);
    }

    return (
        <div className="notif-bell" ref={wrapRef}>
            <button className="header__icon" onClick={handleToggle} title="Уведомления">
                <Icon name="bell" size={18} />
                {unreadCount > 0 && (
                    <span className="notif-bell__badge">
                        {unreadCount > 99 ? '99+' : unreadCount}
                    </span>
                )}
            </button>

            {open && (
                <div className="notif-panel">
                    <div className="notif-panel__header">
                        <p className="notif-panel__title">Уведомления</p>
                        {notifications.length > 0 && (
                            <button
                                className="notif-panel__mark-read"
                                onClick={onMarkAllRead}
                            >
                                Отметить все прочитанными
                            </button>
                        )}
                    </div>

                    <div className="notif-panel__list">
                        {notifications.length === 0 ? (
                            <p className="notif-panel__empty">Уведомлений нет</p>
                        ) : (
                            notifications.map(n => (
                                <div
                                    key={n.id}
                                    className={`notif-item${n.read ? '' : ' notif-item--unread'}`}
                                >
                                    <div className={`notif-item__dot${n.read ? ' notif-item__dot--read' : ''}`} />
                                    <div className="notif-item__body">
                                        <p className="notif-item__title">{n.title}</p>
                                        <p className="notif-item__msg">{n.message}</p>
                                        <span className="notif-item__time">{formatTime(n.createdAtUtc)}</span>
                                    </div>
                                </div>
                            ))
                        )}
                    </div>
                </div>
            )}
        </div>
    );
}
