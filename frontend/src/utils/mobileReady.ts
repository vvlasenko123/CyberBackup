export const MOBILE_READY_PATHS = ['/', '/login', '/logout', '/change-password', '/dashboard', '/profile', '/statement', '/progress', '/questions'];

export const isMobileReady = (pathname: string): boolean =>
    MOBILE_READY_PATHS.includes(pathname) || pathname.startsWith('/questions/');
