import { screen, within } from '@testing-library/react';
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
            http.get(`${API}/public/api/v1/teacher/reports`, () => HttpResponse.json({
                items: [
                    { ...base, reportId: 'r1', studentFullName: 'Иван Петров', status: 1, points: null },
                    { ...base, reportId: 'r2', studentFullName: 'Анна Сидорова', status: 4, points: 10 },
                ]
            })),
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

function teacherData(labs: typeof lab[] = [], reports: object[] = []) {
    setSession('teacher');
    server.use(
        http.get(`${API}/public/api/v1/teacher/laboratories`, () => HttpResponse.json({ items: labs })),
        http.get(`${API}/public/api/v1/teacher/reports`, () => HttpResponse.json({ items: reports })),
    );
}

describe('laboratory lists', () => {
    it('groups student labs alphabetically and sorts within each block', async () => {
        setSession();
        server.use(
            http.get(`${API}/public/api/v1/laboratories`, () => HttpResponse.json({
                items: [
                    { ...lab, id: 'second', title: 'Вторая', sortOrder: 2 },
                    { ...lab, id: 'first', title: 'Первая', sortOrder: 1, isCompleted: true, earnedPoints: 7 },
                    { ...lab, id: 'unassigned', title: 'Без раздела', block: '', difficulty: 3 },
                    { ...lab, id: 'other', title: 'Алгоритмы', block: 'Алгоритмы', difficulty: 2 },
                ]
            })),
            http.get(`${API}/public/api/v1/laboratories/progress/my`, () => HttpResponse.json({ totalLaboratories: 4, completedLaboratories: 1 })),
        );
        renderPage(<LabsPage />, '/labs');
        await screen.findByRole('button', { name: /Первая/ });
        expect(screen.getAllByRole('heading').map(h => h.textContent)).toEqual(['АЛГОРИТМЫ', 'БЕЗ БЛОКА', 'СЕТИ']);
        expect(screen.getAllByRole('button').map(b => b.textContent)).toEqual([
            expect.stringContaining('Алгоритмы'), expect.stringContaining('Без раздела'),
            expect.stringContaining('Первая'), expect.stringContaining('Вторая'),
        ]);
        expect(screen.getByRole('button', { name: /Первая/ })).toHaveTextContent('7 / 10 баллов');
        expect(screen.getByText('4 работ · 1 выполнено')).toBeInTheDocument();
    });
    it('sorts teacher labs by order then title and displays publication state', async () => {
        teacherData([
            { ...lab, id: 'b', title: 'Бета', sortOrder: 2 },
            { ...lab, id: 'a', title: 'Альфа', sortOrder: 2, isPublished: false },
            { ...lab, id: 'c', title: 'Гамма', sortOrder: 1 },
        ]);
        renderPage(<LabsPage />, '/labs');
        await screen.findByRole('button', { name: /Альфа/ });
        const cards = screen.getAllByRole('button').filter(b => /Альфа|Бета|Гамма/.test(b.textContent ?? ''));
        expect(cards.map(b => b.textContent)).toEqual([
            expect.stringContaining('Гамма'), expect.stringContaining('Альфа'), expect.stringContaining('Бета'),
        ]);
        expect(cards[1]).toHaveTextContent('Черновик');
        expect(cards[0]).toHaveTextContent('Опубликована');
        await userEvent.setup().click(cards[1]);
        expect(screen.getByLabelText('Current route')).toHaveTextContent('/labs/a');
    });
    it('opens creation from the empty teacher list', async () => {
        teacherData();
        renderPage(<LabsPage />, '/labs');
        expect(await screen.findByText('Лабораторных работ ещё нет. Создайте первую!')).toBeInTheDocument();
        await userEvent.setup().click(screen.getByRole('button', { name: '+ Создать лабораторную' }));
        expect(screen.getByLabelText('Current route')).toHaveTextContent('/labs/create');
    });
    it('switches between empty reports and the laboratory list', async () => {
        teacherData([lab]);
        renderPage(<LabsPage />, '/labs');
        const user = userEvent.setup();
        await user.click(await screen.findByRole('button', { name: /Отчёты студентов/ }));
        expect(screen.getByText('Отчёты не найдены')).toBeInTheDocument();
        expect(screen.queryByRole('button', { name: /Основы сетей/ })).not.toBeInTheDocument();
        await user.click(screen.getByRole('button', { name: /Мои лабораторные/ }));
        expect(screen.getByRole('button', { name: /Основы сетей/ })).toBeInTheDocument();
    });
    it('sorts reports by submission time and searches by laboratory title', async () => {
        const base = { studentFullName: 'Иван', groupName: null, currentVersionNumber: 1, maxPoints: 10, points: null, status: 3 };
        teacherData([], [
            { ...base, reportId: 'old', laboratoryTitle: 'Сети', lastSubmitDateUtc: '2026-01-01T12:00:00Z' },
            { ...base, reportId: 'new', laboratoryTitle: 'Linux', lastSubmitDateUtc: '2026-02-01T12:00:00Z' },
        ]);
        renderPage(<LabsPage />, '/labs');
        const user = userEvent.setup();
        await user.click(await screen.findByRole('button', { name: /Отчёты студентов/ }));
        const rows = screen.getAllByRole('row').slice(1);
        expect(rows[0]).toHaveTextContent('Linux');
        expect(rows[1]).toHaveTextContent('Сети');
        expect(within(rows[0]).getByText('—')).toBeInTheDocument();
        expect(rows[0]).toHaveTextContent('— / 10');
        await user.type(screen.getByRole('textbox'), 'LINUX');
        expect(screen.queryByText('Сети')).not.toBeInTheDocument();
        expect(screen.getByText('Linux')).toBeInTheDocument();
        await user.selectOptions(screen.getByRole('combobox'), '4');
        expect(screen.getByText('Отчёты не найдены')).toBeInTheDocument();
        await user.selectOptions(screen.getByRole('combobox'), '');
        expect(screen.getByText('Linux')).toBeInTheDocument();
    });
});

it.each(['labs', 'progress', 'both'])('preserves available student data when %s fails', async failing => {
    setSession();
    server.use(http.get(`${API}/public/api/v1/laboratories`, () => failing === 'progress' ? HttpResponse.json({ items: [lab] }) : HttpResponse.error()),
        http.get(`${API}/public/api/v1/laboratories/progress/my`, () => failing === 'labs' ? HttpResponse.json({ totalLaboratories: 1, completedLaboratories: 0 }) : HttpResponse.error()));
    renderPage(<LabsPage />);
    expect(await screen.findByRole('alert')).toHaveTextContent('Не удалось загрузить часть данных');
    if (failing === 'progress') expect(screen.getByRole('button', { name: /Основы сетей/ })).toBeInTheDocument();
    if (failing === 'labs') expect(screen.getByText('1 работ · 0 выполнено')).toBeInTheDocument();
    expect(screen.queryByText('Загрузка...')).not.toBeInTheDocument();
});
it.each(['labs', 'reports'])('preserves available teacher data when %s fails', async failing => {
    setSession('teacher');
    server.use(http.get(`${API}/public/api/v1/teacher/laboratories`, () => failing === 'labs' ? HttpResponse.error() : HttpResponse.json({ items: [lab] })),
        http.get(`${API}/public/api/v1/teacher/reports`, () => failing === 'reports' ? HttpResponse.error() : HttpResponse.json({ items: [{ reportId: 'r1', studentFullName: 'Иван', laboratoryTitle: 'Сети', currentVersionNumber: 1, status: 1, points: null, maxPoints: 10, lastSubmitDateUtc: '2026-01-01' }] })));
    renderPage(<LabsPage />); await screen.findByRole('alert');
    if (failing === 'reports') expect(screen.getByRole('button', { name: /Основы сетей/ })).toBeInTheDocument();
    else {
        await userEvent.setup().click(screen.getByRole('button', { name: /Отчёты студентов/ }));
        expect(screen.getByText('Иван')).toBeInTheDocument();
    }
});
