import axiosInstance from '../../utils/axiosInstance';

export interface UserResponse {
    id: string;
    email: string;
    fullName: string;
    role: number;
    isActive: boolean;
    createdAt?: string;
}

export interface ProfileExtras {
    phone: string;
    telegram: string;
    about: string;
    groupName: string;
    avatarUrl: string | null;
}

export interface ProfileUpdate {
    phone: string;
    telegram: string;
    about: string;
    avatarUrl: string | null;
}

export const ABOUT_MAX_LENGTH = 300;
export const AVATAR_MAX_BYTES = 5 * 1024 * 1024;
export const AVATAR_TYPES = ['image/jpeg', 'image/png', 'image/webp'];

export const fetchUser = async (userId: string): Promise<UserResponse> => {
    const res = await axiosInstance.get<UserResponse>(`/public/api/v1/user/get/${userId}`);
    return res.data;
};

const MOCK_DELAY_MS = 400;
const storageKey = (userId: string) => `profile_mock:${userId}`;

const DEFAULT_EXTRAS: ProfileExtras = {
    phone: '',
    telegram: '',
    about: '',
    groupName: 'РИ-330942',
    avatarUrl: null,
};

const delay = (ms: number) => new Promise(resolve => setTimeout(resolve, ms));

const readStored = (userId: string): ProfileExtras => {
    try {
        const raw = localStorage.getItem(storageKey(userId));
        return raw ? { ...DEFAULT_EXTRAS, ...JSON.parse(raw) } : DEFAULT_EXTRAS;
    } catch {
        return DEFAULT_EXTRAS;
    }
};

export const fetchProfileExtras = async (userId: string): Promise<ProfileExtras> => {
    await delay(MOCK_DELAY_MS);
    return readStored(userId);
};

export const updateProfileExtras = async (userId: string, update: ProfileUpdate): Promise<ProfileExtras> => {
    await delay(MOCK_DELAY_MS);
    const next = { ...readStored(userId), ...update };
    try {
        localStorage.setItem(storageKey(userId), JSON.stringify(next));
    } catch {
    }
    return next;
};

export { DEMO_EMAIL, DEMO_FALLBACK, DEMO_LEADERBOARD, DEMO_PROGRESS } from '../../utils/demoData';

export type ProfileErrors = Partial<Record<'phone' | 'telegram' | 'about' | 'avatar', string>>;

const PHONE_RE = /^\+7 ?\(?\d{3}\)? ?\d{3}-?\d{2}-?\d{2}$/;
const TELEGRAM_RE = /^@[A-Za-z0-9_]{5,32}$/;

export const validateProfile = (update: ProfileUpdate): ProfileErrors => {
    const errors: ProfileErrors = {};
    if (update.phone && !PHONE_RE.test(update.phone.trim())) {
        errors.phone = 'Формат: +7 (900) 123-45-67';
    }
    if (update.telegram && !TELEGRAM_RE.test(update.telegram.trim())) {
        errors.telegram = 'Начинается с @, от 5 символов: латиница, цифры, _';
    }
    if (update.about.length > ABOUT_MAX_LENGTH) {
        errors.about = `Не больше ${ABOUT_MAX_LENGTH} символов`;
    }
    return errors;
};

export const validateAvatar = (file: File): string | null => {
    if (!AVATAR_TYPES.includes(file.type)) return 'Подойдёт JPG, PNG или WebP';
    if (file.size > AVATAR_MAX_BYTES) return 'Файл больше 5 МБ';
    return null;
};
