import { createContext, useContext } from 'react';
import type { MouseEvent } from 'react';
import { Day, DayPicker } from 'react-day-picker';
import type { DayProps } from 'react-day-picker';
import { ru } from 'react-day-picker/locale';
import { Skeleton } from '../../components/Skeleton/Skeleton';
import type { CalendarEvent } from './CalendarPage';
import 'react-day-picker/style.css';
import './DesktopCalendar.css';

type Props = {
    events: CalendarEvent[];
    loading: boolean;
    canDelete: (event: CalendarEvent) => boolean;
    onDelete: (id: string, event: MouseEvent) => void;
    colors: Record<CalendarEvent['eventType'], string>;
};

const EventsContext = createContext<Props | null>(null);

function EventDay(props: DayProps) {
    const context = useContext(EventsContext);
    if (!context) throw new Error('EventDay must be rendered inside DesktopCalendar');
    const { events, loading, canDelete, onDelete, colors } = context;
    const dayEvents = events.filter(event => new Date(event.startsAtUtc).toDateString() === props.day.date.toDateString());

    return <Day {...props}>
        <div className="cal-day__content">
            <span className="cal-day__num">{props.children}</span>
            {loading ? <Skeleton height={20} /> : <div className="cal-day__events">
                {dayEvents.map(event => <div className="cal-event" key={event.id} style={{ borderLeftColor: colors[event.eventType] }}>
                    <span className="cal-event__text">
                        {new Date(event.startsAtUtc).toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' })} {event.title}
                    </span>
                    {canDelete(event) && <button
                        type="button"
                        className="cal-event__delete"
                        title="Удалить"
                        aria-label={`Удалить ${event.title}`}
                        onClick={e => onDelete(event.id, e)}
                    >×</button>}
                </div>)}
            </div>}
        </div>
    </Day>;
}

export function DesktopCalendar(props: Props) {
    return <EventsContext.Provider value={props}>
        <div className="cal-card desktop-calendar" role={props.loading ? 'status' : undefined} aria-busy={props.loading || undefined} aria-label={props.loading ? 'Загрузка расписания' : undefined}>
            <DayPicker
                locale={ru}
                weekStartsOn={1}
                fixedWeeks
                showOutsideDays
                navLayout="around"
                components={{ Day: EventDay }}
                classNames={{ day: 'rdp-day cal-day', outside: 'cal-day--out', today: 'cal-day--today' }}
                labels={{ labelPrevious: () => 'Предыдущий месяц', labelNext: () => 'Следующий месяц' }}
            />
        </div>
    </EventsContext.Provider>;
}
