import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import axiosInstance from '../../utils/axiosInstance';
import './LabCreatePage.css';

type Difficulty = 1 | 2 | 3;

type HintInput = {
    localId: number;
    orderNumber: number;
    title: string;
    text: string;
    penaltyPoints: number;
};

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
};

let hintCounter = 0;

const LabCreatePage = () => {
    const navigate = useNavigate();

    const [form, setForm] = useState<FormState>(INITIAL_FORM);
    const [hints, setHints] = useState<HintInput[]>([]);
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const set = <K extends keyof FormState>(key: K, value: FormState[K]) =>
        setForm(prev => ({ ...prev, [key]: value }));

    const addHint = () => {
        hintCounter += 1;
        setHints(prev => [
            ...prev,
            { localId: hintCounter, orderNumber: prev.length + 1, title: '', text: '', penaltyPoints: 0 },
        ]);
    };

    const updateHint = (localId: number, field: keyof Omit<HintInput, 'localId'>, value: string | number) =>
        setHints(prev => prev.map(h => h.localId === localId ? { ...h, [field]: value } : h));

    const removeHint = (localId: number) =>
        setHints(prev =>
            prev
                .filter(h => h.localId !== localId)
                .map((h, i) => ({ ...h, orderNumber: i + 1 }))
        );

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
                hasFlag: form.hasFlag,
                expectedFlag: form.hasFlag && form.expectedFlag.trim() ? form.expectedFlag.trim() : null,
                isPublished: publish,
                sortOrder: form.sortOrder === '' ? 0 : Number(form.sortOrder),
                hints: hints.map(h => ({
                    orderNumber: h.orderNumber,
                    title: h.title.trim() || null,
                    text: h.text.trim(),
                    penaltyPoints: Number(h.penaltyPoints),
                })),
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
                        <input
                            className="lab-create-input"
                            placeholder="Блок 1: Веб-безопасность"
                            value={form.block}
                            onChange={e => set('block', e.target.value)}
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
                        <label className="lab-create-label">Порядок</label>
                        <input
                            className="lab-create-input"
                            type="number"
                            min={0}
                            placeholder="0"
                            value={form.sortOrder}
                            onChange={e => set('sortOrder', e.target.value === '' ? '' : Number(e.target.value))}
                        />
                    </div>
                </div>
            </div>

            {/* Content */}
            <div className="lab-create-section">
                <p className="lab-create-section-title">Содержание</p>

                <div className="lab-create-field">
                    <label className="lab-create-label">Нарратив</label>
                    <textarea
                        className="lab-create-textarea"
                        placeholder="Ты — молодой специалист по безопасности..."
                        value={form.narrative}
                        onChange={e => set('narrative', e.target.value)}
                        rows={3}
                    />
                </div>

                <div className="lab-create-field">
                    <label className="lab-create-label">Задание (цель)</label>
                    <textarea
                        className="lab-create-textarea"
                        placeholder="Исследуй веб-приложение и найди SQL-уязвимость..."
                        value={form.goal}
                        onChange={e => set('goal', e.target.value)}
                        rows={3}
                    />
                </div>

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

            {/* Flag */}
            <div className="lab-create-section">
                <p className="lab-create-section-title">Флаг</p>

                <label className="lab-create-checkbox-row">
                    <input
                        className="lab-create-checkbox"
                        type="checkbox"
                        checked={form.hasFlag}
                        onChange={e => set('hasFlag', e.target.checked)}
                    />
                    <span className="lab-create-checkbox-label">Лабораторная содержит флаг</span>
                </label>

                {form.hasFlag && (
                    <div className="lab-create-field">
                        <label className="lab-create-label">Ожидаемый флаг</label>
                        <input
                            className="lab-create-input"
                            placeholder="CTF{...}"
                            value={form.expectedFlag}
                            onChange={e => set('expectedFlag', e.target.value)}
                        />
                    </div>
                )}
            </div>

            {/* Hints */}
            <div className="lab-create-section">
                <p className="lab-create-section-title">Подсказки</p>

                <div className="lab-create-hints">
                    {hints.map(hint => (
                        <div key={hint.localId} className="lab-create-hint-card">
                            <div className="lab-create-hint-header">
                                <span className="lab-create-hint-num">Подсказка #{hint.orderNumber}</span>
                                <button
                                    className="lab-create-hint-remove"
                                    onClick={() => removeHint(hint.localId)}
                                    title="Удалить подсказку"
                                >
                                    ×
                                </button>
                            </div>
                            <div className="lab-create-row">
                                <div className="lab-create-field">
                                    <label className="lab-create-label">Заголовок (необязательно)</label>
                                    <input
                                        className="lab-create-input"
                                        placeholder="Подсказка о SQL"
                                        value={hint.title}
                                        onChange={e => updateHint(hint.localId, 'title', e.target.value)}
                                    />
                                </div>
                                <div className="lab-create-field" style={{ maxWidth: 140 }}>
                                    <label className="lab-create-label">Штраф (баллы)</label>
                                    <input
                                        className="lab-create-input"
                                        type="number"
                                        min={0}
                                        placeholder="0"
                                        value={hint.penaltyPoints}
                                        onChange={e => updateHint(hint.localId, 'penaltyPoints', Number(e.target.value))}
                                    />
                                </div>
                            </div>
                            <div className="lab-create-field">
                                <label className="lab-create-label">Текст подсказки</label>
                                <textarea
                                    className="lab-create-textarea"
                                    placeholder="Попробуй одинарную кавычку..."
                                    value={hint.text}
                                    rows={2}
                                    onChange={e => updateHint(hint.localId, 'text', e.target.value)}
                                />
                            </div>
                        </div>
                    ))}

                    <button className="lab-create-add-hint-btn" onClick={addHint}>
                        + Добавить подсказку
                    </button>
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
