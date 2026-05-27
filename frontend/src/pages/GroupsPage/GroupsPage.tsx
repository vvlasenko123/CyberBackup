import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import axiosInstance from '../../utils/axiosInstance';
import './GroupsPage.css';

type GroupListItem = {
    id: string;
    name: string;
    studentCount: number;
    teacherCount: number;
    createdAt: string;
};

const GroupsPage = () => {
    const navigate = useNavigate();
    const [groups, setGroups] = useState<GroupListItem[]>([]);
    const [loading, setLoading] = useState(true);
    const [newGroupName, setNewGroupName] = useState('');
    const [creating, setCreating] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const fetchGroups = async () => {
        try {
            const res = await axiosInstance.get<GroupListItem[]>('/public/api/v1/admin/groups');
            setGroups(res.data);
        } catch {
            // ignore
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => { fetchGroups(); }, []);

    const handleCreate = async () => {
        if (!newGroupName.trim() || creating) return;
        setCreating(true);
        setError(null);
        try {
            await axiosInstance.post('/public/api/v1/admin/groups', { name: newGroupName.trim() });
            setNewGroupName('');
            await fetchGroups();
        } catch {
            setError('Не удалось создать группу');
        } finally {
            setCreating(false);
        }
    };

    const handleDelete = async (groupId: string, groupName: string) => {
        if (!confirm(`Удалить группу «${groupName}»? Студенты и преподаватели будут откреплены.`)) return;
        try {
            await axiosInstance.delete(`/public/api/v1/admin/groups/${groupId}`);
            setGroups(prev => prev.filter(g => g.id !== groupId));
        } catch {
            setError('Не удалось удалить группу');
        }
    };

    if (loading) return <div className="groups-loading">Загрузка...</div>;

    return (
        <div className="groups-page">
            <div className="groups-create-bar">
                <input
                    className="groups-create-input"
                    type="text"
                    placeholder="Название группы (напр. «Группа 4203»)"
                    value={newGroupName}
                    onChange={e => setNewGroupName(e.target.value)}
                    onKeyDown={e => e.key === 'Enter' && handleCreate()}
                />
                <button
                    className="groups-create-btn"
                    onClick={handleCreate}
                    disabled={creating || !newGroupName.trim()}
                >
                    {creating ? 'Создание...' : '+ Создать группу'}
                </button>
            </div>

            {error && <div className="groups-error">{error}</div>}

            {groups.length === 0 ? (
                <div className="groups-empty">Групп ещё нет. Создайте первую группу выше.</div>
            ) : (
                <div className="groups-table-wrap">
                    <table className="groups-table">
                        <thead>
                            <tr>
                                <th>Название</th>
                                <th>Студентов</th>
                                <th>Преподавателей</th>
                                <th></th>
                            </tr>
                        </thead>
                        <tbody>
                            {groups.map(group => (
                                <tr
                                    key={group.id}
                                    className="groups-row"
                                    onClick={() => navigate(`/groups/${group.id}`)}
                                >
                                    <td className="groups-cell--name">{group.name}</td>
                                    <td className="groups-cell--count">{group.studentCount}</td>
                                    <td className="groups-cell--count">{group.teacherCount}</td>
                                    <td>
                                        <button
                                            className="groups-delete-btn"
                                            onClick={e => { e.stopPropagation(); handleDelete(group.id, group.name); }}
                                        >
                                            Удалить
                                        </button>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}
        </div>
    );
};

export default GroupsPage;
