export function tokenFor(role = 'student') {
    return `test.${btoa(JSON.stringify({ sub: 'user-1', role }))}.signature`;
}

export function setSession(role = 'student', mustChange = false) {
    localStorage.setItem('token', tokenFor(role));
    localStorage.setItem('expiresAt', '2099-01-01T00:00:00Z');
    localStorage.setItem('user_role', role);
    localStorage.setItem('user_name', 'Test User');
    localStorage.setItem('must_change_password', String(mustChange));
}
