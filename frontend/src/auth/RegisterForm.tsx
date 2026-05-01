import React, { useState } from 'react';
import type { RegisterData } from '../types';
import { Input } from '../components/Input/Input';
import { EyeIcon, EyeOffIcon } from '../components/Icons';

interface RegisterFormProps {
  onSubmit: (data: RegisterData) => void;
}

export const RegisterForm: React.FC<RegisterFormProps> = ({ onSubmit }) => {
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = () => {
    if (!fullName || !email || !password || !confirmPassword) {
      setError('Заполните все поля');
      return;
    }

    if (password !== confirmPassword) {
      setError('Пароли не совпадают');
      return;
    }

    onSubmit({ fullName, email, password, confirmPassword });
  };

  const getPasswordError = () => {
    if (confirmPassword && password !== confirmPassword) {
      return 'Пароли не совпадают';
    }
    return undefined;
  };

  return (
    <>
      <Input
        label="Полное имя (ФИО)"
        placeholder="Иванов Алексей Иванович"
        value={fullName}
        onChange={setFullName}
      />

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

      <Input
        label="Повторите пароль"
        type="password"
        placeholder="••••••••"
        value={confirmPassword}
        onChange={setConfirmPassword}
        error={getPasswordError()}
      />

      {error && <div className="error-message">{error}</div>}

      <button onClick={handleSubmit} className="submit-button">
        Зарегистрироваться
      </button>
    </>
  );
};