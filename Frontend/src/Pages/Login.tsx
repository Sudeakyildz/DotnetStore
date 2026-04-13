import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { apiFetch, getStoredToken, readJson, setStoredToken, parseErrorMessage } from '../api/client';
import type { LoginResponse } from '../api/types';

const Login = () => {
  const navigate = useNavigate();
  useEffect(() => {
    if (getStoredToken()) navigate('/dashboard', { replace: true });
  }, [navigate]);
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      const res = await apiFetch('/api/Auth/login', {
        method: 'POST',
        body: JSON.stringify({ username, password }),
      });
      if (!res.ok) {
        setError(await parseErrorMessage(res));
        return;
      }
      const data = await readJson<LoginResponse>(res);
      setStoredToken(data.accessToken);
      navigate('/dashboard');
    } catch {
      setError('Sunucuya bağlanılamadı. API çalışıyor mu?');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-vh-100 d-flex align-items-center justify-content-center bg-light">
      <div className="card shadow" style={{ width: '100%', maxWidth: '400px' }}>
        <div className="card-body p-4">
          <h1 className="h4 mb-4 text-center">Yönetici girişi</h1>
          <p className="small text-muted text-center mb-3">
            İlk kurulumda seed kullanıcı: <strong>admin</strong> / <strong>admin123</strong>. Yeni hesap için{' '}
            <Link to="/register">kayıt</Link>.
          </p>
          {error && <div className="alert alert-danger py-2 small">{error}</div>}
          <form onSubmit={handleSubmit}>
            <div className="mb-3">
              <label className="form-label">Kullanıcı adı</label>
              <input
                type="text"
                className="form-control"
                autoComplete="username"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
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
            <button type="submit" className="btn btn-primary w-100" disabled={loading}>
              {loading ? 'Giriş…' : 'Giriş yap'}
            </button>
            <p className="text-center mt-3 mb-0 small">
              <Link to="/register">Hesap oluştur</Link>
            </p>
          </form>
        </div>
      </div>
    </div>
  );
};

export default Login;
