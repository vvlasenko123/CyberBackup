import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';

import ProtectedRoute from './components/ProtectedRoute';
import { AppLayout } from './layout/AppLayout';

import NotFound from './pages/NotFound';
import Unauthorized from './pages/Unauthorized';
import LogOut from './pages/LogOut';

import LoginPage from './pages/LoginPage/LoginPage';
import DashboardPage from './pages/DashboardPage/DashboardPage';
import LabsPage from './pages/LabsPage/LabsPage';
import ProgressPage from './pages/ProgressPage/ProgressPage';
import StatementPage from './pages/StatementPage/StatementPage';
import QuestionsPage from './pages/QuestionsPage/QuestionsPage';
import CalendarPage from './pages/CalendarPage/CalendarPage';
import UsersPage from './pages/UsersPage/UsersPage';
import ChangePasswordPage from './pages/ChangePasswordPage/ChangePasswordPage';

function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<Navigate to="/dashboard" />} />
                <Route path="/login" element={<LoginPage />} />
                <Route path="/logout" element={<LogOut />} />
                <Route path="/unauthorized" element={<Unauthorized />} />
                <Route path="*" element={<NotFound />} />

                {/* Все защищённые страницы внутри AppLayout */}
                <Route
                    element={
                        <ProtectedRoute allowedRoles={['student', 'teacher', 'admin']}>
                            <AppLayout />
                        </ProtectedRoute>
                    }
                >
                    <Route path="/dashboard" element={<DashboardPage />} />

                    <Route path="/labs" element={
                        <ProtectedRoute allowedRoles={['student', 'teacher']}>
                            <LabsPage />
                        </ProtectedRoute>
                    } />

                    <Route path="/progress" element={
                        <ProtectedRoute allowedRoles={['student']}>
                            <ProgressPage />
                        </ProtectedRoute>
                    } />

                    <Route path="/statement" element={
                        <ProtectedRoute allowedRoles={['student', 'teacher']}>
                            <StatementPage />
                        </ProtectedRoute>
                    } />

                    <Route path="/questions" element={
                        <ProtectedRoute allowedRoles={['student', 'teacher']}>
                            <QuestionsPage />
                        </ProtectedRoute>
                    } />

                    <Route path="/calendar" element={
                        <ProtectedRoute allowedRoles={['student', 'teacher', 'admin']}>
                            <CalendarPage />
                        </ProtectedRoute>
                    } />

                    <Route path="/users" element={
                        <ProtectedRoute allowedRoles={['admin']}>
                            <UsersPage />
                        </ProtectedRoute>
                    } />

                    <Route path="/change-password" element={<ChangePasswordPage />} />
                </Route>
            </Routes>
        </BrowserRouter>
    );
}

export default App;
