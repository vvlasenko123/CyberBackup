import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import axiosInstance from '../../utils/axiosInstance';
import BlockAutocomplete from '../../components/BlockAutocomplete/BlockAutocomplete';
import './LabEditPage.css';

type Difficulty = 1 | 2 | 3;


type FormState = {
    title: string;
    shortDescription: string;
    description: string;
    narrative: string;
    goal: string;
    environmentUrl: string;
    credentials: string;
    difficulty: Difficulty;
    block: string;
    maxPoints: number | '';
    hasFlag: boolean;
    expectedFlag: string;
    isPublished: boolean;
    sortOrder: number | '';
    deadlineAtUtc: string;
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
    difficulty: Difficulty;
    block: string;
    maxPoints: number;
    hasFlag: boolean;
    hasExpectedFlag: boolean;
    isPublished: boolean;
    sortOrder: number;
    deadlineAtUtc: string | null;
};

const toLocalDatetime = (iso: string | null): string => {
    if (!iso) return '';
    const d = new Date(iso);
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
};

const LabEditPage = () => {
    const { labId } = useParams<{ labId: string }>();
    const navigate = useNavigate();

    const [form, setForm] = useState<FormState | null>(null);
    const [existingBlocks, setExistingBlocks] = useState<string[]>([]);
    const [loading, setLoading] = useState(true);
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const load = async () => {
            try {
                const [labRes, blocksRes] = await Promise.all([
                    axiosInstance.get<TeacherLabDetail>(`/public/api/v1/teacher/laboratories/${labId}`),
                    axiosInstance.get<string[]>('/public/api/v1/teacher/laboratories/blocks').catch(() => ({ data: [] as string[] })),
                ]);
                const lab = labRes.data;
                setForm({
                    title: lab.title,
                    shortDescription: lab.shortDescription,
                    description: lab.description,
                    narrative: lab.narrative,
                    goal: lab.goal,
                    environmentUrl: lab.environmentUrl ?? '',
                    credentials: lab.credentials ?? '',
                    difficulty: lab.difficulty,
                    block: lab.block,
                    maxPoints: lab.maxPoints,
                    hasFlag: lab.hasFlag,
                    expectedFlag: '',
                    isPublished: lab.isPublished,
                    sortOrder: lab.sortOrder,
                    deadlineAtUtc: toLocalDatetime(lab.deadlineAtUtc),
                });
                setExistingBlocks(blocksRes.data);
            } catch {
                setError('Не удалось загрузить лабораторную работу');
            } finally {
                setLoading(false);
            }
        };
        load();
    }, [labId]);

    const set = <K extends keyof FormState>(key: K, value: FormState[K]) =>
        setForm(prev => prev ? { ...prev, [key]: value } : prev);

    const handleSubmit = async (publish: boolean) => {
        if (!form) return;
        if (!form.title.trim() || !form.block.trim() || form.maxPoints === '' || !form.description.trim()) return;

        setSubmitting(true);
        setError(null);

        try {
            const payload = {
                title: form.title.trim(),
                shortDescription: form.shortDescription.trim(),
                description: form.description.trim(),
                narrative: form.narrative.trim(),
                goal: form.goal.trim(),
                environmentUrl: form.environmentUrl.trim() || null,
                credentials: form.credentials.trim() || null,
                difficulty: form.difficulty,
                block: form.block.trim(),
                maxPoints: Number(form.maxPoints),
                hasFlag: form.hasFlag,
                expectedFlag: null,
                isPublished: publish,
                sortOrder: form.sortOrder === '' ? 0 : Number(form.sortOrder),
                deadlineAtUtc: form.deadlineAtUtc ? new Date(form.deadlineAtUtc).toISOString() : null,
                hints: [],
            };

            await axiosInstance.put(`/public/api/v1/teacher/laboratories/${labId}`, payload);
            navigate(`/labs/${labId}`);
        } catch (err: unknown) {
            const axiosErr = err as { response?: { data?: { message?: string } } };
            setError(axiosErr.response?.data?.message ?? 'Не удалось обновить лабораторную работу');
        } finally {
            setSubmitting(false);
        }
    };

    if (loading) return <div className="lab-edit-loading">Загрузка...</div>;
    if (!form) return <div className="lab-edit-loading">{error ?? 'Не удалось загрузить'}</div>;

    const isValid = form.title.trim() && form.block.trim() && form.maxPoints !== '' && form.description.trim();

    return (
        <div className="lab-edit-page">
            <div className="lab-edit-breadcrumb">
                <button className="lab-edit-breadcrumb-link" onClick={() => navigate(`/labs/${labId}`)}>
                    {form.title}
                </button>
                <span className="lab-edit-breadcrumb-sep">/</span>
                <span className="lab-edit-breadcrumb-current">Редактирование</span>
            </div>

            <div className="lab-edit-section">
                <p className="lab-edit-section-title">Основное</p>

                <div className="lab-edit-field">
                    <label className="lab-edit-label lab-edit-label--required">Название</label>
                    <input
                        className="lab-edit-input"
                        value={form.title}
                        onChange={e => set('title', e.target.value)}
                    />
                </div>

                <div className="lab-edit-field">
                    <label className="lab-edit-label">Краткое описание</label>
                    <input
                        className="lab-edit-input"
                        value={form.shortDescription}
                        onChange={e => set('shortDescription', e.target.value)}
                    />
                </div>

                <div className="lab-edit-row">
                    <div className="lab-edit-field">
                        <label className="lab-edit-label lab-edit-label--required">Блок</label>
                        <BlockAutocomplete
                            value={form.block}
                            onChange={v => set('block', v)}
                            suggestions={existingBlocks}
                            placeholder="Блок 1: Веб-безопасность"
                            inputClassName="lab-edit-input"
                        />
                    </div>
                    <div className="lab-edit-field">
                        <label className="lab-edit-label lab-edit-label--required">Сложность</label>
                        <select
                            className="lab-edit-select"
                            value={form.difficulty}
                            onChange={e => set('difficulty', Number(e.target.value) as Difficulty)}
                        >
                            <option value={1}>Лёгкая</option>
                            <option value={2}>Средняя</option>
                            <option value={3}>Сложная</option>
                        </select>
                    </div>
                    <div className="lab-edit-field">
                        <label className="lab-edit-label lab-edit-label--required">Максимум баллов</label>
                        <input
                            className="lab-edit-input"
                            type="number"
                            min={1}
                            value={form.maxPoints}
                            onChange={e => set('maxPoints', e.target.value === '' ? '' : Number(e.target.value))}
                        />
                    </div>
                    <div className="lab-edit-field">
                        <label className="lab-edit-label">Дедлайн сдачи</label>
                        <input
                            className="lab-edit-input"
                            type="datetime-local"
                            value={form.deadlineAtUtc}
                            onChange={e => set('deadlineAtUtc', e.target.value)}
                        />
                    </div>
                </div>
            </div>

            <div className="lab-edit-section">
                <p className="lab-edit-section-title">Содержание</p>

                <div className="lab-edit-field">
                    <label className="lab-edit-label lab-edit-label--required">Полное описание</label>
                    <textarea
                        className="lab-edit-textarea"
                        value={form.description}
                        onChange={e => set('description', e.target.value)}
                        rows={3}
                    />
                </div>

                <div className="lab-edit-row">
                    <div className="lab-edit-field">
                        <label className="lab-edit-label">URL среды</label>
                        <input
                            className="lab-edit-input"
                            value={form.environmentUrl}
                            onChange={e => set('environmentUrl', e.target.value)}
                        />
                    </div>
                    <div className="lab-edit-field">
                        <label className="lab-edit-label">Учётные данные</label>
                        <input
                            className="lab-edit-input"
                            value={form.credentials}
                            onChange={e => set('credentials', e.target.value)}
                        />
                    </div>
                </div>
            </div>

            {error && <div className="lab-edit-error">{error}</div>}

            <div className="lab-edit-actions">
                <button
                    className="lab-edit-btn-primary"
                    onClick={() => handleSubmit(true)}
                    disabled={submitting || !isValid}
                >
                    {submitting ? 'Сохранение...' : 'Сохранить и опубликовать'}
                </button>
                <button
                    className="lab-edit-btn-primary"
                    onClick={() => handleSubmit(false)}
                    disabled={submitting || !isValid}
                    style={{ background: '#374151' }}
                >
                    Сохранить как черновик
                </button>
                <button
                    className="lab-edit-btn-outline"
                    onClick={() => navigate(`/labs/${labId}`)}
                    disabled={submitting}
                >
                    Отмена
                </button>
            </div>
        </div>
    );
};

export default LabEditPage;
