import { useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { apiJson } from '../../api/client';
import type { PagedResult, StoreCategory, StoreProduct } from '../../api/shopTypes';
import ProductCard from '../components/ProductCard';

const PAGE_SIZE = 12;

const Catalog = () => {
  const [params, setParams] = useSearchParams();
  const q = params.get('q') ?? '';
  const categoryId = params.get('categoryId') ?? '';
  const minPrice = params.get('minPrice') ?? '';
  const maxPrice = params.get('maxPrice') ?? '';
  const sort = params.get('sort') ?? 'newest';
  const inStock = params.get('inStock') === '1';
  const page = Math.max(1, Number(params.get('page') ?? '1') || 1);

  const [categories, setCategories] = useState<StoreCategory[]>([]);
  const [minDraft, setMinDraft] = useState(minPrice);
  const [maxDraft, setMaxDraft] = useState(maxPrice);

  const qs = new URLSearchParams();
  if (q) qs.set('q', q);
  if (categoryId) qs.set('categoryId', categoryId);
  if (minPrice) qs.set('minPrice', minPrice);
  if (maxPrice) qs.set('maxPrice', maxPrice);
  if (inStock) qs.set('inStockOnly', 'true');
  qs.set('sort', sort);
  qs.set('page', String(page));
  qs.set('pageSize', String(PAGE_SIZE));
  const queryKey = qs.toString();

  // Sonuç, hangi sorguya ait olduğuyla saklanır; sorgu değişince eski sonuç "yükleniyor" sayılır.
  const [result, setResult] = useState<{ key: string; data: PagedResult<StoreProduct>; error: string | null } | null>(null);
  const data = result?.key === queryKey ? result.data : null;
  const error = result?.key === queryKey ? result.error : null;

  useEffect(() => {
    void (async () => {
      try {
        setCategories(await apiJson<StoreCategory[]>('/api/store/categories'));
      } catch {
        /* filtre olmadan devam */
      }
    })();
  }, []);

  useEffect(() => {
    let cancelled = false;
    void (async () => {
      try {
        const r = await apiJson<PagedResult<StoreProduct>>(`/api/store/products?${queryKey}`);
        if (!cancelled) setResult({ key: queryKey, data: r, error: null });
      } catch (e) {
        if (!cancelled) {
          setResult({
            key: queryKey,
            data: { items: [], total: 0, page: 1, pageSize: PAGE_SIZE },
            error: e instanceof Error ? e.message : 'Ürünler yüklenemedi.',
          });
        }
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [queryKey]);

  const update = (changes: Record<string, string | null>) => {
    const next = new URLSearchParams(params);
    for (const [k, v] of Object.entries(changes)) {
      if (v === null || v === '') next.delete(k);
      else next.set(k, v);
    }
    if (!('page' in changes)) next.delete('page');
    setParams(next);
  };

  const totalPages = data ? Math.max(1, Math.ceil(data.total / data.pageSize)) : 1;
  const activeCat = categories.find((c) => String(c.id) === categoryId);

  return (
    <div className="container py-4">
      <div className="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
        <div>
          <h1 className="h3 m-0">{q ? `“${q}” için sonuçlar` : activeCat ? activeCat.name : 'Tüm ürünler'}</h1>
          {data && <small className="text-muted">{data.total} ürün bulundu</small>}
        </div>
        <select
          className="form-select w-auto"
          value={sort}
          onChange={(e) => update({ sort: e.target.value })}
          aria-label="Sırala"
        >
          <option value="newest">En yeni</option>
          <option value="priceAsc">Fiyat: artan</option>
          <option value="priceDesc">Fiyat: azalan</option>
          <option value="name">İsme göre (A-Z)</option>
        </select>
      </div>

      <div className="row g-4">
        <aside className="col-lg-3">
          <div className="filter-box">
            <h2 className="h6">Kategori</h2>
            <div className="list-group list-group-flush mb-3">
              <button
                type="button"
                className={`list-group-item list-group-item-action ${!categoryId ? 'active' : ''}`}
                onClick={() => update({ categoryId: null })}
              >
                Tümü
              </button>
              {categories.map((c) => (
                <button
                  key={c.id}
                  type="button"
                  className={`list-group-item list-group-item-action d-flex justify-content-between ${String(c.id) === categoryId ? 'active' : ''}`}
                  onClick={() => update({ categoryId: String(c.id) })}
                >
                  {c.name} <span className="badge text-bg-light">{c.productCount}</span>
                </button>
              ))}
            </div>

            <h2 className="h6">Fiyat aralığı (₺)</h2>
            <form
              className="d-flex gap-2 mb-3"
              onSubmit={(e) => {
                e.preventDefault();
                update({ minPrice: minDraft, maxPrice: maxDraft });
              }}
            >
              <input className="form-control form-control-sm" type="number" min={0} placeholder="En az"
                value={minDraft} onChange={(e) => setMinDraft(e.target.value)} />
              <input className="form-control form-control-sm" type="number" min={0} placeholder="En çok"
                value={maxDraft} onChange={(e) => setMaxDraft(e.target.value)} />
              <button className="btn btn-sm btn-dark" type="submit">Uygula</button>
            </form>

            <div className="form-check mb-3">
              <input className="form-check-input" type="checkbox" id="instock" checked={inStock}
                onChange={(e) => update({ inStock: e.target.checked ? '1' : null })} />
              <label className="form-check-label" htmlFor="instock">Sadece stokta olanlar</label>
            </div>

            {(q || categoryId || minPrice || maxPrice || inStock) && (
              <button
                type="button"
                className="btn btn-link btn-sm p-0"
                onClick={() => {
                  setMinDraft('');
                  setMaxDraft('');
                  setParams(new URLSearchParams());
                }}
              >
                Filtreleri temizle
              </button>
            )}
          </div>
        </aside>

        <div className="col-lg-9">
          {error && <div className="alert alert-danger">{error}</div>}
          {data === null ? (
            <div className="text-muted">Yükleniyor…</div>
          ) : data.items.length === 0 ? (
            <div className="empty-state">
              <div className="fs-1" aria-hidden>🔍</div>
              <strong>Aramana uygun ürün bulunamadı</strong>
              <div className="text-muted">Filtreleri değiştirmeyi dene.</div>
            </div>
          ) : (
            <div className="row g-3">
              {data.items.map((p) => (
                <div className="col-6 col-md-4" key={p.id}>
                  <ProductCard p={p} />
                </div>
              ))}
            </div>
          )}

          {totalPages > 1 && (
            <nav className="mt-4 d-flex justify-content-center" aria-label="Sayfalar">
              <ul className="pagination">
                <li className={`page-item ${page <= 1 ? 'disabled' : ''}`}>
                  <button className="page-link" type="button" onClick={() => update({ page: String(page - 1) })}>Önceki</button>
                </li>
                {Array.from({ length: totalPages }, (_, i) => i + 1).map((n) => (
                  <li key={n} className={`page-item ${n === page ? 'active' : ''}`}>
                    <button className="page-link" type="button" onClick={() => update({ page: String(n) })}>{n}</button>
                  </li>
                ))}
                <li className={`page-item ${page >= totalPages ? 'disabled' : ''}`}>
                  <button className="page-link" type="button" onClick={() => update({ page: String(page + 1) })}>Sonraki</button>
                </li>
              </ul>
            </nav>
          )}
        </div>
      </div>
    </div>
  );
};

export default Catalog;
