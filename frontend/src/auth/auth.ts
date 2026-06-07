const API_BASE = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000';
const API_URL = `${API_BASE}/public/auth`;

export interface LoginResponse {
    accessToken: string;
    expiresAt: string;
}

const parseJwt = (token: string) => {
    try {
        return JSON.parse(atob(token.split('.')[1]));
    } catch {
        return null;
    }
};

const fetchFullName = async (userId: string, token: string): Promise<string | null> => {
    if (!userId) return null;
    try {
        const response = await fetch(`${API_BASE}/public/api/v1/user/get/${userId}`, {
            headers: { 'Authorization': `Bearer ${token}` },
        });
        if (!response.ok) return null;
        const data = await response.json();
        return data.fullName || null;
    } catch {
        return null;
    }
};

export const loginRequest = async (
    email: string,
    password: string
): Promise<LoginResponse> => {
    const response = await fetch(`${API_URL}/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password }),
    });

    if (!response.ok) {
        let errorMessage = 'Неверный email или пароль';
        try {
            const data = await response.json();
            errorMessage = data.message || errorMessage;
        } catch { }
        throw new Error(errorMessage);
    }

    const data: LoginResponse = await response.json();

    const jwtPayload = parseJwt(data.accessToken);
    let userRole = 'student';
    if (jwtPayload?.role === 'teacher') userRole = 'teacher';
    if (jwtPayload?.role === 'admin' || jwtPayload?.role === 'superadmin') userRole = 'admin';

    const userId = jwtPayload?.sub || '';

    localStorage.setItem('token', data.accessToken);
    localStorage.setItem('expiresAt', data.expiresAt);
    localStorage.setItem('user_id', userId);
    localStorage.setItem('user_role', userRole);

    // Admin получает fullName через API, остальные видят email
    const fullName = await fetchFullName(userId, data.accessToken);
    localStorage.setItem('user_name', fullName ?? email);

    return data;
};
