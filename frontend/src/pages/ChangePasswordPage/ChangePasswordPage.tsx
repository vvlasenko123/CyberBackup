import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Input } from '../../components/Input/Input';
import axiosInstance from '../../utils/axiosInstance';
import './ChangePasswordPage.css';

const ChangePasswordPage: React.FC = () => {
    const navigate = useNavigate();
    const [currentPassword, setCurrentPassword] = useState('');
    const [newPassword, setNewPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);

    const isDisabled = !currentPassword || !newPassword || newPassword !== confirmPassword;

    const handleSubmit = async () => {
        if (isDisabled) return;
        try {
            setError('');
            setLoading(true);
            await axiosInstance.post('/public/user/password/change', {
                currentPassword,
                newPassword,
            });
            localStorage.setItem('must_change_password', 'false');
            navigate('/dashboard');
        } catch (e: unknown) {
            const err = e as { response?: { data?: { message?: string } } };
            setError(err?.response?.data?.message || 'Не удалось сменить пароль');
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
                    <div className="login-hero__logo">Neo<span>Lab</span></div>
                    <div className="login-hero__copy">
                        <p className="login-hero__kicker">Образовательная платформа по кибербезопасности</p>
                        <h2 className="login-hero__title">Сначала установите новый пароль</h2>
                    </div>
                </div>

                <div className="login-form">
                    <div className="login-form__inner" onKeyDown={handleKeyDown}>
                        <h1 className="login-form__title">Смена пароля</h1>
                        <p className="login-form__subtitle">
                            Это ваш первый вход. Замените выданный администратором пароль на свой.
                        </p>

                        <Input
                            label="Текущий пароль"
                            type="password"
                            placeholder="••••••••"
                            value={currentPassword}
                            onChange={setCurrentPassword}
                        />
                        <Input
                            label="Новый пароль"
                            type="password"
                            placeholder="••••••••"
                            value={newPassword}
                            onChange={setNewPassword}
                        />
                        <Input
                            label="Повторите пароль"
                            type="password"
                            placeholder="••••••••"
                            value={confirmPassword}
                            onChange={setConfirmPassword}
                            error={confirmPassword && newPassword !== confirmPassword ? 'Пароли не совпадают' : undefined}
                        />

                        {error && <div className="error-message">{error}</div>}

                        <button
                            onClick={handleSubmit}
                            disabled={isDisabled || loading}
                            className="submit-button"
                        >
                            {loading ? 'Сохранение...' : 'Сохранить'}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default ChangePasswordPage;
