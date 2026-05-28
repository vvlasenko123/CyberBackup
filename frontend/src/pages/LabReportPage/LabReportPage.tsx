import { useState, useEffect, useRef } from 'react';
import { useParams, useNavigate, useLocation } from 'react-router-dom';
import axiosInstance from '../../utils/axiosInstance';
import { Icon } from '../../shared/Icon';
import './LabReportPage.css';

const downloadFileByUrl = async (url: string, fallbackName: string) => {
    try {
        const response = await axiosInstance.get(url, { responseType: 'blob' });
        const contentDisposition = response.headers['content-disposition'] as string | undefined;
        let filename = fallbackName;
        if (contentDisposition) {
            const match = contentDisposition.match(/filename\*?=(?:UTF-8'')?["']?([^"';\n]+)/i);
            if (match) filename = decodeURIComponent(match[1]);
        }
        const blob = new Blob([response.data as BlobPart], { type: response.headers['content-type'] as string });
        const blobUrl = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = blobUrl;
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(blobUrl);
    } catch {
        // ignore
    }
};

type ReportStatus = 0 | 1 | 2 | 3 | 4;

type ReportVersion = {
    versionId: string;
    versionNumber: number;
    originalFileName: string;
    fileSizeBytes: number;
    status: ReportStatus;
    points: number | null;
    teacherComment: string | null;
    createDateUtc: string;
    checkedByTeacherFullName: string | null;
    fileDownloadUrl: string | null;
};

type MyReport = {
    reportId: string;
    status: ReportStatus;
    points: number | null;
    teacherComment: string | null;
    allowResubmit: boolean;
    versions: ReportVersion[];
};

type LocationState = {
    labTitle?: string;
};

const STATUS_META: Record<ReportStatus, { label: string; className: string }> = {
    0: { label: 'Не отправлен', className: 'lab-report-status-badge--not-submitted' },
    1: { label: 'На проверке', className: 'lab-report-status-badge--submitted' },
    2: { label: 'На проверке', className: 'lab-report-status-badge--underreview' },
    3: { label: 'Нужны правки', className: 'lab-report-status-badge--revision' },
    4: { label: 'Принята', className: 'lab-report-status-badge--accepted' },
};

const formatDate = (iso: string) => {
    const d = new Date(iso);
    return d.toLocaleDateString('ru-RU', { day: '2-digit', month: '2-digit', year: 'numeric' });
};

const CloudUploadIcon = () => (
    <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
        <polyline points="16 16 12 12 8 16" />
        <line x1="12" y1="12" x2="12" y2="21" />
        <path d="M20.39 18.39A5 5 0 0018 9h-1.26A8 8 0 103 16.3" />
    </svg>
);

const LabReportPage = () => {
    const { labId } = useParams<{ labId: string }>();
    const navigate = useNavigate();
    const location = useLocation();
    const state = location.state as LocationState | null;

    const [view, setView] = useState<'upload' | 'history'>('upload');
    const [report, setReport] = useState<MyReport | null>(null);
    const [labTitle, setLabTitle] = useState(state?.labTitle ?? '');
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const [selectedFile, setSelectedFile] = useState<File | null>(null);
    const [isDragOver, setIsDragOver] = useState(false);
    const [uploading, setUploading] = useState(false);
    const [uploadSuccess, setUploadSuccess] = useState(false);
    const fileInputRef = useRef<HTMLInputElement>(null);

    const fetchReport = async () => {
        try {
            const res = await axiosInstance.get<MyReport>(`/public/api/v1/laboratories/${labId}/reports/my`);
            setReport(res.data);
        } catch {
            // 404 = отчёта ещё нет, 500 = бэкенд не нашёл — в обоих случаях просто показываем форму загрузки
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        const fetchLabTitle = async () => {
            if (labTitle) return;
            try {
                const res = await axiosInstance.get<{ title: string }>(`/public/api/v1/laboratories/${labId}`);
                setLabTitle(res.data.title);
            } catch {
                // ignore
            }
        };

        fetchReport();
        fetchLabTitle();
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [labId]);

    const handleFileSelect = (file: File) => {
        setSelectedFile(file);
        setUploadSuccess(false);
    };

    const handleDrop = (e: React.DragEvent) => {
        e.preventDefault();
        setIsDragOver(false);
        const file = e.dataTransfer.files[0];
        if (file) handleFileSelect(file);
    };

    const handleUpload = async () => {
        if (!selectedFile || uploading) return;
        setUploading(true);
        setError(null);
        try {
            const formData = new FormData();
            formData.append('file', selectedFile);
            await axiosInstance.post(
                `/public/api/v1/laboratories/${labId}/reports`,
                formData,
                { headers: { 'Content-Type': 'multipart/form-data' } }
            );
            setSelectedFile(null);
            setUploadSuccess(true);
            await fetchReport();
            setView('history');
        } catch {
            setError('Не удалось загрузить отчёт. Попробуйте ещё раз.');
        } finally {
            setUploading(false);
        }
    };

    const latestRevisionVersion = report?.versions
        ?.filter(v => v.status === 3)
        .sort((a, b) => b.versionNumber - a.versionNumber)[0] ?? null;

    const canUpload = !report || report.allowResubmit;

    if (loading) return <div className="lab-report-loading">Загрузка...</div>;

    return (
        <div className="lab-report-page">
            <div className="lab-report-breadcrumb">
                <button className="lab-report-breadcrumb-link" onClick={() => navigate('/labs')}>
                    Лабораторные
                </button>
                <span className="lab-report-breadcrumb-sep">/</span>
                <button className="lab-report-breadcrumb-link" onClick={() => navigate(`/labs/${labId}`)}>
                    {labTitle || 'Лабораторная'}
                </button>
                <span className="lab-report-breadcrumb-sep">/</span>
                <span className="lab-report-breadcrumb-current">Отчёт</span>
            </div>

            <div className="lab-report-header">
                <h2 className="lab-report-title">Отчёт: {labTitle}</h2>
                <div className="lab-report-tabs">
                    <button
                        className={`lab-report-tab ${view === 'upload' ? 'lab-report-tab--active' : 'lab-report-tab--inactive'}`}
                        onClick={() => setView('upload')}
                    >
                        Загрузить
                    </button>
                    <button
                        className={`lab-report-tab ${view === 'history' ? 'lab-report-tab--active' : 'lab-report-tab--inactive'}`}
                        onClick={() => setView('history')}
                    >
                        История версий
                    </button>
                </div>
            </div>

            {error && <div className="lab-report-error">{error}</div>}

            {view === 'upload' && (
                <div className="lab-report-upload">
                    {latestRevisionVersion && (
                        <div className="lab-report-revision-card">
                            <div className="lab-report-revision-header">
                                Версия {latestRevisionVersion.versionNumber} · Нужны правки
                            </div>
                            <div className="lab-report-revision-body">
                                <div className="lab-report-revision-avatar">
                                    {latestRevisionVersion.checkedByTeacherFullName?.[0]?.toUpperCase() ?? '?'}
                                </div>
                                <div>
                                    <div className="lab-report-revision-comment-label">
                                        Комментарий преподавателя
                                    </div>
                                    <p className="lab-report-revision-comment-text">
                                        {latestRevisionVersion.teacherComment ?? '—'}
                                    </p>
                                </div>
                            </div>
                        </div>
                    )}

                    {canUpload ? (
                        <>
                            <p className="lab-report-dropzone-label">Загрузить новую версию</p>
                            <div
                                className={[
                                    'lab-report-dropzone',
                                    isDragOver ? 'lab-report-dropzone--over' : '',
                                    selectedFile ? 'lab-report-dropzone--has-file' : '',
                                ].filter(Boolean).join(' ')}
                                onDragOver={e => { e.preventDefault(); setIsDragOver(true); }}
                                onDragLeave={() => setIsDragOver(false)}
                                onDrop={handleDrop}
                                onClick={() => fileInputRef.current?.click()}
                            >
                                <input
                                    ref={fileInputRef}
                                    className="lab-report-dropzone-input"
                                    type="file"
                                    accept=".pdf,.docx,.doc"
                                    onChange={e => {
                                        const file = e.target.files?.[0];
                                        if (file) handleFileSelect(file);
                                    }}
                                    onClick={e => e.stopPropagation()}
                                />
                                <div className="lab-report-dropzone-icon">
                                    {selectedFile
                                        ? <Icon name="check" size={40} color="#22C55E" />
                                        : <CloudUploadIcon />}
                                </div>
                                {selectedFile ? (
                                    <p className="lab-report-dropzone-filename">{selectedFile.name}</p>
                                ) : (
                                    <>
                                        <p className="lab-report-dropzone-text">Перетащи файл или выбери файл</p>
                                        <p className="lab-report-dropzone-hint">PDF, DOCX · не более 10 МБ</p>
                                    </>
                                )}
                            </div>

                            <button
                                className="lab-report-submit"
                                onClick={handleUpload}
                                disabled={!selectedFile || uploading}
                            >
                                {uploading ? 'Отправка...' : 'Отправить отчет'}
                            </button>
                        </>
                    ) : (
                        <div className="lab-report-history-empty">
                            Повторная загрузка отчёта сейчас недоступна
                        </div>
                    )}

                    {uploadSuccess && (
                        <div style={{ color: '#22C55E', fontSize: 14 }}>Отчёт успешно отправлен!</div>
                    )}
                </div>
            )}

            {view === 'history' && (
                <div className="lab-report-history">
                    {!report || !report.versions || report.versions.length === 0 ? (
                        <div className="lab-report-history-empty">Отчёты ещё не загружались</div>
                    ) : (
                        <table className="lab-report-history-table">
                            <thead>
                                <tr>
                                    <th>Версия</th>
                                    <th>Дата</th>
                                    <th>Статус</th>
                                    <th></th>
                                </tr>
                            </thead>
                            <tbody>
                                {report.versions
                                    .sort((a, b) => b.versionNumber - a.versionNumber)
                                    .map(version => {
                                        const statusMeta = STATUS_META[version.status];
                                        return (
                                            <tr key={version.versionId}>
                                                <td className="lab-report-history-version">
                                                    v{version.versionNumber}
                                                </td>
                                                <td>{formatDate(version.createDateUtc)}</td>
                                                <td>
                                                    <span className={`lab-report-status-badge ${statusMeta.className}`}>
                                                        {statusMeta.label}
                                                    </span>
                                                </td>
                                                <td>
                                                    {version.fileDownloadUrl && (
                                                        <button
                                                            className="lab-report-download-btn"
                                                            onClick={() => downloadFileByUrl(version.fileDownloadUrl!, version.originalFileName)}
                                                        >
                                                            Скачать
                                                        </button>
                                                    )}
                                                </td>
                                            </tr>
                                        );
                                    })}
                            </tbody>
                        </table>
                    )}
                </div>
            )}
        </div>
    );
};

export default LabReportPage;
