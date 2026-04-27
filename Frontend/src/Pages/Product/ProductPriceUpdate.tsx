import { useCallback, useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { apiFetch, readJson, parseErrorMessage, getStoredAuthProfile } from '../../api/client';
import type { ProductDetailDto } from '../../api/types';
import { AuthRoles } from '../../lib/authRoles';

const ProductPriceUpdate = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const role = getStoredAuthProfile()?.role ?? '';
  const allowed = role === AuthRoles.Admin || role === AuthRoles.StaffPrices;

  const [product, setProduct] = useState<ProductDetailDto | null>(null);
  const [newPrice, setNewPrice] = useState('');
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    if (!id) return;
    setLoading(true);
    setError(null);
    try {
      const res = await apiFetch(`/api/Products/${id}`);
      if (res.status === 404) {
        navigate('/products', { replace: true });
        return;
      }
      if (!res.ok) {
        setError(await parseErrorMessage(res));
        setProduct(null);
        return;
      }
      const p = await readJson<ProductDetailDto>(res);
      setProduct(p);
      setNewPrice('');
    } catch {
      setError('Ürün yüklenemedi.');
      setProduct(null);
    } finally {
      setLoading(false);
    }
  }, [id, navigate]);

  useEffect(() => {
    if (!allowed) {
      navigate('/products', { replace: true });
      return;
    }
    void load();
  }, [allowed, load, navigate]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!id) return;
    const n = Number(newPrice);
    if (newPrice.trim() === '') {
      setError('Yeni fiyat girmelisiniz.');
      return;
    }
    if (Number.isNaN(n) || n < 0) {
      setError('Geçerli bir fiyat girin.');
      return;
    }
    setError(null);
    setSaving(true);
    try {
      const res = await apiFetch(`/api/Products/${id}/price`, {
        method: 'PUT',
        body: JSON.stringify({ newPrice: n }),
      });
      if (!res.ok) {
        setError(await parseErrorMessage(res));
        return;
      }
      navigate('/products');
    } catch {
      setError('Fiyat kaydedilemedi.');
    } finally {
      setSaving(false);
    }
  };

  if (!allowed) {
    return null;
  }

  if (loading) {
    return <p className="text-muted">Yükleniyor…</p>;
  }

  return (
    <div style={{ maxWidth: '480px' }}>
      <div className="mb-3">
        <Link to="/products" className="small text-decoration-none">
          ← Ürün listesine dön
        </Link>
      </div>
      <h1 className="h4 mb-2">Fiyat güncelleme</h1>
      <p className="text-muted small mb-4">
        Yalnızca güncel fiyat kaydı oluşturulur; ürün adı, stok ve diğer alanlar değişmez.
      </p>

      {product && (
        <div className="card border-0 shadow-sm mb-3">
          <div className="card-body">
            <div className="small text-muted mb-1">Ürün</div>
            <div className="fw-semibold">{product.name}</div>
            <div className="small text-secondary mt-2">
              Güncel fiyat:{' '}
              <span className="text-dark fw-medium">
                {product.activePrice != null
                  ? `${product.activePrice.toLocaleString('tr-TR')} ₺`
                  : '—'}
              </span>
            </div>
          </div>
        </div>
      )}

      {error && <div className="alert alert-danger py-2 small">{error}</div>}

      <form onSubmit={(e) => void handleSubmit(e)} className="card shadow-sm p-4">
        <div className="mb-3">
          <label className="form-label fw-semibold" htmlFor="newPrice">
            Yeni fiyat (₺) *
          </label>
          <input
            id="newPrice"
            type="number"
            step="0.01"
            min="0"
            className="form-control form-control-lg"
            placeholder="0,00"
            value={newPrice}
            onChange={(e) => setNewPrice(e.target.value)}
            autoComplete="off"
            required
          />
        </div>
        <div className="d-flex flex-wrap gap-2">
          <button type="submit" className="btn btn-primary" disabled={saving}>
            {saving ? 'Kaydediliyor…' : 'Fiyatı kaydet'}
          </button>
          <Link to={`/products/edit/${id}`} className="btn btn-outline-secondary">
            Tüm ürün alanları
          </Link>
        </div>
      </form>
    </div>
  );
};

export default ProductPriceUpdate;
