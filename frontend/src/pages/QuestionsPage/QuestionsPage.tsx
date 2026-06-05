import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import axiosInstance from '../../utils/axiosInstance';
import { Icon } from '../../shared/Icon';
import './QuestionsPage.css';


enum QuestionStatus {
    Open     = 0,
    Answered = 1,
    Closed   = 2,
}

interface QuestionListItemDto {
    id: string;
    laboratoryTitle: string | null;
    description: string;
    status: QuestionStatus;
    createdAtUtc: string;
}

interface TeacherQuestionListItemDto {
    id: string;
    studentFullName: string;
    groupName: string | null;
    laboratoryTitle: string | null;
    description: string;
    status: QuestionStatus;
    createdAtUtc: string;
}

interface PagedResult<T> {
    items: T[];
    totalCount: number;
    page: number;
    pageSize: number;
}


function formatDate(iso: string): string {
    const d = new Date(iso);
    return d.toLocaleDateString('ru-RU', { day: '2-digit', month: '2-digit', year: 'numeric' });
}

function StatusBadge({ status }: { status: QuestionStatus }) {
    const map: Record<QuestionStatus, { cls: string; label: string }> = {
        [QuestionStatus.Open]:     { cls: 'qst-badge--open',     label: 'Открыт' },
        [QuestionStatus.Answered]: { cls: 'qst-badge--answered', label: 'Отвечен' },
        [QuestionStatus.Closed]:   { cls: 'qst-badge--closed',   label: 'Закрыт' },
    };
    const { cls, label } = map[status] ?? { cls: 'qst-badge--open', label: 'Неизвестно' };
    return <span className={`qst-badge ${cls}`}>{label}</span>;
}


function StudentQuestionsList() {
    const navigate = useNavigate();
    const [items, setItems] = useState<QuestionListItemDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    useEffect(() => {
        axiosInstance.get<QuestionListItemDto[]>('/public/api/v1/questions/my')
            .then(r => setItems(r.data))
            .catch(() => setError('Не удалось загрузить вопросы'))
            .finally(() => setLoading(false));
    }, []);

    return (
        <div className="qst-page">
            <div className="qst-header qst-header--end">
                <button className="qst-new-btn" onClick={() => navigate('/questions/new')}>
                    <Icon name="plus" size={14} />
                    Задать вопрос
                </button>
            </div>

            {error && <div className="qst-error">{error}</div>}
            {loading && <div className="qst-loading">Загрузка…</div>}

            {!loading && !error && items.length === 0 && (
                <div className="qst-empty">У вас пока нет вопросов</div>
            )}

            {!loading && items.length > 0 && (
                <div className="qst-list">
                    {items.map(q => (
                        <div key={q.id} className="qst-card" onClick={() => navigate(`/questions/${q.id}`)}>
                            <div className="qst-card__header">
                                <p className="qst-card__title">{q.description.slice(0, 80)}{q.description.length > 80 ? '…' : ''}</p>
                                <span className="qst-card__date">{formatDate(q.createdAtUtc)}</span>
                            </div>
                            <div className="qst-card__meta">
                                <StatusBadge status={q.status} />
                                {q.laboratoryTitle && (
                                    <span className="qst-lab-link">{q.laboratoryTitle}</span>
                                )}
                            </div>
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
}


const STATUS_OPTIONS = [
    { value: '', label: 'Все статусы' },
    { value: '0', label: 'Открытые' },
    { value: '1', label: 'Отвеченные' },
    { value: '2', label: 'Закрытые' },
];

function TeacherQuestionsList() {
    const navigate = useNavigate();
    const [items, setItems] = useState<TeacherQuestionListItemDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [labTitles, setLabTitles] = useState<string[]>([]);
    const [statusFilter, setStatusFilter] = useState('');
    const [labFilter, setLabFilter] = useState('');
    const [search, setSearch] = useState('');

    useEffect(() => {
        axiosInstance.get<string[]>('/public/api/v1/teacher/questions/lab-titles')
            .then(r => setLabTitles(r.data))
            .catch(() => {});
    }, []);

    useEffect(() => {
        setLoading(true);
        const params: Record<string, string> = { page: '1', pageSize: '50' };
        if (statusFilter) params.status = statusFilter;
        if (labFilter)    params.laboratoryTitle = labFilter;
        if (search)       params.search = search;

        axiosInstance.get<PagedResult<TeacherQuestionListItemDto>>('/public/api/v1/teacher/questions', { params })
            .then(r => setItems(r.data.items))
            .catch(() => setError('Не удалось загрузить вопросы'))
            .finally(() => setLoading(false));
    }, [statusFilter, labFilter, search]);

    return (
        <div className="qst-page">
            <div className="qst-filters">
                <select
                    className="qst-filter-select"
                    value={statusFilter}
                    onChange={e => setStatusFilter(e.target.value)}
                >
                    {STATUS_OPTIONS.map(o => (
                        <option key={o.value} value={o.value}>{o.label}</option>
                    ))}
                </select>

                {labTitles.length > 0 && (
                    <select
                        className="qst-filter-select"
                        value={labFilter}
                        onChange={e => setLabFilter(e.target.value)}
                    >
                        <option value="">Все лабораторные</option>
                        {labTitles.map(t => (
                            <option key={t} value={t}>{t}</option>
                        ))}
                    </select>
                )}

                <input
                    className="qst-filter-input"
                    placeholder="Поиск по студенту или тексту…"
                    value={search}
                    onChange={e => setSearch(e.target.value)}
                />
            </div>

            {error && <div className="qst-error">{error}</div>}
            {loading && <div className="qst-loading">Загрузка…</div>}

            {!loading && !error && items.length === 0 && (
                <div className="qst-empty">Вопросов не найдено</div>
            )}

            {!loading && items.length > 0 && (
                <div className="qst-list">
                    {items.map(q => (
                        <div key={q.id} className="qst-card" onClick={() => navigate(`/questions/${q.id}`)}>
                            <div className="qst-card__header">
                                <p className="qst-card__title">{q.studentFullName}</p>
                                <span className="qst-card__date">{formatDate(q.createdAtUtc)}</span>
                            </div>
                            <div className="qst-card__meta">
                                <StatusBadge status={q.status} />
                                {q.groupName && (
                                    <span className="qst-group-badge">{q.groupName}</span>
                                )}
                                {q.laboratoryTitle && (
                                    <span className="qst-lab-link">{q.laboratoryTitle}</span>
                                )}
                            </div>
                            <p className="qst-card__desc">{q.description}</p>
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
}


export default function QuestionsPage() {
    const role = localStorage.getItem('user_role');
    const isTeacher = role === 'teacher' || role === 'admin' || role === 'superAdmin';
    return isTeacher ? <TeacherQuestionsList /> : <StudentQuestionsList />;
}
