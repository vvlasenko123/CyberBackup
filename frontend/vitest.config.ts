import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';

export default defineConfig({
    plugins: [react()],
    test: {
        environment: 'jsdom',
        setupFiles: ['./src/test/setup.ts'],
        env: { VITE_API_BASE_URL: 'http://localhost:5000' },
        coverage: {
            provider: 'v8',
            include: [
                'src/components/ProtectedRoute.tsx',
                'src/components/CustomSelect/CustomSelect.tsx',
                'src/components/BlockAutocomplete/BlockAutocomplete.tsx',
                'src/features/auth/auth.ts',
                'src/hooks/useIsDesktop.ts',
                'src/utils/axiosInstance.ts',
                'src/pages/LoginPage/LoginPage.tsx',
                'src/pages/LogOut.tsx',
                'src/pages/LabsPage/LabsPage.tsx',
                'src/pages/LabReportPage/LabReportPage.tsx',
            ],
            reporter: ['text', 'html', 'json-summary'],
        },
    },
});
