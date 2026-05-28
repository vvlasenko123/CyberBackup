import React, { useState, useEffect } from 'react';
import axiosInstance from '../../utils/axiosInstance';
import './DashboardPage.css';


enum PostCategory {
    Event       = 0,
    Laboratory  = 1,
    Information = 2,
}


interface PostItemDto {
    id: string;
    title: string;
    content: string;
    authorFullName: string;
    category: PostCategory;
    createdAtUtc: string;
}

interface PagedResult<T> {
    items: T[];
    totalCount: number;
    page: number;
    pageSize: number;
}

interface GetMyProgressResponse {
    completedLaboratories: number;
    totalLaboratories: number;
    earnedPoints: number;
    progressPercent: number;
}

interface LeaderboardItemDto {
    rank: number;
    studentId: string;
    fullName: string;
    earnedPoints: number;
    isCurrentUser: boolean;
}

interface GetGroupLeaderboardResponse {
    currentUserRank: number;
    items: LeaderboardItemDto[];
}

interface TeacherGradebookItemDto {
    studentId: string;
    fullName: string;
    totalPoints: number;
}

interface TeacherGradebookPagedResult {
    items: TeacherGradebookItemDto[];
    totalCount: number;
}


const CATEGORY_TABS = [
    { label: 'Все',         value: null },
    { label: 'Лабораторные', value: PostCategory.Laboratory },
    { label: 'События',      value: PostCategory.Event },
    { label: 'Информация',   value: PostCategory.Information },
] as const;

const CATEGORY_META: Record<PostCategory, { badge: string; cls: string }> = {
    [PostCategory.Event]:       { badge: 'СОБЫТИЕ', cls: 'event' },
    [PostCategory.Laboratory]:  { badge: 'ЛАБА',    cls: 'laboratory' },
    [PostCategory.Information]: { badge: 'ИНФО',    cls: 'information' },
};

const formatDate = (iso: string): string => {
    const d = new Date(iso);
    const months = ['янв', 'фев', 'мар', 'апр', 'май', 'июн',
                    'июл', 'авг', 'сен', 'окт', 'ноя', 'дек'];
    return `Опубликовано: ${d.getDate()} ${months[d.getMonth()]}`;
};


const StudentSidebar: React.FC = () => {
    const [progress, setProgress]       = useState<GetMyProgressResponse | null>(null);
    const [leaderboard, setLeaderboard] = useState<GetGroupLeaderboardResponse | null>(null);
    const [loadingP, setLoadingP]       = useState(true);
    const [loadingL, setLoadingL]       = useState(true);

    useEffect(() => {
        axiosInstance
            .get<GetMyProgressResponse>('/public/api/v1/laboratories/progress/my')
            .then(res => setProgress(res.data))
            .finally(() => setLoadingP(false));

        axiosInstance
            .get<GetGroupLeaderboardResponse>('/public/api/v1/laboratories/progress/leaderboard')
            .then(res => setLeaderboard(res.data))
            .finally(() => setLoadingL(false));
    }, []);

    return (
        <div className="dash-sidebar">
            {/* Mini progress card */}
            <div className="dash-progress-card">
                <div className="dash-progress-card__label">Мой прогресс</div>

                {loadingP ? (
                    <div style={{ color: 'rgba(255,255,255,0.3)', fontSize: 13, padding: '8px 0' }}>
                        Загрузка...
                    </div>
                ) : progress ? (
                    <>
                        <div className="dash-progress-card__pts">
                            {progress.earnedPoints}
                            <span>pts</span>
                        </div>
                        <div className="dash-progress-card__sub">
                            {progress.completedLaboratories} из {progress.totalLaboratories} лаб выполнено
                        </div>
                        <div className="dash-mini-bar-track">
                            <div
                                className="dash-mini-bar-fill"
                                style={{ width: `${progress.progressPercent}%` }}
                            />
                        </div>
                        <div className="dash-progress-card__rank-row">
                            <span className="dash-progress-card__rank-label">Место в рейтинге</span>
                            <span className="dash-progress-card__rank-value">
                                {loadingL
                                    ? '...'
                                    : leaderboard && leaderboard.currentUserRank > 0
                                        ? `#${leaderboard.currentUserRank}`
                                        : '—'
                                }
                            </span>
                        </div>
                    </>
                ) : null}
            </div>

            {/* Leaderboard */}
            {!loadingL && leaderboard && leaderboard.items.length > 0 && (
                <div className="dash-leaderboard-card">
                    <h3 className="dash-leaderboard-card__title">Рейтинг группы</h3>
                    {leaderboard.items.slice(0, 5).map(item => {
                        const isMe   = item.isCurrentUser;
                        const isTop3 = item.rank <= 3 && !isMe;
                        return (
                            <div
                                key={item.studentId}
                                className={`dash-leader-row${isMe ? ' dash-leader-row--me' : ''}`}
                            >
                                <span className={
                                    'dash-leader-row__rank' +
                                    (isMe   ? ' dash-leader-row__rank--me'   : '') +
                                    (isTop3 ? ' dash-leader-row__rank--top3' : '')
                                }>
                                    #{item.rank}
                                </span>
                                <span className={`dash-leader-row__name${isMe ? ' dash-leader-row__name--me' : ''}`}>
                                    {isMe ? 'Вы' : item.fullName}
                                </span>
                                <span className={`dash-leader-row__pts${isMe ? ' dash-leader-row__pts--me' : ''}`}>
                                    {item.earnedPoints}
                                </span>
                            </div>
                        );
                    })}
                </div>
            )}
        </div>
    );
};


const TeacherSidebar: React.FC = () => {
    const [ranked, setRanked] = useState<TeacherGradebookItemDto[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        axiosInstance
            .get<TeacherGradebookPagedResult>('/public/api/v1/teacher/gradebook?pageSize=100')
            .then(res => {
                const sorted = [...res.data.items]
                    .sort((a, b) => b.totalPoints - a.totalPoints)
                    .slice(0, 5);
                setRanked(sorted);
            })
            .finally(() => setLoading(false));
    }, []);

    if (loading) {
        return (
            <div className="dash-sidebar">
                <div className="dash-leaderboard-card">
                    <h3 className="dash-leaderboard-card__title">Рейтинг группы</h3>
                    <div className="dash-sidebar-note">Загрузка...</div>
                </div>
            </div>
        );
    }

    return (
        <div className="dash-sidebar">
            <div className="dash-leaderboard-card">
                <h3 className="dash-leaderboard-card__title">Рейтинг группы</h3>
                {ranked.length === 0 ? (
                    <div className="dash-sidebar-note">Студентов нет</div>
                ) : (
                    ranked.map((item, idx) => {
                        const rank   = idx + 1;
                        const isTop3 = rank <= 3;
                        return (
                            <div key={item.studentId} className="dash-leader-row">
                                <span className={`dash-leader-row__rank${isTop3 ? ' dash-leader-row__rank--top3' : ''}`}>
                                    #{rank}
                                </span>
                                <span className="dash-leader-row__name">{item.fullName}</span>
                                <span className="dash-leader-row__pts">{item.totalPoints}</span>
                            </div>
                        );
                    })
                )}
            </div>
        </div>
    );
};


interface NewsFeedProps { isTeacher: boolean; }

const CATEGORY_OPTIONS = [
    { label: 'Событие',       value: PostCategory.Event },
    { label: 'Лабораторная',  value: PostCategory.Laboratory },
    { label: 'Информация',    value: PostCategory.Information },
] as const;

const NewsFeed: React.FC<NewsFeedProps> = ({ isTeacher }) => {
    const [activeCategory, setActiveCategory] = useState<PostCategory | null>(null);
    const [posts, setPosts]   = useState<PostItemDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError]   = useState<string | null>(null);

    const [formOpen,    setFormOpen]    = useState(false);
    const [formTitle,   setFormTitle]   = useState('');
    const [formContent, setFormContent] = useState('');
    const [formCategory, setFormCategory] = useState<PostCategory>(PostCategory.Information);
    const [formSubmitting, setFormSubmitting] = useState(false);
    const [formError,   setFormError]   = useState<string | null>(null);

    const fetchPosts = (category: PostCategory | null) => {
        setLoading(true);
        setError(null);
        const params = new URLSearchParams({ pageSize: '20' });
        if (category !== null) params.append('category', String(category));
        axiosInstance
            .get<PagedResult<PostItemDto>>(`/public/api/v1/posts?${params.toString()}`)
            .then(res => setPosts(res.data.items))
            .catch(err => setError(err?.response?.data?.message || 'Не удалось загрузить новости'))
            .finally(() => setLoading(false));
    };

    useEffect(() => { fetchPosts(null); }, []);

    const handleTab = (val: PostCategory | null) => {
        setActiveCategory(val);
        fetchPosts(val);
    };

    const handleCreatePost = async () => {
        if (!formTitle.trim() || !formContent.trim()) return;
        setFormSubmitting(true);
        setFormError(null);
        try {
            await axiosInstance.post('/public/api/v1/posts', {
                title: formTitle.trim(),
                content: formContent.trim(),
                category: formCategory,
            });
            setFormOpen(false);
            setFormTitle('');
            setFormContent('');
            setFormCategory(PostCategory.Information);
            fetchPosts(activeCategory);
        } catch (err: unknown) {
            const axiosErr = err as { response?: { data?: { message?: string } } };
            setFormError(axiosErr.response?.data?.message ?? 'Не удалось создать пост');
        } finally {
            setFormSubmitting(false);
        }
    };

    return (
        <div className="dash-feed">
            <div className="dash-feed-header">
                <h2 className="dash-title">Новости и объявления</h2>
                {isTeacher && (
                    <button
                        className="dash-create-post-btn"
                        onClick={() => setFormOpen(v => !v)}
                    >
                        {formOpen ? 'Отмена' : '+ Объявление'}
                    </button>
                )}
            </div>

            {/* Teacher create-post form */}
            {isTeacher && formOpen && (
                <div className="dash-create-post-form">
                    <div className="dash-create-post-row">
                        <input
                            className="dash-create-post-input"
                            placeholder="Заголовок"
                            value={formTitle}
                            onChange={e => setFormTitle(e.target.value)}
                        />
                        <select
                            className="dash-create-post-select"
                            value={formCategory}
                            onChange={e => setFormCategory(Number(e.target.value) as PostCategory)}
                        >
                            {CATEGORY_OPTIONS.map(o => (
                                <option key={o.value} value={o.value}>{o.label}</option>
                            ))}
                        </select>
                    </div>
                    <textarea
                        className="dash-create-post-textarea"
                        placeholder="Текст объявления..."
                        rows={3}
                        value={formContent}
                        onChange={e => setFormContent(e.target.value)}
                    />
                    {formError && <div className="dash-create-post-error">{formError}</div>}
                    <button
                        className="dash-create-post-submit"
                        onClick={handleCreatePost}
                        disabled={formSubmitting || !formTitle.trim() || !formContent.trim()}
                    >
                        {formSubmitting ? 'Публикация...' : 'Опубликовать'}
                    </button>
                </div>
            )}

            {/* Filter tabs */}
            <div className="dash-tabs">
                {CATEGORY_TABS.map(tab => (
                    <button
                        key={String(tab.value)}
                        className={`dash-tab${activeCategory === tab.value ? ' dash-tab--active' : ''}`}
                        onClick={() => handleTab(tab.value)}
                    >
                        {tab.label}
                    </button>
                ))}
            </div>

            {loading ? (
                <div className="dash-loading">Загрузка...</div>
            ) : error ? (
                <div className="dash-error">{error}</div>
            ) : posts.length === 0 ? (
                <div className="dash-empty">Новостей нет</div>
            ) : (
                <div className="dash-post-list">
                    {posts.map(post => {
                        const meta = CATEGORY_META[post.category];
                        return (
                            <div
                                key={post.id}
                                className={`dash-post dash-post--${meta.cls}`}
                            >
                                <div className="dash-post__header">
                                    <h3 className="dash-post__title">{post.title}</h3>
                                    <span className={`dash-post__badge dash-post__badge--${meta.cls}`}>
                                        {meta.badge}
                                    </span>
                                </div>
                                <p className="dash-post__content">{post.content}</p>
                                <div className="dash-post__meta">
                                    {post.authorFullName} · {formatDate(post.createdAtUtc)}
                                </div>
                            </div>
                        );
                    })}
                </div>
            )}
        </div>
    );
};


const DashboardPage: React.FC = () => {
    const role = localStorage.getItem('user_role') || 'student';
    const isTeacher = role === 'teacher' || role === 'admin';

    return (
        <div className="dash-page">
            <NewsFeed isTeacher={isTeacher} />
            {isTeacher ? <TeacherSidebar /> : <StudentSidebar />}
        </div>
    );
};

export default DashboardPage;
