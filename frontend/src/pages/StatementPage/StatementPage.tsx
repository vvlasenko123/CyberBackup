import React, { useState, useEffect, useCallback } from 'react';
import axiosInstance from '../../utils/axiosInstance';
import './StatementPage.css';


enum LaboratoryReportStatus {
    NotSubmitted = 0,
    Submitted = 1,
    UnderReview = 2,
    RevisionRequired = 3,
    Accepted = 4,
}

enum StudentLaboratoryStatus {
    NotStarted = 0,
    InProgress = 1,
    PendingReview = 2,
    Accepted = 3,
    RevisionRequired = 4,
}


interface GradebookStudentDto {
    id: string;
    fullName: string;
    groupName?: string;
}

interface MyGradebookLaboratoryDto {
    laboratoryId: string;
    title: string;
    laboratoryStatus: StudentLaboratoryStatus;
    status: LaboratoryReportStatus;
    points?: number;
    maxPoints: number;
    teacherComment?: string;
}

interface GetMyGradebookResponse {
    student: GradebookStudentDto;
    attendancePercent: number;
    isExamAllowed: boolean;
    hasAutomaticGrade: boolean;
    totalPoints: number;
    laboratories: MyGradebookLaboratoryDto[];
}

interface TeacherGradebookItemDto {
    studentId: string;
    fullName: string;
    groupName?: string;
    attendancePercent: number;
    isExamAllowed: boolean;
    hasAutomaticGrade: boolean;
    totalPoints: number;
    completedLaboratories: number;
    totalLaboratories: number;
}

interface PagedResult<T> {
    items: T[];
    totalCount: number;
    page: number;
    pageSize: number;
}


type BadgeColor = 'green' | 'blue' | 'orange' | 'red' | 'gray';

interface BadgeInfo {
    label: string;
    color: BadgeColor;
}

const getLaboratoryStatusBadge = (status: StudentLaboratoryStatus): BadgeInfo => {
    switch (status) {
        case StudentLaboratoryStatus.Accepted:
            return { label: 'Завершена', color: 'green' };
        case StudentLaboratoryStatus.InProgress:
        case StudentLaboratoryStatus.PendingReview:
        case StudentLaboratoryStatus.RevisionRequired:
            return { label: 'В процессе', color: 'blue' };
        default:
            return { label: 'Не выполнена', color: 'gray' };
    }
};

const getReportStatusBadge = (status: LaboratoryReportStatus): BadgeInfo | null => {
    switch (status) {
        case LaboratoryReportStatus.Accepted:
            return { label: 'Принят', color: 'green' };
        case LaboratoryReportStatus.Submitted:
        case LaboratoryReportStatus.UnderReview:
            return { label: 'На проверке', color: 'orange' };
        case LaboratoryReportStatus.RevisionRequired:
            return { label: 'На доработке', color: 'red' };
        default:
            return null;
    }
};


const Badge: React.FC<{ info: BadgeInfo }> = ({ info }) => (
    <span className={`stmt-badge stmt-badge--${info.color}`}>{info.label}</span>
);


const StudentGradebook: React.FC = () => {
    const [data, setData] = useState<GetMyGradebookResponse | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        axiosInstance
            .get<GetMyGradebookResponse>('/public/api/v1/gradebook/my')
            .then(res => setData(res.data))
            .catch(err => setError(err?.response?.data?.message || 'Не удалось загрузить ведомость'))
            .finally(() => setLoading(false));
    }, []);

    if (loading) return <div className="stmt-loading">Загрузка...</div>;
    if (error)   return <div className="stmt-error">{error}</div>;
    if (!data)   return null;

    return (
        <div className="stmt-page">
            {/* Карточки статистики */}
            <div className="stmt-stats">
                <div className="stmt-stat-card">
                    <div className="stmt-stat-card__label">Посещаемость</div>
                    <div className="stmt-stat-card__value">
                        {Math.round(data.attendancePercent)}%
                    </div>
                </div>

                <div className="stmt-stat-card">
                    <div className="stmt-stat-card__label">Допуск к зачету</div>
                    <div className={`stmt-stat-card__value stmt-stat-card__value--${data.isExamAllowed ? 'green' : 'red'}`}>
                        {data.isExamAllowed ? 'Да' : 'Нет'}
                    </div>
                </div>

                <div className="stmt-stat-card">
                    <div className="stmt-stat-card__label">Автомат</div>
                    <div className={`stmt-stat-card__value ${data.hasAutomaticGrade ? 'stmt-stat-card__value--blue' : 'stmt-stat-card__value--dim'}`}>
                        {data.hasAutomaticGrade ? data.totalPoints : '—'}
                    </div>
                </div>
            </div>

            {/* Таблица лабораторных */}
            <div className="stmt-table-card">
                <table className="stmt-table">
                    <thead>
                        <tr>
                            <th>Лабораторная</th>
                            <th>Статус</th>
                            <th>Отчет</th>
                        </tr>
                    </thead>
                    <tbody>
                        {data.laboratories.map(lab => {
                            const labBadge    = getLaboratoryStatusBadge(lab.laboratoryStatus);
                            const reportBadge = getReportStatusBadge(lab.status);
                            return (
                                <tr key={lab.laboratoryId}>
                                    <td>{lab.title}</td>
                                    <td><Badge info={labBadge} /></td>
                                    <td>
                                        {reportBadge
                                            ? <Badge info={reportBadge} />
                                            : <span className="stmt-dash">—</span>
                                        }
                                    </td>
                                </tr>
                            );
                        })}
                    </tbody>
                </table>
                {data.laboratories.length === 0 && (
                    <div className="stmt-empty">Лабораторные работы не найдены</div>
                )}
            </div>
        </div>
    );
};


interface EditForm {
    studentId: string;
    fullName: string;
    attendancePercent: number;
    isExamAllowed: boolean;
    hasAutomaticGrade: boolean;
}

const TeacherGradebook: React.FC = () => {
    const [items, setItems]           = useState<TeacherGradebookItemDto[]>([]);
    const [allGroups, setAllGroups]   = useState<string[]>([]);
    const [loading, setLoading]       = useState(true);
    const [error, setError]           = useState<string | null>(null);
    const [groupFilter, setGroupFilter] = useState('');
    const [search, setSearch]         = useState('');
    const [editForm, setEditForm]     = useState<EditForm | null>(null);
    const [saving, setSaving]         = useState(false);
    const [saveError, setSaveError]   = useState<string | null>(null);
    const [exporting, setExporting]   = useState(false);

    const fetchData = useCallback(async (group?: string, q?: string) => {
        setLoading(true);
        setError(null);
        try {
            const params = new URLSearchParams({ pageSize: '100' });
            if (group) params.append('groupName', group);
            if (q)     params.append('search', q);
            const res = await axiosInstance.get<PagedResult<TeacherGradebookItemDto>>(
                `/public/api/v1/teacher/gradebook?${params.toString()}`
            );
            setItems(res.data.items);
            // Строим список уникальных групп при первой загрузке (без фильтров)
            if (!group && !q) {
                const groups = Array.from(
                    new Set(res.data.items.map(i => i.groupName).filter(Boolean))
                ) as string[];
                setAllGroups(groups);
            }
        } catch (err: any) {
            setError(err?.response?.data?.message || 'Не удалось загрузить ведомость');
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => { fetchData(); }, [fetchData]);

    const handleGroupChange = (group: string) => {
        setGroupFilter(group);
        fetchData(group || undefined, search || undefined);
    };

    const handleSearch = (e: React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key === 'Enter') {
            fetchData(groupFilter || undefined, search || undefined);
        }
    };

    const handleExport = async () => {
        setExporting(true);
        try {
            const res = await axiosInstance.get('/public/api/v1/teacher/gradebook/export', {
                responseType: 'blob',
            });
            const url = URL.createObjectURL(new Blob([res.data]));
            const a = document.createElement('a');
            a.href = url;
            a.download = `Ведомость_${new Date().toISOString().slice(0, 10)}.xlsx`;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            URL.revokeObjectURL(url);
        } catch {
            // ignore
        } finally {
            setExporting(false);
        }
    };

    const openEdit = (item: TeacherGradebookItemDto) => {
        setEditForm({
            studentId:        item.studentId,
            fullName:         item.fullName,
            attendancePercent: item.attendancePercent,
            isExamAllowed:    item.isExamAllowed,
            hasAutomaticGrade: item.hasAutomaticGrade,
        });
        setSaveError(null);
    };

    const handleSave = async () => {
        if (!editForm) return;
        setSaving(true);
        setSaveError(null);
        try {
            await axiosInstance.put(`/public/api/v1/teacher/gradebook/${editForm.studentId}`, {
                attendancePercent: editForm.attendancePercent,
                isExamAllowed:    editForm.isExamAllowed,
                hasAutomaticGrade: editForm.hasAutomaticGrade,
            });
            setEditForm(null);
            fetchData(groupFilter || undefined, search || undefined);
        } catch (err: any) {
            setSaveError(err?.response?.data?.message || 'Не удалось сохранить');
        } finally {
            setSaving(false);
        }
    };

    return (
        <div className="stmt-page">
            {/* Фильтры + экспорт (на месте заголовка) */}
            <div className="stmt-toolbar">
                <div className="stmt-filters">
                    <select
                        className="stmt-filter-select"
                        value={groupFilter}
                        onChange={e => handleGroupChange(e.target.value)}
                    >
                        <option value="">Все группы</option>
                        {allGroups.map(g => (
                            <option key={g} value={g}>{g}</option>
                        ))}
                    </select>
                    <input
                        className="stmt-filter-input"
                        type="text"
                        placeholder="Поиск по студентам..."
                        value={search}
                        onChange={e => setSearch(e.target.value)}
                        onKeyDown={handleSearch}
                    />
                </div>
                <button
                    className="stmt-export-btn"
                    onClick={handleExport}
                    disabled={exporting}
                >
                    {exporting ? 'Экспорт...' : '⬇ Экспорт Excel'}
                </button>
            </div>

            {loading ? (
                <div className="stmt-loading">Загрузка...</div>
            ) : error ? (
                <div className="stmt-error">{error}</div>
            ) : (
                <div className="stmt-table-card">
                    <table className="stmt-table">
                        <thead>
                            <tr>
                                <th>Студент</th>
                                <th>Группа</th>
                                <th>Посещаемость</th>
                                <th>Лаб. выполнено</th>
                                <th>Допуск</th>
                                <th>Автомат</th>
                                <th></th>
                            </tr>
                        </thead>
                        <tbody>
                            {items.map(item => (
                                <tr key={item.studentId}>
                                    <td>{item.fullName}</td>
                                    <td>{item.groupName ?? <span className="stmt-dash">—</span>}</td>
                                    <td>{Math.round(item.attendancePercent)}%</td>
                                    <td>{item.completedLaboratories} / {item.totalLaboratories}</td>
                                    <td>
                                        <Badge info={{
                                            label: item.isExamAllowed ? 'Допущен' : 'Не допущен',
                                            color: item.isExamAllowed ? 'green' : 'red',
                                        }} />
                                    </td>
                                    <td className="stmt-autograde">
                                        {item.hasAutomaticGrade
                                            ? item.totalPoints
                                            : <span className="stmt-dash">—</span>
                                        }
                                    </td>
                                    <td>
                                        <button
                                            className="stmt-edit-btn"
                                            onClick={() => openEdit(item)}
                                        >
                                            Редактировать
                                        </button>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                    {items.length === 0 && (
                        <div className="stmt-empty">Студенты не найдены</div>
                    )}
                </div>
            )}

            {/* Модальное окно редактирования */}
            {editForm && (
                <div className="stmt-modal-overlay" onClick={() => setEditForm(null)}>
                    <div className="stmt-modal" onClick={e => e.stopPropagation()}>
                        <h3 className="stmt-modal__title">{editForm.fullName}</h3>

                        <div className="stmt-modal__body">
                            <label className="stmt-modal__field">
                                <span className="stmt-modal__label">Посещаемость (%)</span>
                                <input
                                    className="stmt-modal__input"
                                    type="number"
                                    min={0}
                                    max={100}
                                    step={0.1}
                                    value={editForm.attendancePercent}
                                    onChange={e => setEditForm(f => f
                                        ? { ...f, attendancePercent: parseFloat(e.target.value) || 0 }
                                        : f
                                    )}
                                />
                            </label>

                            <div className="stmt-modal__field stmt-modal__field--toggle">
                                <span className="stmt-modal__label">Допуск к зачёту</span>
                                <input
                                    type="checkbox"
                                    className="stmt-modal__checkbox"
                                    checked={editForm.isExamAllowed}
                                    onChange={e => setEditForm(f => f
                                        ? { ...f, isExamAllowed: e.target.checked }
                                        : f
                                    )}
                                />
                            </div>

                            <div className="stmt-modal__field stmt-modal__field--toggle">
                                <span className="stmt-modal__label">Автомат</span>
                                <input
                                    type="checkbox"
                                    className="stmt-modal__checkbox"
                                    checked={editForm.hasAutomaticGrade}
                                    onChange={e => setEditForm(f => f
                                        ? { ...f, hasAutomaticGrade: e.target.checked }
                                        : f
                                    )}
                                />
                            </div>
                        </div>

                        {saveError && <p className="stmt-modal__error">{saveError}</p>}

                        <div className="stmt-modal__actions">
                            <button
                                className="stmt-modal__cancel"
                                onClick={() => setEditForm(null)}
                            >
                                Отмена
                            </button>
                            <button
                                className="stmt-modal__submit"
                                onClick={handleSave}
                                disabled={saving}
                            >
                                {saving ? 'Сохранение...' : 'Сохранить'}
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};


const StatementPage: React.FC = () => {
    const role = localStorage.getItem('user_role') || 'student';
    return role === 'teacher' || role === 'admin'
        ? <TeacherGradebook />
        : <StudentGradebook />;
};

export default StatementPage;
