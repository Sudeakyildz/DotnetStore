import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import {
  apiFetch,
  getStoredToken,
  readJson,
  setStoredAuthProfile,
  setStoredToken,
  parseErrorMessage,
} from '../api/client';
import type { LoginResponse } from '../api/types';

const Login = () => {
  const navigate = useNavigate();
  useEffect(() => {
    if (getStoredToken()) navigate('/dashboard', { replace: true });
  }, [navigate]);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [kvkk, setKvkk] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    if (!kvkk) {
      setError('Devam etmek için aydınlatma metnini onaylamanız gerekir.');
      return;
    }
    setLoading(true);
    try {
      const res = await apiFetch('/api/Auth/login', {
        method: 'POST',
        body: JSON.stringify({ email, password }),
      });
      if (!res.ok) {
        setError(await parseErrorMessage(res));
        return;
      }
      const data = await readJson<LoginResponse>(res);
      setStoredToken(data.accessToken);
      setStoredAuthProfile({ email: data.email, userName: data.userName, role: data.role });
      navigate('/dashboard');
    } catch {
      setError('Sunucuya bağlanılamadı. API çalışıyor mu?');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-vh-100 d-flex align-items-center justify-content-center bg-light">
      <div className="card shadow" style={{ width: '100%', maxWidth: '420px' }}>
        <div className="card-body p-4">
          <h1 className="h4 mb-2 text-center">Yönetici girişi</h1>
          <p className="small text-muted text-center mb-3">
            Hesabınız size atanmış e-posta ve şifre ile giriş yapın. Herkese açık kayıt kapalıdır.
          </p>
          {error && <div className="alert alert-danger py-2 small">{error}</div>}
          <form onSubmit={handleSubmit}>
            <div className="mb-3">
              <label className="form-label">E-posta</label>
              <input
                type="email"
                className="form-control"
                autoComplete="username"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
            </div>
            <div className="mb-3">
              <label className="form-label">Şifre</label>
              <input
                type="password"
                className="form-control"
                autoComplete="current-password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
              />
            </div>
            <div className="form-check mb-3">
              <input
                className="form-check-input"
                type="checkbox"
                id="kvkk"
                checked={kvkk}
                onChange={(e) => setKvkk(e.target.checked)}
              />
              <label className="form-check-label small" htmlFor="kvkk">
                <Link to="/kvkk" target="_blank" rel="noopener noreferrer">
                  Aydınlatma metnini
                </Link>{' '}
                okudum; kişisel verilerimin bu panel kapsamında işlenmesini kabul ediyorum.
              </label>
            </div>
            <button type="submit" className="btn btn-primary w-100" disabled={loading}>
              {loading ? 'Giriş…' : 'Giriş yap'}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
};

export default Login;
