import { screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { it, expect } from 'vitest';
import { http, HttpResponse } from 'msw';
import ProfilePage from './ProfilePage';
import { API, server } from '../../test/mocks/server';
import { renderPage } from '../../test/render';
import { setSession } from '../../test/helpers/session';
import { progress, leaderboard, student } from '../../test/fixtures';

const mockApi = (fails = false) => server.use(
    http.get(`${API}/public/api/v1/user/get/user-1`, () => fails ? HttpResponse.error() : HttpResponse.json(student)),
    http.get(`${API}/public/api/v1/laboratories/progress/my`, () => fails ? HttpResponse.error() : HttpResponse.json(progress)),
    http.get(`${API}/public/api/v1/laboratories/progress/leaderboard`, () => fails ? HttpResponse.error() : HttpResponse.json(leaderboard)),
);

it('shows student profile with real progress data', async () => {
    setSession('student'); mockApi();
    renderPage(<ProfilePage />);
    expect(await screen.findByRole('heading', { name: 'Иван Петров' })).toBeInTheDocument();
    expect(await screen.findByText('1/2')).toBeInTheDocument();
    expect(screen.getByText('#1')).toBeInTheDocument();
    expect(screen.getByText('Сети')).toBeInTheDocument();
    expect(await screen.findByText('ivan@example.com')).toBeInTheDocument();
});

it('stays usable when the server is unavailable', async () => {
    setSession('student'); mockApi(true);
    renderPage(<ProfilePage />);
    expect(await screen.findByRole('heading', { name: 'Test User' })).toBeInTheDocument();
    expect(await screen.findByText('рейтинг недоступен')).toBeInTheDocument();
    expect(screen.getByText('Пока нет начатых лабораторных')).toBeInTheDocument();
});

it('validates and saves profile edits', async () => {
    setSession('student'); mockApi();
    const user = userEvent.setup();
    renderPage(<ProfilePage />);
    await screen.findAllByText('Не указан');
    await user.click(screen.getByRole('button', { name: /Редактировать/ }));
    const dialog = screen.getByRole('dialog');

    await user.type(within(dialog).getByPlaceholderText('+7 (900) 123-45-67'), '123');
    await user.click(within(dialog).getByRole('button', { name: 'Сохранить' }));
    expect(within(dialog).getByText('Формат: +7 (900) 123-45-67')).toBeInTheDocument();

    await user.clear(within(dialog).getByPlaceholderText('+7 (900) 123-45-67'));
    await user.type(within(dialog).getByPlaceholderText('+7 (900) 123-45-67'), '+7 (900) 123-45-67');
    await user.type(within(dialog).getByPlaceholderText('@username'), '@ivan_petrov');
    await user.click(within(dialog).getByRole('button', { name: 'Сохранить' }));

    expect(await screen.findByText('Профиль сохранён')).toBeInTheDocument();
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    expect(screen.getByText('@ivan_petrov')).toBeInTheDocument();
    expect(screen.getByText('+7 (900) 123-45-67')).toBeInTheDocument();
});
