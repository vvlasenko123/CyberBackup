import React, { useState } from 'react';
import { LoginForm } from './LoginForm';
import { RegisterForm } from './RegisterForm';
import type { LoginCredentials, RegisterData, User, UserRole } from '../types';
import { PasswordChangeForm } from './PasswordChangeForm';
import './auth.css';

interface AuthScreenProps {
  onLogin: (role: UserRole, user: User) => void;
}

type AuthTab = 'login' | 'register';

export const AuthScreen: React.FC<AuthScreenProps> = ({ onLogin }) => {
  const [activeTab, setActiveTab] = useState<AuthTab>('login');
  const [mustChangePassword, setMustChangePassword] = useState(false);
  const [tempUser, setTempUser] = useState<{ email: string } | null>(null);

  const handleLogin = (credentials: LoginCredentials) => {
    if (credentials.password === '12345') {
      setTempUser({ email: credentials.email });
      setMustChangePassword(true);
      return;
    }

    let role: UserRole = 'student';
    if (credentials.email.includes('admin')) role = 'admin';
    else if (credentials.email.includes('teacher') || credentials.email.includes('prof')) role = 'teacher';

    onLogin(role, { name: role === 'admin' ? 'Орлов И.П.' : role === 'teacher' ? 'Смирнов В.А.' : 'Иванов Алексей' });
  };

  const handleRegister = (data: RegisterData) => {
    onLogin('student', { name: data.fullName });
  };

  const handlePasswordChange = (newPassword: string) => {
    onLogin('student', { name: 'Иванов Алексей' });
  };

  if (mustChangePassword) {
    return <PasswordChangeForm onSubmit={handlePasswordChange} />;
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

        {/* <div className="demo-info">
          <strong>Demo:</strong> любой email → студент · «admin» в email → Admin · «teacher/prof» → Преподаватель · пароль{' '}
          <code>12345</code> → смена пароля
        </div> */}
      </div>
    </div>
  );
};