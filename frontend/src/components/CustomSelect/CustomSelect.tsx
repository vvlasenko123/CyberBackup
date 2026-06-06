import { useState, useRef, useEffect } from 'react';
import './CustomSelect.css';

export interface SelectOption {
    value: string;
    label: string;
    group?: string;
    isAction?: boolean;
}

interface Props {
    value: string;
    onChange: (value: string) => void;
    options: SelectOption[];
    placeholder?: string;
    className?: string;
    disabled?: boolean;
}

const CustomSelect = ({ value, onChange, options, placeholder, className, disabled }: Props) => {
    const [open, setOpen] = useState(false);
    const wrapRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        const handleClickOutside = (e: MouseEvent) => {
            if (wrapRef.current && !wrapRef.current.contains(e.target as Node)) {
                setOpen(false);
            }
        };
        document.addEventListener('mousedown', handleClickOutside);
        return () => document.removeEventListener('mousedown', handleClickOutside);
    }, []);

    const selectedLabel = options.find(o => o.value === value)?.label ?? placeholder ?? '';
    const isPlaceholder = !value && !options.find(o => o.value === value && o.value !== '');

    const ungrouped = options.filter(o => !o.group);
    const groups = [...new Set(options.filter(o => o.group).map(o => o.group!))];

    const handleSelect = (val: string) => {
        onChange(val);
        setOpen(false);
    };

    return (
        <div className={`csel-wrap${className ? ` ${className}` : ''}`} ref={wrapRef}>
            <button
                type="button"
                className={`csel-trigger${open ? ' csel-trigger--open' : ''}${disabled ? ' csel-trigger--disabled' : ''}`}
                onClick={() => !disabled && setOpen(o => !o)}
                disabled={disabled}
            >
                <span className={isPlaceholder && !value ? 'csel-placeholder' : ''}>
                    {selectedLabel}
                </span>
                <svg
                    className={`csel-chevron${open ? ' csel-chevron--open' : ''}`}
                    width="16" height="16" viewBox="0 0 24 24"
                    fill="none" stroke="currentColor" strokeWidth="2"
                >
                    <polyline points="6 9 12 15 18 9" />
                </svg>
            </button>

            {open && (
                <div className="csel-dropdown">
                    {ungrouped.map(opt => (
                        <div
                            key={opt.value}
                            className={[
                                'csel-item',
                                opt.value === value ? 'csel-item--active' : '',
                                opt.isAction ? 'csel-item--action' : '',
                            ].filter(Boolean).join(' ')}
                            onMouseDown={e => { e.preventDefault(); handleSelect(opt.value); }}
                        >
                            {opt.label}
                        </div>
                    ))}
                    {groups.map(group => (
                        <div key={group}>
                            <div className="csel-group-label">{group}</div>
                            {options.filter(o => o.group === group).map(opt => (
                                <div
                                    key={opt.value}
                                    className={[
                                        'csel-item',
                                        opt.value === value ? 'csel-item--active' : '',
                                        opt.isAction ? 'csel-item--action' : '',
                                    ].filter(Boolean).join(' ')}
                                    onMouseDown={e => { e.preventDefault(); handleSelect(opt.value); }}
                                >
                                    {opt.label}
                                </div>
                            ))}
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
};

export default CustomSelect;
