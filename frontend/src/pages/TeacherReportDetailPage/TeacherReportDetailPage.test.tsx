import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { it, expect, vi } from 'vitest';
import { http, HttpResponse } from 'msw';
import TeacherReportDetailPage from './TeacherReportDetailPage';
import { API, server } from '../../test/mocks/server';
import { renderPage } from '../../test/render';
const endpoint = `${API}/public/api/v1/teacher/reports/r1`;
const report = { reportId: 'r1', laboratory: { id: 'lab-1', title: 'Сети', maxPoints: 10 },
    student: { id: 's1', fullName: 'Иван', groupName: 'РИ-1' }, status: 1, points: null, teacherComment: null, allowResubmit: false, versions: [] };
it.each([true, false])('reviews a report (accepted=%s), validates points and refreshes data', async accepted => {
    let body: unknown; let reads = 0;
    server.use(http.get(endpoint, () => { reads++; return HttpResponse.json(report); }),
        http.post(`${endpoint}/review`, async ({ request }) => { body = await request.json(); return new HttpResponse(null, { status: 204 }); }));
    renderPage(<TeacherReportDetailPage />, '/teacher/reports/:reportId', '/teacher/reports/r1');
    const user = userEvent.setup();
    await user.click(await screen.findByRole('button', { name: 'Принять отчёт' }));
    expect(screen.getByText('Укажите баллы для принятия отчёта')).toBeInTheDocument();
    expect(body).toBeUndefined();
    if (accepted) await user.type(screen.getByRole('spinbutton'), '8');
    else {
        await user.click(screen.getByRole('radio', { name: 'Нужны правки' }));
        await user.click(screen.getByRole('checkbox'));
    }
    await user.type(screen.getByRole('textbox'), ' Комментарий ');
    await user.click(screen.getByRole('button', { name: accepted ? 'Принять отчёт' : 'Запросить правки' }));
    expect(await screen.findByText('Проверка сохранена')).toBeInTheDocument();
    expect(body).toEqual({ status: accepted ? 4 : 3, points: accepted ? 8 : null, comment: 'Комментарий', allowResubmit: !accepted });
    expect(reads).toBe(2);
});
it('retains review fields after failure and retries', async () => {
    let calls = 0;
    server.use(http.get(endpoint, () => HttpResponse.json(report)), http.post(`${endpoint}/review`, () => new HttpResponse(null, { status: ++calls === 1 ? 500 : 204 })));
    renderPage(<TeacherReportDetailPage />, '/teacher/reports/:reportId', '/teacher/reports/r1');
    const user = userEvent.setup();
    await user.type(await screen.findByRole('spinbutton'), '0');
    await user.click(screen.getByRole('button', { name: 'Принять отчёт' }));
    expect(await screen.findByText('Не удалось сохранить проверку. Попробуйте ещё раз.')).toBeInTheDocument();
    expect(screen.getByRole('spinbutton')).toHaveValue(0);
    await user.click(screen.getByRole('button', { name: 'Принять отчёт' }));
    expect(await screen.findByText('Проверка сохранена')).toBeInTheDocument();
});

it.each(['-1', '11'])('rejects points outside the allowed range: %s', async points => {
    server.use(http.get(endpoint, () => HttpResponse.json(report)));
    renderPage(<TeacherReportDetailPage />, '/teacher/reports/:reportId', '/teacher/reports/r1'); const user = userEvent.setup();
    await user.type(await screen.findByRole('spinbutton'), points);
    await user.click(screen.getByRole('button', { name: 'Принять отчёт' }));
    expect(screen.getByText('Баллы должны быть от 0 до максимума за лабораторную')).toBeInTheDocument();
});
it('downloads the latest version through its fallback URL and releases the blob', async () => {
    server.use(http.get(endpoint, () => HttpResponse.json({ ...report, versions: [{ versionId: 'v1', versionNumber: 1, status: 4,
        originalFileName: 'review.pdf', fileSizeBytes: 2048, createDateUtc: '2026-01-01', checkedDateUtc: '2026-01-02', checkedByTeacherFullName: 'Учитель', teacherComment: 'Принято' }] })),
        http.get(`${endpoint}/versions/v1/file`, () => new HttpResponse('pdf', { headers: { 'Content-Type': 'application/pdf' } })));
    const create = vi.spyOn(URL, 'createObjectURL').mockReturnValue('blob:review');
    const revoke = vi.spyOn(URL, 'revokeObjectURL').mockImplementation(() => {});
    let name = ''; vi.spyOn(HTMLAnchorElement.prototype, 'click').mockImplementation(function(this: HTMLAnchorElement) { name = this.download; });
    renderPage(<TeacherReportDetailPage />, '/teacher/reports/:reportId', '/teacher/reports/r1');
    await userEvent.setup().click(await screen.findByRole('button', { name: 'Скачать' }));
    await waitFor(() => expect(revoke).toHaveBeenCalledWith('blob:review'));
    expect(create).toHaveBeenCalledOnce(); expect(name).toBe('review.pdf'); expect(screen.getByText('Принято')).toBeInTheDocument();
});
