import { useEffect, useState } from 'react';
import { Link, Navigate, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { apiJson } from '../../api/client';
import type { LoginResponse } from '../../api/types';
import type { StoreProduct } from '../../api/shopTypes';
import ProductCard from '../components/ProductCard';
import { useShop } from '../ShopContext';

/** Yalnızca giriş yapmış müşteriler. */
export const RequireCustomer = () => {
  const { profile, isCustomer } = useShop();
  const location = useLocation();
  if (!profile) return <Navigate to="/giris" replace state={{ from: location.pathname }} />;
  if (!isCustomer) return <Navigate to="/dashboard" replace />;
  return <Outlet />;
};

export const FavoritesPage = () => {
  const { favorites } = useShop();
  const [items, setItems] = useState<StoreProduct[] | null>(null);

  useEffect(() => {
    void (async () => {
      try {
        setItems(await apiJson<StoreProduct[]>('/api/favorites'));
      } catch {
        setItems([]);
      }
    })();
    // favoriler çıkarılınca liste de tazelensin
  }, [favorites.length]);

  return (
    <div className="container py-4">
      <h1 className="h3 mb-3">Favorilerim</h1>
      {items === null ? (
        <div className="text-muted">Yükleniyor…</div>
      ) : items.length === 0 ? (
        <div className="empty-state">
          <div className="fs-1" aria-hidden>♡</div>
          <strong>Henüz favori ürünün yok</strong>
          <Link to="/urunler" className="btn btn-brand mt-3">Ürünlere göz at</Link>
        </div>
      ) : (
        <div className="row g-3">
          {items.map((p) => (
            <div className="col-6 col-md-4 col-lg-3" key={p.id}><ProductCard p={p} /></div>
          ))}
        </div>
      )}
    </div>
  );
};

export const AccountPage = () => {
  const { profile, notify } = useShop();
  const [cur, setCur] = useState('');
  const [next, setNext] = useState('');
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setBusy(true);
    try {
      await apiJson<void>('/api/Auth/password', {
        method: 'PUT',
        body: JSON.stringify({ currentPassword: cur, newPassword: next }),
      });
      notify('Şifren güncellendi.');
      setCur('');
      setNext('');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Şifre değiştirilemedi.');
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="container py-4" style={{ maxWidth: 560 }}>
      <h1 className="h3 mb-3">Hesap bilgilerim</h1>
      <div className="summary-box mb-3">
        <div><strong>{profile?.userName}</strong></div>
        <div className="text-muted">{profile?.email}</div>
        <Link to="/hesabim/siparisler" className="d-inline-block mt-2">Siparişlerim →</Link>
      </div>
      <form className="summary-box" onSubmit={(e) => void submit(e)}>
        <h2 className="h6">Şifre değiştir</h2>
        {error && <div className="alert alert-danger py-2 small">{error}</div>}
        <label className="form-label" htmlFor="cp">Mevcut şifre</label>
        <input id="cp" className="form-control mb-2" type="password" required autoComplete="current-password"
          value={cur} onChange={(e) => setCur(e.target.value)} />
        <label className="form-label" htmlFor="np">Yeni şifre (en az 8 karakter)</label>
        <input id="np" className="form-control mb-3" type="password" required minLength={8} autoComplete="new-password"
          value={next} onChange={(e) => setNext(e.target.value)} />
        <button className="btn btn-brand" disabled={busy} type="submit">{busy ? 'Kaydediliyor…' : 'Güncelle'}</button>
      </form>
    </div>
  );
};

const AuthShell = ({ title, children, footer }: { title: string; children: React.ReactNode; footer: React.ReactNode }) => (
  <div className="container py-5" style={{ maxWidth: 440 }}>
    <div className="summary-box">
      <h1 className="h4 mb-3 text-center">{title}</h1>
      {children}
      <div className="text-center small mt-3">{footer}</div>
    </div>
  </div>
);

export const ShopLogin = () => {
  const { signIn } = useShop();
  const navigate = useNavigate();
  const location = useLocation();
  const from = (location.state as { from?: string } | null)?.from;
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setBusy(true);
    try {
      const data = await apiJson<LoginResponse>('/api/Auth/login', {
        method: 'POST',
        body: JSON.stringify({ email, password }),
      });
      await signIn(data);
      navigate(data.role === 'Musteri' ? (from ?? '/') : '/dashboard', { replace: true });
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Giriş yapılamadı.');
    } finally {
      setBusy(false);
    }
  };

  return (
    <AuthShell title="Giriş yap" footer={<>Hesabın yok mu? <Link to="/kayit" state={{ from }}>Üye ol</Link></>}>
      {error && <div className="alert alert-danger py-2 small" role="alert">{error}</div>}
      <form onSubmit={(e) => void submit(e)}>
        <label className="form-label" htmlFor="le">E-posta</label>
        <input id="le" type="email" className="form-control mb-2" required autoComplete="username"
          value={email} onChange={(e) => setEmail(e.target.value)} />
        <label className="form-label" htmlFor="lp">Şifre</label>
        <input id="lp" type="password" className="form-control mb-3" required autoComplete="current-password"
          value={password} onChange={(e) => setPassword(e.target.value)} />
        <button className="btn btn-brand w-100" disabled={busy} type="submit">{busy ? 'Giriş…' : 'Giriş yap'}</button>
      </form>
    </AuthShell>
  );
};

export const ShopRegister = () => {
  const { signIn } = useShop();
  const navigate = useNavigate();
  const location = useLocation();
  const from = (location.state as { from?: string } | null)?.from;
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [kvkk, setKvkk] = useState(false);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setBusy(true);
    try {
      const data = await apiJson<LoginResponse>('/api/Auth/register', {
        method: 'POST',
        body: JSON.stringify({ username, email, password }),
      });
      await signIn(data);
      navigate(from ?? '/', { replace: true });
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Kayıt yapılamadı.');
    } finally {
      setBusy(false);
    }
  };

  return (
    <AuthShell title="Üye ol" footer={<>Zaten üye misin? <Link to="/giris" state={{ from }}>Giriş yap</Link></>}>
      {error && <div className="alert alert-danger py-2 small" role="alert">{error}</div>}
      <form onSubmit={(e) => void submit(e)}>
        <label className="form-label" htmlFor="ru">Ad soyad / kullanıcı adı</label>
        <input id="ru" className="form-control mb-2" required minLength={3} maxLength={100} autoComplete="name"
          value={username} onChange={(e) => setUsername(e.target.value)} />
        <label className="form-label" htmlFor="re">E-posta</label>
        <input id="re" type="email" className="form-control mb-2" required autoComplete="email"
          value={email} onChange={(e) => setEmail(e.target.value)} />
        <label className="form-label" htmlFor="rp">Şifre (en az 6 karakter)</label>
        <input id="rp" type="password" className="form-control mb-3" required minLength={6} autoComplete="new-password"
          value={password} onChange={(e) => setPassword(e.target.value)} />
        <div className="form-check mb-3">
          <input id="rk" className="form-check-input" type="checkbox" checked={kvkk} onChange={(e) => setKvkk(e.target.checked)} required />
          <label htmlFor="rk" className="form-check-label small">
            <Link to="/kvkk" target="_blank">Aydınlatma metnini</Link> okudum, kabul ediyorum.
          </label>
        </div>
        <button className="btn btn-brand w-100" disabled={busy || !kvkk} type="submit">{busy ? 'Kaydediliyor…' : 'Üye ol'}</button>
      </form>
    </AuthShell>
  );
};
