import { useEffect, useState } from 'react';

const COARSE_POINTER_QUERY = '(pointer: coarse)';

const evaluateIsDesktop = (): boolean => {
    if (typeof window === 'undefined' || typeof window.matchMedia !== 'function') {
        return true;
    }

    return !window.matchMedia(COARSE_POINTER_QUERY).matches;
};

export const useIsDesktop = (): boolean => {
    const [isDesktop, setIsDesktop] = useState<boolean>(evaluateIsDesktop);

    useEffect(() => {
        const pointerQuery = window.matchMedia(COARSE_POINTER_QUERY);
        const update = () => setIsDesktop(evaluateIsDesktop());

        pointerQuery.addEventListener('change', update);

        update();

        return () => {
            pointerQuery.removeEventListener('change', update);
        };
    }, []);

    return isDesktop;
};
