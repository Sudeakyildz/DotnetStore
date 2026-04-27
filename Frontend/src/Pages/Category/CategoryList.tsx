import { useCallback, useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { apiFetch, readJson, parseErrorMessage, getStoredAuthProfile } from '../../api/client';
import { AuthRoles } from '../../lib/authRoles';
import type { CategoryDto } from '../../api/types';

const SWATCHES = [
  { bg: '#ecfdf5', fg: '#047857', border: '#a7f3d0' },
  { bg: '#eff6ff', fg: '#1d4ed8', border: '#bfdbfe' },
  { bg: '#faf5ff', fg: '#6b21a8', border: '#e9d5ff' },
  { bg: '#fff7ed', fg: '#c2410c', border: '#fed7aa' },
  { bg: '#f0fdfa', fg: '#0f766e', border: '#99f6e4' },
];

function swatchForId(id: number) {
  return SWATCHES[Math.abs(id) % SWATCHES.length];
}

const CategoryList = () => {
  const role = getStoredAuthProfile()?.role ?? '';
  const canEdit = role === AuthRoles.Admin || role === AuthRoles.StaffCategories;

  const [items, setItems] = useState<CategoryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setError(null);
    setLoading(true);
    try {
      const res = await apiFetch('/api/Categories');
      if (!res.ok) {
        setError(await parseErrorMessage(res));
        return;
      }
      setItems(await readJson<CategoryDto[]>(res));
    } catch {
      setError('Liste yüklenemedi.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void load();
  }, [load]);

  const sorted = useMemo(() => [...items].sort((a, b) => a.name.localeCompare(b.name, 'tr')), [items]);

  const handleDelete = async (id: number, name: string) => {
    if (!window.confirm(`“${name}” grubunu silmek istediğinize emin misiniz?`)) return;
    try {
      const res = await apiFetch(`/api/Categories/${id}`, { method: 'DELETE' });
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
    return (
      <div className="category-page">
        <div className="skeleton-line mb-3" style={{ maxWidth: '280px' }} />
        <div className="skeleton-line skeleton-line--tall" />
      </div>
    );
  }

  return (
    <div className="category-page">
      <header className="page-header-block mb-4">
        <div>
          <h1 className="page-title h4 mb-1">Ürün grupları</h1>
          <p className="text-muted small mb-0">Katalog hiyerarşinizi buradan yönetin.</p>
        </div>
        {canEdit && (
          <Link to="/categories/create" className="btn btn-admin-primary">
            Yeni grup
          </Link>
        )}
      </header>

      {error && <div className="alert alert-danger border-0 shadow-sm">{error}</div>}

      <div className="table-card table-card--categories">
        <div className="table-responsive">
          <table className="table table-categories align-middle mb-0">
            <thead>
              <tr>
                <th scope="col" className="ps-4">
                  Grup
                </th>
                <th scope="col">Slug</th>
                <th scope="col" className="d-none d-md-table-cell">
                  Oluşturulma
                </th>
                <th scope="col" className="text-end pe-4">
                  İşlemler
                </th>
              </tr>
            </thead>
            <tbody>
              {sorted.length === 0 ? (
                <tr>
                  <td colSpan={4} className="text-center text-muted py-5">
                    Henüz grup yok. Yeni grup ekleyerek başlayın.
                  </td>
                </tr>
              ) : (
                sorted.map((c) => {
                  const s = swatchForId(c.id);
                  const initial = c.name.trim().charAt(0).toUpperCase() || '?';
                  return (
                    <tr key={c.id}>
                      <td className="ps-4">
                        <div className="d-flex align-items-center gap-3">
                          <div
                            className="category-avatar flex-shrink-0"
                            style={{
                              background: s.bg,
                              color: s.fg,
                              borderColor: s.border,
                            }}
                            aria-hidden
                          >
                            {initial}
                          </div>
                          <div>
                            <div className="fw-semibold text-dark">{c.name}</div>
                            {c.description ? (
                              <div className="small text-muted text-truncate" style={{ maxWidth: 'min(360px, 50vw)' }}>
                                {c.description}
                              </div>
                            ) : null}
                          </div>
                        </div>
                      </td>
                      <td>
                        {c.slug ? (
                          <span className="slug-pill">{c.slug}</span>
                        ) : (
                          <span className="text-muted">—</span>
                        )}
                      </td>
                      <td className="d-none d-md-table-cell text-muted small">
                        {new Date(c.createdAt).toLocaleString('tr-TR')}
                      </td>
                      <td className="text-end pe-4">
                        {canEdit ? (
                          <>
                            <Link
                              to={`/categories/edit/${c.id}`}
                              className="btn btn-sm btn-outline-secondary btn-table-action me-1"
                            >
                              Düzenle
                            </Link>
                            <button
                              type="button"
                              className="btn btn-sm btn-outline-danger btn-table-action"
                              onClick={() => void handleDelete(c.id, c.name)}
                            >
                              Sil
                            </button>
                          </>
                        ) : (
                          <span className="text-muted small">Salt okunur</span>
                        )}
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

export default CategoryList;
