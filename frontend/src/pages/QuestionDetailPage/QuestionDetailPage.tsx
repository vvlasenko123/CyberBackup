import { ReportPreview } from '../../components/ReportPreview/ReportPreview';
import { isAxiosError } from 'axios';
import { DetailSkeleton } from '../../components/Skeleton/Skeleton';
import React, { useState, useEffect, useCallback, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import axiosInstance from '../../utils/axiosInstance';
import { Icon } from '../../components/Icon';
import '../QuestionsPage/QuestionsPage.css';

const MAX_ATTACHMENTS = 5;
const MAX_ATTACHMENT_SIZE = 10 * 1024 * 1024;
const ATTACHMENT_EXTENSION = /\.(jpe?g|png|gif|pdf|docx)$/i;

const QuestionStatus = {
    Open: 0,
    Answered: 1,
    Closed: 2,
} as const;
type QuestionStatus = typeof QuestionStatus[keyof typeof QuestionStatus];

interface QuestionAttachmentDto {
    id: string;
    fileName: string;
    contentType: string;
    sizeBytes: number;
}

interface QuestionReplyDto {
    attachments?: QuestionAttachmentDto[];
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
        [QuestionStatus.Open]: { cls: 'qst-badge--open', label: 'Открыт' },
        [QuestionStatus.Answered]: { cls: 'qst-badge--answered', label: 'Отвечен' },
        [QuestionStatus.Closed]: { cls: 'qst-badge--closed', label: 'Закрыт' },
    };
    const { cls, label } = map[status] ?? { cls: 'qst-badge--open', label: '—' };
    return <span className={`qst-badge ${cls}`}>{label}</span>;
}


function fileSize(bytes: number) {
    return bytes < 1024 * 1024 ? `${Math.ceil(bytes / 1024)} КБ` : `${(bytes / 1024 / 1024).toFixed(1)} МБ`;
}

function Attachment({ file, baseUrl }: { file: QuestionAttachmentDto; baseUrl: string }) {
    const [busy, setBusy] = useState(false);
    const [error, setError] = useState('');
    const attachmentUrl = `${baseUrl}/attachments/${file.id}`;

    async function download() {
        setBusy(true);
        setError('');
        try {
            const response = await axiosInstance.get(attachmentUrl, { responseType: 'blob' });
            const url = URL.createObjectURL(new Blob([response.data], { type: file.contentType }));
            const link = document.createElement('a');
            link.href = url;
            link.download = file.fileName;
            document.body.appendChild(link);
            link.click();
            link.remove();
            window.setTimeout(() => URL.revokeObjectURL(url), 1000);
        } catch {
            setError('Не удалось загрузить вложение. Попробуйте ещё раз.');
        } finally { setBusy(false); }
    }

    return <div className="qst-attachment">
        <span className="qst-attachment__name">{file.fileName}</span>
        <span className="qst-attachment__size">{fileSize(file.sizeBytes)}</span>
        <div className="qst-attachment__actions">
            {file.contentType.startsWith('image/') && <ReportPreview
                url={attachmentUrl} filename={file.fileName} />}
            <button type="button" disabled={busy} onClick={() => void download()}>{busy ? 'Загрузка…' : 'Скачать'}</button>
        </div>
        {error && <span role="alert" className="qst-error">{error}</span>}
    </div>;
}

function Message({ msg, isTeacherRole, baseUrl }: { msg: QuestionReplyDto; isTeacherRole: boolean; baseUrl: string }) {
    const isMine = isTeacherRole ? msg.isFromTeacher : !msg.isFromTeacher;
    return (
        <div className={`qst-msg ${isMine ? 'qst-msg--mine' : 'qst-msg--theirs'}`}>
            <span className="qst-msg__name">{msg.senderFullName}</span>
            <div className="qst-msg__bubble">
                {msg.content}
                {msg.attachments?.map(file => <Attachment key={file.id} file={file} baseUrl={baseUrl} />)}
            </div>
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
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    const [files, setFiles] = useState<File[]>([]);
    const fileInputRef = useRef<HTMLInputElement>(null);
    const sendInProgress = useRef(false);
    const [msgText, setMsgText] = useState('');
    const [sending, setSending] = useState(false);
    const [closing, setClosing] = useState(false);
    const [sendError, setSendError] = useState('');

    const baseUrl = `/public/api/v1/${isTeacher ? 'teacher/' : ''}questions/${questionId}`;

    const content = msgText.trim();
    const hasMessage = Boolean(content) || files.length > 0;
    const isBusy = sending || closing;

    const chatEndRef = useRef<HTMLDivElement>(null);

    const loadQuestion = useCallback(() => {
        if (!questionId) return;
        axiosInstance.get<QuestionDetailDto>(baseUrl)
            .then(r => { setQuestion(r.data); setError(''); })
            .catch(() => setError('Не удалось загрузить вопрос'))
            .finally(() => setLoading(false));
    }, [questionId, baseUrl]);

    useEffect(() => { loadQuestion(); }, [loadQuestion]);

    useEffect(() => {
        chatEndRef.current?.scrollIntoView({ behavior: 'smooth' });
    }, [question?.messages.length]);

    async function handleSend(e: React.FormEvent) {
        e.preventDefault();
        if (!hasMessage || !questionId || sendInProgress.current || closing) return;
        sendInProgress.current = true;
        setSendError('');
        setSending(true);
        try {
            const url = `${baseUrl}/${isTeacher ? 'reply' : 'message'}`;
            if (files.length) {
                const body = new FormData();
                body.append('content', content);
                files.forEach(file => body.append('files', file));
                await axiosInstance.post(`${url}/attachments`, body, { headers: { 'Content-Type': 'multipart/form-data' } });
            } else {
                await axiosInstance.post(url, { content });
            }
            setFiles([]);
            setMsgText('');
            loadQuestion();
        } catch (error) {
            const message = isAxiosError(error) && error.response?.status === 400
                ? error.response.data?.message : null;
            setSendError(typeof message === 'string' ? message : 'Не удалось отправить сообщение');
        } finally {
            setSending(false);
            sendInProgress.current = false;
        }
    }

    function selectFiles(event: React.ChangeEvent<HTMLInputElement>) {
        const selected = Array.from(event.target.files ?? []);
        event.target.value = '';
        if (files.length + selected.length > MAX_ATTACHMENTS) {
            setSendError('Можно прикрепить до 5 файлов');
            return;
        }
        if (selected.some(file => !ATTACHMENT_EXTENSION.test(file.name) || file.size === 0 || file.size > MAX_ATTACHMENT_SIZE)) {
            setSendError('Выберите непустые JPG, PNG, GIF, PDF или DOCX до 10 МБ');
            return;
        }
        setFiles(current => [...current, ...selected]);
        setSendError('');
    }

    async function handleClose() {
        if (!questionId || !window.confirm('Закрыть вопрос? Новые сообщения будут недоступны.')) return;
        setClosing(true);
        try {
            await axiosInstance.post(`${baseUrl}/close`);
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
                <Icon name="chevronLeft" size={14} />
                Назад к вопросам
            </button>

            {loading && <DetailSkeleton panelClassName="qst-question-card" form />}
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
                                <Message key={m.id} msg={m} isTeacherRole={isTeacher} baseUrl={baseUrl} />
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
                            <div className="qst-chat-bar__compose">
                                <div className="qst-chat-bar__hint" id="attachment-hint">До 5 файлов по 10 МБ: JPG, PNG, GIF, PDF, DOCX</div>
                                {files.length > 0 && <ul className="qst-selected-files">
                                    {files.map((file, index) => <li key={`${file.name}-${index}`}>
                                        <span>{file.name} · {fileSize(file.size)}</span>
                                        <button type="button" disabled={sending} aria-label={`Удалить ${file.name}`}
                                            onClick={() => setFiles(current => current.filter((_, i) => i !== index))}>×</button>
                                    </li>)}
                                </ul>}
                                <input ref={fileInputRef} type="file" multiple hidden accept=".jpg,.jpeg,.png,.gif,.pdf,.docx"
                                    aria-label="Вложения" onChange={selectFiles} disabled={isBusy} />
                                <textarea
                                    disabled={isBusy}
                                    className="qst-chat-bar__input"
                                    placeholder="Написать сообщение… (Ctrl+Enter — отправить)"
                                    value={msgText}
                                    onChange={handleTextareaInput}
                                    onKeyDown={handleKeyDown}
                                    maxLength={3000}
                                    rows={1}
                                />
                            </div>
                            <button type="button" className="qst-chat-bar__close" disabled={isBusy || files.length >= MAX_ATTACHMENTS}
                                aria-describedby="attachment-hint" onClick={() => fileInputRef.current?.click()}>Прикрепить</button>
                            <button
                                type="button"
                                className="qst-chat-bar__close"
                                onClick={handleClose}
                                disabled={isBusy}
                                title="Закрыть вопрос"
                            >
                                {closing ? '…' : 'Закрыть'}
                            </button>
                            <button
                                type="submit"
                                className="qst-chat-bar__send"
                                disabled={isBusy || !hasMessage}
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
