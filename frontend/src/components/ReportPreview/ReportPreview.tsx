import { useEffect, useId, useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import axiosInstance from '../../utils/axiosInstance';
import './ReportPreview.css';

type Props = { url: string; filename: string };
type Preview = { kind: 'pdf'; url: string } | { kind: 'image'; url: string } | { kind: 'docx'; html: string };

const IMAGE_TYPES: Record<string, string> = { jpg: 'image/jpeg', jpeg: 'image/jpeg', png: 'image/png', gif: 'image/gif' };

const DOCX_POLICY = "default-src 'none'; img-src data:; font-src data:; style-src 'unsafe-inline'; base-uri 'none'; form-action 'none'";

function PreviewDialog({ url, filename, onClose }: Props & { onClose: () => void }) {
    const dialog = useRef<HTMLDialogElement>(null);
    const titleId = useId();
    const [preview, setPreview] = useState<Preview | null>(null);
    const [error, setError] = useState(false);
    const [imageReady, setImageReady] = useState(false);
    const extension = filename.split('.').pop()?.toLowerCase();
    const imageType = IMAGE_TYPES[extension ?? ''];
    const supported = extension === 'pdf' || extension === 'docx' || Boolean(imageType);

    useEffect(() => {
        dialog.current?.showModal();
    }, []);

    useEffect(() => {
        if (!supported) return;
        const controller = new AbortController();
        let objectUrl: string | undefined;
        async function load() {
            try {
                const response = await axiosInstance.get<ArrayBuffer>(url, {
                    responseType: 'arraybuffer', signal: controller.signal,
                });
                if (controller.signal.aborted) return;
                if (imageType || extension === 'pdf') {
                    objectUrl = URL.createObjectURL(new Blob([response.data], { type: imageType || 'application/pdf' }));
                    setPreview({ kind: imageType ? 'image' : 'pdf', url: objectUrl });
                } else {
                    const { renderAsync } = await import('docx-preview');
                    if (controller.signal.aborted) return;
                    // Render off-document; the result is displayed only in a sandboxed frame.
                    const container = document.createElement('div');
                    await renderAsync(response.data, container, undefined, {
                        useBase64URL: true, renderAltChunks: false,
                    });
                    if (controller.signal.aborted) return;
                    setPreview({ kind: 'docx', html: `<!doctype html><html><head><meta charset="utf-8"><meta http-equiv="Content-Security-Policy" content="${DOCX_POLICY}"><style>body{margin:0} .docx-wrapper{padding:16px!important}</style></head><body>${container.innerHTML}</body></html>` });
                }
            } catch {
                if (!controller.signal.aborted) setError(true);
            }
        }
        void load();
        return () => {
            controller.abort();
            if (objectUrl) URL.revokeObjectURL(objectUrl);
        };
    }, [url, extension, supported, imageType]);

    const loading = supported && !error && (!preview || (preview.kind === 'image' && !imageReady));

    return createPortal(<dialog ref={dialog} className="report-preview-dialog" aria-labelledby={titleId} onCancel={onClose}>
        <header className="report-preview-header">
            <h2 id={titleId}>{filename}</h2>
            <button type="button" onClick={onClose} autoFocus>Закрыть</button>
        </header>
        <div className="report-preview-content" aria-busy={loading}>
            {loading && <div className="report-preview-loading" role="status">
                <span className="report-preview-spinner" aria-hidden="true" />
                <span>Загрузка {imageType ? 'изображения' : 'документа'}…</span>
            </div>}
            {!supported ? <p role="status">Предпросмотр этого формата недоступен. Скачайте файл или загрузите отчёт в формате PDF или DOCX.</p>
                : error ? <p role="alert">Не удалось открыть {imageType ? 'изображение' : 'документ'}. Закройте просмотр и попробуйте ещё раз или скачайте файл.</p>
                    : preview?.kind === 'image' ? <img className="report-preview-image" src={preview.url} alt={filename}
                        onLoad={() => setImageReady(true)} onError={() => setError(true)} />
                        : preview?.kind === 'pdf' ? <iframe className="report-preview-frame" title={`Предпросмотр: ${filename}`} src={preview.url} />
                            : preview?.kind === 'docx' ? <iframe className="report-preview-frame" title={`Предпросмотр: ${filename}`} sandbox="" srcDoc={preview.html} /> : null}
        </div>
        {preview && preview.kind !== 'image' && <p className="report-preview-hint">Если документ отображается некорректно, скачайте оригинал.</p>}
    </dialog>, document.body);
}

export function ReportPreview({ url, filename }: Props) {
    const [open, setOpen] = useState(false);
    return <>
        <button type="button" className="report-preview-button" onClick={() => setOpen(true)}>Предпросмотр</button>
        {open && <PreviewDialog url={url} filename={filename} onClose={() => setOpen(false)} />}
    </>;
}
