import { BrowserRouter, Routes, Route, Navigate, useLocation } from 'react-router-dom';
import type { ReactNode } from 'react';

import ProtectedRoute from './components/ProtectedRoute';
import { AppLayout } from './components/AppLayout/AppLayout';
import { useIsDesktop } from './hooks/useIsDesktop';
import DesktopOnlyScreen from './pages/DesktopOnly/DesktopOnlyScreen';

import NotFound from './pages/NotFound';
import Unauthorized from './pages/Unauthorized';
import LogOut from './pages/LogOut';

import LoginPage from './pages/LoginPage/LoginPage';
import DashboardPage from './pages/DashboardPage/DashboardPage';
import LabsPage from './pages/LabsPage/LabsPage';
import LabDetailPage from './pages/LabDetailPage/LabDetailPage';
import LabReportPage from './pages/LabReportPage/LabReportPage';
import LabCreatePage from './pages/LabCreatePage/LabCreatePage';
import ProgressPage from './pages/ProgressPage/ProgressPage';
import StatementPage from './pages/StatementPage/StatementPage';
import QuestionsPage from './pages/QuestionsPage/QuestionsPage';
import QuestionCreatePage from './pages/QuestionCreatePage/QuestionCreatePage';
import QuestionDetailPage from './pages/QuestionDetailPage/QuestionDetailPage';
import CalendarPage from './pages/CalendarPage/CalendarPage';
import UsersPage from './pages/UsersPage/UsersPage';
import ChangePasswordPage from './pages/ChangePasswordPage/ChangePasswordPage';
import TeacherReportDetailPage from './pages/TeacherReportDetailPage/TeacherReportDetailPage';
import GroupsPage from './pages/GroupsPage/GroupsPage';
import GroupDetailPage from './pages/GroupDetailPage/GroupDetailPage';
import LabEditPage from './pages/LabEditPage/LabEditPage';
import ProfilePage from './pages/ProfilePage/ProfilePage';
import { isMobileReady } from './utils/mobileReady';

function DeviceGate({ children }: { children: ReactNode }) {
    const isDesktop = useIsDesktop();
    const { pathname } = useLocation();

    if (!isDesktop && !isMobileReady(pathname)) {
        return <DesktopOnlyScreen />;
    }

    return <>{children}</>;
}

function App() {
    return (
        <BrowserRouter>
            <DeviceGate>
                <Routes>
                    <Route path="/" element={<Navigate to="/dashboard" />} />
                    <Route path="/login" element={<LoginPage />} />
                    <Route path="/change-password" element={<ChangePasswordPage />} />
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

                        <Route path="/labs/create" element={
                            <ProtectedRoute allowedRoles={['teacher']}>
                                <LabCreatePage />
                            </ProtectedRoute>
                        } />

                        <Route path="/labs/:labId" element={
                            <ProtectedRoute allowedRoles={['student', 'teacher']}>
                                <LabDetailPage />
                            </ProtectedRoute>
                        } />

                        <Route path="/labs/:labId/edit" element={
                            <ProtectedRoute allowedRoles={['teacher']}>
                                <LabEditPage />
                            </ProtectedRoute>
                        } />

                        <Route path="/labs/:labId/report" element={
                            <ProtectedRoute allowedRoles={['student']}>
                                <LabReportPage />
                            </ProtectedRoute>
                        } />

                        <Route path="/teacher/reports/:reportId" element={
                            <ProtectedRoute allowedRoles={['teacher', 'admin']}>
                                <TeacherReportDetailPage />
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

                        <Route path="/questions/new" element={
                            <ProtectedRoute allowedRoles={['student']}>
                                <QuestionCreatePage />
                            </ProtectedRoute>
                        } />

                        <Route path="/questions/:questionId" element={
                            <ProtectedRoute allowedRoles={['student', 'teacher', 'admin']}>
                                <QuestionDetailPage />
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

                        <Route path="/groups" element={
                            <ProtectedRoute allowedRoles={['admin']}>
                                <GroupsPage />
                            </ProtectedRoute>
                        } />

                        <Route path="/groups/:groupId" element={
                            <ProtectedRoute allowedRoles={['admin']}>
                                <GroupDetailPage />
                            </ProtectedRoute>
                        } />

                        <Route path="/profile" element={
                            <ProtectedRoute allowedRoles={['student', 'teacher', 'admin']}>
                                <ProfilePage />
                            </ProtectedRoute>
                        } />

                    </Route>
                </Routes>
            </DeviceGate>
        </BrowserRouter>
    );
}

export default App;
