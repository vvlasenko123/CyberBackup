import React, { useState, useEffect } from 'react';
import axiosInstance from '../../utils/axiosInstance';
import './ProgressPage.css';


enum StudentLaboratoryStatus {
    NotStarted = 0,
    InProgress = 1,
    PendingReview = 2,
    Accepted = 3,
    RevisionRequired = 4,
}


interface MyProgressLaboratoryDto {
    laboratoryId: string;
    title: string;
    status: StudentLaboratoryStatus;
    earnedPoints: number;
    maxPoints: number;
}

interface GetMyProgressResponse {
    totalLaboratories: number;
    completedLaboratories: number;
    earnedPoints: number;
    totalPoints: number;
    progressPercent: number;
    laboratories: MyProgressLaboratoryDto[];
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

// ─── Helpers ──────────────────────────────────────────────────────────────────

type BarColor = 'blue' | 'green' | 'orange' | 'red' | 'gray';

const getLabBarColor = (status: StudentLaboratoryStatus): BarColor => {
    switch (status) {
        case StudentLaboratoryStatus.Accepted:        return 'green';
        case StudentLaboratoryStatus.PendingReview:   return 'orange';
        case StudentLaboratoryStatus.RevisionRequired: return 'red';
        case StudentLaboratoryStatus.InProgress:      return 'blue';
        default:                                       return 'gray';
    }
};

const getLabBarPercent = (lab: MyProgressLaboratoryDto): number => {
    if (lab.maxPoints === 0) return lab.status === StudentLaboratoryStatus.Accepted ? 100 : 0;
    return Math.min(Math.round((lab.earnedPoints / lab.maxPoints) * 100), 100);
};

// ─── Progress Page ────────────────────────────────────────────────────────────

const ProgressPage: React.FC = () => {
    const [progress, setProgress] = useState<GetMyProgressResponse | null>(null);
    const [leaderboard, setLeaderboard] = useState<GetGroupLeaderboardResponse | null>(null);
    const [loadingProgress, setLoadingProgress] = useState(true);
    const [loadingLeaderboard, setLoadingLeaderboard] = useState(true);
    const [errorProgress, setErrorProgress] = useState<string | null>(null);
    const [errorLeaderboard, setErrorLeaderboard] = useState<string | null>(null);

    useEffect(() => {
        axiosInstance
            .get<GetMyProgressResponse>('/public/api/v1/laboratories/progress/my')
            .then(res => setProgress(res.data))
            .catch(err => setErrorProgress(err?.response?.data?.message || 'Не удалось загрузить прогресс'))
            .finally(() => setLoadingProgress(false));

        axiosInstance
            .get<GetGroupLeaderboardResponse>('/public/api/v1/laboratories/progress/leaderboard')
            .then(res => setLeaderboard(res.data))
            .catch(() => setErrorLeaderboard('Рейтинг недоступен'))
            .finally(() => setLoadingLeaderboard(false));
    }, []);

    const isLoading = loadingProgress;

    return (
        <div className="prog-page">
            {isLoading ? (
                <div className="prog-loading">Загрузка...</div>
            ) : errorProgress ? (
                <div className="prog-error">{errorProgress}</div>
            ) : !progress ? null : (
                <>
                    {/* ── Stat cards ── */}
                    <div className="prog-stats">
                        <div className="prog-stat-card">
                            <div className="prog-stat-card__label">Лаб выполнено</div>
                            <div className="prog-stat-card__value">
                                {progress.completedLaboratories}
                                <span style={{ fontSize: 20, color: 'rgba(255,255,255,0.4)', fontWeight: 500 }}>
                                    /{progress.totalLaboratories}
                                </span>
                            </div>
                        </div>

                        <div className="prog-stat-card">
                            <div className="prog-stat-card__label">Баллов набрано</div>
                            <div className="prog-stat-card__value prog-stat-card__value--blue">
                                {progress.earnedPoints}
                            </div>
                        </div>

                        <div className="prog-stat-card">
                            <div className="prog-stat-card__label">Место в рейтинге</div>
                            <div className="prog-stat-card__value prog-stat-card__value--orange">
                                {loadingLeaderboard
                                    ? '...'
                                    : leaderboard && leaderboard.currentUserRank > 0
                                        ? `#${leaderboard.currentUserRank}`
                                        : '—'
                                }
                            </div>
                        </div>
                    </div>

                    {/* ── Overall progress ── */}
                    <div className="prog-overall-card">
                        <div className="prog-overall-card__header">
                            <h3 className="prog-overall-card__title">Общий прогресс</h3>
                            <span className="prog-overall-card__percent">{progress.progressPercent}%</span>
                        </div>
                        <div className="prog-bar-track">
                            <div
                                className="prog-bar-fill"
                                style={{ width: `${progress.progressPercent}%` }}
                            />
                        </div>
                        <div className="prog-overall-card__subtitle">
                            {progress.completedLaboratories} из {progress.totalLaboratories} лабораторных выполнено
                        </div>
                    </div>

                    {/* ── Two-column layout ── */}
                    <div className="prog-columns">
                        {/* Labs progress panel */}
                        <div className="prog-labs-card">
                            <h3 className="prog-labs-card__title">Прогресс по лабораторным работам</h3>

                            {progress.laboratories.length === 0 ? (
                                <div className="prog-empty" style={{ padding: '20px 0' }}>
                                    Лабораторных работ нет
                                </div>
                            ) : (
                                progress.laboratories.map(lab => {
                                    const barPercent = getLabBarPercent(lab);
                                    const barColor   = getLabBarColor(lab.status);
                                    const hasPoints  = lab.earnedPoints > 0;

                                    return (
                                        <div key={lab.laboratoryId} className="prog-lab-item">
                                            <div className="prog-lab-item__header">
                                                <span className="prog-lab-item__name">{lab.title}</span>
                                                <span className={`prog-lab-item__points${hasPoints ? ' prog-lab-item__points--earned' : ''}`}>
                                                    {lab.earnedPoints} / {lab.maxPoints}
                                                </span>
                                            </div>
                                            <div className="prog-lab-bar-track">
                                                <div
                                                    className={`prog-bar-fill prog-bar-fill--${barColor}`}
                                                    style={{ width: `${barPercent}%` }}
                                                />
                                            </div>
                                        </div>
                                    );
                                })
                            )}
                        </div>

                        {/* Leaderboard panel */}
                        <div className="prog-leaderboard-card">
                            <h3 className="prog-leaderboard-card__title">Рейтинг группы</h3>

                            {loadingLeaderboard ? (
                                <div className="prog-loading" style={{ padding: '20px 0' }}>
                                    Загрузка...
                                </div>
                            ) : errorLeaderboard ? (
                                <div className="prog-leaderboard-empty">{errorLeaderboard}</div>
                            ) : !leaderboard || leaderboard.items.length === 0 ? (
                                <div className="prog-leaderboard-empty">
                                    Вы ещё не добавлены в группу
                                </div>
                            ) : (
                                leaderboard.items.map(item => {
                                    const isMe   = item.isCurrentUser;
                                    const isTop3 = item.rank <= 3 && !isMe;

                                    return (
                                        <div
                                            key={item.studentId}
                                            className={`prog-leader-item${isMe ? ' prog-leader-item--me' : ''}`}
                                        >
                                            <span className={
                                                `prog-leader-item__rank` +
                                                (isMe   ? ' prog-leader-item__rank--me'   : '') +
                                                (isTop3 ? ' prog-leader-item__rank--top3' : '')
                                            }>
                                                #{item.rank}
                                            </span>
                                            <span className={`prog-leader-item__name${isMe ? ' prog-leader-item__name--me' : ''}`}>
                                                {isMe ? 'Вы' : item.fullName}
                                            </span>
                                            <span className={`prog-leader-item__points${isMe ? ' prog-leader-item__points--me' : ''}`}>
                                                {item.earnedPoints}
                                            </span>
                                        </div>
                                    );
                                })
                            )}
                        </div>
                    </div>
                </>
            )}
        </div>
    );
};

export default ProgressPage;
