import { useState, useEffect, useRef } from 'react';
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
    const [modalOpen, setModalOpen] = useState(false);
    const [newGroupName, setNewGroupName] = useState('');
    const [creating, setCreating] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [renamingId, setRenamingId] = useState<string | null>(null);
    const [renameValue, setRenameValue] = useState('');
    const inputRef = useRef<HTMLInputElement>(null);

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

    const openModal = () => {
        setNewGroupName('');
        setError(null);
        setModalOpen(true);
        setTimeout(() => inputRef.current?.focus(), 50);
    };

    const closeModal = () => {
        if (creating) return;
        setModalOpen(false);
        setNewGroupName('');
        setError(null);
    };

    const handleCreate = async () => {
        if (!newGroupName.trim() || creating) return;
        setCreating(true);
        setError(null);
        try {
            await axiosInstance.post('/public/api/v1/admin/groups', { name: newGroupName.trim() });
            setModalOpen(false);
            setNewGroupName('');
            await fetchGroups();
        } catch {
            setError('Не удалось создать группу');
        } finally {
            setCreating(false);
        }
    };

    const startRename = (groupId: string, currentName: string) => {
        setRenamingId(groupId);
        setRenameValue(currentName);
    };

    const cancelRename = () => {
        setRenamingId(null);
        setRenameValue('');
    };

    const submitRename = async (groupId: string) => {
        const name = renameValue.trim();
        if (!name) return;
        try {
            await axiosInstance.put(`/public/api/v1/admin/groups/${groupId}`, { name });
            setGroups(prev => prev.map(g => (g.id === groupId ? { ...g, name } : g)));
            cancelRename();
        } catch (e: unknown) {
            const err = e as { response?: { data?: { message?: string } } };
            setError(err?.response?.data?.message || 'Не удалось переименовать группу');
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
            <div className="groups-header">
                <button className="groups-create-btn" onClick={openModal}>
                    + Создать группу
                </button>
            </div>

            {error && !modalOpen && <div className="groups-error">{error}</div>}

            {groups.length === 0 ? (
                <div className="groups-empty">Групп ещё нет. Создайте первую группу.</div>
            ) : (
                <div className="groups-table-wrap">
                    <table className="groups-table">
                        <thead>
                            <tr>
                                <th className="groups-th--name">Название</th>
                                <th className="groups-th--count">Студентов</th>
                                <th className="groups-th--count">Преподавателей</th>
                                <th></th>
                            </tr>
                        </thead>
                        <tbody>
                            {groups.map(group => (
                                <tr
                                    key={group.id}
                                    className="groups-row"
                                    onClick={() => { if (renamingId !== group.id) navigate(`/groups/${group.id}`); }}
                                >
                                    <td className="groups-cell--name">
                                        {renamingId === group.id ? (
                                            <input
                                                className="groups-rename-input"
                                                autoFocus
                                                value={renameValue}
                                                onClick={e => e.stopPropagation()}
                                                onChange={e => setRenameValue(e.target.value)}
                                                onKeyDown={e => {
                                                    e.stopPropagation();
                                                    if (e.key === 'Enter') submitRename(group.id);
                                                    if (e.key === 'Escape') cancelRename();
                                                }}
                                            />
                                        ) : (
                                            group.name
                                        )}
                                    </td>
                                    <td className="groups-cell--count">{group.studentCount}</td>
                                    <td className="groups-cell--count">{group.teacherCount}</td>
                                    <td>
                                        <div className="groups-row-actions">
                                            {renamingId === group.id ? (
                                                <>
                                                    <button
                                                        className="groups-rename-btn"
                                                        onClick={e => { e.stopPropagation(); submitRename(group.id); }}
                                                    >
                                                        Сохранить
                                                    </button>
                                                    <button
                                                        className="groups-rename-btn"
                                                        onClick={e => { e.stopPropagation(); cancelRename(); }}
                                                    >
                                                        Отмена
                                                    </button>
                                                </>
                                            ) : (
                                                <button
                                                    className="groups-rename-btn"
                                                    onClick={e => { e.stopPropagation(); startRename(group.id, group.name); }}
                                                >
                                                    Переименовать
                                                </button>
                                            )}
                                            <button
                                                className="groups-delete-btn"
                                                onClick={e => { e.stopPropagation(); handleDelete(group.id, group.name); }}
                                            >
                                                Удалить
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}

            {modalOpen && (
                <div className="groups-modal-overlay" onClick={closeModal}>
                    <div className="groups-modal" onClick={e => e.stopPropagation()}>
                        <h3 className="groups-modal-title">Новая группа</h3>
                        <input
                            ref={inputRef}
                            className="groups-modal-input"
                            type="text"
                            placeholder="Например: РИ-111111"
                            value={newGroupName}
                            onChange={e => setNewGroupName(e.target.value)}
                            onKeyDown={e => {
                                if (e.key === 'Enter') handleCreate();
                                if (e.key === 'Escape') closeModal();
                            }}
                        />
                        {error && <div className="groups-modal-error">{error}</div>}
                        <div className="groups-modal-actions">
                            <button className="groups-modal-cancel" onClick={closeModal} disabled={creating}>
                                Отмена
                            </button>
                            <button
                                className="groups-modal-confirm"
                                onClick={handleCreate}
                                disabled={creating || !newGroupName.trim()}
                            >
                                {creating ? 'Создание...' : 'Создать'}
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default GroupsPage;
