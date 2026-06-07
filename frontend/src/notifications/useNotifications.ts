import { useState, useEffect, useRef, useCallback } from 'react';
import axiosInstance from '../utils/axiosInstance';

const RECORD_SEP = '\x1e';

const API_BASE = (import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000').replace(/\/$/, '');
const WS_BASE = API_BASE.replace(/^http/, 'ws');

export interface AppNotification {
    id: string;
    title: string;
    message: string;
    createdAtUtc: string;
    read: boolean;
}

type ApiNotification = {
    id: string;
    title: string;
    message: string;
    isRead: boolean;
    createdAtUtc: string;
};

const RECONNECT_DELAYS = [1000, 2000, 5000, 10000, 30000];

export function useNotifications() {
    const [notifications, setNotifications] = useState<AppNotification[]>([]);
    const [latestToast, setLatestToast] = useState<AppNotification | null>(null);

    const wsRef = useRef<WebSocket | null>(null);
    const pingRef = useRef<ReturnType<typeof setInterval> | null>(null);
    const retryRef = useRef<ReturnType<typeof setTimeout> | null>(null);
    const retryCount = useRef(0);
    const destroyed = useRef(false);

    const connect = useCallback(() => {
        const token = localStorage.getItem('token');

        if (!token || destroyed.current) {
            return;
        }

        const url = `${WS_BASE}/notification-hub?access_token=${encodeURIComponent(token)}`;
        const ws = new WebSocket(url);
        wsRef.current = ws;

        ws.onopen = () => {
            console.log('[SignalR] WebSocket connected, sending handshake');
            ws.send(JSON.stringify({ protocol: 'json', version: 1 }) + RECORD_SEP);
        };

        ws.onmessage = (event: MessageEvent<string>) => {
            const frames = event.data.split(RECORD_SEP).filter(Boolean);

            for (const frame of frames) {
                let msg: Record<string, unknown>;

                try {
                    msg = JSON.parse(frame);
                } catch {
                    continue;
                }

                if (Object.keys(msg).length === 0) {
                    console.log('[SignalR] Handshake OK — notifications ready');
                    retryCount.current = 0;

                    pingRef.current = setInterval(() => {
                        if (ws.readyState === WebSocket.OPEN) {
                            ws.send(JSON.stringify({ type: 6 }) + RECORD_SEP);
                        }
                    }, 15_000);

                    continue;
                }

                if (msg.type === 1 && msg.target === 'NotificationReceived') {
                    const args = msg.arguments as Array<{
                        id: string;
                        title: string;
                        message: string;
                        createdAtUtc: string;
                    }>;

                    const payload = args?.[0];

                    if (!payload) {
                        continue;
                    }

                    const notification: AppNotification = {
                        id: payload.id,
                        title: payload.title,
                        message: payload.message,
                        createdAtUtc: payload.createdAtUtc,
                        read: false,
                    };

                    console.log('[SignalR] Notification received:', notification);
                    setNotifications(prev => [notification, ...prev].slice(0, 50));
                    setLatestToast(notification);
                }
            }
        };

        ws.onclose = ev => {
            console.warn('[SignalR] WebSocket closed', ev.code, ev.reason);

            if (pingRef.current) {
                clearInterval(pingRef.current);
                pingRef.current = null;
            }

            if (destroyed.current) {
                return;
            }

            const delay = RECONNECT_DELAYS[Math.min(retryCount.current, RECONNECT_DELAYS.length - 1)];
            retryCount.current++;
            retryRef.current = setTimeout(connect, delay);
        };

        ws.onerror = ev => {
            console.error('[SignalR] WebSocket error', ev);
            ws.close();
        };
    }, []);

    useEffect(() => {
        const token = localStorage.getItem('token');

        if (!token) {
            return;
        }

        axiosInstance.get<ApiNotification[]>('/public/api/v1/notifications')
            .then(res => {
                const loaded: AppNotification[] = res.data.map(n => ({
                    id: n.id,
                    title: n.title,
                    message: n.message,
                    createdAtUtc: n.createdAtUtc,
                    read: n.isRead,
                }));

                setNotifications(prev => {
                    const ids = new Set(prev.map(n => n.id));
                    const merged = [...prev, ...loaded.filter(n => !ids.has(n.id))];

                    merged.sort((a, b) => new Date(b.createdAtUtc).getTime() - new Date(a.createdAtUtc).getTime());

                    return merged.slice(0, 50);
                });
            })
            .catch(() => { });
    }, []);

    useEffect(() => {
        destroyed.current = false;
        connect();

        return () => {
            destroyed.current = true;

            if (retryRef.current) {
                clearTimeout(retryRef.current);
            }

            if (pingRef.current) {
                clearInterval(pingRef.current);
            }

            wsRef.current?.close();
        };
    }, [connect]);

    const unreadCount = notifications.filter(n => !n.read).length;

    const markAllRead = useCallback(() => {
        setNotifications(prev => prev.map(n => ({ ...n, read: true })));
        axiosInstance.post('/public/api/v1/notifications/read-all').catch(() => { });
    }, []);

    const dismissToast = useCallback(() => setLatestToast(null), []);

    return { notifications, unreadCount, markAllRead, latestToast, dismissToast };
}
