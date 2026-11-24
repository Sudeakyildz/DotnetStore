import { Link, useNavigate } from 'react-router-dom';
import { getStoredAuthProfile } from '../api/client';
import { useShop } from '../shop/ShopContext';

const Navbar = () => {
  const navigate = useNavigate();
  const { signOut } = useShop();
  const profile = getStoredAuthProfile();

  const handleLogout = () => {
    void signOut().then(() => navigate('/yonetim'));
  };

  return (
    <nav className="navbar navbar-expand-lg navbar-dark admin-navbar border-bottom">
      <div className="container-fluid">
        <div className="d-flex flex-column py-1">
          <Link className="navbar-brand fw-semibold p-0" to="/dashboard">
            DotnetStore
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
          <Link className="btn btn-outline-light btn-sm rounded-pill px-3" to="/">
            Mağazayı gör
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
