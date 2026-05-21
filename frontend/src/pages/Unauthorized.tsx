import { Link } from 'react-router-dom';

const Unauthorized = () => {
    return (
        <div
            style={{
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                justifyContent: 'center',
                height: '100vh',
                textAlign: 'center',
                gap: '15px',
                backgroundColor: '#0F1720',
                color: '#E5E7EB',
            }}
        >
            <h1 style={{ color: '#EF4444', fontSize: '48px', margin: 0 }}>401</h1>
            <h3 style={{ margin: 0 }}>
                Доступ запрещён.<br />У вас нет прав для просмотра этой страницы.
            </h3>
            <Link to="/dashboard" style={{ color: '#4EA1FF' }}>
                Вернуться на главную
            </Link>
        </div>
    );
};

export default Unauthorized;
