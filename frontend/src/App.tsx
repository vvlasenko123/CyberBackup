import React, { useState } from 'react';
import type { User, UserRole } from './types';
import { AuthScreen } from './auth/AuthScreen';
import './styles.css'

const App: React.FC = () => {
  const [authed, setAuthed] = useState(true);
  const [role, setRole] = useState<UserRole>('student');
  const [user, setUser] = useState<User>({ name: 'Иванов Алексей' });

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

  return <AuthScreen onLogin={handleLogin} />;

};

export default App;