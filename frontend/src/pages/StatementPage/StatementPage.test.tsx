import { screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { it, expect } from 'vitest';
import { http, HttpResponse } from 'msw';
import StatementPage from './StatementPage';
import { API, server } from '../../test/mocks/server';
import { renderPage } from '../../test/render';
import { setSession } from '../../test/helpers/session';
it('shows student attendance, admission and laboratory/report statuses', async () => {
    setSession(); server.use(http.get(`${API}/public/api/v1/gradebook/my`, () => HttpResponse.json({
        student: { id: 's1', fullName: 'Иван' },
        attendancePercent: 79.6, isExamAllowed: true, hasAutomaticGrade: false, totalPoints: 10,
        laboratories: [0, 1, 2, 3, 4].map(status => ({ laboratoryId: String(status), title: `Работа ${status}`, laboratoryStatus: status, status, maxPoints: 10 }))
    })));
    renderPage(<StatementPage />); expect(await screen.findByText('80%')).toBeInTheDocument();
    expect(screen.getByText('Да')).toBeInTheDocument(); expect(screen.getByText('Принят')).toBeInTheDocument();
    expect(screen.getByText('На доработке')).toBeInTheDocument(); expect(screen.getAllByText('На проверке')).toHaveLength(2);
});
it('shows student load error', async () => {
    setSession(); server.use(http.get(`${API}/public/api/v1/gradebook/my`, () => HttpResponse.error()));
    renderPage(<StatementPage />); expect(await screen.findByText('Не удалось загрузить ведомость')).toBeInTheDocument();
});
it('filters teacher gradebook and saves attendance/admission changes', async () => {
    setSession('teacher'); let params = new URLSearchParams(); let body: unknown;
    const item = {
        studentId: 's1', fullName: 'Иван Петров', groupName: 'РИ-1', attendancePercent: 50, lessonsAttended: 5, totalLessons: 10,
        isExamAllowed: false, hasAutomaticGrade: false, totalPoints: 10, completedLaboratories: 1, totalLaboratories: 2
    };
    server.use(http.get(`${API}/public/api/v1/teacher/gradebook`, ({ request }) => { params = new URL(request.url).searchParams; return HttpResponse.json({ items: [item] }); }),
        http.put(`${API}/public/api/v1/teacher/gradebook/s1`, async ({ request }) => { body = await request.json(); return new HttpResponse(null, { status: 204 }); }));
    const { container } = renderPage(<StatementPage />); const user = userEvent.setup(); await screen.findByText('Иван Петров');
    await user.selectOptions(screen.getByRole('combobox'), 'РИ-1');
    await user.type(screen.getByPlaceholderText('Поиск по студентам...'), 'Иван{Enter}');
    await waitFor(() => expect(Object.fromEntries(params)).toMatchObject({ groupName: 'РИ-1', search: 'Иван' }));
    await user.click(await screen.findByRole('button', { name: 'Редактировать' }));
    const modal = within(container.querySelector('.stmt-modal') as HTMLElement);
    const count = modal.getAllByRole('textbox')[0]; await user.clear(count); await user.type(count, '7');
    await user.click(modal.getAllByRole('checkbox')[0]); await user.click(modal.getByRole('button', { name: 'Сохранить' }));
    await waitFor(() => expect(container.querySelector('.stmt-modal')).toBeNull());
    expect(body).toMatchObject({ lessonsAttended: 7, totalLessons: 10, isExamAllowed: true, hasAutomaticGrade: false });
});
