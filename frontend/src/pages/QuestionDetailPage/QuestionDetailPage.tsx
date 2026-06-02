import React, { useState, useEffect, useCallback, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import axiosInstance from '../../utils/axiosInstance';
import { Icon } from '../../shared/Icon';
import '../QuestionsPage/QuestionsPage.css';

enum QuestionStatus {
    Open     = 0,
    Answered = 1,
    Closed   = 2,
}

interface QuestionReplyDto {
    id: string;
    senderFullName: string;
    isFromTeacher: boolean;
    content: string;
    createdAtUtc: string;
}

interface QuestionDetailDto {
    id: string;
    studentFullName: string;
    studentGroupName: string | null;
    laboratoryTitle: string | null;
    description: string;
    status: QuestionStatus;
    createdAtUtc: string;
    messages: QuestionReplyDto[];
}


function formatTime(iso: string): string {
    const d = new Date(iso);
    return d.toLocaleString('ru-RU', {
        day: '2-digit', month: '2-digit', year: 'numeric',
        hour: '2-digit', minute: '2-digit',
    });
}

function StatusBadge({ status }: { status: QuestionStatus }) {
    const map: Record<QuestionStatus, { cls: string; label: string }> = {
        [QuestionStatus.Open]:     { cls: 'qst-badge--open',     label: 'Открыт' },
        [QuestionStatus.Answered]: { cls: 'qst-badge--answered', label: 'Отвечен' },
        [QuestionStatus.Closed]:   { cls: 'qst-badge--closed',   label: 'Закрыт' },
    };
    const { cls, label } = map[status] ?? { cls: 'qst-badge--open', label: '—' };
    return <span className={`qst-badge ${cls}`}>{label}</span>;
}


function Message({ msg, isTeacherRole }: { msg: QuestionReplyDto; isTeacherRole: boolean }) {
    const isMine = isTeacherRole ? msg.isFromTeacher : !msg.isFromTeacher;
    return (
        <div className={`qst-msg ${isMine ? 'qst-msg--mine' : 'qst-msg--theirs'}`}>
            <span className="qst-msg__name">{msg.senderFullName}</span>
            <div className="qst-msg__bubble">{msg.content}</div>
            <span className="qst-msg__time">{formatTime(msg.createdAtUtc)}</span>
        </div>
    );
}


export default function QuestionDetailPage() {
    const { questionId } = useParams<{ questionId: string }>();
    const navigate = useNavigate();

    const role = localStorage.getItem('user_role');
    const isTeacher = role === 'teacher' || role === 'admin' || role === 'superAdmin';

    const [question, setQuestion] = useState<QuestionDetailDto | null>(null);
    const [loading, setLoading]   = useState(true);
    const [error, setError]       = useState('');

    const [msgText, setMsgText]       = useState('');
    const [sending, setSending]       = useState(false);
    const [closing, setClosing]       = useState(false);
    const [sendError, setSendError]   = useState('');

    const chatEndRef = useRef<HTMLDivElement>(null);

    const loadQuestion = useCallback(() => {
        if (!questionId) return;
        const url = isTeacher
            ? `/public/api/v1/teacher/questions/${questionId}`
            : `/public/api/v1/questions/${questionId}`;
        axiosInstance.get<QuestionDetailDto>(url)
            .then(r => { setQuestion(r.data); setError(''); })
            .catch(() => setError('Не удалось загрузить вопрос'))
            .finally(() => setLoading(false));
    }, [questionId, isTeacher]);

    useEffect(() => { loadQuestion(); }, [loadQuestion]);

    useEffect(() => {
        chatEndRef.current?.scrollIntoView({ behavior: 'smooth' });
    }, [question?.messages.length]);

    async function handleSend(e: React.FormEvent) {
        e.preventDefault();
        if (!msgText.trim() || !questionId) return;
        setSendError('');
        setSending(true);
        try {
            const url = isTeacher
                ? `/public/api/v1/teacher/questions/${questionId}/reply`
                : `/public/api/v1/questions/${questionId}/message`;
            await axiosInstance.post(url, { content: msgText.trim() });
            setMsgText('');
            loadQuestion();
        } catch {
            setSendError('Не удалось отправить сообщение');
        } finally {
            setSending(false);
        }
    }

    async function handleClose() {
        if (!questionId || !window.confirm('Закрыть вопрос? Новые сообщения будут недоступны.')) return;
        setClosing(true);
        try {
            const url = isTeacher
                ? `/public/api/v1/teacher/questions/${questionId}/close`
                : `/public/api/v1/questions/${questionId}/close`;
            await axiosInstance.post(url);
            loadQuestion();
        } catch {
            setSendError('Не удалось закрыть вопрос');
        } finally {
            setClosing(false);
        }
    }

    function handleTextareaInput(e: React.ChangeEvent<HTMLTextAreaElement>) {
        const el = e.target;
        el.style.height = 'auto';
        el.style.height = `${Math.min(el.scrollHeight, 120)}px`;
        setMsgText(el.value);
    }

    function handleKeyDown(e: React.KeyboardEvent<HTMLTextAreaElement>) {
        if ((e.ctrlKey || e.metaKey) && e.key === 'Enter') {
            e.preventDefault();
            handleSend(e as unknown as React.FormEvent);
        }
    }

    const isClosed = question?.status === QuestionStatus.Closed;

    return (
        <div className="qst-page">
            <button className="qst-back" onClick={() => navigate('/questions')}>
                <Icon name="chevron-left" size={14} />
                Назад к вопросам
            </button>

            {loading && <div className="qst-loading">Загрузка…</div>}
            {!loading && error && <div className="qst-error">{error}</div>}

            {!loading && question && (
                <>
                    {/* Header */}
                    <div className="qst-header" style={{ marginTop: 4 }}>
                        <h1 className="qst-detail-title" style={{ margin: 0 }}>
                            {isTeacher
                                ? `${question.studentFullName}${question.studentGroupName ? ` · ${question.studentGroupName}` : ''}`
                                : 'Мой вопрос'}
                        </h1>
                        <StatusBadge status={question.status} />
                    </div>

                    {question.laboratoryTitle && (
                        <p style={{ fontSize: 12, color: '#027ef2', margin: '0 0 16px', fontWeight: 600 }}>
                            {question.laboratoryTitle}
                        </p>
                    )}

                    {/* Initial question card */}
                    <div className="qst-question-card">
                        <div className="qst-question-card__meta">
                            {isTeacher
                                ? <><strong style={{ color: 'rgba(255,255,255,0.65)' }}>{question.studentFullName}</strong> · </>
                                : ''}
                            {formatTime(question.createdAtUtc)}
                        </div>
                        <div className="qst-question-card__label">ВОПРОС</div>
                        <p className="qst-question-card__text">{question.description}</p>
                    </div>

                    {/* Chat messages */}
                    {question.messages.length > 0 ? (
                        <div className="qst-chat">
                            {question.messages.map(m => (
                                <Message key={m.id} msg={m} isTeacherRole={isTeacher} />
                            ))}
                            <div ref={chatEndRef} />
                        </div>
                    ) : (
                        <div className="qst-chat-empty">
                            {isClosed ? 'Вопрос закрыт без ответа' : 'Сообщений пока нет'}
                        </div>
                    )}

                    {sendError && <div className="qst-error" style={{ marginBottom: 10 }}>{sendError}</div>}

                    {/* Input bar or closed notice */}
                    {isClosed ? (
                        <p className="qst-closed-notice">Вопрос закрыт — переписка завершена</p>
                    ) : (
                        <form className="qst-chat-bar" onSubmit={handleSend}>
                            <textarea
                                className="qst-chat-bar__input"
                                placeholder="Написать сообщение… (Ctrl+Enter — отправить)"
                                value={msgText}
                                onChange={handleTextareaInput}
                                onKeyDown={handleKeyDown}
                                maxLength={3000}
                                rows={1}
                            />
                            <button
                                type="button"
                                className="qst-chat-bar__close"
                                onClick={handleClose}
                                disabled={closing}
                                title="Закрыть вопрос"
                            >
                                {closing ? '…' : 'Закрыть'}
                            </button>
                            <button
                                type="submit"
                                className="qst-chat-bar__send"
                                disabled={sending || !msgText.trim()}
                            >
                                {sending ? '…' : 'Отправить'}
                            </button>
                        </form>
                    )}
                </>
            )}
        </div>
    );
}
