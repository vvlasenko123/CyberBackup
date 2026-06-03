import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import axiosInstance from '../../utils/axiosInstance';
import './LabsPage.css';

type LabDifficulty = 1 | 2 | 3;
type LabStatus = 0 | 1 | 2 | 3 | 4;
type ReportStatus = 0 | 1 | 2 | 3 | 4;

type LabListItem = {
    id: string;
    title: string;
    shortDescription: string;
    difficulty: LabDifficulty;
    difficultyName: string;
    block: string;
    maxPoints: number;
    earnedPoints: number;
    status: LabStatus;
    statusName: string;
    isCompleted: boolean;
    progressPercent: number;
    sortOrder: number;
};

type PagedResult = {
    items: LabListItem[];
    totalCount: number;
    page: number;
    pageSize: number;
};

type ProgressSummary = {
    totalLaboratories: number;
    completedLaboratories: number;
    pendingReviewLaboratories: number;
    revisionRequiredLaboratories: number;
    totalPoints: number;
    earnedPoints: number;
    progressPercent: number;
};

type TeacherReportListItem = {
    reportId: string;
    laboratoryId: string;
    laboratoryTitle: string;
    studentId: string;
    studentFullName: string;
    groupName: string | null;
    currentVersionNumber: number;
    status: ReportStatus;
    points: number | null;
    maxPoints: number;
    allowResubmit: boolean;
    createDateUtc: string;
    updateDateUtc: string | null;
    lastSubmitDateUtc: string;
};

type TeacherReportsPagedResult = {
    items: TeacherReportListItem[];
    totalCount: number;
    page: number;
    pageSize: number;
};

type TeacherLabListItem = {
    id: string;
    title: string;
    shortDescription: string;
    difficulty: LabDifficulty;
    block: string;
    maxPoints: number;
    hasFlag: boolean;
    isPublished: boolean;
    sortOrder: number;
    createDateUtc: string;
    updateDateUtc: string | null;
};

type TeacherLabsPagedResult = {
    items: TeacherLabListItem[];
    totalCount: number;
    page: number;
    pageSize: number;
};

const DIFFICULTY_LABEL: Record<number, string> = {
    1: 'ЛЁГКАЯ',
    2: 'СРЕДНЯЯ',
    3: 'СЛОЖНАЯ',
};

const REPORT_STATUS_META: Record<ReportStatus, { label: string; className: string }> = {
    0: { label: 'Не отправлен', className: 'labs-report-status--notsubmitted' },
    1: { label: 'На проверке',  className: 'labs-report-status--submitted' },
    2: { label: 'На проверке',  className: 'labs-report-status--underreview' },
    3: { label: 'Нужны правки', className: 'labs-report-status--revision' },
    4: { label: 'Принята',      className: 'labs-report-status--accepted' },
};

const formatDate = (iso: string) => new Date(iso).toLocaleDateString('ru-RU', {
    day: '2-digit', month: '2-digit', year: 'numeric',
});

const getDifficultyClass = (d: LabDifficulty) => {
    if (d === 1) return 'easy';
    if (d === 2) return 'medium';
    return 'hard';
};

const StatusIcon = ({ status }: { status: LabStatus }) => {
    if (status === 3) {
        return (
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
                <circle cx="12" cy="12" r="11" fill="#22C55E" />
                <polyline
                    points="7,12.5 10.5,16 17,9"
                    stroke="white"
                    strokeWidth="2.2"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    fill="none"
                />
            </svg>
        );
    }
    if (status === 1) {
        return <div className="labs-item-spinner" />;
    }
    if (status === 2) {
        return (
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
                <circle cx="12" cy="12" r="11" stroke="#F59E0B" strokeWidth="2" />
                <line x1="12" y1="7.5" x2="12" y2="13" stroke="#F59E0B" strokeWidth="2" strokeLinecap="round" />
                <circle cx="12" cy="16.5" r="1" fill="#F59E0B" />
            </svg>
        );
    }
    if (status === 4) {
        return (
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
                <circle cx="12" cy="12" r="11" stroke="#EF4444" strokeWidth="2" />
                <line x1="12" y1="7.5" x2="12" y2="13" stroke="#EF4444" strokeWidth="2" strokeLinecap="round" />
                <circle cx="12" cy="16.5" r="1" fill="#EF4444" />
            </svg>
        );
    }
    return (
        <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
            <circle cx="12" cy="12" r="11" stroke="#4B5563" strokeWidth="2" />
        </svg>
    );
};


const LabsPage = () => {
    const navigate = useNavigate();
    const role = localStorage.getItem('user_role') || 'student';

    const [labs, setLabs] = useState<LabListItem[]>([]);
    const [teacherReports, setTeacherReports] = useState<TeacherReportListItem[]>([]);
    const [teacherLabs, setTeacherLabs] = useState<TeacherLabListItem[]>([]);
    const [teacherTab, setTeacherTab] = useState<'labs' | 'reports'>('labs');
    const [progress, setProgress] = useState<ProgressSummary | null>(null);
    const [loading, setLoading] = useState(true);
    const [statusFilter, setStatusFilter] = useState<string>('');
    const [searchFilter, setSearchFilter] = useState('');

    useEffect(() => {
        const fetchStudent = async () => {
            try {
                const [labsRes, progressRes] = await Promise.all([
                    axiosInstance.get<PagedResult>('/public/api/v1/laboratories', {
                        params: { page: 1, pageSize: 100 },
                    }),
                    axiosInstance.get<ProgressSummary>('/public/api/v1/laboratories/progress/my'),
                ]);
                setLabs(labsRes.data.items);
                setProgress(progressRes.data);
            } catch {
                // ignore
            } finally {
                setLoading(false);
            }
        };

        const fetchTeacher = async () => {
            try {
                const [reportsRes, labsRes] = await Promise.all([
                    axiosInstance.get<TeacherReportsPagedResult>('/public/api/v1/teacher/reports', {
                        params: { page: 1, pageSize: 200 },
                    }),
                    axiosInstance.get<TeacherLabsPagedResult>('/public/api/v1/teacher/laboratories', {
                        params: { page: 1, pageSize: 200 },
                    }),
                ]);
                setTeacherReports(reportsRes.data.items);
                setTeacherLabs(labsRes.data.items);
            } catch {
                // ignore
            } finally {
                setLoading(false);
            }
        };

        if (role === 'student') {
            fetchStudent();
        } else if (role === 'teacher') {
            fetchTeacher();
        } else {
            setLoading(false);
        }
    }, [role]);

    const groupByBlock = <T extends { block: string; sortOrder: number }>(items: T[]) =>
        items.reduce<Record<string, T[]>>((acc, item) => {
            const key = item.block || 'Без блока';
            if (!acc[key]) acc[key] = [];
            acc[key].push(item);
            return acc;
        }, {});

    if (loading) {
        return <div className="labs-loading">Загрузка...</div>;
    }

    const studentGrouped = groupByBlock(labs);

    const filteredReports = teacherReports.filter(r => {
        if (statusFilter && String(r.status) !== statusFilter) return false;
        if (searchFilter) {
            const q = searchFilter.toLowerCase();
            return r.studentFullName.toLowerCase().includes(q) || r.laboratoryTitle.toLowerCase().includes(q);
        }
        return true;
    });

    return (
        <div className="labs-page">
            <div className="labs-page-header">
                {role === 'student' && progress && (
                    <p className="labs-page-subtitle">
                        {progress.totalLaboratories} работ · {progress.completedLaboratories} выполнено
                    </p>
                )}
                {role === 'teacher' && (
                    <button
                        className="labs-page-create-btn"
                        onClick={() => navigate('/labs/create')}
                    >
                        + Создать лабораторную
                    </button>
                )}
            </div>

            {role === 'student' && (
                <>
                    {Object.keys(studentGrouped).length === 0 && (
                        <div className="labs-empty">Лабораторные работы не найдены</div>
                    )}
                    {Object.entries(studentGrouped)
                        .sort(([a], [b]) => a.localeCompare(b, 'ru'))
                        .map(([block, items]) => (
                            <div key={block} className="labs-block">
                                <h3 className="labs-block-title">{block.toUpperCase()}</h3>
                                <div className="labs-block-list">
                                    {items
                                        .sort((a, b) => a.sortOrder - b.sortOrder)
                                        .map(lab => (
                                            <button
                                                key={lab.id}
                                                className="labs-item"
                                                onClick={() => navigate(`/labs/${lab.id}`)}
                                            >
                                                <div className="labs-item-status">
                                                    <StatusIcon status={lab.status} />
                                                </div>
                                                <div className="labs-item-info">
                                                    <span className="labs-item-title">{lab.title}</span>
                                                    <div className="labs-item-meta">
                                                        <span className={`labs-item-difficulty labs-item-difficulty--${getDifficultyClass(lab.difficulty)}`}>
                                                            {DIFFICULTY_LABEL[lab.difficulty]}
                                                        </span>
                                                        {lab.isCompleted ? (
                                                            <span className="labs-item-points labs-item-points--earned">
                                                                {lab.earnedPoints} <span className="labs-item-points-max">/ {lab.maxPoints} баллов</span>
                                                            </span>
                                                        ) : (
                                                            <span className="labs-item-points">{lab.maxPoints} баллов</span>
                                                        )}
                                                    </div>
                                                </div>
                                            </button>
                                        ))}
                                </div>
                            </div>
                        ))}
                </>
            )}

            {role === 'teacher' && (
                <>
                    {/* Вкладки для препода */}
                    <div className="labs-teacher-tabs">
                        <button
                            className={`labs-teacher-tab ${teacherTab === 'labs' ? 'labs-teacher-tab--active' : ''}`}
                            onClick={() => setTeacherTab('labs')}
                        >
                            Мои лабораторные
                            <span className="labs-teacher-tab-count">{teacherLabs.length}</span>
                        </button>
                        <button
                            className={`labs-teacher-tab ${teacherTab === 'reports' ? 'labs-teacher-tab--active' : ''}`}
                            onClick={() => setTeacherTab('reports')}
                        >
                            Отчёты студентов
                            <span className="labs-teacher-tab-count">{teacherReports.length}</span>
                        </button>
                    </div>

                    {/* Список лаб препода */}
                    {teacherTab === 'labs' && (
                        <>
                            {teacherLabs.length === 0 ? (
                                <div className="labs-empty">Лабораторных работ ещё нет. Создайте первую!</div>
                            ) : (
                                <div className="labs-teacher-labs-list">
                                    {teacherLabs
                                        .sort((a, b) => a.sortOrder - b.sortOrder || a.title.localeCompare(b.title, 'ru'))
                                        .map(lab => (
                                            <button
                                                key={lab.id}
                                                className="labs-teacher-lab-card"
                                                onClick={() => navigate(`/labs/${lab.id}`)}
                                            >
                                                <div className="labs-teacher-lab-header">
                                                    <span className="labs-teacher-lab-title">{lab.title}</span>
                                                    <span className={`labs-teacher-lab-badge labs-teacher-lab-badge--${lab.isPublished ? 'published' : 'draft'}`}>
                                                        {lab.isPublished ? 'Опубликована' : 'Черновик'}
                                                    </span>
                                                </div>
                                                {lab.shortDescription && (
                                                    <p className="labs-teacher-lab-desc">{lab.shortDescription}</p>
                                                )}
                                                <div className="labs-teacher-lab-meta">
                                                    <span className={`labs-item-difficulty labs-item-difficulty--${lab.difficulty === 1 ? 'easy' : lab.difficulty === 2 ? 'medium' : 'hard'}`}>
                                                        {DIFFICULTY_LABEL[lab.difficulty]}
                                                    </span>
                                                    <span className="labs-teacher-lab-block">{lab.block}</span>
                                                    <span className="labs-item-points">{lab.maxPoints} баллов</span>
                                                    {lab.hasFlag && <span className="labs-teacher-lab-flag">🚩 Флаг</span>}
                                                </div>
                                            </button>
                                        ))}
                                </div>
                            )}
                        </>
                    )}

                    {/* Отчёты студентов */}
                    {teacherTab === 'reports' && (
                        <>
                            <div className="labs-reports-filters">
                                <input
                                    className="labs-reports-search"
                                    type="text"
                                    placeholder="Поиск по студенту или лабораторной..."
                                    value={searchFilter}
                                    onChange={e => setSearchFilter(e.target.value)}
                                />
                                <select
                                    className="labs-reports-status-select"
                                    value={statusFilter}
                                    onChange={e => setStatusFilter(e.target.value)}
                                >
                                    <option value="">Все статусы</option>
                                    <option value="1">На проверке</option>
                                    <option value="2">На проверке</option>
                                    <option value="3">Нужны правки</option>
                                    <option value="4">Принята</option>
                                </select>
                            </div>

                            {filteredReports.length === 0 ? (
                                <div className="labs-empty">Отчёты не найдены</div>
                            ) : (
                                <div className="labs-reports-table-wrap">
                                    <table className="labs-reports-table">
                                        <thead>
                                            <tr>
                                                <th>Студент</th>
                                                <th>Группа</th>
                                                <th>Лабораторная</th>
                                                <th>Версия</th>
                                                <th>Дата</th>
                                                <th>Статус</th>
                                                <th>Баллы</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            {filteredReports
                                                .sort((a, b) => new Date(b.lastSubmitDateUtc).getTime() - new Date(a.lastSubmitDateUtc).getTime())
                                                .map(report => {
                                                    const meta = REPORT_STATUS_META[report.status];
                                                    return (
                                                        <tr
                                                            key={report.reportId}
                                                            className="labs-reports-row"
                                                            onClick={() => navigate(`/teacher/reports/${report.reportId}`)}
                                                        >
                                                            <td className="labs-reports-cell--student">{report.studentFullName}</td>
                                                            <td className="labs-reports-cell--muted">{report.groupName ?? '—'}</td>
                                                            <td>{report.laboratoryTitle}</td>
                                                            <td className="labs-reports-cell--muted">v{report.currentVersionNumber}</td>
                                                            <td className="labs-reports-cell--muted">{formatDate(report.lastSubmitDateUtc)}</td>
                                                            <td>
                                                                <span className={`labs-report-status ${meta.className}`}>
                                                                    {meta.label}
                                                                </span>
                                                            </td>
                                                            <td className="labs-reports-cell--muted">
                                                                {report.points !== null ? `${report.points} / ${report.maxPoints}` : `— / ${report.maxPoints}`}
                                                            </td>
                                                        </tr>
                                                    );
                                                })}
                                        </tbody>
                                    </table>
                                </div>
                            )}
                        </>
                    )}
                </>
            )}
        </div>
    );
};

export default LabsPage;
