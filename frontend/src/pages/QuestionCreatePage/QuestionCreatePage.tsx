import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import axiosInstance from '../../utils/axiosInstance';
import { Icon } from '../../shared/Icon';
import '../QuestionsPage/QuestionsPage.css';

interface LabOption {
    id: string;
    title: string;
}

interface PagedResult<T> {
    items: T[];
    totalCount: number;
}

export default function QuestionCreatePage() {
    const navigate = useNavigate();

    const userName = localStorage.getItem('user_name') ?? '';

    const [labs, setLabs]                       = useState<LabOption[]>([]);
    const [labsLoading, setLabsLoading]         = useState(true);
    const [selectedLabId, setSelectedLabId]     = useState('');
    const [description, setDescription]         = useState('');
    const [submitting, setSubmitting]           = useState(false);
    const [error, setError]                     = useState('');

    useEffect(() => {
        axiosInstance
            .get<PagedResult<LabOption>>('/public/api/v1/laboratories', {
                params: { page: 1, pageSize: 200 },
            })
            .then(r => {
                setLabs(r.data.items);

                const params = new URLSearchParams(window.location.search);
                const preTitle = params.get('lab');
                if (preTitle) {
                    const match = r.data.items.find(
                        l => l.title.toLowerCase() === preTitle.toLowerCase()
                    );
                    if (match) setSelectedLabId(match.id);
                }
            })
            .catch(() => {})
            .finally(() => setLabsLoading(false));
    }, []);

    const selectedLabTitle =
        labs.find(l => l.id === selectedLabId)?.title ?? null;

    async function handleSubmit(e: React.FormEvent) {
        e.preventDefault();
        if (!description.trim()) {
            setError('Введите описание вопроса');
            return;
        }
        setError('');
        setSubmitting(true);
        try {
            const res = await axiosInstance.post<{ id: string }>('/public/api/v1/questions', {
                laboratoryTitle: selectedLabTitle,
                description: description.trim(),
            });
            navigate(`/questions/${res.data.id}`);
        } catch {
            setError('Не удалось создать вопрос. Попробуйте снова.');
            setSubmitting(false);
        }
    }

    return (
        <div className="qst-page">
            <button className="qst-back" onClick={() => navigate('/questions')}>
                <Icon name="chevron-left" size={14} />
                Назад к вопросам
            </button>

            <div className="qst-create-card">
                <h1 className="qst-create-title">Задать вопрос</h1>

                {error && <div className="qst-error">{error}</div>}

                <form onSubmit={handleSubmit}>
                    <div className="qst-form-field">
                        <label className="qst-form-label">ФИО</label>
                        <input
                            className="qst-form-input"
                            value={userName}
                            readOnly
                        />
                    </div>

                    <div className="qst-form-field">
                        <label className="qst-form-label">Лабораторная работа (необязательно)</label>
                        <select
                            className="qst-form-input"
                            value={selectedLabId}
                            onChange={e => setSelectedLabId(e.target.value)}
                            disabled={labsLoading}
                            style={{ cursor: labsLoading ? 'not-allowed' : 'pointer', colorScheme: 'dark' }}
                        >
                            <option value="">
                                {labsLoading ? 'Загрузка…' : '— Не привязывать к лабораторной —'}
                            </option>
                            {labs.map(l => (
                                <option key={l.id} value={l.id}>{l.title}</option>
                            ))}
                        </select>
                    </div>

                    <div className="qst-form-field">
                        <label className="qst-form-label">Описание вопроса *</label>
                        <textarea
                            className="qst-form-textarea"
                            placeholder="Опишите ваш вопрос подробно…"
                            value={description}
                            onChange={e => setDescription(e.target.value)}
                            maxLength={3000}
                            required
                        />
                    </div>

                    <div style={{ display: 'flex', gap: 10, justifyContent: 'flex-end' }}>
                        <button
                            type="button"
                            className="qst-btn-secondary"
                            onClick={() => navigate('/questions')}
                        >
                            Отмена
                        </button>
                        <button
                            type="submit"
                            className="qst-btn-primary"
                            disabled={submitting}
                        >
                            {submitting ? 'Отправка…' : 'Отправить'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}
