import React, { useState, useEffect } from 'react';
import axiosInstance from '../../utils/axiosInstance';
import './CalendarPage.css';

// 0=Lecture, 1=Webinar, 2=Exam, 3=Lab, 4=Other — matches CalendarEventType enum
type EventType = 0 | 1 | 2 | 3 | 4;

interface CalendarEvent {
    id: string;
    userId: string;
    title: string;
    eventType: EventType;
    status: 0 | 1 | 2;
    startsAtUtc: string;
    endsAtUtc: string;
    notifyAtUtc?: string;
    createdAtUtc: string;
    updatedAtUtc: string;
}

const MONTHS_RU = [
    'Январь', 'Февраль', 'Март', 'Апрель', 'Май', 'Июнь',
    'Июль', 'Август', 'Сентябрь', 'Октябрь', 'Ноябрь', 'Декабрь',
];

const WEEKDAYS = ['ПН', 'ВТ', 'СР', 'ЧТ', 'ПТ', 'СБ', 'ВС'];

const EVENT_TYPE_LABELS: Record<EventType, string> = {
    0: 'Лекция',
    1: 'Вебинар',
    2: 'Экзамен',
    3: 'Лабораторная',
    4: 'Другое',
};

const EVENT_TYPE_COLORS: Record<EventType, string> = {
    0: '#027ef2', // Лекция — синий
    1: '#22c55e', // Вебинар — зелёный
    2: '#ef4444', // Экзамен — красный
    3: '#f97316', // Лабораторная — оранжевый
    4: '#8b5cf6', // Другое — фиолетовый
};

const getCalendarDays = (year: number, month: number): Date[] => {
    const firstDay = new Date(year, month, 1);
    const dow = firstDay.getDay();
    const offset = dow === 0 ? -6 : 1 - dow;
    const start = new Date(year, month, 1 + offset);
    return Array.from({ length: 42 }, (_, i) =>
        new Date(start.getFullYear(), start.getMonth(), start.getDate() + i)
    );
};

const isSameDay = (a: Date, b: Date) =>
    a.getFullYear() === b.getFullYear() &&
    a.getMonth() === b.getMonth() &&
    a.getDate() === b.getDate();

const formatTime = (iso: string) => {
    const d = new Date(iso);
    return d.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' });
};

const toLocalInput = (d: Date) => {
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
};

export const CalendarPage: React.FC = () => {
    const today = new Date();
    const role = localStorage.getItem('user_role') || 'student';
    const canCreate = role === 'admin' || role === 'teacher';

    const [currentDate, setCurrentDate] = useState(
        new Date(today.getFullYear(), today.getMonth(), 1)
    );
    const [events, setEvents] = useState<CalendarEvent[]>([]);
    const [loading, setLoading] = useState(true);
    const [showModal, setShowModal] = useState(false);
    const [submitting, setSubmitting] = useState(false);
    const [formError, setFormError] = useState<string | null>(null);
    const [form, setForm] = useState({
        title: '',
        eventType: 0 as EventType,
        startsAtUtc: '',
        endsAtUtc: '',
        notifyAtUtc: '',
    });

    const calendarDays = getCalendarDays(currentDate.getFullYear(), currentDate.getMonth());

    useEffect(() => {
        fetchEvents();
    }, []);

    const fetchEvents = async () => {
        try {
            setLoading(true);
            const res = await axiosInstance.get<CalendarEvent[]>('/public/calendar/events');
            setEvents(res.data);
        } catch {
            // ignore
        } finally {
            setLoading(false);
        }
    };

    const openModal = () => {
        const now = new Date();
        const later = new Date(now.getTime() + 60 * 60 * 1000);
        setForm({
            title: '',
            eventType: 0,
            startsAtUtc: toLocalInput(now),
            endsAtUtc: toLocalInput(later),
            notifyAtUtc: '',
        });
        setFormError(null);
        setShowModal(true);
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setSubmitting(true);
        setFormError(null);
        try {
            await axiosInstance.post('/public/calendar/event', {
                title: form.title,
                eventType: form.eventType,
                startsAtUtc: new Date(form.startsAtUtc).toISOString(),
                endsAtUtc: new Date(form.endsAtUtc).toISOString(),
                ...(form.notifyAtUtc && { notifyAtUtc: new Date(form.notifyAtUtc).toISOString() }),
            });
            await fetchEvents();
            setShowModal(false);
        } catch (err: any) {
            setFormError(err?.response?.data?.message || 'Не удалось создать событие');
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <div className="cal-page">
            <div className="cal-page__header">
                <h2 className="cal-page__title">Расписание</h2>
                {canCreate && (
                    <button className="cal-page__add-btn" onClick={openModal}>
                        + Добавить событие
                    </button>
                )}
            </div>

            {/* Legend */}
            <div className="cal-legend">
                {(Object.entries(EVENT_TYPE_LABELS) as [string, string][]).map(([key, label]) => (
                    <span key={key} className="cal-legend__item">
                        <span
                            className="cal-legend__dot"
                            style={{ background: EVENT_TYPE_COLORS[Number(key) as EventType] }}
                        />
                        {label}
                    </span>
                ))}
            </div>

            <div className="cal-card">
                <div className="cal-nav">
                    <button className="cal-nav__arrow" onClick={() =>
                        setCurrentDate(d => new Date(d.getFullYear(), d.getMonth() - 1, 1))
                    }>‹</button>
                    <span className="cal-nav__label">
                        {MONTHS_RU[currentDate.getMonth()]} {currentDate.getFullYear()}
                    </span>
                    <button className="cal-nav__arrow" onClick={() =>
                        setCurrentDate(d => new Date(d.getFullYear(), d.getMonth() + 1, 1))
                    }>›</button>
                </div>

                <div className="cal-grid">
                    {WEEKDAYS.map(d => (
                        <div key={d} className="cal-weekday">{d}</div>
                    ))}

                    {loading ? (
                        <div className="cal-loading">Загрузка...</div>
                    ) : calendarDays.map((day, i) => {
                        const inMonth = day.getMonth() === currentDate.getMonth();
                        const isToday = isSameDay(day, today);
                        const dayEvents = events.filter(e => isSameDay(new Date(e.startsAtUtc), day));

                        return (
                            <div key={i} className={[
                                'cal-day',
                                !inMonth && 'cal-day--out',
                                isToday && 'cal-day--today',
                            ].filter(Boolean).join(' ')}>
                                <span className="cal-day__num">{day.getDate()}</span>
                                <div className="cal-day__events">
                                    {dayEvents.map(ev => (
                                        <div
                                            key={ev.id}
                                            className="cal-event"
                                            style={{ borderLeftColor: EVENT_TYPE_COLORS[ev.eventType] }}
                                        >
                                            <span className="cal-event__title">{ev.title}</span>
                                            <span className="cal-event__time">{formatTime(ev.startsAtUtc)}</span>
                                        </div>
                                    ))}
                                </div>
                            </div>
                        );
                    })}
                </div>
            </div>

            {showModal && (
                <div className="cal-modal-overlay" onClick={() => setShowModal(false)}>
                    <div className="cal-modal" onClick={e => e.stopPropagation()}>
                        <h3 className="cal-modal__title">Новое событие</h3>
                        <form onSubmit={handleSubmit} className="cal-modal__form">
                            <label className="cal-modal__field">
                                <span className="cal-modal__label">Название *</span>
                                <input
                                    className="cal-modal__input"
                                    type="text"
                                    value={form.title}
                                    onChange={e => setForm(f => ({ ...f, title: e.target.value }))}
                                    placeholder="Введите название"
                                    required
                                />
                            </label>
                            <label className="cal-modal__field">
                                <span className="cal-modal__label">Тип события</span>
                                <select
                                    className="cal-modal__input cal-modal__select"
                                    value={form.eventType}
                                    onChange={e => setForm(f => ({ ...f, eventType: Number(e.target.value) as EventType }))}
                                    style={{ borderLeftColor: EVENT_TYPE_COLORS[form.eventType], borderLeftWidth: 3 }}
                                >
                                    {(Object.entries(EVENT_TYPE_LABELS) as [string, string][]).map(([val, label]) => (
                                        <option key={val} value={val}>{label}</option>
                                    ))}
                                </select>
                            </label>
                            <label className="cal-modal__field">
                                <span className="cal-modal__label">Начало *</span>
                                <input
                                    className="cal-modal__input"
                                    type="datetime-local"
                                    value={form.startsAtUtc}
                                    onChange={e => setForm(f => ({ ...f, startsAtUtc: e.target.value }))}
                                    required
                                />
                            </label>
                            <label className="cal-modal__field">
                                <span className="cal-modal__label">Конец *</span>
                                <input
                                    className="cal-modal__input"
                                    type="datetime-local"
                                    value={form.endsAtUtc}
                                    onChange={e => setForm(f => ({ ...f, endsAtUtc: e.target.value }))}
                                    required
                                />
                            </label>
                            <label className="cal-modal__field">
                                <span className="cal-modal__label">Уведомление</span>
                                <input
                                    className="cal-modal__input"
                                    type="datetime-local"
                                    value={form.notifyAtUtc}
                                    onChange={e => setForm(f => ({ ...f, notifyAtUtc: e.target.value }))}
                                />
                            </label>
                            {formError && <p className="cal-modal__error">{formError}</p>}
                            <div className="cal-modal__actions">
                                <button
                                    type="button"
                                    className="cal-modal__cancel"
                                    onClick={() => setShowModal(false)}
                                >
                                    Отмена
                                </button>
                                <button
                                    type="submit"
                                    className="cal-modal__submit"
                                    disabled={submitting}
                                >
                                    {submitting ? 'Создание...' : 'Создать'}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
};

export default CalendarPage;
