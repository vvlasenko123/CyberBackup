import { useState, useRef, useEffect } from 'react';
import './BlockAutocomplete.css';

type Props = {
    value: string;
    onChange: (value: string) => void;
    suggestions: string[];
    placeholder?: string;
    inputClassName?: string;
};

const BlockAutocomplete = ({ value, onChange, suggestions, placeholder, inputClassName }: Props) => {
    const [open, setOpen] = useState(false);
    const containerRef = useRef<HTMLDivElement>(null);

    const filtered = suggestions.filter(s =>
        s.toLowerCase().includes(value.toLowerCase())
    );

    useEffect(() => {
        const handleClickOutside = (e: MouseEvent) => {
            if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
                setOpen(false);
            }
        };
        document.addEventListener('mousedown', handleClickOutside);
        return () => document.removeEventListener('mousedown', handleClickOutside);
    }, []);

    const handleSelect = (block: string) => {
        onChange(block);
        setOpen(false);
    };

    return (
        <div className="blk-wrap" ref={containerRef}>
            <input
                className={inputClassName}
                placeholder={placeholder}
                value={value}
                autoComplete="off"
                onChange={e => {
                    onChange(e.target.value);
                    setOpen(true);
                }}
                onFocus={() => setOpen(true)}
            />
            {open && filtered.length > 0 && (
                <div className="blk-dropdown">
                    {filtered.map(block => (
                        <div
                            key={block}
                            className={`blk-item${block === value ? ' blk-item--active' : ''}`}
                            onMouseDown={e => {
                                e.preventDefault(); // не снимаем фокус с инпута
                                handleSelect(block);
                            }}
                        >
                            {block}
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
};

export default BlockAutocomplete;
