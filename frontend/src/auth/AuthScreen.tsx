import React, { useState } from 'react';
import { LoginForm } from './LoginForm';
import { RegisterForm } from './RegisterForm';
import type { LoginCredentials, RegisterData, User, UserRole } from '../types';
import { PasswordChangeForm } from './PasswordChangeForm';
import './auth.css';
import { loginRequest, registerRequest } from './auth';

interface AuthScreenProps {
  onLogin: (role: UserRole, user: User) => void;
}

type AuthTab = 'login' | 'register';

const mapRole = (role: number): UserRole => {
  switch (role) {
    case 1:
      return 'teacher';
    case 2:
      return 'admin';
    default:
      return 'student';
  }
};

const parseJwt = (token: string) => {
  try {
    const base64Payload = token.split('.')[1];
    const payload = atob(base64Payload);
    return JSON.parse(payload);
  } catch {
    return null;
  }
};

export const AuthScreen: React.FC<AuthScreenProps> = ({ onLogin }) => {
  const [activeTab, setActiveTab] = useState<AuthTab>('login');

  // врменно отключили, при необходимости добавим ручку
  const [mustChangePassword] = useState(false);

  const handleLogin = async (credentials: LoginCredentials) => {
    const response = await loginRequest(
      credentials.email,
      credentials.password
    );

    localStorage.setItem(
      'token',
      response.accessToken
    );

    const jwtPayload = parseJwt(response.accessToken);

    let role: UserRole = 'student';

    if (jwtPayload?.role === 'teacher') {
      role = 'teacher';
    }

    if (jwtPayload?.role === 'admin') {
      role = 'admin';
    }

    onLogin(role, {
      name: credentials.email,
    });
  };

  const handleRegister = async (
    data: RegisterData
  ) => {
    const response = await registerRequest(
      data.fullName,
      data.email,
      data.password
    );

    localStorage.setItem(
      'token',
      response.accessToken
    );

    const role = mapRole(response.role);

    onLogin(role, {
      name: data.fullName,
    });
  };

  const handlePasswordChange = (
    newPassword: string
  ) => {
    console.log(newPassword);
  };

  if (mustChangePassword) {
    return (
      <PasswordChangeForm
        onSubmit={handlePasswordChange}
      />
    );
  }

  return (
    <div className="login-page">
      <div className="login-card">
        <div className="logo">
          Neo<span className="logo-accent">Lab</span>
        </div>
        <div className="subtitle">Образовательная платформа по кибербезопасности</div>

        <div className="tabs">
          <button
            className={`tab ${activeTab === 'login' ? 'active' : ''}`}
            onClick={() => setActiveTab('login')}
          >
            Войти
          </button>
          <button
            className={`tab ${activeTab === 'register' ? 'active' : ''}`}
            onClick={() => setActiveTab('register')}
          >
            Регистрация
          </button>
        </div>

        {activeTab === 'login' ? (
          <LoginForm onSubmit={handleLogin} />
        ) : (
          <RegisterForm onSubmit={handleRegister} />
        )}
      </div>
    </div>
  );
};