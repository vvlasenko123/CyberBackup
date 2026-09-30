import { File, Blob } from 'node:buffer';
import { fetch, Request, Response, Headers, FormData } from 'undici';
import '@testing-library/jest-dom/vitest';
import { cleanup } from '@testing-library/react';
import { afterAll, afterEach, beforeAll, beforeEach, expect, vi } from 'vitest';
import { server } from './mocks/server';

Object.assign(globalThis, { fetch, Request, Response, Headers, FormData, File, Blob });

const unhandledRequests: string[] = [];
beforeAll(() => server.listen({
    onUnhandledRequest(request, print) {
        unhandledRequests.push(`${request.method} ${request.url}`);
        print.error();
    },
}));
beforeEach(() => {
    unhandledRequests.length = 0;
    localStorage.clear();
    window.history.replaceState({}, '', '/');
});
afterEach(() => {
    cleanup();
    server.resetHandlers();
    vi.restoreAllMocks();
    vi.unstubAllGlobals();
    vi.useRealTimers();
    localStorage.clear();
    expect(unhandledRequests, 'Unexpected HTTP requests').toEqual([]);
});
afterAll(() => server.close());
