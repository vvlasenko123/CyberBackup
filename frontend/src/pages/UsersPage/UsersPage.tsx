import React, { useState, useEffect, useCallback, useRef } from 'react';
import axiosInstance from '../../utils/axiosInstance';
import './UsersPage.css';

type UserRoleStr = 'student' | 'teacher' | 'admin' | 'superadmin';

interface UserRecord {
    id: string;
    fullName: string;
    email: string;
    role: number; // 0=student,1=teacher,2=admin,3=superadmin
}

interface CreateForm {
    fullName: string;
    email: string;
    password: string;
    role: number;
}

interface ImportResult {
    fullName: string;
    email: string;
    password?: string;
    success: boolean;
    error?: string;
}

interface GroupListItem {
    id: string;
    name: string;
}

const ROLE_LABELS: Record<number, string> = {
    0: 'Студент',
    1: 'Преподаватель',
    2: 'Администратор',
    3: 'Суперадмин',
};

const ROLE_CLASS: Record<number, UserRoleStr> = {
    0: 'student',
    1: 'teacher',
    2: 'admin',
    3: 'superadmin',
};

const PAGE_SIZE = 15;

const CopyButton: React.FC<{ text: string }> = ({ text }) => {
    const [copied, setCopied] = useState(false);

    const handleCopy = () => {
        navigator.clipboard.writeText(text).then(() => {
            setCopied(true);
            setTimeout(() => setCopied(false), 1500);
        });
    };

    return (
        <button
            className="users-copy-btn"
            onClick={handleCopy}
            title="Копировать пароль"
        >
            {copied ? '✓' : '⎘'}
        </button>
    );
};

export const UsersPage: React.FC = () => {
    const [users, setUsers] = useState<UserRecord[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [search, setSearch] = useState('');
    const [page, setPage] = useState(1);

    const [showCreate, setShowCreate] = useState(false);
    const [form, setForm] = useState<CreateForm>({ fullName: '', email: '', password: '', role: 0 });
    const [creating, setCreating] = useState(false);
    const [createError, setCreateError] = useState<string | null>(null);

    const [importResults, setImportResults] = useState<ImportResult[] | null>(null);
    const [importing, setImporting] = useState(false);
    const [importError, setImportError] = useState<string | null>(null);

    // Group selection for import
    const [groups, setGroups] = useState<GroupListItem[]>([]);
    const [selectedGroupId, setSelectedGroupId] = useState<string>('');
    const [showNewGroupInput, setShowNewGroupInput] = useState(false);
    const [newGroupName, setNewGroupName] = useState('');
    const [creatingGroup, setCreatingGroup] = useState(false);
    const fileInputRef = useRef<HTMLInputElement>(null);

    const fetchUsers = useCallback(async () => {
        setLoading(true);
        setError(null);
        try {
            const res = await axiosInstance.get<UserRecord[]>('/public/api/v1/user/get-all');
            setUsers(res.data);
        } catch (e: unknown) {
            const err = e as { response?: { data?: { message?: string } } };
            setError(err?.response?.data?.message || 'Не удалось загрузить пользователей');
        } finally {
            setLoading(false);
        }
    }, []);

    const fetchGroups = useCallback(async () => {
        try {
            const res = await axiosInstance.get<GroupListItem[]>('/public/api/v1/admin/groups');
            setGroups(res.data);
        } catch {
            // non-critical
        }
    }, []);

    useEffect(() => { fetchUsers(); }, [fetchUsers]);
    useEffect(() => { fetchGroups(); }, [fetchGroups]);

    // Reset to page 1 whenever search changes
    useEffect(() => { setPage(1); }, [search]);

    const filtered = users
        .filter(u =>
            u.fullName?.toLowerCase().includes(search.toLowerCase()) ||
            u.email?.toLowerCase().includes(search.toLowerCase())
        )
        .sort((a, b) => b.role - a.role); // superadmin→admin→teacher→student

    const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
    const paginated = filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);

    const handleCreate = async () => {
        if (!form.fullName.trim() || !form.email.trim() || !form.password.trim()) return;
        setCreating(true);
        setCreateError(null);
        try {
            await axiosInstance.post('/public/api/v1/user/create', {
                fullName: form.fullName.trim(),
                email: form.email.trim(),
                password: form.password.trim(),
                role: form.role,
            });
            setShowCreate(false);
            setForm({ fullName: '', email: '', password: '', role: 0 });
            await fetchUsers();
        } catch (e: unknown) {
            const err = e as { response?: { data?: { message?: string } } };
            setCreateError(err?.response?.data?.message || 'Не удалось создать пользователя');
        } finally {
            setCreating(false);
        }
    };

    const handleDelete = async (id: string, name: string) => {
        if (!confirm(`Удалить пользователя «${name}»?`)) return;
        try {
            await axiosInstance.delete(`/public/api/v1/user/delete/${id}`);
            setUsers(prev => prev.filter(u => u.id !== id));
        } catch (e: unknown) {
            const err = e as { response?: { data?: { message?: string } } };
            alert(err?.response?.data?.message || 'Не удалось удалить пользователя');
        }
    };

    const handleGroupSelectChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        const val = e.target.value;
        if (val === '__new__') {
            setShowNewGroupInput(true);
            setSelectedGroupId('');
        } else {
            setShowNewGroupInput(false);
            setSelectedGroupId(val);
        }
    };

    const handleCreateGroup = async () => {
        if (!newGroupName.trim()) return;
        setCreatingGroup(true);
        try {
            const res = await axiosInstance.post<{ id: string }>('/public/api/v1/admin/groups', { name: newGroupName.trim() });
            await fetchGroups();
            setSelectedGroupId(res.data.id);
            setShowNewGroupInput(false);
            setNewGroupName('');
        } catch (e: unknown) {
            const err = e as { response?: { data?: { message?: string } } };
            alert(err?.response?.data?.message || 'Не удалось создать группу');
        } finally {
            setCreatingGroup(false);
        }
    };

    const handleFileImport = async (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (!file) return;
        setImporting(true);
        setImportError(null);
        setImportResults(null);
        try {
            const formData = new FormData();
            formData.append('file', file);
            if (selectedGroupId) {
                formData.append('groupId', selectedGroupId);
            }
            const res = await axiosInstance.post<{ imported: ImportResult[] }>(
                '/public/api/v1/user/bulk-import',
                formData,
                { headers: { 'Content-Type': 'multipart/form-data' } }
            );
            setImportResults(res.data.imported);
            await fetchUsers();
        } catch (e: unknown) {
            const err = e as { response?: { data?: { message?: string } } };
            setImportError(err?.response?.data?.message || 'Ошибка импорта');
        } finally {
            setImporting(false);
            e.target.value = '';
        }
    };

    return (
        <div className="users-page">
            <div className="users-header">
                <h2 className="users-title">Пользователи</h2>
                <button className="users-btn-primary" onClick={() => setShowCreate(true)}>
                    + Создать пользователя
                </button>
            </div>

            {error && <div className="users-error">{error}</div>}

            <input
                className="users-search"
                type="text"
                placeholder="Поиск по имени или email..."
                value={search}
                onChange={e => setSearch(e.target.value)}
            />

            {loading ? (
                <div className="users-loading">Загрузка...</div>
            ) : (
                <>
                    <div className="users-table-card">
                        <table className="users-table">
                            <thead>
                                <tr>
                                    <th>ФИО</th>
                                    <th>Email</th>
                                    <th>Роль</th>
                                    <th></th>
                                </tr>
                            </thead>
                            <tbody>
                                {paginated.map(u => (
                                    <tr key={u.id}>
                                        <td>{u.fullName}</td>
                                        <td>{u.email}</td>
                                        <td>
                                            <span className={`users-role-badge users-role-badge--${ROLE_CLASS[u.role] ?? 'student'}`}>
                                                {ROLE_LABELS[u.role] ?? 'Студент'}
                                            </span>
                                        </td>
                                        <td>
                                            {u.role !== 3 && (
                                                <button
                                                    className="users-btn-danger"
                                                    onClick={() => handleDelete(u.id, u.fullName)}
                                                >
                                                    Удалить
                                                </button>
                                            )}
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                        {filtered.length === 0 && (
                            <div className="users-empty">
                                {search ? 'Пользователи не найдены' : 'Нет пользователей'}
                            </div>
                        )}
                    </div>

                    {totalPages > 1 && (
                        <div className="users-pagination">
                            <button
                                className="users-pagination-btn"
                                onClick={() => setPage(p => p - 1)}
                                disabled={page === 1}
                            >
                                ← Назад
                            </button>
                            <span className="users-pagination-info">
                                {page} / {totalPages} &nbsp;·&nbsp; {filtered.length} пользователей
                            </span>
                            <button
                                className="users-pagination-btn"
                                onClick={() => setPage(p => p + 1)}
                                disabled={page === totalPages}
                            >
                                Вперёд →
                            </button>
                        </div>
                    )}
                </>
            )}

            {showCreate && (
                <div className="users-modal-overlay" onClick={() => setShowCreate(false)}>
                    <div className="users-modal" onClick={e => e.stopPropagation()}>
                        <h3 className="users-modal__title">Создать пользователя</h3>
                        <div className="users-modal__field">
                            <label className="users-modal__label">ФИО *</label>
                            <input className="users-modal__input" value={form.fullName}
                                onChange={e => setForm(f => ({ ...f, fullName: e.target.value }))} />
                        </div>
                        <div className="users-modal__field">
                            <label className="users-modal__label">Email *</label>
                            <input className="users-modal__input" type="email" value={form.email}
                                onChange={e => setForm(f => ({ ...f, email: e.target.value }))} />
                        </div>
                        <div className="users-modal__field">
                            <label className="users-modal__label">Пароль *</label>
                            <input className="users-modal__input" type="password" value={form.password}
                                onChange={e => setForm(f => ({ ...f, password: e.target.value }))} />
                        </div>
                        <div className="users-modal__field">
                            <label className="users-modal__label">Роль</label>
                            <select className="users-modal__select" value={form.role}
                                onChange={e => setForm(f => ({ ...f, role: Number(e.target.value) }))}>
                                <option value={0}>Студент</option>
                                <option value={1}>Преподаватель</option>
                                <option value={2}>Администратор</option>
                            </select>
                        </div>
                        {createError && <div className="users-modal__error">{createError}</div>}
                        <div className="users-modal__actions">
                            <button className="users-btn-secondary" onClick={() => setShowCreate(false)}>Отмена</button>
                            <button className="users-btn-primary" onClick={handleCreate} disabled={creating}>
                                {creating ? 'Создание...' : 'Создать'}
                            </button>
                        </div>
                    </div>
                </div>
            )}

            <div className="users-import-section">
                <h3 className="users-import-title">Массовый импорт студентов</h3>
                <p className="users-import-desc">
                    Загрузите RTF-файл со списком группы
                </p>

                <div className="users-import-group-row">
                    <select
                        className="users-import-group-select"
                        value={showNewGroupInput ? '__new__' : selectedGroupId}
                        onChange={handleGroupSelectChange}
                    >
                        <option value="">— Без группы —</option>
                        {groups.map(g => (
                            <option key={g.id} value={g.id}>{g.name}</option>
                        ))}
                        <option value="__new__">+ Создать новую группу</option>
                    </select>

                    {showNewGroupInput && (
                        <>
                            <input
                                className="users-import-group-input"
                                placeholder="Название группы"
                                value={newGroupName}
                                onChange={e => setNewGroupName(e.target.value)}
                                onKeyDown={e => { if (e.key === 'Enter') handleCreateGroup(); }}
                                autoFocus
                            />
                            <button
                                className="users-btn-primary"
                                onClick={handleCreateGroup}
                                disabled={creatingGroup || !newGroupName.trim()}
                            >
                                {creatingGroup ? '...' : 'Создать'}
                            </button>
                            <button
                                className="users-btn-secondary"
                                onClick={() => { setShowNewGroupInput(false); setNewGroupName(''); }}
                            >
                                Отмена
                            </button>
                        </>
                    )}
                </div>

                <input
                    ref={fileInputRef}
                    type="file"
                    accept=".rtf"
                    onChange={handleFileImport}
                    disabled={importing}
                    style={{ display: 'block', marginBottom: 8, color: '#D1D5DB' }}
                />
                {importing && <div style={{ color: '#9CA3AF', fontSize: 14 }}>Импортируем...</div>}
                {importError && <div className="users-error">{importError}</div>}
                {importResults && (
                    <div className="users-import-result">
                        <div className="users-import-result-title">
                            Результат: {importResults.filter(r => r.success).length} создано,{' '}
                            {importResults.filter(r => !r.success).length} ошибок
                        </div>
                        {importResults.map((r, i) => (
                            <div key={i} className={`users-import-row users-import-row--${r.success ? 'ok' : 'err'}`}>
                                {r.success ? '✓' : '✗'} {r.fullName} — {r.email}
                                {r.success && r.password && (
                                    <span className="users-import-password">
                                        пароль: <code className="users-import-password__code">{r.password}</code>
                                        <CopyButton text={r.password} />
                                    </span>
                                )}
                                {!r.success && r.error && <span> — {r.error}</span>}
                            </div>
                        ))}
                    </div>
                )}
            </div>
        </div>
    );
};

export default UsersPage;
