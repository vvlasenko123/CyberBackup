import { setupServer } from 'msw/node';

export const API = 'http://localhost:5000';
export const server = setupServer();
