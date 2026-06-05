import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import axiosInstance from '../../utils/axiosInstance';
import BlockAutocomplete from '../../components/BlockAutocomplete/BlockAutocomplete';
import './LabCreatePage.css';

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

type CreateLabResponse = {
    id: string;
    title: string;
};

const INITIAL_FORM: FormState = {
    title: '',
    shortDescription: '',
    description: '',
    narrative: '',
    goal: '',
    environmentUrl: '',
    credentials: '',
    difficulty: 1,
    block: '',
    maxPoints: '',
    hasFlag: false,
    expectedFlag: '',
    isPublished: false,
    sortOrder: '',
    deadlineAtUtc: '',
};

const LabCreatePage = () => {
    const navigate = useNavigate();

    const [form, setForm] = useState<FormState>(INITIAL_FORM);
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [existingBlocks, setExistingBlocks] = useState<string[]>([]);

    useEffect(() => {
        axiosInstance.get<string[]>('/public/api/v1/teacher/laboratories/blocks')
            .then(res => setExistingBlocks(res.data))
            .catch(() => {});
    }, []);

    const set = <K extends keyof FormState>(key: K, value: FormState[K]) =>
        setForm(prev => ({ ...prev, [key]: value }));

    const handleSubmit = async (publish: boolean) => {
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
                hasFlag: false,
                expectedFlag: null,
                isPublished: publish,
                sortOrder: 0,
                deadlineAtUtc: form.deadlineAtUtc ? new Date(form.deadlineAtUtc).toISOString() : null,
                hints: [],
            };

            const res = await axiosInstance.post<CreateLabResponse>('/public/api/v1/teacher/laboratories', payload);
            navigate(`/labs/${res.data.id}`);
        } catch (err: unknown) {
            const axiosErr = err as { response?: { data?: { message?: string } } };
            setError(axiosErr.response?.data?.message ?? 'Не удалось создать лабораторную работу');
        } finally {
            setSubmitting(false);
        }
    };

    const isValid = form.title.trim() && form.block.trim() && form.maxPoints !== '' && form.description.trim();

    return (
        <div className="lab-create-page">
            <div className="lab-create-breadcrumb">
                <button className="lab-create-breadcrumb-link" onClick={() => navigate('/labs')}>
                    Лабораторные
                </button>
                <span className="lab-create-breadcrumb-sep">/</span>
                <span className="lab-create-breadcrumb-current">Создать лабораторную</span>
            </div>

            {/* Basic info */}
            <div className="lab-create-section">
                <p className="lab-create-section-title">Основное</p>

                <div className="lab-create-field">
                    <label className="lab-create-label lab-create-label--required">Название</label>
                    <input
                        className="lab-create-input"
                        placeholder="SQL Workshop"
                        value={form.title}
                        onChange={e => set('title', e.target.value)}
                    />
                </div>

                <div className="lab-create-field">
                    <label className="lab-create-label lab-create-label--required">Краткое описание</label>
                    <input
                        className="lab-create-input"
                        placeholder="Одна строка для списка лабораторных"
                        value={form.shortDescription}
                        onChange={e => set('shortDescription', e.target.value)}
                    />
                </div>

                <div className="lab-create-row">
                    <div className="lab-create-field">
                        <label className="lab-create-label lab-create-label--required">Блок</label>
                        <BlockAutocomplete
                            value={form.block}
                            onChange={v => set('block', v)}
                            suggestions={existingBlocks}
                            placeholder="Блок 1: Веб-безопасность"
                            inputClassName="lab-create-input"
                        />
                    </div>
                    <div className="lab-create-field">
                        <label className="lab-create-label lab-create-label--required">Сложность</label>
                        <select
                            className="lab-create-select"
                            value={form.difficulty}
                            onChange={e => set('difficulty', Number(e.target.value) as Difficulty)}
                        >
                            <option value={1}>Лёгкая</option>
                            <option value={2}>Средняя</option>
                            <option value={3}>Сложная</option>
                        </select>
                    </div>
                    <div className="lab-create-field">
                        <label className="lab-create-label lab-create-label--required">Максимум баллов</label>
                        <input
                            className="lab-create-input"
                            type="number"
                            min={1}
                            placeholder="150"
                            value={form.maxPoints}
                            onChange={e => set('maxPoints', e.target.value === '' ? '' : Number(e.target.value))}
                        />
                    </div>
                    <div className="lab-create-field">
                        <label className="lab-create-label">Дедлайн сдачи</label>
                        <input
                            className="lab-create-input"
                            type="datetime-local"
                            value={form.deadlineAtUtc}
                            onChange={e => set('deadlineAtUtc', e.target.value)}
                        />
                    </div>
                </div>
            </div>

            {/* Content */}
            <div className="lab-create-section">
                <p className="lab-create-section-title">Содержание</p>

                <div className="lab-create-field">
                    <label className="lab-create-label lab-create-label--required">Полное описание</label>
                    <textarea
                        className="lab-create-textarea"
                        placeholder="Подробное описание для студентов..."
                        value={form.description}
                        onChange={e => set('description', e.target.value)}
                        rows={3}
                    />
                </div>

                <div className="lab-create-row">
                    <div className="lab-create-field">
                        <label className="lab-create-label">URL среды</label>
                        <input
                            className="lab-create-input"
                            placeholder="http://lab.local:8080/"
                            value={form.environmentUrl}
                            onChange={e => set('environmentUrl', e.target.value)}
                        />
                    </div>
                    <div className="lab-create-field">
                        <label className="lab-create-label">Учётные данные</label>
                        <input
                            className="lab-create-input"
                            placeholder="guest/guest"
                            value={form.credentials}
                            onChange={e => set('credentials', e.target.value)}
                        />
                    </div>
                </div>
            </div>

            {error && <div className="lab-create-error">{error}</div>}

            <div className="lab-create-actions">
                <button
                    className="lab-create-btn-primary"
                    onClick={() => handleSubmit(true)}
                    disabled={submitting || !isValid}
                >
                    {submitting ? 'Создание...' : 'Опубликовать'}
                </button>
                <button
                    className="lab-create-btn-primary"
                    onClick={() => handleSubmit(false)}
                    disabled={submitting || !isValid}
                    style={{ background: '#374151', }}
                >
                    Сохранить как черновик
                </button>
                <button
                    className="lab-create-btn-outline"
                    onClick={() => navigate('/labs')}
                    disabled={submitting}
                >
                    Отмена
                </button>
            </div>
        </div>
    );
};

export default LabCreatePage;
