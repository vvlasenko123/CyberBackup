import { useState, useRef, useEffect, useMemo } from 'react';
import './MultiUserPicker.css';

export interface PickerUser {
    id: string;
    fullName: string;
    email: string;
}

interface Props {
    users: PickerUser[];
    onAdd: (ids: string[]) => Promise<void>;
    accent?: 'student' | 'teacher';
    triggerLabel?: string;
    emptyText?: string;
    searchPlaceholder?: string;
}

const MultiUserPicker = ({
    users,
    onAdd,
    accent = 'student',
    triggerLabel = 'Выбрать из списка',
    emptyText = 'Нет доступных пользователей',
    searchPlaceholder = 'Фильтр по имени или email...',
}: Props) => {
    const [open, setOpen] = useState(false);
    const [filter, setFilter] = useState('');
    const [selected, setSelected] = useState<Set<string>>(new Set());
    const [submitting, setSubmitting] = useState(false);
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

    // Сбрасываем выбор тех, кого уже нет в списке доступных (после добавления)
    useEffect(() => {
        setSelected(prev => {
            const valid = new Set(users.map(u => u.id));
            const next = new Set([...prev].filter(id => valid.has(id)));
            return next.size === prev.size ? prev : next;
        });
    }, [users]);

    const filtered = useMemo(() => {
        const q = filter.trim().toLowerCase();
        if (!q) return users;
        return users.filter(u =>
            u.fullName.toLowerCase().includes(q) || u.email.toLowerCase().includes(q)
        );
    }, [users, filter]);

    const allFilteredSelected = filtered.length > 0 && filtered.every(u => selected.has(u.id));

    const toggle = (id: string) => {
        setSelected(prev => {
            const next = new Set(prev);
            if (next.has(id)) next.delete(id);
            else next.add(id);
            return next;
        });
    };

    const toggleAllFiltered = () => {
        setSelected(prev => {
            const next = new Set(prev);
            if (allFilteredSelected) {
                filtered.forEach(u => next.delete(u.id));
            } else {
                filtered.forEach(u => next.add(u.id));
            }
            return next;
        });
    };

    const handleAdd = async () => {
        if (selected.size === 0) return;
        setSubmitting(true);
        try {
            await onAdd([...selected]);
            setSelected(new Set());
            setFilter('');
            setOpen(false);
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <div className={`mup-wrap mup-wrap--${accent}`} ref={wrapRef}>
            <button
                type="button"
                className={`mup-trigger${open ? ' mup-trigger--open' : ''}`}
                onClick={() => setOpen(o => !o)}
            >
                <span className="mup-trigger-text">
                    {selected.size > 0 ? `Выбрано: ${selected.size}` : triggerLabel}
                </span>
                <svg
                    className={`mup-chevron${open ? ' mup-chevron--open' : ''}`}
                    width="16" height="16" viewBox="0 0 24 24"
                    fill="none" stroke="currentColor" strokeWidth="2"
                >
                    <polyline points="6 9 12 15 18 9" />
                </svg>
            </button>

            {open && (
                <div className="mup-panel">
                    <div className="mup-panel-search">
                        <input
                            className="mup-filter"
                            type="text"
                            placeholder={searchPlaceholder}
                            value={filter}
                            onChange={e => setFilter(e.target.value)}
                            autoFocus
                        />
                    </div>

                    {filtered.length > 0 && (
                        <button type="button" className="mup-selectall" onClick={toggleAllFiltered}>
                            <span className={`mup-checkbox${allFilteredSelected ? ' mup-checkbox--checked' : ''}`}>
                                {allFilteredSelected && (
                                    <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3">
                                        <polyline points="20 6 9 17 4 12" />
                                    </svg>
                                )}
                            </span>
                            {allFilteredSelected ? 'Снять выделение' : `Выбрать всех (${filtered.length})`}
                        </button>
                    )}

                    <div className="mup-list">
                        {filtered.length === 0 ? (
                            <p className="mup-empty">{emptyText}</p>
                        ) : (
                            filtered.map(u => {
                                const checked = selected.has(u.id);
                                return (
                                    <div
                                        key={u.id}
                                        className={`mup-item${checked ? ' mup-item--checked' : ''}`}
                                        onClick={() => toggle(u.id)}
                                    >
                                        <span className={`mup-checkbox${checked ? ' mup-checkbox--checked' : ''}`}>
                                            {checked && (
                                                <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3">
                                                    <polyline points="20 6 9 17 4 12" />
                                                </svg>
                                            )}
                                        </span>
                                        <div className="mup-avatar">{u.fullName[0]?.toUpperCase()}</div>
                                        <div className="mup-info">
                                            <span className="mup-name">{u.fullName}</span>
                                            <span className="mup-email">{u.email}</span>
                                        </div>
                                    </div>
                                );
                            })
                        )}
                    </div>

                    <div className="mup-footer">
                        <button
                            type="button"
                            className="mup-add-btn"
                            disabled={selected.size === 0 || submitting}
                            onClick={handleAdd}
                        >
                            {submitting ? 'Добавление...' : `Добавить выбранных${selected.size > 0 ? ` (${selected.size})` : ''}`}
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
};

export default MultiUserPicker;
