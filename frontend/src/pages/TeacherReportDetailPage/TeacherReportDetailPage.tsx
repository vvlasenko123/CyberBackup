import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import axiosInstance from '../../utils/axiosInstance';
import './TeacherReportDetailPage.css';

type ReportStatus = 0 | 1 | 2 | 3 | 4;

type ReportVersion = {
    versionId: string;
    versionNumber: number;
    originalFileName: string;
    fileSizeBytes: number;
    contentType: string | null;
    status: ReportStatus;
    points: number | null;
    teacherComment: string | null;
    createDateUtc: string;
    checkedByTeacherFullName: string | null;
    checkedDateUtc: string | null;
    fileDownloadUrl: string | null;
};

type ReportDetail = {
    reportId: string;
    laboratory: { id: string; title: string; maxPoints: number };
    student: { id: string; fullName: string; groupName: string | null };
    status: ReportStatus;
    points: number | null;
    teacherComment: string | null;
    allowResubmit: boolean;
    versions: ReportVersion[];
};

const STATUS_META: Record<ReportStatus, { label: string; className: string }> = {
    0: { label: 'Не отправлен', className: 'trp-status--notsubmitted' },
    1: { label: 'На проверке',  className: 'trp-status--submitted' },
    2: { label: 'На проверке',  className: 'trp-status--underreview' },
    3: { label: 'Нужны правки', className: 'trp-status--revision' },
    4: { label: 'Принята',      className: 'trp-status--accepted' },
};

const formatDate = (iso: string) => new Date(iso).toLocaleDateString('ru-RU', {
    day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit',
});

const formatBytes = (bytes: number) => {
    if (bytes < 1024) return `${bytes} Б`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} КБ`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} МБ`;
};

const TeacherReportDetailPage = () => {
    const { reportId } = useParams<{ reportId: string }>();
    const navigate = useNavigate();

    const [report, setReport] = useState<ReportDetail | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const [reviewStatus, setReviewStatus] = useState<'3' | '4'>('4');
    const [reviewPoints, setReviewPoints] = useState<string>('');
    const [reviewComment, setReviewComment] = useState('');
    const [allowResubmit, setAllowResubmit] = useState(false);
    const [submitting, setSubmitting] = useState(false);
    const [reviewSuccess, setReviewSuccess] = useState(false);

    const fetchReport = async () => {
        try {
            const res = await axiosInstance.get<ReportDetail>(`/public/api/v1/teacher/reports/${reportId}`);
            setReport(res.data);
            if (res.data.teacherComment) setReviewComment(res.data.teacherComment);
            if (res.data.points !== null) setReviewPoints(String(res.data.points));
        } catch {
            setError('Не удалось загрузить отчёт');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchReport();
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [reportId]);

    const handleReview = async () => {
        if (submitting) return;
        if (reviewStatus === '4' && reviewPoints === '') {
            setError('Укажите баллы для принятия отчёта');
            return;
        }
        setSubmitting(true);
        setError(null);
        try {
            await axiosInstance.post(`/public/api/v1/teacher/reports/${reportId}/review`, {
                status: Number(reviewStatus),
                points: reviewStatus === '4' && reviewPoints !== '' ? Number(reviewPoints) : null,
                comment: reviewComment.trim() || null,
                allowResubmit,
            });
            setReviewSuccess(true);
            await fetchReport();
        } catch {
            setError('Не удалось сохранить проверку. Попробуйте ещё раз.');
        } finally {
            setSubmitting(false);
        }
    };

    const handleDownload = async (version: ReportVersion) => {
        const url = version.fileDownloadUrl
            ?? `/public/api/v1/teacher/reports/${reportId}/versions/${version.versionId}/file`;
        try {
            const response = await axiosInstance.get(url, { responseType: 'blob' });
            const blob = new Blob([response.data as BlobPart], { type: response.headers['content-type'] as string });
            const blobUrl = window.URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = blobUrl;
            link.download = version.originalFileName || 'report';
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            window.URL.revokeObjectURL(blobUrl);
        } catch {
            // ignore
        }
    };

    if (loading) return <div className="trp-loading">Загрузка...</div>;
    if (!report) return <div className="trp-loading">{error ?? 'Отчёт не найден'}</div>;

    const latestVersion = report.versions
        .slice()
        .sort((a, b) => b.versionNumber - a.versionNumber)[0];

    const currentMeta = STATUS_META[report.status];
    const maxPts = report.laboratory.maxPoints;

    return (
        <div className="trp-page">
            <div className="trp-breadcrumb">
                <button className="trp-breadcrumb-link" onClick={() => navigate('/labs')}>
                    Отчёты студентов
                </button>
                <span className="trp-breadcrumb-sep">/</span>
                <span className="trp-breadcrumb-current">{report.student.fullName}</span>
            </div>

            <div className="trp-header">
                <div className="trp-header-info">
                    <h2 className="trp-title">{report.laboratory.title}</h2>
                    <div className="trp-student-row">
                        <span className="trp-student-name">{report.student.fullName}</span>
                        {report.student.groupName && (
                            <span className="trp-student-group">{report.student.groupName}</span>
                        )}
                        <span className={`trp-status-badge ${currentMeta.className}`}>
                            {currentMeta.label}
                        </span>
                    </div>
                </div>
                {report.points !== null && (
                    <div className="trp-points-display">
                        <span className="trp-points-value">{report.points}</span>
                        <span className="trp-points-max">/ {maxPts}</span>
                    </div>
                )}
            </div>

            <div className="trp-layout">
                <div className="trp-versions">
                    <h3 className="trp-section-title">Версии отчёта</h3>
                    {report.versions.length === 0 ? (
                        <div className="trp-empty">Версии не найдены</div>
                    ) : (
                        <div className="trp-versions-list">
                            {report.versions
                                .sort((a, b) => b.versionNumber - a.versionNumber)
                                .map(version => {
                                    const vm = STATUS_META[version.status];
                                    const isLatest = latestVersion?.versionId === version.versionId;
                                    return (
                                        <div key={version.versionId} className={`trp-version-card ${isLatest ? 'trp-version-card--latest' : ''}`}>
                                            <div className="trp-version-header">
                                                <span className="trp-version-num">
                                                    v{version.versionNumber}
                                                    {isLatest && <span className="trp-version-latest-tag">последняя</span>}
                                                </span>
                                                <span className={`trp-status-badge ${vm.className}`}>{vm.label}</span>
                                            </div>
                                            <div className="trp-version-meta">
                                                <span className="trp-version-filename">{version.originalFileName}</span>
                                                <span className="trp-version-size">{formatBytes(version.fileSizeBytes)}</span>
                                            </div>
                                            <div className="trp-version-date">
                                                Загружено: {formatDate(version.createDateUtc)}
                                            </div>
                                            {version.checkedByTeacherFullName && version.checkedDateUtc && (
                                                <div className="trp-version-checked">
                                                    Проверено: {version.checkedByTeacherFullName} · {formatDate(version.checkedDateUtc)}
                                                </div>
                                            )}
                                            {version.teacherComment && (
                                                <div className="trp-version-comment">
                                                    <span className="trp-version-comment-label">Комментарий:</span>
                                                    <p className="trp-version-comment-text">{version.teacherComment}</p>
                                                </div>
                                            )}
                                            <button
                                                className="trp-download-btn"
                                                onClick={() => handleDownload(version)}
                                            >
                                                Скачать
                                            </button>
                                        </div>
                                    );
                                })}
                        </div>
                    )}
                </div>

                <div className="trp-review">
                    <h3 className="trp-section-title">Проверка</h3>
                    <div className="trp-review-card">
                        {reviewSuccess && (
                            <div className="trp-review-success">Проверка сохранена</div>
                        )}

                        <div className="trp-review-field">
                            <label className="trp-review-label">Решение</label>
                            <div className="trp-review-radios">
                                <label className={`trp-review-radio ${reviewStatus === '4' ? 'trp-review-radio--active' : ''}`}>
                                    <input
                                        type="radio"
                                        name="reviewStatus"
                                        value="4"
                                        checked={reviewStatus === '4'}
                                        onChange={() => { setReviewStatus('4'); setAllowResubmit(false); }}
                                    />
                                    Принять
                                </label>
                                <label className={`trp-review-radio trp-review-radio--revision ${reviewStatus === '3' ? 'trp-review-radio--active-revision' : ''}`}>
                                    <input
                                        type="radio"
                                        name="reviewStatus"
                                        value="3"
                                        checked={reviewStatus === '3'}
                                        onChange={() => setReviewStatus('3')}
                                    />
                                    Нужны правки
                                </label>
                            </div>
                        </div>

                        {reviewStatus === '4' && (
                            <div className="trp-review-field">
                                <label className="trp-review-label">
                                    Баллы <span className="trp-review-label--muted">(макс. {maxPts})</span>
                                </label>
                                <input
                                    className="trp-review-input"
                                    type="number"
                                    min={0}
                                    max={maxPts}
                                    placeholder={`0 – ${maxPts}`}
                                    value={reviewPoints}
                                    onChange={e => setReviewPoints(e.target.value)}
                                />
                            </div>
                        )}

                        <div className="trp-review-field">
                            <label className="trp-review-label">Комментарий</label>
                            <textarea
                                className="trp-review-textarea"
                                rows={4}
                                placeholder="Необязательный комментарий для студента..."
                                value={reviewComment}
                                onChange={e => setReviewComment(e.target.value)}
                            />
                        </div>

                        {reviewStatus === '3' && (
                            <label className="trp-review-checkbox-row">
                                <input
                                    type="checkbox"
                                    checked={allowResubmit}
                                    onChange={e => setAllowResubmit(e.target.checked)}
                                />
                                <span>Разрешить повторную загрузку</span>
                            </label>
                        )}

                        {error && <div className="trp-review-error">{error}</div>}

                        <button
                            className={`trp-review-submit ${reviewStatus === '3' ? 'trp-review-submit--revision' : ''}`}
                            onClick={handleReview}
                            disabled={submitting}
                        >
                            {submitting ? 'Сохранение...' : reviewStatus === '4' ? 'Принять отчёт' : 'Запросить правки'}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default TeacherReportDetailPage;
