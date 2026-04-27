import { useCallback, useEffect, useState } from 'react';
import { apiFetch, readJson, parseErrorMessage } from '../api/client';
import type { UserListItemDto } from '../api/types';

const formatDt = (iso: string | null) => {
  if (!iso) return '—';
  try {
    return new Date(iso).toLocaleString('tr-TR');
  } catch {
    return iso;
  }
};

const AdminUsers = () => {
  const [rows, setRows] = useState<UserListItemDto[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await apiFetch('/api/Users');
      if (!res.ok) {
        setError(await parseErrorMessage(res));
        setRows([]);
        return;
      }
      setRows(await readJson<UserListItemDto[]>(res));
    } catch {
      setError('Liste yüklenemedi.');
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
      <h1 className="h3 mb-4">Kullanıcılar ve oturum bilgileri</h1>
      <p className="text-muted small mb-3">
        Burada her hesabın <strong>ilk giriş</strong> ve <strong>son giriş</strong> zamanlarını (UTC kaydı, yerel
        saate çevrilmiş gösterim) görebilirsiniz. Hesap oluşturulma ve güncellenme tarihleri de listelenir.
      </p>
      {error && <div className="alert alert-danger">{error}</div>}
      {loading ? (
        <p className="text-muted">Yükleniyor…</p>
      ) : (
        <div className="table-responsive card shadow-sm">
          <table className="table table-sm table-striped mb-0 align-middle">
            <thead className="table-light">
              <tr>
                <th>E-posta</th>
                <th>Görünen ad</th>
                <th>Rol</th>
                <th>İlk giriş</th>
                <th>Son giriş</th>
                <th>Oluşturulma</th>
              </tr>
            </thead>
            <tbody>
              {(rows ?? []).map((u) => (
                <tr key={u.id}>
                  <td>{u.email}</td>
                  <td>{u.userName}</td>
                  <td>
                    <span className="badge text-bg-secondary">{u.role}</span>
                  </td>
                  <td className="small">{formatDt(u.firstLoginAt)}</td>
                  <td className="small">{formatDt(u.lastLoginAt)}</td>
                  <td className="small">{formatDt(u.createdAt)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};

export default AdminUsers;
