import React from 'react';
import './DesktopOnlyScreen.css';

const MonitorIcon: React.FC = () => (
    <svg
        width="40"
        height="40"
        viewBox="0 0 24 24"
        fill="none"
        stroke="currentColor"
        strokeWidth="1.6"
        strokeLinecap="round"
        strokeLinejoin="round"
        aria-hidden="true"
    >
        <rect x="2" y="3" width="20" height="14" rx="2" />
        <path d="M8 21h8" />
        <path d="M12 17v4" />
    </svg>
);

const DesktopOnlyScreen: React.FC = () => (
    <div className="desktop-only">
        <div className="desktop-only__card">
            <div className="desktop-only__hero animated-gradient">
                <div className="desktop-only__logo">
                    Neo<span>Lab</span>
                </div>
                <div className="desktop-only__icon">
                    <MonitorIcon />
                </div>
            </div>

            <div className="desktop-only__body">
                <p className="desktop-only__kicker">Образовательная платформа по кибербезопасности</p>
                <h1 className="desktop-only__title">NeoLab пока работает только на десктопе</h1>
                <p className="desktop-only__subtitle">
                    Откройте платформу с ноутбука или компьютера — мобильная версия скоро появится.
                </p>
            </div>
        </div>
    </div>
);

export default DesktopOnlyScreen;
