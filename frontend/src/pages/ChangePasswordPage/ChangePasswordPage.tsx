import React, { useState } from 'react';
import { Input } from '../../components/Input/Input';
import './ChangePasswordPage.css';

const ChangePasswordPage: React.FC = () => {
    const [newPassword, setNewPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');

    const isDisabled = !newPassword || newPassword !== confirmPassword;

    const handleSubmit = () => {
        if (isDisabled) return;
        // TODO: вызов API для смены пароля
        console.log('Смена пароля:', newPassword);
    };

    return (
        <div className="change-password-page">
            <div className="change-password-card">
                <div className="change-password-title">Смена пароля</div>
                <div className="change-password-subtitle">Установите новый постоянный пароль.</div>

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

                <button
                    onClick={handleSubmit}
                    disabled={isDisabled}
                    className="change-password-button"
                >
                    Сохранить
                </button>
            </div>
        </div>
    );
};

export default ChangePasswordPage;
