import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';
import { http, HttpResponse } from 'msw';
import LabReportPage from './LabReportPage';
import { API, server } from '../../test/mocks/server';
import { renderPage } from '../../test/render';

const endpoint = `${API}/public/api/v1/laboratories/lab-1`;
function setupReport() {
    server.use(
        http.get(endpoint, () => HttpResponse.json({ title: 'Основы сетей' })),
        http.get(`${endpoint}/reports/my`, () => new HttpResponse(null, { status: 404 })),
    );
}
function renderReport() {
    return renderPage(<LabReportPage />, '/labs/:labId/report', '/labs/lab-1/report');
}

describe('LabReportPage', () => {
    it('disables submission until a file is selected', async () => {
        setupReport();
        renderReport();
        expect(await screen.findByRole('button', { name: 'Отправить отчет' })).toBeDisabled();
    });
    it.each([true, false])('uploads a file (success: %s)', async success => {
        let fileName = '';
        let fileContent = '';
        let reads = 0;
        setupReport();
        server.use(
            http.get(`${endpoint}/reports/my`, () => {
                reads++;
                return HttpResponse.json({ allowResubmit: true, versions: [] });
            }),
            http.post(`${endpoint}/reports`, async ({ request }) => {
                const form = await request.formData();
                const file = form.get('file') as File;
                fileName = file.name;
                fileContent = await file.text();
                return new HttpResponse(null, { status: success ? 201 : 500 });
            }),
        );
        const { container } = renderReport();
        await screen.findByRole('button', { name: 'Отправить отчет' });
        // The existing file input has no accessible name; keep production markup unchanged.
        const input = container.querySelector<HTMLInputElement>('input[type="file"]')!;
        const user = userEvent.setup();
        await user.upload(input, new File(['report contents'], 'report.pdf', { type: 'application/pdf' }));
        await user.click(screen.getByRole('button', { name: 'Отправить отчет' }));
        if (success) {
            expect(await screen.findByText('Отчёты ещё не загружались')).toBeInTheDocument();
            expect(reads).toBe(2);
        } else {
            expect(await screen.findByText('Не удалось загрузить отчёт. Попробуйте ещё раз.')).toBeInTheDocument();
            expect(screen.getByRole('button', { name: 'Отправить отчет' })).toBeEnabled();
            expect(screen.getByText('report.pdf')).toBeInTheDocument();
        }
        expect(fileName).toBe('report.pdf');
        expect(fileContent).toBe('report contents');
    });
});
