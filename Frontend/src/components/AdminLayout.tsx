import { Link, Outlet } from 'react-router-dom';
import { getStoredAuthProfile } from '../api/client';
import { AuthRoles } from '../lib/authRoles';
import { formatMoney } from '../lib/format';
import Navbar from './Navbar';
import Sidebar from './Sidebar';
import { useOrderAlerts } from './useOrderAlerts';

const AdminLayout = () => {
  const isAdmin = getStoredAuthProfile()?.role === AuthRoles.Admin;
  const { pendingCount, alerts, dismiss } = useOrderAlerts(isAdmin);

  return (
    <div className="admin-app d-flex flex-column min-vh-100">
      <Navbar />
      <div className="d-flex flex-grow-1">
        <Sidebar pendingOrders={pendingCount} />
        <main className="admin-main flex-grow-1 p-4 p-md-5 overflow-auto">
          <Outlet />
        </main>
      </div>

      <div style={{ position: 'fixed', top: 70, right: 16, zIndex: 2500, width: 320, maxWidth: 'calc(100vw - 32px)' }} aria-live="polite">
        {alerts.map((a) => (
          <div key={a.id} className="alert alert-success shadow d-flex gap-2 align-items-start py-2 mb-2">
            <div className="flex-grow-1">
              <strong>🛒 Yeni sipariş #{a.id}</strong>
              <div className="small">
                {a.customerName ?? 'Müşteri'} · {formatMoney(a.total)}
              </div>
              <Link to="/admin/orders?status=3" className="small" onClick={() => dismiss(a.id)}>
                Siparişlere git →
              </Link>
            </div>
            <button type="button" className="btn-close" aria-label="Kapat" onClick={() => dismiss(a.id)} />
          </div>
        ))}
      </div>
    </div>
  );
};

export default AdminLayout;
