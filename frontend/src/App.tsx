import React, { useState } from 'react';
import type { User, UserRole } from './types';
import { AuthScreen } from './auth/AuthScreen';
import { AppLayout } from './layout/AppLayout';

const App: React.FC = () => {
  const [authed, setAuthed] = useState(false);
  const [role, setRole] = useState<UserRole>('student');
  const [user, setUser] = useState<User>({
    name: 'Степа Мокрушин',
  });

  const handleLogin = (r: UserRole, u: User) => {
    setRole(r);
    setUser(u);
    setAuthed(true);
  };

  const handleLogout = () => {
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