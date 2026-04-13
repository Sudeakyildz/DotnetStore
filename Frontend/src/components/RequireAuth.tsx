import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { getStoredToken } from '../api/client';

const RequireAuth = () => {
  const location = useLocation();
  if (!getStoredToken()) {
    return <Navigate to="/" replace state={{ from: location.pathname }} />;
  }
  return <Outlet />;
};

export default RequireAuth;
