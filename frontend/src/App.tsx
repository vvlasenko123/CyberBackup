import React, { useEffect, useState } from 'react';
import type { User, UserRole } from './types';
import { AuthScreen } from './auth/AuthScreen';
import { AppLayout } from './layout/AppLayout';

const parseJwt = (token: string) => {
  try {
    const base64Payload = token.split('.')[1];
    const payload = atob(base64Payload);
    return JSON.parse(payload);
  } catch {
    return null;
  }
};

const App: React.FC = () => {
  const [authed, setAuthed] = useState(false);
  const [role, setRole] = useState<UserRole>('student');
  const [user, setUser] = useState<User>({
    name: '',
  });

  useEffect(() => {
    const token = localStorage.getItem('token');
    const expiresAt =
      localStorage.getItem('expiresAt');

    if (!token || !expiresAt) {
      return;
    }

    const expirationDate = new Date(expiresAt);

    if (expirationDate < new Date()) {
      localStorage.removeItem('token');
      localStorage.removeItem('accessToken');
      localStorage.removeItem('expiresAt');
      return;
    }

    const jwtPayload = parseJwt(token);

    if (!jwtPayload) {
      return;
    }

    let userRole: UserRole = 'student';

    if (jwtPayload.role === 'teacher') {
      userRole = 'teacher';
    }

    if (jwtPayload.role === 'admin') {
      userRole = 'admin';
    }

    setRole(userRole);

    setUser({
      name: 'Степа Мокрушин',
    });

    setAuthed(true);
  }, []);

  const handleLogin = (r: UserRole, u: User) => {
    setRole(r);
    setUser(u);
    setAuthed(true);
  };

  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('accessToken');
    localStorage.removeItem('expiresAt');

    setAuthed(false);
    setRole('student');
    setUser({ name: '' });
  };

  return !authed ? (
    <AuthScreen onLogin={handleLogin} />
  ) : (
    <AppLayout
      role={role}
      user={user}
      onLogout={handleLogout}
      onRoleChange={setRole}
    />
  );
};

export default App;