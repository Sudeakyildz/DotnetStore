import { NavLink } from 'react-router-dom';
import { getStoredAuthProfile } from '../api/client';
import { AuthRoles } from '../lib/authRoles';

const linkClass = ({ isActive }: { isActive: boolean }) =>
  `nav-link px-3 py-2 ${isActive ? 'active' : 'text-white-50'}`;

const Sidebar = () => {
  const auth = getStoredAuthProfile();
  const role = auth?.role ?? '';

  const showDash = true;
  const showUsers = role === AuthRoles.Admin;
  const showCategories =
    role === AuthRoles.Admin || role === AuthRoles.StaffCategories;
  const showProducts =
    role === AuthRoles.Admin || role === AuthRoles.StaffPrices || role === AuthRoles.StaffFeatures;
  const showFeatures = role === AuthRoles.Admin || role === AuthRoles.StaffFeatures;

  return (
    <aside className="admin-sidebar text-white flex-shrink-0 d-flex flex-column">
      <div className="sidebar-brand">
        <div className="sidebar-brand-mark">S</div>
        <div>
          <div className="sidebar-brand-title">Staj Store</div>
          <div className="sidebar-brand-sub">Yönetim paneli</div>
        </div>
      </div>
      <nav className="nav flex-column px-3 pb-3 gap-1 flex-grow-1">
        {showDash && (
          <>
            <small
              className="text-secondary text-uppercase px-3 mb-1"
              style={{ fontSize: '0.65rem', letterSpacing: '0.12em' }}
            >
              Menü
            </small>
            <NavLink to="/dashboard" end className={linkClass}>
              Ana sayfa
            </NavLink>
            {showUsers && (
              <>
                <NavLink to="/admin/users" className={linkClass}>
                  Kullanıcılar
                </NavLink>
                <NavLink to="/admin/activity" className={linkClass}>
                  İşlem kayıtları
                </NavLink>
                <NavLink to="/admin/orders" className={linkClass}>
                  Müşteri siparişleri
                </NavLink>
              </>
            )}
          </>
        )}

        {showCategories && (
          <>
            <hr className="border-secondary border-opacity-25 my-2" />
            <small
              className="text-secondary text-uppercase px-3 mb-1"
              style={{ fontSize: '0.65rem', letterSpacing: '0.12em' }}
            >
              Ürün grupları
            </small>
            <NavLink to="/categories" className={linkClass}>
              Grup listesi
            </NavLink>
            <NavLink to="/categories/create" className={linkClass}>
              Grup kayıt
            </NavLink>
          </>
        )}

        {showProducts && (
          <>
            <hr className="border-secondary border-opacity-25 my-2" />
            <small
              className="text-secondary text-uppercase px-3 mb-1"
              style={{ fontSize: '0.65rem', letterSpacing: '0.12em' }}
            >
              Ürünler
            </small>
            <NavLink to="/products" className={linkClass}>
              Ürün listesi &amp; arama
            </NavLink>
            {(role === AuthRoles.Admin || role === AuthRoles.StaffPrices) && (
              <div className="small text-white-50 px-3 mb-2" style={{ fontSize: '0.75rem' }}>
                Fiyat için listede <strong className="text-warning">Fiyat</strong> düğmesini kullanın.
              </div>
            )}
            {role === AuthRoles.Admin && (
              <NavLink to="/products/create" className={linkClass}>
                Ürün kayıt
              </NavLink>
            )}
          </>
        )}

        {showFeatures && (
          <>
            <hr className="border-secondary border-opacity-25 my-2" />
            <small
              className="text-secondary text-uppercase px-3 mb-1"
              style={{ fontSize: '0.65rem', letterSpacing: '0.12em' }}
            >
              Özellikler
            </small>
            <NavLink to="/features" className={linkClass}>
              Ürün özellikleri
            </NavLink>
            <NavLink to="/features/create" className={linkClass}>
              Özellik ekle
            </NavLink>
          </>
        )}
      </nav>
    </aside>
  );
};

export default Sidebar;
