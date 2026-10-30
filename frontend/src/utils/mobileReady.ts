export const MOBILE_READY_PATHS = ['/', '/login', '/logout', '/change-password', '/dashboard', '/profile'];

export const isMobileReady = (pathname: string): boolean => MOBILE_READY_PATHS.includes(pathname);
