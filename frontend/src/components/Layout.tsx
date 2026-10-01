import { Link, Outlet, useNavigate } from 'react-router-dom';
import { useAuthStore } from '../store/authStore';

export default function Layout() {
  const { user, logout } = useAuthStore();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/');
  };

  return (
    <div className="app-shell">
      <header className="app-header">
        <h1>🗺️ Карта меток</h1>
        {user ? (
          <>
            <span className="user-chip">
              {user.userName}
              {user.roles.includes('Admin') ? ' (Admin)' : ''}
            </span>
            <button onClick={handleLogout}>Выйти</button>
          </>
        ) : (
          <>
            <Link to="/login">Войти</Link>
            <Link to="/register">Регистрация</Link>
          </>
        )}
      </header>
      <Outlet />
    </div>
  );
}