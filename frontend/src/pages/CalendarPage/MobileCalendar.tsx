import { useEffect, useMemo, useRef, useState } from 'react';
import type { MouseEvent } from 'react';
import { DayPicker } from 'react-day-picker';
import { ru } from 'react-day-picker/locale';
import 'react-day-picker/style.css';
import type { CalendarEvent } from './CalendarPage';
import './MobileCalendar.css';

type Props = {
    events: CalendarEvent[];
    loading: boolean;
    canCreate: boolean;
    canDelete: (event: CalendarEvent) => boolean;
    onCreate: (date: Date) => void;
    onDelete: (id: string, event: MouseEvent) => void;
    labels: Record<CalendarEvent['eventType'], string>;
    colors: Record<CalendarEvent['eventType'], string>;
};

const dayKey = (date: Date) => `${date.getFullYear()}-${date.getMonth()}-${date.getDate()}`;
const time = (value: string) => new Date(value).toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' });
const dateTime = (date: Date, hour: number) => new Date(date.getFullYear(), date.getMonth(), date.getDate(), hour);

export function MobileCalendar({ events, loading, canCreate, canDelete, onCreate, onDelete, labels, colors }: Props) {
    const [firstMonth, setFirstMonth] = useState(() => new Date(new Date().getFullYear(), new Date().getMonth(), 1));
    const [monthCount, setMonthCount] = useState(3);
    const [selected, setSelected] = useState<Date>(new Date());
    const [showDay, setShowDay] = useState(false);
    const dayHeading = useRef<HTMLHeadingElement>(null);
    const calendarHeading = useRef<HTMLHeadingElement>(null);
    const returnToCalendar = useRef(false);

    const eventsByDay = useMemo(() => {
        const days = new Map<string, CalendarEvent[]>();
        for (const event of events) {
            const key = dayKey(new Date(event.startsAtUtc));
            const list = days.get(key) ?? [];
            list.push(event);
            days.set(key, list);
        }
        for (const list of days.values()) list.sort((a, b) => Date.parse(a.startsAtUtc) - Date.parse(b.startsAtUtc));
        return days;
    }, [events]);

    useEffect(() => {
        if (showDay) dayHeading.current?.focus();
        else if (returnToCalendar.current) {
            calendarHeading.current?.focus();
            returnToCalendar.current = false;
        }
    }, [showDay]);

    const selectedEvents = eventsByDay.get(dayKey(selected)) ?? [];
    const selectDay = (date: Date) => {
        setSelected(date);
        setShowDay(true);
    };

    return (
        <section className="mobile-calendar" aria-label="Мобильный календарь">
            {showDay ? <>
                <div className="mobile-calendar__toolbar">
                    <button type="button" onClick={() => { returnToCalendar.current = true; setShowDay(false); }}>‹ К месяцам</button>
                    <button type="button" onClick={() => selectDay(new Date())}>Сегодня</button>
                </div>
                <div className="mobile-calendar__day-heading">
                    <button type="button" aria-label="Предыдущий день" onClick={() => selectDay(dateTime(selected, -24))}>‹</button>
                    <h3 ref={dayHeading} tabIndex={-1}>{selected.toLocaleDateString('ru-RU', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' })}</h3>
                    <button type="button" aria-label="Следующий день" onClick={() => selectDay(dateTime(selected, 24))}>›</button>
                </div>
                {loading ? <p role="status">Загружаем расписание…</p> : <>
                    {selectedEvents.length === 0 && <p className="mobile-calendar__empty">На этот день событий нет</p>}
                    {canCreate && <button className="mobile-calendar__create" type="button" onClick={() => onCreate(dateTime(selected, 9))}>+ Событие на эту дату</button>}
                    <ol className="mobile-calendar__hours" aria-label="Расписание по часам">
                        {Array.from({ length: 24 }, (_, hour) => <li key={hour} className="mobile-calendar__hour">
                            <span className="mobile-calendar__time">{String(hour).padStart(2, '0')}:00</span>
                            <div className="mobile-calendar__hour-content">
                                {selectedEvents.filter(event => new Date(event.startsAtUtc).getHours() === hour).map(event => (
                                    <article className="mobile-calendar__event" key={event.id} style={{ borderLeftColor: colors[event.eventType] }}>
                                        <div>
                                            <time>{time(event.startsAtUtc)} — {time(event.endsAtUtc)}{dayKey(new Date(event.endsAtUtc)) !== dayKey(selected) && ` (${new Date(event.endsAtUtc).toLocaleDateString('ru-RU')})`}</time>
                                            <strong>{event.title}</strong>
                                            <small>{labels[event.eventType]}</small>
                                        </div>
                                        {canDelete(event) && <button type="button" aria-label={`Удалить ${event.title}`} onClick={e => onDelete(event.id, e)}>×</button>}
                                    </article>
                                ))}
                                {canCreate && <button className="mobile-calendar__add-hour" type="button" aria-label={`Создать событие в ${hour}:00`} onClick={() => onCreate(dateTime(selected, hour))}>+</button>}
                            </div>
                        </li>)}
                    </ol>
                </>}
            </> : <>
                <div className="mobile-calendar__toolbar">
                    <h3 ref={calendarHeading} tabIndex={-1}>Календарь</h3>
                    <button type="button" onClick={() => {
                        const today = new Date();
                        setFirstMonth(new Date(today.getFullYear(), today.getMonth(), 1));
                        setMonthCount(3);
                        setSelected(today);
                    }}>Сегодня</button>
                </div>
                <button className="mobile-calendar__more" type="button" onClick={() => {
                    setFirstMonth(month => new Date(month.getFullYear(), month.getMonth() - 3, 1));
                    setMonthCount(count => count + 3);
                }}>Показать предыдущие месяцы</button>
                {loading && <p role="status">Загружаем события…</p>}
                <DayPicker
                    mode="single"
                    required
                    locale={ru}
                    weekStartsOn={1}
                    month={firstMonth}
                    numberOfMonths={monthCount}
                    hideNavigation
                    selected={selected}
                    onSelect={selectDay}
                    modifiers={{ scheduled: (date: Date) => eventsByDay.has(dayKey(date)) }}
                    modifiersClassNames={{ scheduled: 'mobile-calendar__scheduled' }}
                    labels={{ labelDayButton: (date: Date) => `${date.toLocaleDateString('ru-RU', { day: 'numeric', month: 'long', year: 'numeric' })}${eventsByDay.has(dayKey(date)) ? ', есть события' : ''}` }}
                />
                <button className="mobile-calendar__more" type="button" onClick={() => setMonthCount(count => count + 3)}>Показать следующие месяцы</button>
            </>}
        </section>
    );
}
