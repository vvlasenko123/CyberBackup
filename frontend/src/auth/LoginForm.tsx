import React, { useState } from 'react';
import type { LoginCredentials } from '../types';
import { EyeIcon, EyeOffIcon } from '../components/Icons';
import { Input } from '../components/Input';

interface LoginFormProps {
  onSubmit: (credentials: LoginCredentials) => void;
}

export const LoginForm: React.FC<LoginFormProps> = ({ onSubmit }) => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = () => {
    if (!email || !password) {
      setError('Заполните все поля');
      return;
    }

    setError('');
    setLoading(true);

    setTimeout(() => {
      setLoading(false);
      
      if (email === 'bad@bad.ru') {
        setError('Неверный email или пароль');
        return;
      }

      onSubmit({ email, password });
    }, 600);
  };

  return (
    <>
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

      {error && <div className="error-message">{error}</div>}

      <button
        onClick={handleSubmit}
        disabled={loading}
        className="submit-button"
      >
        {loading ? 'Вход...' : 'Войти'}
      </button>
    </>
  );
};