import { Link } from 'react-router-dom';

const NotFound = () => {
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
            <h1 style={{ color: '#EF4444', fontSize: '48px', margin: 0 }}>404</h1>
            <h3 style={{ margin: 0 }}>Эта страница не существует.</h3>
            <Link to="/dashboard" style={{ color: '#4EA1FF' }}>
                Вернуться на главную
            </Link>
        </div>
    );
};

export default NotFound;
