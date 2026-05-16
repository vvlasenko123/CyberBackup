const API_URL = 'http://localhost:5000/public/auth';

export interface LoginResponse {
    accessToken: string;
    expiresAt: string;
}

export interface RegisterResponse {
    userId: string;
    role: number;
    accessToken: string;
    expiresAtUtc: string;
}

export const loginRequest = async (
    email: string,
    password: string
): Promise<LoginResponse> => {
    const response = await fetch(`${API_URL}/login`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            email,
            password,
        }),
    });

    if (!response.ok) {
        let errorMessage = 'Неверный email или пароль';

        try {
            const data = await response.json();
            errorMessage = data.message || errorMessage;
        } catch { }

        throw new Error(errorMessage);
    }

    return response.json();
};

export const registerRequest = async (
    fullName: string,
    email: string,
    password: string
): Promise<RegisterResponse> => {
    const response = await fetch(`${API_URL}/register`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            fullName,
            email,
            password,
        }),
    });

    if (!response.ok) {
        let errorMessage = 'Ошибка регистрации';

        try {
            const data = await response.json();
            errorMessage = data.message || errorMessage;
        } catch { }

        throw new Error(errorMessage);
    }

    return response.json();
};