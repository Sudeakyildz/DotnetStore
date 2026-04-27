import { useEffect } from 'react';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import {
  apiFetch,
  getStoredAuthProfile,
  getStoredToken,
  readJson,
  setStoredAuthProfile,
} from '../api/client';
import type { MeResponse } from '../api/types';

const RequireAuth = () => {
  const location = useLocation();
  const token = getStoredToken();

  useEffect(() => {
    if (!token || getStoredAuthProfile()) return;
    void (async () => {
      try {
        const res = await apiFetch('/api/Auth/me');
        if (!res.ok) return;
        const me = await readJson<MeResponse>(res);
        setStoredAuthProfile({
          email: me.email,
          userName: me.userName,
          role: me.role,
        });
      } catch {
        /* ignore */
      }
    })();
  }, [token]);

  if (!token) {
    return <Navigate to="/" replace state={{ from: location.pathname }} />;
  }
  return <Outlet />;
};

export default RequireAuth;
