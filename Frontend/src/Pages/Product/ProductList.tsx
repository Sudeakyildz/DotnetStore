import { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { apiFetch, readJson, parseErrorMessage } from '../../api/client';
import type { CategoryDto, ProductListItemDto } from '../../api/types';
import { productStatusLabel } from '../../lib/productStatus';
import { productImageSrc } from '../../lib/productImage';

type Filters = { q: string; categoryId: string; minPrice: string; maxPrice: string };

const ProductList = () => {
  const [products, setProducts] = useState<ProductListItemDto[]>([]);
  const [categories, setCategories] = useState<CategoryDto[]>([]);
  const [q, setQ] = useState('');
  const [categoryId, setCategoryId] = useState('');
  const [minPrice, setMinPrice] = useState('');
  const [maxPrice, setMaxPrice] = useState('');
  const [applied, setApplied] = useState<Filters>({
    q: '',
    categoryId: '',
    minPrice: '',
    maxPrice: '',
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadCategories = useCallback(async () => {
    try {
      const res = await apiFetch('/api/Categories');
      if (res.ok) {
        setCategories(await readJson<CategoryDto[]>(res));
      }
    } catch {
      /* ignore */
    }
  }, []);

  const runSearch = useCallback(async (f: Filters) => {
    setError(null);
    setLoading(true);
    try {
      const params = new URLSearchParams();
      if (f.q.trim()) params.set('q', f.q.trim());
      if (f.categoryId) params.set('categoryId', f.categoryId);
      if (f.minPrice !== '') params.set('minPrice', f.minPrice);
      if (f.maxPrice !== '') params.set('maxPrice', f.maxPrice);
      const qs = params.toString();
      const res = await apiFetch(`/api/Products${qs ? `?${qs}` : ''}`);
      if (!res.ok) {
        setError(await parseErrorMessage(res));
        return;
      }
      setProducts(await readJson<ProductListItemDto[]>(res));
    } catch {
      setError('Ürünler yüklenemedi.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadCategories();
  }, [loadCategories]);

  useEffect(() => {
    void runSearch(applied);
  }, [applied, runSearch]);

  const handleApply = () => {
    setApplied({ q, categoryId, minPrice, maxPrice });
  };

  const handleDelete = async (id: number, name: string) => {
    if (!window.confirm(`“${name}” ürününü silmek istediğinize emin misiniz?`)) return;
    try {
      const res = await apiFetch(`/api/Products/${id}`, { method: 'DELETE' });
      if (!res.ok) {
        alert(await parseErrorMessage(res));
        return;
      }
      await runSearch(applied);
    } catch {
      alert('Silme işlemi başarısız.');
    }
  };

  return (
    <div>
      <div className="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
        <h1 className="page-title h3 mb-0">Ürün listesi &amp; arama</h1>
        <Link to="/products/create" className="btn btn-primary">
          Yeni ürün
        </Link>
      </div>

      <div className="card filter-card mb-4">
        <div className="card-body row g-3 align-items-end">
          <div className="col-md-4">
            <label className="form-label">Metin araması</label>
            <input
              type="search"
              className="form-control"
              placeholder="Ad veya açıklama…"
              value={q}
              onChange={(e) => setQ(e.target.value)}
              onKeyDown={(e) => e.key === 'Enter' && handleApply()}
            />
          </div>
          <div className="col-md-3">
            <label className="form-label">Ürün grubu</label>
            <select className="form-select" value={categoryId} onChange={(e) => setCategoryId(e.target.value)}>
              <option value="">Tümü</option>
              {categories.map((c) => (
                <option key={c.id} value={String(c.id)}>
                  {c.name}
                </option>
              ))}
            </select>
          </div>
          <div className="col-md-2">
            <label className="form-label">Min fiyat</label>
            <input
              type="number"
              step="0.01"
              min="0"
              className="form-control"
              value={minPrice}
              onChange={(e) => setMinPrice(e.target.value)}
            />
          </div>
          <div className="col-md-2">
            <label className="form-label">Max fiyat</label>
            <input
              type="number"
              step="0.01"
              min="0"
              className="form-control"
              value={maxPrice}
              onChange={(e) => setMaxPrice(e.target.value)}
            />
          </div>
          <div className="col-md-1">
            <button type="button" className="btn btn-primary w-100" onClick={handleApply}>
              Uygula
            </button>
          </div>
        </div>
      </div>

      {error && <div className="alert alert-danger">{error}</div>}

      {loading ? (
        <p className="text-muted">Yükleniyor…</p>
      ) : (
        <div className="table-responsive card">
          <table className="table table-hover table-products mb-0 align-middle">
            <thead className="table-light">
              <tr>
                <th className="text-center" style={{ width: '72px' }}>
                  Görsel
                </th>
                <th>ID</th>
                <th>Ad</th>
                <th>Grup</th>
                <th>Stok</th>
                <th>Durum</th>
                <th>Fiyat</th>
                <th className="text-end">İşlemler</th>
              </tr>
            </thead>
            <tbody>
              {products.length === 0 ? (
                <tr>
                  <td colSpan={8} className="text-center text-muted py-4">
                    Sonuç yok.
                  </td>
                </tr>
              ) : (
                products.map((p) => (
                  <tr key={p.id}>
                    <td className="text-center">
                      {productImageSrc(p.imageUrl) ? (
                        <img
                          className="product-thumb"
                          src={productImageSrc(p.imageUrl)}
                          alt=""
                          loading="lazy"
                          referrerPolicy="no-referrer"
                        />
                      ) : (
                        <div
                          className="product-thumb mx-auto d-flex align-items-center justify-content-center small text-muted"
                          style={{ fontSize: '0.65rem' }}
                        >
                          —
                        </div>
                      )}
                    </td>
                    <td>{p.id}</td>
                    <td className="fw-medium">{p.name}</td>
                    <td>{p.categoryName ?? '—'}</td>
                    <td>{p.stock}</td>
                    <td>
                      <span className="badge rounded-pill text-bg-secondary">{productStatusLabel(p.status)}</span>
                    </td>
                    <td>
                      {p.activePrice != null ? `${p.activePrice.toLocaleString('tr-TR')} ₺` : '—'}
                    </td>
                    <td className="text-end">
                      <Link to={`/products/edit/${p.id}`} className="btn btn-sm btn-outline-secondary me-1">
                        Düzenle
                      </Link>
                      <button
                        type="button"
                        className="btn btn-sm btn-outline-danger"
                        onClick={() => void handleDelete(p.id, p.name)}
                      >
                        Sil
                      </button>
                    </td>
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

export default ProductList;
