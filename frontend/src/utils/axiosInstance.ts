import axios from 'axios';

// TODO: задайте переменную VITE_API_URL в файле .env
// VITE_API_URL=http://localhost:5000/
const axiosInstance = axios.create({
    baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5000/',
    headers: {
        'Content-Type': 'application/json',
    },
});

axiosInstance.interceptors.request.use(config => {
    const token = localStorage.getItem('token');
    if (token) {
        config.headers['Authorization'] = 'Bearer ' + token;
    }
    return config;
});

axiosInstance.interceptors.response.use(
    response => response,
    error => {
        if (error.response?.status === 401) {
            localStorage.removeItem('token');
            localStorage.removeItem('expiresAt');
            localStorage.removeItem('user_role');
            localStorage.removeItem('user_name');
            window.location.href = '/login';
        }
        return Promise.reject(error);
    }
);

export default axiosInstance;
