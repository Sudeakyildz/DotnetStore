import { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { apiFetch, readJson, parseErrorMessage, getStoredAuthProfile } from '../../api/client';
import { AuthRoles } from '../../lib/authRoles';
import type { FeatureDto } from '../../api/types';
import { featureDataTypeLabel } from '../../lib/featureDataType';

const FeatureList = () => {
  const role = getStoredAuthProfile()?.role ?? '';
  const canEdit = role === AuthRoles.Admin || role === AuthRoles.StaffFeatures;

  const [items, setItems] = useState<FeatureDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setError(null);
    setLoading(true);
    try {
      const res = await apiFetch('/api/Features');
      if (!res.ok) {
        setError(await parseErrorMessage(res));
        return;
      }
      setItems(await readJson<FeatureDto[]>(res));
    } catch {
      setError('Liste yüklenemedi.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void load();
  }, [load]);

  const handleDelete = async (id: number, name: string) => {
    if (!window.confirm(`“${name}” özelliğini silmek istediğinize emin misiniz?`)) return;
    try {
      const res = await apiFetch(`/api/Features/${id}`, { method: 'DELETE' });
      if (!res.ok) {
        alert(await parseErrorMessage(res));
        return;
      }
      await load();
    } catch {
      alert('Silme işlemi başarısız.');
    }
  };

  if (loading) {
    return <p className="text-muted">Yükleniyor…</p>;
  }

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h1 className="h3 mb-0">Ürün özellikleri</h1>
        {canEdit && (
          <Link to="/features/create" className="btn btn-primary">
            Yeni özellik
          </Link>
        )}
      </div>
      {error && <div className="alert alert-danger">{error}</div>}
      <div className="table-responsive card shadow-sm">
        <table className="table table-hover mb-0">
          <thead className="table-light">
            <tr>
              <th>ID</th>
              <th>Ad</th>
              <th>Veri tipi</th>
              <th className="text-end">İşlemler</th>
            </tr>
          </thead>
          <tbody>
            {items.length === 0 ? (
              <tr>
                <td colSpan={4} className="text-center text-muted py-4">
                  Kayıt yok.
                </td>
              </tr>
            ) : (
              items.map((f) => (
                <tr key={f.id}>
                  <td>{f.id}</td>
                  <td>{f.name}</td>
                  <td>
                    <span className="badge text-bg-secondary">{featureDataTypeLabel(f.dataType)}</span>
                  </td>
                  <td className="text-end">
                    {canEdit && (
                      <button
                        type="button"
                        className="btn btn-sm btn-outline-danger"
                        onClick={() => void handleDelete(f.id, f.name)}
                      >
                        Sil
                      </button>
                    )}
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default FeatureList;
