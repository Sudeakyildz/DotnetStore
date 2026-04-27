import { Navigate } from 'react-router-dom';
import { getStoredAuthProfile } from '../api/client';

type Props = {
  allow: readonly string[];
  children: React.ReactNode;
};

const RequireRole = ({ allow, children }: Props) => {
  const auth = getStoredAuthProfile();
  if (!auth || !allow.includes(auth.role)) {
    return <Navigate to="/dashboard" replace />;
  }
  return <>{children}</>;
};

export default RequireRole;
