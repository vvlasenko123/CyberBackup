import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';
import { http, HttpResponse } from 'msw';
import LabsPage from './LabsPage';
import { API, server } from '../../test/mocks/server';
import { renderPage } from '../../test/render';
import { setSession, tokenFor } from '../../test/helpers/session';

const lab = { id: 'lab-1', title: 'Основы сетей', shortDescription: 'Практика', difficulty: 1, block: 'Сети', maxPoints: 10, earnedPoints: 0, status: 0, isCompleted: false, sortOrder: 1, isPublished: true };

describe('LabsPage', () => {
    it('loads student labs with authorization and opens a lab', async () => {
        setSession();
        let authorization: string | null = null;
        let params = '';
        server.use(
            http.get(`${API}/public/api/v1/laboratories`, ({ request }) => {
                authorization = request.headers.get('Authorization');
                params = new URL(request.url).search;
                return HttpResponse.json({ items: [lab] });
            }),
            http.get(`${API}/public/api/v1/laboratories/progress/my`, () => HttpResponse.json({ totalLaboratories: 1, completedLaboratories: 0 })),
        );
        renderPage(<LabsPage />, '/labs');
        expect(screen.getByText('Загрузка...')).toBeInTheDocument();
        const button = await screen.findByRole('button', { name: /Основы сетей/ });
        expect(screen.getByText('1 работ · 0 выполнено')).toBeInTheDocument();
        expect(authorization).toBe(`Bearer ${tokenFor()}`);
        expect(new URLSearchParams(params).get('pageSize')).toBe('100');
        expect(screen.queryByRole('button', { name: /Создать/ })).not.toBeInTheDocument();
        await userEvent.setup().click(button);
        expect(screen.getByLabelText('Current route')).toHaveTextContent('/labs/lab-1');
    });
    it('shows an empty student list', async () => {
        setSession();
        server.use(
            http.get(`${API}/public/api/v1/laboratories`, () => HttpResponse.json({ items: [] })),
            http.get(`${API}/public/api/v1/laboratories/progress/my`, () => HttpResponse.json({ totalLaboratories: 0, completedLaboratories: 0 })),
        );
        renderPage(<LabsPage />, '/labs');
        expect(await screen.findByText('Лабораторные работы не найдены')).toBeInTheDocument();
    });
    it('filters teacher reports by student and status and opens a report', async () => {
        setSession('teacher');
        const base = { laboratoryTitle: 'Основы сетей', laboratoryId: 'lab-1', groupName: 'Группа 1', currentVersionNumber: 1, maxPoints: 10, lastSubmitDateUtc: '2026-01-01T12:00:00Z' };
        server.use(
            http.get(`${API}/public/api/v1/teacher/laboratories`, () => HttpResponse.json({ items: [lab] })),
            http.get(`${API}/public/api/v1/teacher/reports`, () => HttpResponse.json({ items: [
                { ...base, reportId: 'r1', studentFullName: 'Иван Петров', status: 1, points: null },
                { ...base, reportId: 'r2', studentFullName: 'Анна Сидорова', status: 4, points: 10 },
            ] })),
        );
        renderPage(<LabsPage />, '/labs');
        const user = userEvent.setup();
        await user.click(await screen.findByRole('button', { name: /Отчёты студентов/ }));
        await user.type(screen.getByPlaceholderText('Поиск по студенту или лабораторной...'), 'иван');
        expect(screen.getByText('Иван Петров')).toBeInTheDocument();
        expect(screen.queryByText('Анна Сидорова')).not.toBeInTheDocument();
        await user.selectOptions(screen.getByRole('combobox'), '4');
        expect(screen.getByText('Отчёты не найдены')).toBeInTheDocument();
        await user.clear(screen.getByRole('textbox'));
        await user.click(screen.getByText('Анна Сидорова'));
        expect(screen.getByLabelText('Current route')).toHaveTextContent('/teacher/reports/r2');
    });
});
