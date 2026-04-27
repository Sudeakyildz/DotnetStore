import { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { apiFetch, readJson, parseErrorMessage } from '../api/client';
import type { AuditLogListItemDto } from '../api/types';

const actionLabel = (code: string) => {
  const map: Record<string, string> = {
    Login: 'Giriş',
    Logout: 'Çıkış',
    ProductCreate: 'Ürün oluşturma',
    ProductUpdate: 'Ürün güncelleme',
    ProductDelete: 'Ürün silme',
    ProductPriceUpdate: 'Fiyat güncelleme',
    ProductFeaturesUpdate: 'Ürün özellikleri güncelleme',
    CategoryCreate: 'Grup oluşturma',
    CategoryUpdate: 'Grup güncelleme',
    CategoryDelete: 'Grup silme',
    FeatureCreate: 'Özellik oluşturma',
    FeatureUpdate: 'Özellik güncelleme',
    FeatureDelete: 'Özellik silme',
  };
  return map[code] ?? code;
};

const formatUtc = (iso: string) => {
  try {
    const d = new Date(iso);
    return d.toLocaleString('tr-TR', { timeZone: 'UTC' }) + ' UTC';
  } catch {
    return iso;
  }
};

const AdminActivity = () => {
  const [rows, setRows] = useState<AuditLogListItemDto[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await apiFetch('/api/AuditLogs?take=400');
      if (!res.ok) {
        setError(await parseErrorMessage(res));
        setRows([]);
        return;
      }
      setRows(await readJson<AuditLogListItemDto[]>(res));
    } catch {
      setError('Kayıtlar yüklenemedi.');
      setRows([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void load();
  }, [load]);

  return (
    <div>
      <h1 className="h3 mb-2">İşlem kayıtları</h1>
      <p className="text-muted small mb-3">
        <strong>Giriş / çıkış:</strong> Panelde &quot;Çıkış&quot; dediğinizde sunucuya bildirim gider ve burada
        görünür; sekme kapatma tek başına çıkış kaydı oluşturmaz. <strong>Güncelleme:</strong> ürün, grup ve
        özellik işlemleri kayda yazılır. Kullanıcıların{' '}
        <Link to="/admin/users">ilk / son giriş</Link> tarihleri ayrı listede.
      </p>
      {error && <div className="alert alert-danger">{error}</div>}
      {loading ? (
        <p className="text-muted">Yükleniyor…</p>
      ) : (
        <div className="table-responsive card shadow-sm">
          <table className="table table-sm table-striped mb-0 align-middle">
            <thead className="table-light">
              <tr>
                <th>Zaman (UTC)</th>
                <th>Kullanıcı</th>
                <th>İşlem</th>
                <th>Detay</th>
              </tr>
            </thead>
            <tbody>
              {(rows ?? []).length === 0 ? (
                <tr>
                  <td colSpan={4} className="text-muted text-center py-4">
                    Kayıt yok.
                  </td>
                </tr>
              ) : (
                (rows ?? []).map((r) => (
                  <tr key={r.id}>
                    <td className="small text-nowrap">{formatUtc(r.createdAtUtc)}</td>
                    <td className="small">
                      <div>{r.userName}</div>
                      <div className="text-muted">{r.userEmail}</div>
                    </td>
                    <td>
                      <span className="badge text-bg-secondary">{actionLabel(r.action)}</span>
                    </td>
                    <td className="small">{r.details ?? '—'}</td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};

export default AdminActivity;
