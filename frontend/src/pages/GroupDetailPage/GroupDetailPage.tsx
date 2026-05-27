import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import axiosInstance from '../../utils/axiosInstance';
import './GroupDetailPage.css';

type GroupMember = {
    userId: string;
    fullName: string;
    email: string;
};

type GroupDetail = {
    id: string;
    name: string;
    createdAt: string;
    students: GroupMember[];
    teachers: GroupMember[];
};

type UserItem = {
    id: string;
    fullName: string;
    email: string;
    role: number; // 0=student, 1=teacher, 2=admin, 3=superadmin
};

const ROLE_STUDENT = 0;
const ROLE_TEACHER = 1;

const GroupDetailPage = () => {
    const { groupId } = useParams<{ groupId: string }>();
    const navigate = useNavigate();

    const [group, setGroup] = useState<GroupDetail | null>(null);
    const [allUsers, setAllUsers] = useState<UserItem[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const [studentSearch, setStudentSearch] = useState('');
    const [teacherSearch, setTeacherSearch] = useState('');
    const [addingStudentId, setAddingStudentId] = useState<string | null>(null);
    const [addingTeacherId, setAddingTeacherId] = useState<string | null>(null);
    const [removingId, setRemovingId] = useState<string | null>(null);

    const fetchGroup = async () => {
        try {
            const res = await axiosInstance.get<GroupDetail>(`/public/api/v1/admin/groups/${groupId}`);
            setGroup(res.data);
        } catch {
            setError('Группа не найдена');
        }
    };

    useEffect(() => {
        const init = async () => {
            try {
                const [groupRes, usersRes] = await Promise.all([
                    axiosInstance.get<GroupDetail>(`/public/api/v1/admin/groups/${groupId}`),
                    axiosInstance.get<UserItem[]>('/public/api/v1/user/get-all'),
                ]);
                setGroup(groupRes.data);
                setAllUsers(usersRes.data);
            } catch {
                setError('Не удалось загрузить данные');
            } finally {
                setLoading(false);
            }
        };
        init();
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [groupId]);

    const handleAddStudent = async (userId: string) => {
        setAddingStudentId(userId);
        setError(null);
        try {
            await axiosInstance.post(`/public/api/v1/admin/groups/${groupId}/students/${userId}`);
            await fetchGroup();
            setStudentSearch('');
        } catch {
            setError('Не удалось добавить студента');
        } finally {
            setAddingStudentId(null);
        }
    };

    const handleRemoveStudent = async (userId: string) => {
        setRemovingId(userId);
        try {
            await axiosInstance.delete(`/public/api/v1/admin/groups/${groupId}/students/${userId}`);
            setGroup(prev => prev ? { ...prev, students: prev.students.filter(s => s.userId !== userId) } : prev);
        } catch {
            setError('Не удалось убрать студента');
        } finally {
            setRemovingId(null);
        }
    };

    const handleAddTeacher = async (userId: string) => {
        setAddingTeacherId(userId);
        setError(null);
        try {
            await axiosInstance.post(`/public/api/v1/admin/groups/${groupId}/teachers/${userId}`);
            await fetchGroup();
            setTeacherSearch('');
        } catch {
            setError('Не удалось добавить преподавателя');
        } finally {
            setAddingTeacherId(null);
        }
    };

    const handleRemoveTeacher = async (userId: string) => {
        setRemovingId(userId);
        try {
            await axiosInstance.delete(`/public/api/v1/admin/groups/${groupId}/teachers/${userId}`);
            setGroup(prev => prev ? { ...prev, teachers: prev.teachers.filter(t => t.userId !== userId) } : prev);
        } catch {
            setError('Не удалось убрать преподавателя');
        } finally {
            setRemovingId(null);
        }
    };

    if (loading) return <div className="gdp-loading">Загрузка...</div>;
    if (!group) return <div className="gdp-loading">{error ?? 'Группа не найдена'}</div>;

    const memberStudentIds = new Set(group.students.map(s => s.userId));
    const memberTeacherIds = new Set(group.teachers.map(t => t.userId));

    const availableStudents = allUsers.filter(u =>
        u.role === ROLE_STUDENT &&
        !memberStudentIds.has(u.id) &&
        (studentSearch === '' ||
            u.fullName.toLowerCase().includes(studentSearch.toLowerCase()) ||
            u.email.toLowerCase().includes(studentSearch.toLowerCase()))
    );

    const availableTeachers = allUsers.filter(u =>
        u.role === ROLE_TEACHER &&
        !memberTeacherIds.has(u.id) &&
        (teacherSearch === '' ||
            u.fullName.toLowerCase().includes(teacherSearch.toLowerCase()) ||
            u.email.toLowerCase().includes(teacherSearch.toLowerCase()))
    );

    return (
        <div className="gdp-page">
            <div className="gdp-breadcrumb">
                <button className="gdp-breadcrumb-link" onClick={() => navigate('/groups')}>
                    Группы
                </button>
                <span className="gdp-breadcrumb-sep">/</span>
                <span className="gdp-breadcrumb-current">{group.name}</span>
            </div>

            <h2 className="gdp-title">{group.name}</h2>

            {error && <div className="gdp-error">{error}</div>}

            <div className="gdp-layout">
                {/* ── Студенты ── */}
                <section className="gdp-section">
                    <h3 className="gdp-section-title">Студенты <span className="gdp-count">{group.students.length}</span></h3>

                    {/* Текущие участники */}
                    <div className="gdp-members">
                        {group.students.length === 0 ? (
                            <p className="gdp-members-empty">Студентов нет</p>
                        ) : (
                            group.students.map(s => (
                                <div key={s.userId} className="gdp-member-row">
                                    <div className="gdp-member-avatar">{s.fullName[0]?.toUpperCase()}</div>
                                    <div className="gdp-member-info">
                                        <span className="gdp-member-name">{s.fullName}</span>
                                        <span className="gdp-member-email">{s.email}</span>
                                    </div>
                                    <button
                                        className="gdp-remove-btn"
                                        disabled={removingId === s.userId}
                                        onClick={() => handleRemoveStudent(s.userId)}
                                    >
                                        ×
                                    </button>
                                </div>
                            ))
                        )}
                    </div>

                    {/* Поиск для добавления */}
                    <div className="gdp-add-section">
                        <input
                            className="gdp-search"
                            type="text"
                            placeholder="Найти студента по имени или email..."
                            value={studentSearch}
                            onChange={e => setStudentSearch(e.target.value)}
                        />
                        {studentSearch && (
                            <div className="gdp-search-results">
                                {availableStudents.length === 0 ? (
                                    <p className="gdp-search-empty">Нет доступных студентов</p>
                                ) : (
                                    availableStudents.slice(0, 8).map(u => (
                                        <div key={u.id} className="gdp-search-row">
                                            <div className="gdp-search-info">
                                                <span className="gdp-search-name">{u.fullName}</span>
                                                <span className="gdp-search-email">{u.email}</span>
                                            </div>
                                            <button
                                                className="gdp-add-btn"
                                                disabled={addingStudentId === u.id}
                                                onClick={() => handleAddStudent(u.id)}
                                            >
                                                {addingStudentId === u.id ? '...' : '+ Добавить'}
                                            </button>
                                        </div>
                                    ))
                                )}
                            </div>
                        )}
                    </div>
                </section>

                {/* ── Преподаватели ── */}
                <section className="gdp-section">
                    <h3 className="gdp-section-title">Преподаватели <span className="gdp-count">{group.teachers.length}</span></h3>

                    <div className="gdp-members">
                        {group.teachers.length === 0 ? (
                            <p className="gdp-members-empty">Преподавателей нет</p>
                        ) : (
                            group.teachers.map(t => (
                                <div key={t.userId} className="gdp-member-row">
                                    <div className="gdp-member-avatar gdp-member-avatar--teacher">{t.fullName[0]?.toUpperCase()}</div>
                                    <div className="gdp-member-info">
                                        <span className="gdp-member-name">{t.fullName}</span>
                                        <span className="gdp-member-email">{t.email}</span>
                                    </div>
                                    <button
                                        className="gdp-remove-btn"
                                        disabled={removingId === t.userId}
                                        onClick={() => handleRemoveTeacher(t.userId)}
                                    >
                                        ×
                                    </button>
                                </div>
                            ))
                        )}
                    </div>

                    <div className="gdp-add-section">
                        <input
                            className="gdp-search"
                            type="text"
                            placeholder="Найти преподавателя по имени или email..."
                            value={teacherSearch}
                            onChange={e => setTeacherSearch(e.target.value)}
                        />
                        {teacherSearch && (
                            <div className="gdp-search-results">
                                {availableTeachers.length === 0 ? (
                                    <p className="gdp-search-empty">Нет доступных преподавателей</p>
                                ) : (
                                    availableTeachers.slice(0, 8).map(u => (
                                        <div key={u.id} className="gdp-search-row">
                                            <div className="gdp-search-info">
                                                <span className="gdp-search-name">{u.fullName}</span>
                                                <span className="gdp-search-email">{u.email}</span>
                                            </div>
                                            <button
                                                className="gdp-add-btn gdp-add-btn--teacher"
                                                disabled={addingTeacherId === u.id}
                                                onClick={() => handleAddTeacher(u.id)}
                                            >
                                                {addingTeacherId === u.id ? '...' : '+ Добавить'}
                                            </button>
                                        </div>
                                    ))
                                )}
                            </div>
                        )}
                    </div>
                </section>
            </div>
        </div>
    );
};

export default GroupDetailPage;
