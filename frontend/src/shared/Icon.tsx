import React from 'react';

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

export const Icon: React.FC<IconProps> = ({
    name,
    size = 16,
    color = 'currentColor',
    strokeWidth = 1.5,
}) => {
    const paths: Record<IconName, React.ReactNode> = {
        home: (
            <>
                <path d="M3 9l9-7 9 7v11a2 2 0 01-2 2H5a2 2 0 01-2-2z" />
                <polyline points="9 22 9 12 15 12 15 22" />
            </>
        ),

        flask: (
            <>
                <path d="M9 3h6" />
                <path d="M11 3v6l-4 9h10l-4-9V3" />
                <circle cx="12" cy="17" r="1" />
            </>
        ),

        trending: (
            <>
                <polyline points="22 7 13.5 15.5 8.5 10.5 2 17" />
                <polyline points="16 7 22 7 22 13" />
            </>
        ),

        file: (
            <>
                <path d="M14 2H6a2 2 0 00-2 2v16a2 2 0 002 2h12a2 2 0 002-2V8z" />
                <polyline points="14 2 14 8 20 8" />
            </>
        ),

        table: (
            <>
                <rect x="3" y="3" width="18" height="18" rx="2" />
                <path d="M3 9h18M3 15h18M9 3v18" />
            </>
        ),

        inbox: (
            <>
                <polyline points="22 12 16 12 14 15 10 15 8 12 2 12" />
                <path d="M5.45 5.11L2 12v6a2 2 0 002 2h16a2 2 0 002-2v-6l-3.45-6.89A2 2 0 0016.76 4H7.24a2 2 0 00-1.79 1.11z" />
            </>
        ),

        users: (
            <>
                <path d="M17 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2" />
                <circle cx="9" cy="7" r="4" />
                <path d="M23 21v-2a4 4 0 00-3-3.87" />
                <path d="M16 3.13a4 4 0 010 7.75" />
            </>
        ),

        bell: (
            <>
                <path d="M18 8a6 6 0 00-12 0c0 7-3 9-3 9h18s-3-2-3-9" />
                <path d="M13.73 21a2 2 0 01-3.46 0" />
            </>
        ),

        logout: (
            <>
                <path d="M9 21H5a2 2 0 01-2-2V5a2 2 0 012-2h4" />
                <polyline points="16 17 21 12 16 7" />
                <line x1="21" y1="12" x2="9" y2="12" />
            </>
        ),

        flag: (
            <>
                <path d="M4 15s1-1 4-1 5 2 8 2 4-1 4-1V3s-1 1-4 1-5-2-8-2-4 1-4 1z" />
                <line x1="4" y1="22" x2="4" y2="15" />
            </>
        ),

        lightbulb: (
            <>
                <path d="M9 18h6" />
                <path d="M10 22h4" />
                <path d="M12 2a7 7 0 017 7c0 2.38-1.19 4.47-3 5.74V17a2 2 0 01-2 2h-4a2 2 0 01-2-2v-2.26C6.19 13.47 5 11.38 5 9a7 7 0 017-7z" />
            </>
        ),

        upload: (
            <>
                <polyline points="16 16 12 12 8 16" />
                <line x1="12" y1="12" x2="12" y2="21" />
            </>
        ),

        download: (
            <>
                <polyline points="8 17 12 21 16 17" />
                <line x1="12" y1="12" x2="12" y2="21" />
            </>
        ),

        check: (
            <>
                <polyline points="20 6 9 17 4 12" />
            </>
        ),

        x: (
            <>
                <line x1="18" y1="6" x2="6" y2="18" />
                <line x1="6" y1="6" x2="18" y2="18" />
            </>
        ),

        search: (
            <>
                <circle cx="11" cy="11" r="8" />
                <line x1="21" y1="21" x2="16.65" y2="16.65" />
            </>
        ),

        chevronRight: (
            <>
                <polyline points="9 18 15 12 9 6" />
            </>
        ),

        chevronLeft: (
            <>
                <polyline points="15 18 9 12 15 6" />
            </>
        ),

        chevronDown: (
            <>
                <polyline points="6 9 12 15 18 9" />
            </>
        ),

        shield: (
            <>
                <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z" />
            </>
        ),

        newspaper: (
            <>
                <path d="M4 22h16a2 2 0 002-2V4a2 2 0 00-2-2H8a2 2 0 00-2 2v16a4 4 0 01-4-4V6" />
            </>
        ),

        user: (
            <>
                <path d="M20 21v-2a4 4 0 00-4-4H8a4 4 0 00-4 4v2" />
                <circle cx="12" cy="7" r="4" />
            </>
        ),

        settings: (
            <>
                <circle cx="12" cy="12" r="3" />
            </>
        ),

        plus: (
            <>
                <line x1="12" y1="5" x2="12" y2="19" />
                <line x1="5" y1="12" x2="19" y2="12" />
            </>
        ),

        messageSquare: (
            <>
                <path d="M21 15a2 2 0 01-2 2H7l-4 4V5a2 2 0 012-2h14a2 2 0 012 2z" />
            </>
        ),

        calendar: (
            <>
                <rect x="3" y="4" width="18" height="18" rx="2" />
            </>
        ),

        edit: (
            <>
                <path d="M11 4H4a2 2 0 00-2 2v14a2 2 0 002 2h14a2 2 0 002-2v-7" />
            </>
        ),

        trash: (
            <>
                <polyline points="3 6 5 6 21 6" />
            </>
        ),

        eye: (
            <>
                <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" />
            </>
        ),

        eyeOff: (
            <>
                <line x1="1" y1="1" x2="23" y2="23" />
            </>
        ),

        copy: (
            <>
                <rect x="9" y="9" width="13" height="13" rx="2" />
            </>
        ),

        checkCircle: (
            <>
                <polyline points="22 4 12 14.01 9 11.01" />
            </>
        ),

        alertCircle: (
            <>
                <circle cx="12" cy="12" r="10" />
            </>
        ),

        clock: (
            <>
                <circle cx="12" cy="12" r="10" />
            </>
        ),

        filter: (
            <>
                <polygon points="22 3 2 3 10 12.46 10 19 14 21 14 12.46 22 3" />
            </>
        ),
    };

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
            style={{
                display: 'inline-block',
                flexShrink: 0,
            }}
        >
            {paths[name]}
        </svg>
    );
};

export default Icon;