import { Link, useNavigate } from 'react-router-dom';
import { apiFetch, getStoredAuthProfile, setStoredToken } from '../api/client';

const Navbar = () => {
  const navigate = useNavigate();
  const profile = getStoredAuthProfile();

  const handleLogout = () => {
    void (async () => {
      try {
        await apiFetch('/api/Auth/logout', { method: 'POST' });
      } catch {
        /* yine de oturumu kapat */
      } finally {
        setStoredToken(null);
        navigate('/');
      }
    })();
  };

  return (
    <nav className="navbar navbar-expand-lg navbar-dark admin-navbar border-bottom">
      <div className="container-fluid">
        <div className="d-flex flex-column py-1">
          <Link className="navbar-brand fw-semibold p-0" to="/dashboard">
            Staj Store
          </Link>
          <span className="navbar-tagline d-none d-sm-block">Operasyon &amp; katalog</span>
        </div>
        <div className="d-flex align-items-center gap-2 flex-wrap justify-content-end">
          {profile && (
            <span className="text-white-50 small d-none d-md-inline">
              {profile.userName} · <span className="text-secondary">{profile.role}</span>
            </span>
          )}
          <Link className="btn btn-outline-light btn-sm rounded-pill px-3" to="/dashboard">
            Özet
          </Link>
          <button
            type="button"
            className="btn btn-sm text-dark rounded-pill px-3"
            style={{ background: '#fbbf24' }}
            onClick={handleLogout}
          >
            Çıkış
          </button>
        </div>
      </div>
    </nav>
  );
};

export default Navbar;
