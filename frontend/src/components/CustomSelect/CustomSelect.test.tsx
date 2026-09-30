import { useState } from 'react';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import CustomSelect from './CustomSelect';

const options = [{ value: 'one', label: 'Первый' }, { value: 'two', label: 'Второй', group: 'Группа' }];
function ControlledSelect() {
    const [value, setValue] = useState('');
    return <CustomSelect value={value} onChange={setValue} options={options} placeholder="Выберите" />;
}
describe('CustomSelect', () => {
    it('selects an option and closes the list', async () => {
        render(<ControlledSelect />);
        const user = userEvent.setup();
        await user.click(screen.getByRole('button', { name: 'Выберите' }));
        expect(screen.getByText('Группа')).toBeInTheDocument();
        await user.click(screen.getByText('Второй'));
        expect(screen.getByRole('button', { name: 'Второй' })).toBeInTheDocument();
        expect(screen.queryByText('Первый')).not.toBeInTheDocument();
    });
    it('closes on an outside click', async () => {
        render(<ControlledSelect />);
        const user = userEvent.setup();
        await user.click(screen.getByRole('button'));
        await user.click(document.body);
        expect(screen.queryByText('Первый')).not.toBeInTheDocument();
    });
    it('does not open when disabled', async () => {
        const onChange = vi.fn();
        render(<CustomSelect value="" onChange={onChange} options={options} disabled />);
        await userEvent.setup().click(screen.getByRole('button'));
        expect(screen.queryByText('Первый')).not.toBeInTheDocument();
        expect(onChange).not.toHaveBeenCalled();
    });
});
