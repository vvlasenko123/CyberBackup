import React, { useCallback, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Skeleton, SkeletonRegion } from '../../components/Skeleton/Skeleton';
import { Icon, type IconName } from '../../components/Icon';
import axiosInstance from '../../utils/axiosInstance';
import {
    DEMO_EMAIL,
    DEMO_FALLBACK,
    DEMO_LEADERBOARD,
    DEMO_PROGRESS,
    fetchProfileExtras,
    fetchUser,
    updateProfileExtras,
    type ProfileExtras,
    type ProfileUpdate,
} from '../../features/profile/profileApi';
import { ProfileEditModal } from './ProfileEditModal';
import './ProfilePage.css';


const StudentLaboratoryStatus = {
    NotStarted: 0,
    InProgress: 1,
    PendingReview: 2,
    Accepted: 3,
    RevisionRequired: 4,
} as const;
type StudentLaboratoryStatus = typeof StudentLaboratoryStatus[keyof typeof StudentLaboratoryStatus];

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

interface GetGroupLeaderboardResponse {
    currentUserRank: number;
    items: { studentId: string }[];
}

const ROLE_LABEL: Record<string, string> = {
    student: 'Студент',
    teacher: 'Преподаватель',
    admin: 'Администратор',
};

const STATUS_META: Record<StudentLaboratoryStatus, { label: string; cls: string }> = {
    [StudentLaboratoryStatus.NotStarted]: { label: 'Не начата', cls: 'muted' },
    [StudentLaboratoryStatus.InProgress]: { label: 'В работе', cls: 'blue' },
    [StudentLaboratoryStatus.PendingReview]: { label: 'На проверке', cls: 'orange' },
    [StudentLaboratoryStatus.Accepted]: { label: 'Принята', cls: 'green' },
    [StudentLaboratoryStatus.RevisionRequired]: { label: 'На доработке', cls: 'red' },
};

const getInitials = (name: string) =>
    name
        .split(/\s+/)
        .filter(Boolean)
        .slice(0, 2)
        .map(part => part[0].toUpperCase())
        .join('') || '?';

const formatSince = (iso?: string) => {
    if (!iso) return null;
    const d = new Date(iso);
    if (!Number.isFinite(d.getTime())) return null;
    return d.toLocaleDateString('ru-RU', { month: 'long', year: 'numeric' });
};


const StatTile: React.FC<{ icon: IconName; label: string; value: React.ReactNode; hint?: React.ReactNode; accent?: string }> = ({
    icon, label, value, hint, accent = 'blue',
}) => (
    <div className={`profile-stat profile-stat--${accent}`}>
        <div className="profile-stat__icon"><Icon name={icon} size={18} /></div>
        <div className="profile-stat__label">{label}</div>
        <div className="profile-stat__value">{value}</div>
        {hint && <div className="profile-stat__hint">{hint}</div>}
    </div>
);


const ContactRow: React.FC<{ icon: IconName; label: string; value: string; href?: string }> = ({ icon, label, value, href }) => (
    <div className="profile-contact">
        <div className="profile-contact__icon"><Icon name={icon} size={16} /></div>
        <div className="profile-contact__body">
            <div className="profile-contact__label">{label}</div>
            {value
                ? href
                    ? <a className="profile-contact__value" href={href} target="_blank" rel="noreferrer">{value}</a>
                    : <div className="profile-contact__value">{value}</div>
                : <div className="profile-contact__value profile-contact__value--empty">Не указан</div>}
        </div>
    </div>
);


const ProfilePage: React.FC = () => {
    const navigate = useNavigate();
    const userId = localStorage.getItem('user_id') || '';
    const role = localStorage.getItem('user_role') || 'student';
    const isStudent = role === 'student';

    const [fullName, setFullName] = useState(localStorage.getItem('user_name') || '');
    const [email, setEmail] = useState('');
    const [createdAt, setCreatedAt] = useState<string | undefined>();
    const [extras, setExtras] = useState<ProfileExtras | null>(null);
    const [progress, setProgress] = useState<GetMyProgressResponse | null>(null);
    const [leaderboard, setLeaderboard] = useState<GetGroupLeaderboardResponse | null>(null);
    const [loadingStats, setLoadingStats] = useState(isStudent);
    const [editing, setEditing] = useState(false);
    const [toast, setToast] = useState('');
    const [demo, setDemo] = useState(false);

    useEffect(() => {
        fetchUser(userId)
            .then(user => {
                setFullName(user.fullName);
                setEmail(user.email);
                setCreatedAt(user.createdAt);
            })
            .catch(() => {
                if (DEMO_FALLBACK) setEmail(DEMO_EMAIL);
            });

        fetchProfileExtras(userId).then(setExtras);

        if (!isStudent) return;
        Promise.allSettled([
            axiosInstance.get<GetMyProgressResponse>('/public/api/v1/laboratories/progress/my'),
            axiosInstance.get<GetGroupLeaderboardResponse>('/public/api/v1/laboratories/progress/leaderboard'),
        ]).then(([p, l]) => {
            if (p.status === 'fulfilled') setProgress(p.value.data);
            if (l.status === 'fulfilled') setLeaderboard(l.value.data);
            if (DEMO_FALLBACK && p.status === 'rejected') {
                setProgress(DEMO_PROGRESS as unknown as GetMyProgressResponse);
                setLeaderboard(DEMO_LEADERBOARD);
                setDemo(true);
            }
            setLoadingStats(false);
        });
    }, [userId, isStudent]);

    useEffect(() => {
        if (!toast) return;
        const timer = setTimeout(() => setToast(''), 3000);
        return () => clearTimeout(timer);
    }, [toast]);

    const handleSave = useCallback(async (update: ProfileUpdate) => {
        const saved = await updateProfileExtras(userId, update);
        setExtras(saved);
        setEditing(false);
        setToast('Профиль сохранён');
    }, [userId]);

    const closeEditor = useCallback(() => setEditing(false), []);

    const displayName = fullName || 'Пользователь';
    const displayEmail = email || (fullName.includes('@') ? fullName : '');
    const initials = getInitials(displayName);
    const since = formatSince(createdAt);
    const groupSize = leaderboard?.items.length ?? 0;
    const recentLabs = progress?.laboratories.slice(0, 5) ?? [];

    return (
        <div className="profile-page">
            <section className="profile-hero">
                <div className="profile-hero__cover" />
                <div className="profile-hero__body">
                    <div className="profile-avatar profile-avatar--lg">
                        {extras?.avatarUrl ? <img src={extras.avatarUrl} alt="" /> : initials}
                    </div>

                    <div className="profile-hero__info">
                        <h2 className="profile-hero__name">{displayName}</h2>
                        <div className="profile-hero__chips">
                            <span className="profile-chip profile-chip--role">{ROLE_LABEL[role] ?? role}</span>
                            {isStudent && extras?.groupName && (
                                <span className="profile-chip">
                                    <Icon name="users" size={13} /> {extras.groupName}
                                </span>
                            )}
                            {since && <span className="profile-chip profile-chip--muted">На платформе с {since}</span>}
                        </div>
                    </div>

                    <button className="profile-btn profile-btn--primary profile-hero__edit" onClick={() => setEditing(true)} disabled={!extras}>
                        <Icon name="edit" size={15} /> Редактировать
                    </button>
                </div>
            </section>

            <div className="profile-grid">
                <div className="profile-main">
                    {isStudent && (
                        <section className="profile-card">
                            <div className="profile-card__head">
                                <h3 className="profile-card__title">Статистика</h3>
                                {demo && (
                                    <span className="profile-chip profile-chip--demo" title="Сервер недоступен — показан пример данных">
                                        Демо-данные
                                    </span>
                                )}
                            </div>
                            {loadingStats ? (
                                <SkeletonRegion className="profile-stats">
                                    {[0, 1, 2, 3].map(i => (
                                        <div key={i} className="profile-stat"><Skeleton height={86} /></div>
                                    ))}
                                </SkeletonRegion>
                            ) : (
                                <div className="profile-stats">
                                    <StatTile
                                        icon="trending"
                                        label="Баллы"
                                        value={progress ? progress.earnedPoints : '—'}
                                        hint={progress ? `из ${progress.totalPoints}` : 'нет данных'}
                                    />
                                    <StatTile
                                        icon="checkCircle"
                                        accent="green"
                                        label="Лабораторные"
                                        value={progress ? `${progress.completedLaboratories}/${progress.totalLaboratories}` : '—'}
                                        hint="выполнено"
                                    />
                                    <StatTile
                                        icon="flag"
                                        accent="violet"
                                        label="Место в группе"
                                        value={leaderboard && leaderboard.currentUserRank > 0 ? `#${leaderboard.currentUserRank}` : '—'}
                                        hint={groupSize ? `из ${groupSize}` : 'рейтинг недоступен'}
                                    />
                                    <StatTile
                                        icon="clock"
                                        accent="orange"
                                        label="Прогресс курса"
                                        value={progress ? `${progress.progressPercent}%` : '—'}
                                        hint={
                                            <span className="profile-stat__bar">
                                                <span style={{ transform: `scaleX(${Math.min(progress?.progressPercent ?? 0, 100) / 100})` }} />
                                            </span>
                                        }
                                    />
                                </div>
                            )}
                        </section>
                    )}

                    {isStudent && (
                        <section className="profile-card">
                            <div className="profile-card__head">
                                <h3 className="profile-card__title">Последние работы</h3>
                                <button className="profile-btn profile-btn--text" onClick={() => navigate('/progress')}>
                                    Весь прогресс <Icon name="chevronRight" size={14} />
                                </button>
                            </div>
                            {loadingStats ? (
                                <SkeletonRegion><Skeleton height={44} /><Skeleton height={44} /></SkeletonRegion>
                            ) : recentLabs.length === 0 ? (
                                <div className="profile-empty">Пока нет начатых лабораторных</div>
                            ) : (
                                <ul className="profile-labs">
                                    {recentLabs.map(lab => {
                                        const meta = STATUS_META[lab.status] ?? STATUS_META[StudentLaboratoryStatus.NotStarted];
                                        return (
                                            <li key={lab.laboratoryId}>
                                                <button className="profile-lab" onClick={() => navigate(`/labs/${lab.laboratoryId}`)}>
                                                    <span className="profile-lab__title">{lab.title}</span>
                                                    <span className="profile-lab__points">{lab.earnedPoints}/{lab.maxPoints}</span>
                                                    <span className={`profile-status profile-status--${meta.cls}`}>{meta.label}</span>
                                                </button>
                                            </li>
                                        );
                                    })}
                                </ul>
                            )}
                        </section>
                    )}

                    <section className="profile-card">
                        <div className="profile-card__head">
                            <h3 className="profile-card__title">О себе</h3>
                        </div>
                        {!extras ? (
                            <SkeletonRegion><Skeleton height={14} /><Skeleton width="70%" height={14} /></SkeletonRegion>
                        ) : extras.about ? (
                            <p className="profile-about">{extras.about}</p>
                        ) : (
                            <button className="profile-empty profile-empty--action" onClick={() => setEditing(true)}>
                                <Icon name="plus" size={15} /> Расскажите о себе — это увидят преподаватели и одногруппники
                            </button>
                        )}
                    </section>
                </div>

                <aside className="profile-side">
                    <section className="profile-card">
                        <h3 className="profile-card__title">Контакты</h3>
                        {!extras ? (
                            <SkeletonRegion><Skeleton height={40} /><Skeleton height={40} /><Skeleton height={40} /></SkeletonRegion>
                        ) : (
                            <div className="profile-contacts">
                                <ContactRow icon="inbox" label="Email" value={displayEmail} href={displayEmail ? `mailto:${displayEmail}` : undefined} />
                                <ContactRow icon="messageSquare" label="Telegram" value={extras.telegram} href={extras.telegram ? `https://t.me/${extras.telegram.slice(1)}` : undefined} />
                                <ContactRow icon="user" label="Телефон" value={extras.phone} />
                            </div>
                        )}
                    </section>

                    <section className="profile-card">
                        <h3 className="profile-card__title">Безопасность</h3>
                        <div className="profile-security">
                            <div className="profile-security__row">
                                <Icon name="shield" size={16} />
                                <span>Пароль</span>
                                <button className="profile-btn profile-btn--text" onClick={() => navigate('/change-password')}>
                                    Сменить
                                </button>
                            </div>
                        </div>
                    </section>
                </aside>
            </div>

            {editing && extras && (
                <ProfileEditModal
                    fullName={displayName}
                    email={displayEmail}
                    initials={initials}
                    extras={extras}
                    onClose={closeEditor}
                    onSave={handleSave}
                />
            )}

            {toast && (
                <div className="profile-toast" role="status">
                    <Icon name="check" size={16} /> {toast}
                </div>
            )}
        </div>
    );
};

export default ProfilePage;
