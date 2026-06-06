import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import axiosInstance from '../../utils/axiosInstance';
import { Icon } from '../../shared/Icon';
import './LabDetailPage.css';

type LabDifficulty = 1 | 2 | 3;

type StudentHint = {
    id: string;
    orderNumber: number;
    title: string | null;
    penaltyPoints: number;
    isOpened: boolean;
    text: string | null;
};

type StudentLabDetail = {
    id: string;
    title: string;
    shortDescription: string;
    description: string;
    narrative: string;
    goal: string;
    environmentUrl: string | null;
    credentials: string | null;
    difficulty: LabDifficulty;
    difficultyName: string;
    block: string;
    maxPoints: number;
    earnedPoints: number;
    hasFlag: boolean;
    flagAlreadySubmitted: boolean;
    reportStatus: number;
    allowReportUpload: boolean;
    canResubmitReport: boolean;
    deadlineAtUtc: string | null;
    hints: StudentHint[];
    report: { reportId: string; status: number } | null;
};

type TeacherHint = {
    id: string;
    orderNumber: number;
    title: string | null;
    penaltyPoints: number;
    text: string;
};

type TeacherLabDetail = {
    id: string;
    title: string;
    shortDescription: string;
    description: string;
    narrative: string;
    goal: string;
    environmentUrl: string | null;
    credentials: string | null;
    difficulty: LabDifficulty;
    block: string;
    maxPoints: number;
    hasFlag: boolean;
    isPublished: boolean;
    deadlineAtUtc: string | null;
    hints: TeacherHint[];
};

type SubmitFlagResponse = {
    isCorrect: boolean;
    message: string;
    earnedPoints: number;
    status: string;
};

type OpenHintResponse = {
    hintId: string;
    orderNumber: number;
    text: string;
    penaltyPoints: number;
    totalPenaltyPoints: number;
    availablePoints: number;
};

const DIFFICULTY_LABEL: Record<number, string> = {
    1: 'Легкая',
    2: 'Средняя',
    3: 'Сложная',
};

const STATUS_LABEL: Record<number, { label: string; bannerClass: string }> = {
    1: { label: 'В работе', bannerClass: '' },
    2: { label: 'Ожидает проверки', bannerClass: 'lab-detail-banner--pending' },
    3: { label: 'Требует доработки', bannerClass: 'lab-detail-banner--revision' },
    4: { label: 'Лабораторная работа выполнена', bannerClass: 'lab-detail-banner--success' },
};

const BookIcon = () => (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="#60A5FA" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
        <path d="M4 19.5A2.5 2.5 0 016.5 17H20" />
        <path d="M6.5 2H20v20H6.5A2.5 2.5 0 014 19.5v-15A2.5 2.5 0 016.5 2z" />
    </svg>
);

const LabDetailPage = () => {
    const { labId } = useParams<{ labId: string }>();
    const navigate = useNavigate();
    const role = localStorage.getItem('user_role') || 'student';

    const [studentLab, setStudentLab] = useState<StudentLabDetail | null>(null);
    const [teacherLab, setTeacherLab] = useState<TeacherLabDetail | null>(null);
    const [hints, setHints] = useState<StudentHint[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const [flagInput, setFlagInput] = useState('');
    const [flagSubmitting, setFlagSubmitting] = useState(false);
    const [flagError, setFlagError] = useState<string | null>(null);

    const [openingHintId, setOpeningHintId] = useState<string | null>(null);

    useEffect(() => {
        const fetchStudent = async () => {
            try {
                const res = await axiosInstance.get<StudentLabDetail>(`/public/api/v1/laboratories/${labId}`);
                setStudentLab(res.data);
                setHints(res.data.hints ?? []);
            } catch {
                setError('Не удалось загрузить лабораторную работу');
            } finally {
                setLoading(false);
            }
        };

        const fetchTeacher = async () => {
            try {
                const res = await axiosInstance.get<TeacherLabDetail>(`/public/api/v1/teacher/laboratories/${labId}`);
                setTeacherLab(res.data);
            } catch {
                setError('Не удалось загрузить лабораторную работу');
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
    }, [labId, role]);

    const handleOpenHint = async (hintId: string) => {
        if (openingHintId) return;
        setOpeningHintId(hintId);
        try {
            const res = await axiosInstance.post<OpenHintResponse>(
                `/public/api/v1/laboratories/${labId}/hints/${hintId}/open`
            );
            setHints(prev => prev.map(h =>
                h.id === hintId ? { ...h, isOpened: true, text: res.data.text } : h
            ));
        } catch {
            // ignore
        } finally {
            setOpeningHintId(null);
        }
    };

    const handleSubmitFlag = async () => {
        if (!flagInput.trim() || flagSubmitting) return;
        setFlagSubmitting(true);
        setFlagError(null);
        try {
            const res = await axiosInstance.post<SubmitFlagResponse>(
                `/public/api/v1/laboratories/${labId}/flag`,
                { flag: flagInput.trim() }
            );
            if (res.data.isCorrect) {
                setStudentLab(prev => prev ? { ...prev, flagAlreadySubmitted: true, earnedPoints: res.data.earnedPoints } : prev);
                setFlagInput('');
            } else {
                setFlagError(res.data.message || 'Неверный флаг');
            }
        } catch {
            setFlagError('Не удалось отправить флаг');
        } finally {
            setFlagSubmitting(false);
        }
    };

    if (loading) return <div className="lab-detail-loading">Загрузка...</div>;
    if (error) return <div className="lab-detail-error">{error}</div>;

    if (role === 'teacher' && teacherLab) {
        return <TeacherView lab={teacherLab} onBack={() => navigate('/labs')} onEdit={() => navigate(`/labs/${labId}/edit`)} />;
    }

    if (role === 'student' && studentLab) {
        const lab = studentLab;
        const statusInfo = STATUS_LABEL[lab.reportStatus] ?? null;
        const isAccepted = lab.reportStatus === 4;

        return (
            <div className="lab-detail-page">
                <div className="lab-detail-breadcrumb">
                    <button className="lab-detail-breadcrumb-link" onClick={() => navigate('/labs')}>
                        Лабораторные
                    </button>
                    <span className="lab-detail-breadcrumb-sep">/</span>
                    <span className="lab-detail-breadcrumb-current">{lab.title}</span>
                </div>

                <div className="lab-detail-content">
                    <div className="lab-detail-main">
                        {statusInfo && (statusInfo.bannerClass || isAccepted) && (
                            <div className={`lab-detail-banner ${statusInfo.bannerClass}`}>
                                <Icon name="checkCircle" size={18} color={isAccepted ? '#86EFAC' : undefined} />
                                {isAccepted
                                    ? `Лабораторная работа выполнена — ${lab.earnedPoints} очков получено`
                                    : statusInfo.label}
                            </div>
                        )}

                        {/* Начальный экран: красивая шапка когда студент только открыл лабу */}
                        {!statusInfo && lab.reportStatus === 0 && (
                            <div className="lab-detail-intro">
                                <div className="lab-detail-intro-badge">
                                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                                        <circle cx="12" cy="12" r="10" />
                                        <line x1="12" y1="8" x2="12" y2="12" />
                                        <line x1="12" y1="16" x2="12.01" y2="16" />
                                    </svg>
                                    Задание доступно
                                </div>
                                <h2 className="lab-detail-intro-title">{lab.title}</h2>
                                {lab.shortDescription && (
                                    <p className="lab-detail-intro-subtitle">{lab.shortDescription}</p>
                                )}
                            </div>
                        )}

                        {lab.narrative && (
                            <div className="lab-detail-narrative">
                                <div className="lab-detail-narrative-header">
                                    <BookIcon />
                                    <span className="lab-detail-narrative-label">НАРРАТИВ</span>
                                </div>
                                <p className="lab-detail-narrative-text">{lab.narrative}</p>
                            </div>
                        )}

                        {lab.description && (
                            <div className="lab-detail-card">
                                <h3 className="lab-detail-card-title">Описание</h3>
                                <p className="lab-detail-task-description">{lab.description}</p>
                            </div>
                        )}

                        {(lab.goal || lab.environmentUrl || lab.credentials) && (
                            <div className="lab-detail-card">
                                <h3 className="lab-detail-card-title">Задание</h3>
                                {lab.goal && (
                                    <p className="lab-detail-task-description">{lab.goal}</p>
                                )}
                                {lab.environmentUrl && (
                                    <div className="lab-detail-task-row">
                                        <span className="lab-detail-task-label">Цель:</span>
                                        <a
                                            href={lab.environmentUrl}
                                            target="_blank"
                                            rel="noopener noreferrer"
                                            className="lab-detail-task-link"
                                        >
                                            {lab.environmentUrl}
                                        </a>
                                    </div>
                                )}
                                {lab.credentials && (
                                    <div className="lab-detail-task-row">
                                        <span className="lab-detail-task-label">Учетные данные:</span>
                                        <span className="lab-detail-task-creds">{lab.credentials}</span>
                                    </div>
                                )}
                            </div>
                        )}

                        {hints.length > 0 && (
                            <div className="lab-detail-card">
                                <h3 className="lab-detail-card-title">Подсказки</h3>
                                <div className="lab-detail-hints">
                                    {hints
                                        .sort((a, b) => a.orderNumber - b.orderNumber)
                                        .map(hint => (
                                            <div key={hint.id} className="lab-detail-hint-row">
                                                {hint.isOpened ? (
                                                    <div className="lab-detail-hint-open">
                                                        {hint.text}
                                                    </div>
                                                ) : (
                                                    <button
                                                        className="lab-detail-hint-btn"
                                                        onClick={() => handleOpenHint(hint.id)}
                                                        disabled={openingHintId === hint.id}
                                                    >
                                                        {openingHintId === hint.id
                                                            ? 'Открываю...'
                                                            : `Открыть подсказку #${hint.orderNumber}`}
                                                    </button>
                                                )}
                                                {!hint.isOpened && hint.penaltyPoints > 0 && (
                                                    <span className="lab-detail-hint-penalty">
                                                        -{hint.penaltyPoints} баллов
                                                    </span>
                                                )}
                                            </div>
                                        ))}
                                </div>
                            </div>
                        )}

                        <div className="lab-detail-actions">
                            {lab.allowReportUpload && (() => {
                                const deadlinePassed = lab.deadlineAtUtc
                                    ? new Date(lab.deadlineAtUtc) < new Date()
                                    : false;
                                return (
                                    <div style={{ display: 'flex', flexDirection: 'column', gap: 4 }}>
                                        <button
                                            className="lab-detail-btn-primary"
                                            onClick={() => !deadlinePassed && navigate(`/labs/${lab.id}/report`, { state: { labTitle: lab.title } })}
                                            disabled={deadlinePassed}
                                        >
                                            Загрузить отчет
                                        </button>
                                        {deadlinePassed && (
                                            <span style={{ fontSize: 12, color: '#EF4444' }}>
                                                Срок сдачи истёк
                                            </span>
                                        )}
                                    </div>
                                );
                            })()}
                            <button
                                className="lab-detail-btn-outline"
                                onClick={() => navigate('/questions/new?lab=' + encodeURIComponent(lab.title))}
                            >
                                Задать вопрос
                            </button>
                        </div>
                    </div>

                    <div className="lab-detail-sidebar">
                        {lab.hasFlag && (
                            <div className="lab-detail-sidebar-card">
                                <h4 className="lab-detail-sidebar-title">
                                    <Icon name="flag" size={14} color="#8A94A6" />
                                    Сдача флага
                                </h4>
                                {lab.flagAlreadySubmitted ? (
                                    <div className="lab-detail-flag-submitted">
                                        <Icon name="check" size={14} color="#86EFAC" />
                                        Флаг уже сдан
                                    </div>
                                ) : (
                                    <>
                                        <input
                                            className="lab-detail-flag-input"
                                            type="text"
                                            placeholder="Введите флаг..."
                                            value={flagInput}
                                            onChange={e => setFlagInput(e.target.value)}
                                            onKeyDown={e => e.key === 'Enter' && handleSubmitFlag()}
                                        />
                                        <button
                                            className="lab-detail-flag-submit"
                                            onClick={handleSubmitFlag}
                                            disabled={flagSubmitting || !flagInput.trim()}
                                        >
                                            {flagSubmitting ? 'Отправка...' : 'Сдать флаг'}
                                        </button>
                                        {flagError && (
                                            <p className="lab-detail-flag-error">{flagError}</p>
                                        )}
                                    </>
                                )}
                            </div>
                        )}

                        <div className="lab-detail-sidebar-card">
                            <h4 className="lab-detail-sidebar-title">Информация</h4>
                            <div className="lab-detail-info-row">
                                <span className="lab-detail-info-label">Сложность</span>
                                <span className="lab-detail-info-value">{DIFFICULTY_LABEL[lab.difficulty]}</span>
                            </div>
                            <div className="lab-detail-info-row">
                                <span className="lab-detail-info-label">Баллов</span>
                                <span className="lab-detail-info-value">{lab.maxPoints} баллов</span>
                            </div>
                            <div className="lab-detail-info-row">
                                <span className="lab-detail-info-label">Блок</span>
                                <span className="lab-detail-info-value">{lab.block}</span>
                            </div>
                            {lab.deadlineAtUtc && (
                                <div className="lab-detail-info-row">
                                    <span className="lab-detail-info-label">Дедлайн</span>
                                    <span className="lab-detail-info-value" style={{
                                        color: new Date(lab.deadlineAtUtc) < new Date() ? '#ef4444' : 'inherit'
                                    }}>
                                        {new Date(lab.deadlineAtUtc).toLocaleString('ru-RU', {
                                            day: '2-digit', month: '2-digit', year: 'numeric',
                                            hour: '2-digit', minute: '2-digit'
                                        })}
                                    </span>
                                </div>
                            )}
                        </div>
                    </div>
                </div>
            </div>
        );
    }

    return null;
};

type TeacherViewProps = {
    lab: TeacherLabDetail;
    onBack: () => void;
    onEdit: () => void;
};

const TeacherView = ({ lab, onBack, onEdit }: TeacherViewProps) => (
    <div className="lab-detail-page">
        <div className="lab-detail-breadcrumb">
            <button className="lab-detail-breadcrumb-link" onClick={onBack}>
                Лабораторные
            </button>
            <span className="lab-detail-breadcrumb-sep">/</span>
            <span className="lab-detail-breadcrumb-current">{lab.title}</span>
        </div>

        <div className="lab-detail-content">
            <div className="lab-detail-main">
                {lab.narrative && (
                    <div className="lab-detail-narrative">
                        <div className="lab-detail-narrative-header">
                            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="#60A5FA" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
                                <path d="M4 19.5A2.5 2.5 0 016.5 17H20" />
                                <path d="M6.5 2H20v20H6.5A2.5 2.5 0 014 19.5v-15A2.5 2.5 0 016.5 2z" />
                            </svg>
                            <span className="lab-detail-narrative-label">НАРРАТИВ</span>
                        </div>
                        <p className="lab-detail-narrative-text">{lab.narrative}</p>
                    </div>
                )}

                {lab.description && (
                    <div className="lab-detail-card">
                        <h3 className="lab-detail-card-title">Описание</h3>
                        <p className="lab-detail-task-description">{lab.description}</p>
                    </div>
                )}

                {(lab.goal || lab.environmentUrl || lab.credentials) && (
                    <div className="lab-detail-card">
                        <h3 className="lab-detail-card-title">Задание</h3>
                        {lab.goal && <p className="lab-detail-task-description">{lab.goal}</p>}
                        {lab.environmentUrl && (
                            <div className="lab-detail-task-row">
                                <span className="lab-detail-task-label">Цель:</span>
                                <a href={lab.environmentUrl} target="_blank" rel="noopener noreferrer" className="lab-detail-task-link">
                                    {lab.environmentUrl}
                                </a>
                            </div>
                        )}
                        {lab.credentials && (
                            <div className="lab-detail-task-row">
                                <span className="lab-detail-task-label">Учетные данные:</span>
                                <span className="lab-detail-task-creds">{lab.credentials}</span>
                            </div>
                        )}
                    </div>
                )}

                {lab.hints.length > 0 && (
                    <div className="lab-detail-card">
                        <h3 className="lab-detail-card-title">Подсказки</h3>
                        <div className="lab-detail-hints">
                            {lab.hints
                                .sort((a, b) => a.orderNumber - b.orderNumber)
                                .map(hint => (
                                    <div key={hint.id} className="lab-detail-hint-row">
                                        <div className="lab-detail-teacher-hint">
                                            {hint.title && <strong>{hint.title}: </strong>}
                                            {hint.text}
                                            {hint.penaltyPoints > 0 && (
                                                <div className="lab-detail-teacher-hint-meta">
                                                    Штраф: {hint.penaltyPoints} баллов
                                                </div>
                                            )}
                                        </div>
                                    </div>
                                ))}
                        </div>
                    </div>
                )}
            </div>

            <div className="lab-detail-sidebar">
                <div className="lab-detail-sidebar-card">
                    <h4 className="lab-detail-sidebar-title">Информация</h4>
                    <div className="lab-detail-info-row">
                        <span className="lab-detail-info-label">Статус</span>
                        <span className={`lab-detail-publish-badge lab-detail-publish-badge--${lab.isPublished ? 'published' : 'draft'}`}>
                            {lab.isPublished ? 'Опубликована' : 'Черновик'}
                        </span>
                    </div>
                    <div className="lab-detail-info-row">
                        <span className="lab-detail-info-label">Баллов</span>
                        <span className="lab-detail-info-value">{lab.maxPoints} баллов</span>
                    </div>
                    <div className="lab-detail-info-row">
                        <span className="lab-detail-info-label">Блок</span>
                        <span className="lab-detail-info-value">{lab.block}</span>
                    </div>
                    <div className="lab-detail-info-row">
                        <span className="lab-detail-info-label">Флаг</span>
                        <span className="lab-detail-info-value">{lab.hasFlag ? 'Есть' : 'Нет'}</span>
                    </div>
                    {lab.deadlineAtUtc && (
                        <div className="lab-detail-info-row">
                            <span className="lab-detail-info-label">Дедлайн</span>
                            <span className="lab-detail-info-value" style={{
                                color: new Date(lab.deadlineAtUtc) < new Date() ? '#ef4444' : 'inherit'
                            }}>
                                {new Date(lab.deadlineAtUtc).toLocaleString('ru-RU', {
                                    day: '2-digit', month: '2-digit', year: 'numeric',
                                    hour: '2-digit', minute: '2-digit'
                                })}
                            </span>
                        </div>
                    )}
                    <div style={{ marginTop: 16 }}>
                        <button
                            className="lab-detail-btn-primary"
                            style={{ width: '100%' }}
                            onClick={onEdit}
                        >
                            Редактировать
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>
);

export default LabDetailPage;
