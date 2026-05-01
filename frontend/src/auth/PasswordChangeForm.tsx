import React, { useState } from 'react';
import { Input } from '../components/Input/Input';

interface PasswordChangeFormProps {
  onSubmit: (newPassword: string) => void;
}

export const PasswordChangeForm: React.FC<PasswordChangeFormProps> = ({ onSubmit }) => {
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');

  const isDisabled = !newPassword || newPassword !== confirmPassword;

  return (
    <div className="login-page">
      <div className="login-card">
        <div className="logo">
          Neo<span className="logo-accent">Lab</span>
        </div>
        <div className="subtitle">Образовательная платформа по кибербезопасности</div>

        <div className="title">Придумайте новый пароль</div>
        <div className="subtitle-small">Это первый вход — установите постоянный пароль.</div>

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
          onClick={() => onSubmit(newPassword)}
          disabled={isDisabled}
          className="submit-button"
        >
          Сохранить и продолжить
        </button>
      </div>
    </div>
  );
};