import type { CSSProperties, ReactNode } from 'react';
import './Skeleton.css';

export function Skeleton({ width = '100%', height = 16, className = '' }: {
    width?: CSSProperties['width'];
    height?: CSSProperties['height'];
    className?: string;
}) {
    return <span aria-hidden="true" className={`skeleton ${className}`} style={{ width, height }} />;
}

export function SkeletonRegion({ children, className = '', label = 'Загрузка...' }: {
    children: ReactNode;
    className?: string;
    label?: string;
}) {
    return (
        <div className={`skeleton-region ${className}`} role="status" aria-label={label} aria-busy="true">
            <span className="skeleton-sr-only">{label}</span>
            <div aria-hidden="true">{children}</div>
        </div>
    );
}

export function SkeletonLines({ count = 3 }: { count?: number }) {
    return <div className="skeleton-stack">{Array.from({ length: count }, (_, i) => (
        <Skeleton key={i} width={i === count - 1 ? '65%' : '100%'} />
    ))}</div>;
}

export function ListSkeleton({ rows = 5, variant = 'card', className = '', itemClassName = '', listClassName = '' }: {
    rows?: number;
    variant?: 'card' | 'row' | 'post';
    className?: string;
    itemClassName?: string;
    listClassName?: string;
}) {
    return <SkeletonRegion className={className}>
        <div className={listClassName || `skeleton-list skeleton-list--${variant}`}>
            {Array.from({ length: rows }, (_, i) => (
                <div key={i} className={`skeleton-list-item ${itemClassName || `skeleton-list-item--${variant}`}`}>
                    <div className={variant === 'row' ? 'skeleton-row-content' : 'skeleton-stack'}>
                    {variant === 'row' ? <>
                        <Skeleton width={24} height={24} />
                        <Skeleton width={`${60 - i % 3 * 8}%`} height={14} />
                        <Skeleton width={32} height={14} />
                    </> : <>
                        <Skeleton width={`${65 - i % 3 * 8}%`} height={20} />
                        {variant === 'post' && <SkeletonLines count={2} />}
                        <Skeleton width="35%" height={14} />
                    </>}
                    </div>
                </div>
            ))}
        </div>
    </SkeletonRegion>;
}

export function TableSkeleton({ rows = 6, columns = 4, tableClassName = '', cardClassName = '' }: {
    rows?: number;
    columns?: number;
    tableClassName?: string;
    cardClassName?: string;
}) {
    const cells = (header: boolean) => Array.from({ length: columns }, (_, col) => {
        const Cell = header ? 'th' : 'td';
        return <Cell key={col}><Skeleton height={header ? 12 : 16} width={col === 0 ? '75%' : '60%'} /></Cell>;
    });
    return <SkeletonRegion>
        <div className={cardClassName || 'skeleton-panel'}>
            <table className={tableClassName || 'skeleton-table'}>
                <thead><tr>{cells(true)}</tr></thead>
                <tbody>{Array.from({ length: rows }, (_, row) => <tr key={row}>{cells(false)}</tr>)}</tbody>
            </table>
        </div>
    </SkeletonRegion>;
}

export function StatsSkeleton({ prefix }: { prefix: 'prog' | 'stmt' }) {
    return <SkeletonRegion className={`${prefix}-stats`}>
        <div className="skeleton-stats">
            {Array.from({ length: 3 }, (_, i) => <div className={`${prefix}-stat-card`} key={i}>
                <div className={`${prefix}-stat-card__label`}><Skeleton width="60%" height={16} /></div>
                <Skeleton width={80} height={35} />
            </div>)}
        </div>
    </SkeletonRegion>;
}

export function DetailSkeleton({ className = '', layoutClassName = '', sidebarClassName = '', panelClassName = 'skeleton-panel', sidebarPanelClassName = 'skeleton-panel', form = false }: {
    className?: string;
    layoutClassName?: string;
    sidebarClassName?: string;
    panelClassName?: string;
    sidebarPanelClassName?: string;
    form?: boolean;
}) {
    return <div className={className}>
        <SkeletonRegion><Skeleton width="35%" height={20} /></SkeletonRegion>
        <div className={layoutClassName}>
            <SkeletonRegion className="skeleton-detail-main">
                <div className={panelClassName}><div className="skeleton-stack">
                    <Skeleton width="55%" height={28} />
                    {form ? Array.from({ length: 3 }, (_, i) => <div className="skeleton-stack" key={i}>
                        <Skeleton width={120} height={14} /><Skeleton height={42} />
                    </div>) : <SkeletonLines count={5} />}
                    <Skeleton height={form ? 160 : 100} />
                </div></div>
            </SkeletonRegion>
            {sidebarClassName && <div className={sidebarClassName}>
                <SkeletonRegion className={sidebarPanelClassName}><SkeletonLines count={6} /></SkeletonRegion>
            </div>}
        </div>
    </div>;
}

export function ProgressSkeleton() {
    return <>
        <StatsSkeleton prefix="prog" />
        <SkeletonRegion className="prog-overall-card"><SkeletonLines /></SkeletonRegion>
        <div className="prog-columns">
            <div className="prog-labs-card">
                <h3 className="prog-labs-card__title">Прогресс по лабораторным работам</h3>
                <SkeletonRegion>{Array.from({ length: 5 }, (_, i) => <div className="prog-lab-item" key={i}>
                    <div className="prog-lab-item__header"><Skeleton width="60%" /><Skeleton width={40} /></div>
                    <Skeleton height={6} />
                </div>)}</SkeletonRegion>
            </div>
            <div className="prog-leaderboard-card">
                <h3 className="prog-leaderboard-card__title">Рейтинг группы</h3>
                <ListSkeleton variant="row" />
            </div>
        </div>
    </>;
}
