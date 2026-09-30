import { act, renderHook } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { useIsDesktop } from './useIsDesktop';

describe('useIsDesktop', () => {
    it.each([true, false])('evaluates coarse pointer: %s', coarse => {
        const query = { matches: coarse, addEventListener: vi.fn(), removeEventListener: vi.fn() };
        const matchMedia = vi.fn(() => query);
        vi.stubGlobal('matchMedia', matchMedia);
        const { result, unmount } = renderHook(() => useIsDesktop());
        expect(result.current).toBe(!coarse);
        expect(matchMedia).toHaveBeenCalledWith('(pointer: coarse)');
        const listener = query.addEventListener.mock.calls[0][1];
        act(() => { query.matches = !coarse; listener(); });
        expect(result.current).toBe(coarse);
        unmount();
        expect(query.removeEventListener).toHaveBeenCalledWith('change', listener);
    });
});
