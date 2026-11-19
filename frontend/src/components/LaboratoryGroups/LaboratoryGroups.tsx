import { useEffect, useId, useState } from 'react';
import axiosInstance from '../../utils/axiosInstance';
import './LaboratoryGroups.css';

type Group = { id: string; name: string };
type Props = {
    groupIds: string[];
    restricted: boolean;
    disabled?: boolean;
    onChange: (groupIds: string[], restricted: boolean) => void;
};

export default function LaboratoryGroups({ groupIds, restricted, disabled, onChange }: Props) {
    const id = useId();
    const [groups, setGroups] = useState<Group[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(false);
    const [attempt, setAttempt] = useState(0);

    useEffect(() => {
        let active = true;
        setLoading(true);
        setError(false);
        axiosInstance.get<Group[]>('/public/api/v1/teacher/groups')
            .then(response => { if (active) setGroups(response.data); })
            .catch(() => { if (active) setError(true); })
            .finally(() => { if (active) setLoading(false); });
        return () => { active = false; };
    }, [attempt]);

    const options = [...groups, ...groupIds.filter(groupId => !groups.some(g => g.id === groupId))
        .map(groupId => ({ id: groupId, name: `Недоступная группа (${groupId})` }))];

    const selection = restricted
        ? options.filter(group => groupIds.includes(group.id)).map(group => group.name).join(', ') || 'Выберите группы'
        : 'Все группы';

    return (
        <fieldset className="laboratory-groups" disabled={disabled}>
            <legend id={`${id}-label`}>Группы</legend>
            <details className="laboratory-groups-dropdown">
                <summary aria-labelledby={`${id}-label ${id}-selection`} tabIndex={disabled ? -1 : 0}
                    onClick={event => { if (disabled) event.preventDefault(); }}>
                    <span id={`${id}-selection`}>{selection}</span>
                </summary>
                <div className="laboratory-groups-options">
                    <label>
                        <input type="checkbox" checked={!restricted} onChange={() => onChange([], false)} />
                        Все группы
                    </label>
                    {loading && <p role="status">Загрузка групп…</p>}
                    {error && <div role="alert">Не удалось загрузить группы. <button type="button"
                        onClick={() => setAttempt(value => value + 1)}>Повторить</button></div>}
                    {!loading && !error && groups.length === 0 && <p>Нет доступных групп. Обратитесь к администратору.</p>}
                    {options.map(group => <label key={group.id}>
                        <input type="checkbox" checked={restricted && groupIds.includes(group.id)}
                            onChange={event => {
                                const selected = event.target.checked
                                    ? [...groupIds, group.id] : groupIds.filter(value => value !== group.id);
                                onChange(selected, selected.length > 0);
                            }} />
                        {group.name}
                    </label>)}
                </div>
            </details>
            {restricted && groupIds.length === 0 &&
                <p className="laboratory-groups-help">Выберите хотя бы одну группу.</p>}
        </fieldset>
    );
}
