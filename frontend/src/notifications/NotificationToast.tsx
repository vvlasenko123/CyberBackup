import React, { useEffect } from 'react';
import type { AppNotification } from './useNotifications';
import { Icon } from '../shared/Icon';
import './Notifications.css';

interface Props {
    toast: AppNotification | null;
    onDismiss: () => void;
}

const AUTO_DISMISS_MS = 5000;

export function NotificationToast({ toast, onDismiss }: Props) {
    useEffect(() => {
        if (!toast) return;
        const t = setTimeout(onDismiss, AUTO_DISMISS_MS);
        return () => clearTimeout(t);
    }, [toast, onDismiss]);

    if (!toast) return null;

    return (
        <div className="notif-toast-wrap">
            <div className="notif-toast">
                <div className="notif-toast__icon">
                    <Icon name="bell" size={16} />
                </div>
                <div className="notif-toast__body">
                    <p className="notif-toast__title">{toast.title}</p>
                    <p className="notif-toast__msg">{toast.message}</p>
                </div>
                <button className="notif-toast__close" onClick={onDismiss}>
                    <Icon name="x" size={14} />
                </button>
            </div>
        </div>
    );
}
