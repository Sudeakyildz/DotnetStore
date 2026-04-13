import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { apiFetch, readJson, setStoredToken, parseErrorMessage } from '../api/client';
import type { LoginResponse } from '../api/types';

const Register = () => {
  const navigate = useNavigate();
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      const res = await apiFetch('/api/Auth/register', {
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
      setError('Sunucuya bağlanılamadı.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-vh-100 d-flex align-items-center justify-content-center bg-light">
      <div className="card shadow" style={{ width: '100%', maxWidth: '400px' }}>
        <div className="card-body p-4">
          <h1 className="h4 mb-4 text-center">Kayıt</h1>
          {error && <div className="alert alert-danger py-2 small">{error}</div>}
          <form onSubmit={handleSubmit}>
            <div className="mb-3">
              <label className="form-label">Kullanıcı adı</label>
              <input
                className="form-control"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                minLength={3}
                required
              />
            </div>
            <div className="mb-3">
              <label className="form-label">Şifre (min 6)</label>
              <input
                type="password"
                className="form-control"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                minLength={6}
                required
              />
            </div>
            <button type="submit" className="btn btn-primary w-100" disabled={loading}>
              {loading ? 'Kayıt…' : 'Kayıt ol ve giriş yap'}
            </button>
            <p className="text-center mt-3 mb-0 small">
              <Link to="/">Girişe dön</Link>
            </p>
          </form>
        </div>
      </div>
    </div>
  );
};

export default Register;
