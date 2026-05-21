import React from 'react';
import HomeIcon from '../components/icons/home-icon';
import LogoutIcon from '../components/icons/logout-icon';
import FilledBellIcon from '../components/icons/filled-bell-icon';
import UsersIcon from '../components/icons/users-icon';
import BulbSvg from '../components/icons/bulb-svg';
import ChartLineIcon from '../components/icons/chart-line-icon';
import MessageCircleIcon from '../components/icons/message-circle-icon';
import ClockIcon from '../components/icons/clock-icon';
import FileDescriptionIcon from '../components/icons/file-description-icon';

export type IconName =
    | 'home'
    | 'flask'
    | 'trending'
    | 'file'
    | 'table'
    | 'inbox'
    | 'users'
    | 'bell'
    | 'logout'
    | 'flag'
    | 'lightbulb'
    | 'upload'
    | 'download'
    | 'check'
    | 'x'
    | 'search'
    | 'chevronRight'
    | 'chevronLeft'
    | 'chevronDown'
    | 'shield'
    | 'newspaper'
    | 'user'
    | 'settings'
    | 'plus'
    | 'messageSquare'
    | 'calendar'
    | 'edit'
    | 'trash'
    | 'eye'
    | 'eyeOff'
    | 'copy'
    | 'checkCircle'
    | 'alertCircle'
    | 'clock'
    | 'filter';

interface IconProps {
    name: IconName;
    size?: number;
    color?: string;
    strokeWidth?: number;
}

// Animated itshover icons — rendered directly with the same size/color props
const ANIMATED_ICONS: Partial<Record<IconName, React.FC<{ size?: number; color?: string; strokeWidth?: number }>>> = {
    home: HomeIcon,
    logout: LogoutIcon,
    bell: FilledBellIcon,
    users: UsersIcon,
    lightbulb: BulbSvg,
    trending: ChartLineIcon,
    messageSquare: MessageCircleIcon,
    calendar: ClockIcon,
    table: FileDescriptionIcon,
};

export const Icon: React.FC<IconProps> = ({
    name,
    size = 16,
    color = 'currentColor',
    strokeWidth = 1.5,
}) => {
    const AnimatedIcon = ANIMATED_ICONS[name];
    if (AnimatedIcon) {
        return <AnimatedIcon size={size} color={color} strokeWidth={strokeWidth} />;
    }

    // Fallback SVG for icons not replaced yet
    const paths: Partial<Record<IconName, React.ReactNode>> = {
        flask: (
            <>
                <path d="M9 3h6" />
                <path d="M11 3v6l-4 9h10l-4-9V3" />
                <circle cx="12" cy="17" r="1" />
            </>
        ),
        file: (
            <>
                <path d="M14 2H6a2 2 0 00-2 2v16a2 2 0 002 2h12a2 2 0 002-2V8z" />
                <polyline points="14 2 14 8 20 8" />
            </>
        ),
        inbox: (
            <>
                <polyline points="22 12 16 12 14 15 10 15 8 12 2 12" />
                <path d="M5.45 5.11L2 12v6a2 2 0 002 2h16a2 2 0 002-2v-6l-3.45-6.89A2 2 0 0016.76 4H7.24a2 2 0 00-1.79 1.11z" />
            </>
        ),
        flag: (
            <>
                <path d="M4 15s1-1 4-1 5 2 8 2 4-1 4-1V3s-1 1-4 1-5-2-8-2-4 1-4 1z" />
                <line x1="4" y1="22" x2="4" y2="15" />
            </>
        ),
        upload: (<><polyline points="16 16 12 12 8 16" /><line x1="12" y1="12" x2="12" y2="21" /></>),
        download: (<><polyline points="8 17 12 21 16 17" /><line x1="12" y1="12" x2="12" y2="21" /></>),
        check: (<polyline points="20 6 9 17 4 12" />),
        x: (<><line x1="18" y1="6" x2="6" y2="18" /><line x1="6" y1="6" x2="18" y2="18" /></>),
        search: (<><circle cx="11" cy="11" r="8" /><line x1="21" y1="21" x2="16.65" y2="16.65" /></>),
        chevronRight: (<polyline points="9 18 15 12 9 6" />),
        chevronLeft: (<polyline points="15 18 9 12 15 6" />),
        chevronDown: (<polyline points="6 9 12 15 18 9" />),
        shield: (<path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z" />),
        newspaper: (<path d="M4 22h16a2 2 0 002-2V4a2 2 0 00-2-2H8a2 2 0 00-2 2v16a4 4 0 01-4-4V6" />),
        user: (<><path d="M20 21v-2a4 4 0 00-4-4H8a4 4 0 00-4 4v2" /><circle cx="12" cy="7" r="4" /></>),
        settings: (<circle cx="12" cy="12" r="3" />),
        plus: (<><line x1="12" y1="5" x2="12" y2="19" /><line x1="5" y1="12" x2="19" y2="12" /></>),
        edit: (<path d="M11 4H4a2 2 0 00-2 2v14a2 2 0 002 2h14a2 2 0 002-2v-7" />),
        trash: (<polyline points="3 6 5 6 21 6" />),
        eye: (<path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" />),
        eyeOff: (<line x1="1" y1="1" x2="23" y2="23" />),
        copy: (<rect x="9" y="9" width="13" height="13" rx="2" />),
        checkCircle: (<polyline points="22 4 12 14.01 9 11.01" />),
        alertCircle: (<circle cx="12" cy="12" r="10" />),
        clock: (<><circle cx="12" cy="12" r="10" /><polyline points="12 6 12 12 16 14" /></>),
        filter: (<polygon points="22 3 2 3 10 12.46 10 19 14 21 14 12.46 22 3" />),
    };

    const pathContent = paths[name];
    if (!pathContent) return null;

    return (
        <svg
            width={size}
            height={size}
            viewBox="0 0 24 24"
            fill="none"
            stroke={color}
            strokeWidth={strokeWidth}
            strokeLinecap="round"
            strokeLinejoin="round"
            style={{ display: 'inline-block', flexShrink: 0 }}
        >
            {pathContent}
        </svg>
    );
};

export default Icon;
