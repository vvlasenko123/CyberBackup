import React, { useState, useEffect, useCallback, useRef } from 'react';
import axiosInstance from '../../utils/axiosInstance';
import CustomSelect from '../../components/CustomSelect/CustomSelect';
import type { SelectOption } from '../../components/CustomSelect/CustomSelect';
import './UsersPage.css';

type UserRoleStr = 'student' | 'teacher' | 'admin' | 'superadmin';

interface UserRecord {
    id: string;
    fullName: string;
    email: string;
    role: number; // 0=student,1=teacher,2=admin,3=superadmin
    isActive?: boolean;
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

    const [editUser, setEditUser] = useState<UserRecord | null>(null);
    const [editForm, setEditForm] = useState({ fullName: '', email: '', role: 0, isActive: true });
    const [editPassword, setEditPassword] = useState('');
    const [savingEdit, setSavingEdit] = useState(false);
    const [editError, setEditError] = useState<string | null>(null);

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

    const openEdit = async (u: UserRecord) => {
        setEditError(null);
        setEditPassword('');
        try {
            const res = await axiosInstance.get(`/public/api/v1/user/get/${u.id}`);
            setEditForm({
                fullName: res.data.fullName,
                email: res.data.email,
                role: res.data.role,
                isActive: res.data.isActive ?? true,
            });
        } catch {
            setEditForm({ fullName: u.fullName, email: u.email, role: u.role, isActive: u.isActive ?? true });
        }
        setEditUser(u);
    };

    const generatePassword = () => {
        const chars = 'ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789';
        let p = '';
        for (let i = 0; i < 10; i++) p += chars[Math.floor(Math.random() * chars.length)];
        setEditPassword(p);
    };

    const handleSaveEdit = async () => {
        if (!editUser) return;
        if (!editForm.fullName.trim() || !editForm.email.trim()) return;
        setSavingEdit(true);
        setEditError(null);
        try {
            await axiosInstance.put(`/public/api/v1/user/update/${editUser.id}`, {
                fullName: editForm.fullName.trim(),
                email: editForm.email.trim(),
                role: editForm.role,
                isActive: editForm.isActive,
                password: editPassword ? editPassword : null,
                mustChangePassword: editPassword ? true : false,
            });
            setEditUser(null);
            await fetchUsers();
        } catch (e: unknown) {
            const err = e as { response?: { data?: { message?: string } } };
            setEditError(err?.response?.data?.message || 'Не удалось сохранить изменения');
        } finally {
            setSavingEdit(false);
        }
    };

    const exportImportCsv = () => {
        if (!importResults) return;
        const rows = importResults
            .filter(r => r.success && r.password)
            .map(r => `"${r.fullName}";"${r.email}";"${r.password}"`);
        const csv = ['"ФИО";"Email";"Пароль"', ...rows].join('\r\n');
        const blob = new Blob(['﻿' + csv], { type: 'text/csv;charset=utf-8;' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'credentials.csv';
        a.click();
        URL.revokeObjectURL(url);
    };

    const printImport = () => {
        if (!importResults) return;
        const rows = importResults
            .filter(r => r.success && r.password)
            .map(r => `<tr><td>${r.fullName}</td><td>${r.email}</td><td>${r.password}</td></tr>`)
            .join('');
        const w = window.open('', '_blank');
        if (!w) return;
        w.document.write(`<html><head><title>Доступы</title></head><body>
            <table border="1" cellpadding="6" style="border-collapse:collapse">
            <tr><th>ФИО</th><th>Email</th><th>Пароль</th></tr>${rows}</table>
            <script>window.print()</script></body></html>`);
        w.document.close();
    };

    const handleGroupSelectChange = (val: string) => {
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
            <div className="users-header users-header--end">
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
                                                <div className="users-row-actions">
                                                    <button
                                                        className="users-btn-secondary"
                                                        onClick={() => openEdit(u)}
                                                    >
                                                        Редактировать
                                                    </button>
                                                    <button
                                                        className="users-btn-danger"
                                                        onClick={() => handleDelete(u.id, u.fullName)}
                                                    >
                                                        Удалить
                                                    </button>
                                                </div>
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
                                className="users-pagination-btn users-pagination-btn--arrow"
                                onClick={() => setPage(p => p - 1)}
                                disabled={page === 1}
                                title="Предыдущая страница"
                            >
                                ‹
                            </button>
                            <span className="users-pagination-info">
                                {page} / {totalPages} &nbsp;·&nbsp; {filtered.length} пользователей
                            </span>
                            <button
                                className="users-pagination-btn users-pagination-btn--arrow"
                                onClick={() => setPage(p => p + 1)}
                                disabled={page === totalPages}
                                title="Следующая страница"
                            >
                                ›
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

            {editUser && (
                <div className="users-modal-overlay" onClick={() => setEditUser(null)}>
                    <div className="users-modal" onClick={e => e.stopPropagation()}>
                        <h3 className="users-modal__title">Редактировать пользователя</h3>
                        <div className="users-modal__field">
                            <label className="users-modal__label">ФИО *</label>
                            <input className="users-modal__input" value={editForm.fullName}
                                onChange={e => setEditForm(f => ({ ...f, fullName: e.target.value }))} />
                        </div>
                        <div className="users-modal__field">
                            <label className="users-modal__label">Email *</label>
                            <input className="users-modal__input" type="email" value={editForm.email}
                                onChange={e => setEditForm(f => ({ ...f, email: e.target.value }))} />
                        </div>
                        <div className="users-modal__field">
                            <label className="users-modal__label">Роль</label>
                            <select className="users-modal__select" value={editForm.role}
                                onChange={e => setEditForm(f => ({ ...f, role: Number(e.target.value) }))}>
                                <option value={0}>Студент</option>
                                <option value={1}>Преподаватель</option>
                                <option value={2}>Администратор</option>
                            </select>
                        </div>
                        <div className="users-modal__field">
                            <label className="users-modal__label">Аккаунт</label>
                            <label className="users-toggle">
                                <input
                                    type="checkbox"
                                    className="users-toggle__input"
                                    checked={editForm.isActive}
                                    onChange={e => setEditForm(f => ({ ...f, isActive: e.target.checked }))}
                                />
                                <span className="users-toggle__track">
                                    <span className="users-toggle__thumb" />
                                </span>
                                <span className="users-toggle__label">
                                    {editForm.isActive ? 'Активен' : 'Неактивен'}
                                </span>
                            </label>
                        </div>
                        <div className="users-modal__field">
                            <label className="users-modal__label">Сбросить пароль</label>
                            <div className="users-modal__password-row">
                                <input className="users-modal__input" type="text"
                                    placeholder="Оставьте пустым, чтобы не менять"
                                    value={editPassword} onChange={e => setEditPassword(e.target.value)} />
                                <button type="button" className="users-btn-secondary" onClick={generatePassword}>
                                    Сгенерировать
                                </button>
                                {editPassword && <CopyButton text={editPassword} />}
                            </div>
                            {editPassword && (
                                <small className="users-modal__hint">
                                    Передайте пароль пользователю — при следующем входе он обязан его сменить.
                                </small>
                            )}
                        </div>
                        {editError && <div className="users-modal__error">{editError}</div>}
                        <div className="users-modal__actions">
                            <button className="users-btn-secondary" onClick={() => setEditUser(null)}>Отмена</button>
                            <button className="users-btn-primary" onClick={handleSaveEdit} disabled={savingEdit}>
                                {savingEdit ? 'Сохранение...' : 'Сохранить'}
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

                <div className="users-import-group-wrapper">
                <p className="users-import-group-subtitle">Выберите группу</p>
                <div className="users-import-group-row">
                    <CustomSelect
                        className="users-import-csel"
                        value={showNewGroupInput ? '__new__' : selectedGroupId}
                        onChange={handleGroupSelectChange}
                        options={[
                            { value: '', label: 'Без группы' },
                            ...groups.map<SelectOption>(g => ({ value: g.id, label: g.name, group: 'Группы' })),
                            { value: '__new__', label: '+ Создать новую группу', isAction: true },
                        ]}
                    />

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
                </div>

                <input
                    ref={fileInputRef}
                    type="file"
                    accept=".rtf"
                    onChange={handleFileImport}
                    disabled={importing}
                    style={{ display: 'none' }}
                />
                <button
                    className={`users-import-file-btn${importing ? ' users-import-file-btn--loading' : ''}`}
                    onClick={() => !importing && fileInputRef.current?.click()}
                    disabled={importing}
                >
                    {importing ? (
                        <>
                            <span className="users-import-spinner" />
                            Загружаем...
                        </>
                    ) : (
                        <>
                            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                                <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/>
                                <polyline points="17 8 12 3 7 8"/>
                                <line x1="12" y1="3" x2="12" y2="15"/>
                            </svg>
                            Выбрать RTF-файл
                        </>
                    )}
                </button>
                {importError && <div className="users-error">{importError}</div>}
                {importResults && (
                    <div className="users-import-result">
                        <div className="users-import-result-title">
                            Результат: {importResults.filter(r => r.success).length} создано,{' '}
                            {importResults.filter(r => !r.success).length} ошибок
                        </div>
                        {importResults.some(r => r.success && r.password) && (
                            <div className="users-import-export">
                                <button className="users-btn-secondary" onClick={exportImportCsv}>Скачать CSV</button>
                                <button className="users-btn-secondary" onClick={printImport}>Печать</button>
                            </div>
                        )}
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
