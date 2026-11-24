import { useEffect, useState } from 'react';
import { Link, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { apiJson } from '../api/client';
import type { StoreCategory } from '../api/shopTypes';
import { useShop } from './ShopContext';

const ShopLayout = () => {
  const { profile, isCustomer, isStaff, cart, signOut, toasts } = useShop();
  const navigate = useNavigate();
  const location = useLocation();
  const [q, setQ] = useState('');
  const [categories, setCategories] = useState<StoreCategory[]>([]);

  useEffect(() => {
    void (async () => {
      try {
        setCategories(await apiJson<StoreCategory[]>('/api/store/categories'));
      } catch {
        /* menü olmadan da çalışır */
      }
    })();
  }, []);

  useEffect(() => {
    window.scrollTo({ top: 0 });
  }, [location.pathname]);

  const submitSearch = (e: React.FormEvent) => {
    e.preventDefault();
    navigate(`/urunler${q.trim() ? `?q=${encodeURIComponent(q.trim())}` : ''}`);
  };

  const count = cart?.itemCount ?? 0;
  const activeCat = location.pathname === '/urunler' ? new URLSearchParams(location.search).get('categoryId') : null;

  return (
    <div className="shop">
      <div className="shop-topbar">
        <div className="container d-flex justify-content-between">
          <span>Siparişlerde 500 ₺ ve üzeri kargo ücretsiz</span>
          <span className="d-none d-md-inline">Güvenli ödeme · Kolay iade</span>
        </div>
      </div>

      <header className="shop-header">
        <div className="container d-flex align-items-center gap-3 py-3">
          <Link to="/" className="shop-logo">
            Dotnet<span>Store</span>
          </Link>

          <form className="shop-search flex-grow-1" onSubmit={submitSearch} role="search">
            <input
              type="search"
              className="form-control"
              placeholder="Ürün ara…"
              value={q}
              onChange={(e) => setQ(e.target.value)}
              aria-label="Ürün ara"
            />
            <button type="submit" className="btn btn-brand">Ara</button>
          </form>

          <nav className="d-flex align-items-center gap-2 shop-actions">
            {isStaff && (
              <Link to="/dashboard" className="btn btn-outline-dark btn-sm">Yönetim</Link>
            )}
            {isCustomer && (
              <Link to="/favoriler" className="shop-icon-link" title="Favorilerim">
                <span aria-hidden>♡</span>
                <small className="d-none d-lg-inline">Favoriler</small>
              </Link>
            )}
            {profile ? (
              <div className="dropdown">
                <button className="btn btn-light btn-sm dropdown-toggle" data-bs-toggle="dropdown" type="button">
                  {profile.userName}
                </button>
                <ul className="dropdown-menu dropdown-menu-end">
                  {isCustomer && (
                    <>
                      <li><Link className="dropdown-item" to="/hesabim/siparisler">Siparişlerim</Link></li>
                      <li><Link className="dropdown-item" to="/hesabim">Hesap bilgilerim</Link></li>
                      <li><hr className="dropdown-divider" /></li>
                    </>
                  )}
                  <li>
                    <button
                      className="dropdown-item"
                      type="button"
                      onClick={() => void signOut().then(() => navigate('/'))}
                    >
                      Çıkış yap
                    </button>
                  </li>
                </ul>
              </div>
            ) : (
              <>
                <Link to="/giris" className="btn btn-outline-dark btn-sm">Giriş</Link>
                <Link to="/kayit" className="btn btn-brand btn-sm">Üye ol</Link>
              </>
            )}
            <Link to="/sepet" className="shop-cart" aria-label={`Sepet, ${count} ürün`}>
              <span aria-hidden>🛒</span>
              {count > 0 && <span className="shop-cart-badge">{count}</span>}
            </Link>
          </nav>
        </div>

        <div className="shop-catnav">
          <div className="container d-flex gap-3 overflow-auto">
            <Link
              to="/urunler"
              className={`shop-catlink ${activeCat === null && location.pathname === '/urunler' ? 'active' : ''}`}
            >
              Tüm ürünler
            </Link>
            {categories.filter((c) => c.productCount > 0).map((c) => (
              <Link
                key={c.id}
                to={`/urunler?categoryId=${c.id}`}
                className={`shop-catlink ${activeCat === String(c.id) ? 'active' : ''}`}
              >
                {c.name}
              </Link>
            ))}
          </div>
        </div>
      </header>

      <main className="shop-main">
        <Outlet />
      </main>

      <footer className="shop-footer">
        <div className="container py-4 d-flex flex-wrap justify-content-between gap-3">
          <div>
            <strong>DotnetStore</strong>
            <div className="small text-muted">ASP.NET Core + React ile geliştirilen demo e-ticaret platformu.</div>
          </div>
          <div className="small text-muted">
            Bu site bir demodur: ödemeler simüle edilir, gerçek para çekilmez.
            <br />
            <Link to="/kvkk">Aydınlatma metni</Link> · <Link to="/yonetim">Yönetici girişi</Link>
          </div>
        </div>
      </footer>

      <div className="shop-toasts" aria-live="polite">
        {toasts.map((t) => (
          <div key={t.id} className={`alert alert-${t.kind} shadow-sm py-2 mb-2`}>{t.message}</div>
        ))}
      </div>
    </div>
  );
};

export default ShopLayout;
