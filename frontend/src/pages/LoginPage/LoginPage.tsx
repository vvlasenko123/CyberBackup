import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { loginRequest } from '../../auth/auth';
import { EyeIcon, EyeOffIcon } from '../../components/Icons';
import { Input } from '../../components/Input/Input';
import './LoginPage.css';

const LoginPage: React.FC = () => {
    const navigate = useNavigate();
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [showPassword, setShowPassword] = useState(false);
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);

    const handleSubmit = async () => {
        if (!email || !password) {
            setError('Заполните все поля');
            return;
        }
        try {
            setError('');
            setLoading(true);
            await loginRequest(email, password);
            navigate('/dashboard');
        } catch (e) {
            if (e instanceof Error) {
                setError(e.message === 'Failed to fetch' ? 'Ошибка соединения' : e.message);
            } else {
                setError('Ошибка входа');
            }
        } finally {
            setLoading(false);
        }
    };

    const handleKeyDown = (e: React.KeyboardEvent) => {
        if (e.key === 'Enter') handleSubmit();
    };

    return (
        <div className="login-page">
            <div className="login-card">
                <div className="logo">
                    Neo<span className="logo-accent">Lab</span>
                </div>
                <div className="subtitle">Образовательная платформа по кибербезопасности</div>

                <Input
                    label="Email"
                    type="email"
                    placeholder="student@neolab.ru"
                    value={email}
                    onChange={setEmail}
                />

                <div className="password-field">
                    <label className="input-label">Пароль</label>
                    <div className="password-input-wrapper">
                        <input
                            type={showPassword ? 'text' : 'password'}
                            placeholder="••••••••"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            onKeyDown={handleKeyDown}
                            className="password-input"
                        />
                        <button
                            type="button"
                            className="password-toggle"
                            onClick={() => setShowPassword(!showPassword)}
                        >
                            {showPassword ? <EyeOffIcon size={16} /> : <EyeIcon size={16} />}
                        </button>
                    </div>
                </div>

                {error && <div className="error-message">{error}</div>}

                <button
                    onClick={handleSubmit}
                    disabled={loading}
                    className="submit-button"
                >
                    {loading ? 'Вход...' : 'Войти'}
                </button>
            </div>
        </div>
    );
};

export default LoginPage;
