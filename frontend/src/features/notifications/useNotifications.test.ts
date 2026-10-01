import { act, renderHook, waitFor } from '@testing-library/react';
import { beforeEach, it, expect, vi } from 'vitest';
import axiosInstance from '../../utils/axiosInstance';
import { useNotifications } from './useNotifications';
import { setSession, tokenFor } from '../../test/helpers/session';
vi.mock('../../utils/axiosInstance', () => ({ default: { get: vi.fn(), post: vi.fn() } }));
class Socket {
    static OPEN = 1;
    static instances: Socket[] = [];
    readyState = 1;
    onopen: (() => void) | null = null;
    onmessage: ((event: { data: string }) => void) | null = null;
    onclose: ((event: { code: number; reason: string }) => void) | null = null;
    onerror: (() => void) | null = null;
    send = vi.fn();
    close = vi.fn(() => { this.readyState = 3; this.onclose?.({ code: 1000, reason: '' }); });
    url: string;
    constructor(url: string) { this.url = url; Socket.instances.push(this); }
    receive(value: unknown) { this.onmessage?.({ data: JSON.stringify(value) + '\x1e' }); }
}
const notification = (id: string) => ({ id, title: `Уведомление ${id}`, message: 'Проверено', createdAtUtc: '2026-01-01T12:00:00Z' });
beforeEach(() => {
    Socket.instances = []; vi.stubGlobal('WebSocket', Socket);
    vi.mocked(axiosInstance.get).mockReset().mockResolvedValue({ data: [] });
    vi.mocked(axiosInstance.post).mockReset().mockResolvedValue({});
    vi.spyOn(console, 'log').mockImplementation(() => {});
    vi.spyOn(console, 'warn').mockImplementation(() => {});
    vi.spyOn(console, 'error').mockImplementation(() => {});
});
it('does not request or connect without a session', () => {
    renderHook(useNotifications); expect(Socket.instances).toHaveLength(0); expect(axiosInstance.get).not.toHaveBeenCalled();
});
it('merges HTTP history with live messages without duplicates and marks all read', async () => {
    setSession(); let resolve!: (value: { data: object[] }) => void;
    vi.mocked(axiosInstance.get).mockReturnValue(new Promise(r => { resolve = r; }));
    const { result } = renderHook(useNotifications); const ws = Socket.instances[0];
    expect(ws.url).toContain(`access_token=${encodeURIComponent(tokenFor())}`);
    act(() => { ws.onopen?.(); ws.receive({}); ws.receive({ type: 1, target: 'NotificationReceived', arguments: [notification('live')] }); });
    expect(ws.send).toHaveBeenCalledWith('{"protocol":"json","version":1}\x1e');
    expect(result.current.latestToast?.id).toBe('live');
    await act(async () => resolve({ data: [{ ...notification('live'), isRead: false }, { ...notification('old'), isRead: true }] }));
    expect(result.current.notifications.map(n => n.id)).toEqual(['live', 'old']); expect(result.current.unreadCount).toBe(1);
    act(() => { result.current.markAllRead(); result.current.dismissToast(); });
    expect(result.current.unreadCount).toBe(0); expect(result.current.latestToast).toBeNull();
    expect(axiosInstance.post).toHaveBeenCalledWith('/public/api/v1/notifications/read-all');
});
it('handles malformed frames, limits history to 50 and survives HTTP failures', async () => {
    setSession(); vi.mocked(axiosInstance.get).mockRejectedValue(new Error('offline')); vi.mocked(axiosInstance.post).mockRejectedValue(new Error('offline'));
    const { result } = renderHook(useNotifications); const ws = Socket.instances[0];
    act(() => {
        ws.onmessage?.({ data: 'bad json\x1e' }); ws.receive({ type: 1, target: 'NotificationReceived', arguments: [] });
        ws.receive({ type: 6 });
        for (let i = 0; i < 55; i++) ws.receive({ type: 1, target: 'NotificationReceived', arguments: [notification(String(i))] });
        result.current.markAllRead();
    });
    await waitFor(() => expect(result.current.notifications).toHaveLength(50));
    expect(result.current.notifications[0].id).toBe('54'); expect(result.current.unreadCount).toBe(0);
});
it('pings, reconnects with backoff and resets backoff after handshake', async () => {
    setSession(); vi.useFakeTimers();
    const { unmount } = renderHook(useNotifications);
    await act(async () => {});
    act(() => Socket.instances[0].receive({}));
    act(() => vi.advanceTimersByTime(15_000));
    expect(Socket.instances[0].send).toHaveBeenCalledWith('{"type":6}\x1e');
    act(() => Socket.instances[0].close());
    act(() => vi.advanceTimersByTime(999)); expect(Socket.instances).toHaveLength(1);
    act(() => vi.advanceTimersByTime(1)); expect(Socket.instances).toHaveLength(2);
    act(() => Socket.instances[1].close());
    act(() => vi.advanceTimersByTime(1999)); expect(Socket.instances).toHaveLength(2);
    act(() => vi.advanceTimersByTime(1)); expect(Socket.instances).toHaveLength(3);
    act(() => { Socket.instances[2].receive({}); Socket.instances[2].close(); vi.advanceTimersByTime(1000); });
    expect(Socket.instances).toHaveLength(4);
    unmount(); expect(vi.getTimerCount()).toBe(0); expect(Socket.instances[3].close).toHaveBeenCalled();
    act(() => vi.advanceTimersByTime(60_000)); expect(Socket.instances).toHaveLength(4);
});
it('cancels pending reconnect on unmount and closes socket on error', async () => {
    setSession(); vi.useFakeTimers(); const { unmount } = renderHook(useNotifications); await act(async () => {});
    act(() => Socket.instances[0].onerror?.()); expect(Socket.instances[0].close).toHaveBeenCalled();
    unmount(); act(() => vi.advanceTimersByTime(60_000)); expect(Socket.instances).toHaveLength(1); expect(vi.getTimerCount()).toBe(0);
});
