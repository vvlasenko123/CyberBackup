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
            const res = await loginRequest(email, password);
            navigate(res.mustChangePassword ? '/change-password' : '/dashboard');
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
            <div className="login-shell">
                <div className="login-hero animated-gradient">
                    <div className="login-hero__logo">
                        Neo<span>Lab</span>
                    </div>
                    <div className="login-hero__copy">
                        <p className="login-hero__kicker">Образовательная платформа по кибербезопасности</p>
                        <h2 className="login-hero__title">
                            Все ваши лабораторки, отчёты и прогресс — в одном месте
                        </h2>
                    </div>
                </div>

                <div className="login-form">
                    <div className="login-form__inner">
                        <h1 className="login-form__title">С возвращением</h1>
                        <p className="login-form__subtitle">
                            Войдите, используя логин и пароль, которые вам выдал администратор.
                        </p>

                        <Input
                            label="Email"
                            type="email"
                            placeholder="student@urfu.ru"
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
            </div>
        </div>
    );
};

export default LoginPage;
