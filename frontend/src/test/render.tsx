/* eslint-disable react-refresh/only-export-components -- Test render helper, never loaded by the application. */
import type { ReactElement } from 'react';
import { render } from '@testing-library/react';
import { MemoryRouter, Route, Routes, useLocation } from 'react-router-dom';

function Destination() {
    const location = useLocation();
    return <output aria-label="Current route">{location.pathname}</output>;
}

export function renderPage(page: ReactElement, path = '/', entry = path) {
    window.history.replaceState({}, '', entry);
    return render(
        <MemoryRouter initialEntries={[entry]}>
            <Routes>
                <Route path={path} element={page} />
                <Route path="*" element={<Destination />} />
            </Routes>
        </MemoryRouter>,
    );
}
