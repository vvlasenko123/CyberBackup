import { useState } from 'react';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';
import BlockAutocomplete from './BlockAutocomplete';

function Autocomplete() {
    const [value, setValue] = useState('');
    return <BlockAutocomplete value={value} onChange={setValue} suggestions={['Сети', 'Linux']} />;
}
describe('BlockAutocomplete', () => {
    it('filters case-insensitively and selects a suggestion', async () => {
        render(<Autocomplete />);
        const user = userEvent.setup();
        await user.type(screen.getByRole('textbox'), 'lin');
        expect(screen.queryByText('Сети')).not.toBeInTheDocument();
        await user.click(screen.getByText('Linux'));
        expect(screen.getByRole('textbox')).toHaveValue('Linux');
        expect(screen.queryByText('Linux')).not.toBeInTheDocument();
    });
    it('closes suggestions on outside click and hides non-matches', async () => {
        render(<Autocomplete />);
        const user = userEvent.setup();
        await user.click(screen.getByRole('textbox'));
        expect(screen.getByText('Сети')).toBeInTheDocument();
        await user.click(document.body);
        expect(screen.queryByText('Сети')).not.toBeInTheDocument();
        await user.type(screen.getByRole('textbox'), 'unknown');
        expect(screen.queryByText('Linux')).not.toBeInTheDocument();
    });
});
