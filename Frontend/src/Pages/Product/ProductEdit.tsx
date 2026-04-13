import { useCallback, useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { apiFetch, readJson, parseErrorMessage } from '../../api/client';
import type { CategoryDto, FeatureDto, ProductDetailDto } from '../../api/types';
import { ProductStatus } from '../../lib/productStatus';
import { productImageSrc } from '../../lib/productImage';

const ProductEdit = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [categories, setCategories] = useState<CategoryDto[]>([]);
  const [features, setFeatures] = useState<FeatureDto[]>([]);
  const [categoryId, setCategoryId] = useState('');
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [stock, setStock] = useState('0');
  const [status, setStatus] = useState(String(ProductStatus.Active));
  const [imageUrl, setImageUrl] = useState('');
  const [newPrice, setNewPrice] = useState('');
  const [currentPriceLabel, setCurrentPriceLabel] = useState('');
  const [featureInputs, setFeatureInputs] = useState<Record<number, string>>({});
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    if (!id) return;
    setLoading(true);
    setError(null);
    try {
      const [cRes, fRes, pRes] = await Promise.all([
        apiFetch('/api/Categories'),
        apiFetch('/api/Features'),
        apiFetch(`/api/Products/${id}`),
      ]);
      if (cRes.ok) setCategories(await readJson<CategoryDto[]>(cRes));
      if (fRes.ok) setFeatures(await readJson<FeatureDto[]>(fRes));
      if (pRes.status === 404) {
        navigate('/products', { replace: true });
        return;
      }
      if (!pRes.ok) {
        setError(await parseErrorMessage(pRes));
        return;
      }
      const p = await readJson<ProductDetailDto>(pRes);
      setCategoryId(String(p.categoryId));
      setName(p.name);
      setDescription(p.description ?? '');
      setStock(String(p.stock));
      setStatus(String(p.status));
      setImageUrl(p.imageUrl ?? '');
      setCurrentPriceLabel(
        p.activePrice != null ? `${p.activePrice.toLocaleString('tr-TR')} ₺` : '—',
      );
      const fv: Record<number, string> = {};
      for (const row of p.featureValues ?? []) {
        fv[row.featureId] = row.value;
      }
      setFeatureInputs(fv);
      setNewPrice('');
    } catch {
      setError('Veri yüklenemedi.');
    } finally {
      setLoading(false);
    }
  }, [id, navigate]);

  useEffect(() => {
    void load();
  }, [load]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!id) return;

    const featureValues = features
      .map((f) => ({
        featureId: f.id,
        value: (featureInputs[f.id] ?? '').trim(),
      }))
      .filter((x) => x.value.length > 0);

    let newPriceVal: number | null = null;
    if (newPrice.trim() !== '') {
      const n = Number(newPrice);
      if (Number.isNaN(n) || n < 0) {
        setError('Yeni fiyat geçersiz.');
        return;
      }
      newPriceVal = n;
    }

    setError(null);
    setSaving(true);
    try {
      const res = await apiFetch(`/api/Products/${id}`, {
        method: 'PUT',
        body: JSON.stringify({
          categoryId: Number(categoryId),
          name,
          description: description || null,
          stock: Number(stock) || 0,
          status: Number(status),
          imageUrl: imageUrl || null,
          newPrice: newPriceVal,
          featureValues,
        }),
      });
      if (!res.ok) {
        setError(await parseErrorMessage(res));
        return;
      }
      navigate('/products');
    } catch {
      setError('Güncelleme başarısız.');
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return <p className="text-muted">Yükleniyor…</p>;
  }

  return (
    <div>
      <div className="mb-3">
        <Link to="/products" className="small text-decoration-none">
          ← Ürün listesine dön
        </Link>
      </div>
      <h1 className="h3 mb-4">Ürün güncelleme</h1>
      {error && <div className="alert alert-danger">{error}</div>}
      <form onSubmit={handleSubmit} className="card shadow-sm p-4" style={{ maxWidth: '640px' }}>
        <div className="mb-3">
          <label className="form-label">Ürün grubu *</label>
          <select
            className="form-select"
            required
            value={categoryId}
            onChange={(e) => setCategoryId(e.target.value)}
          >
            {categories.map((c) => (
              <option key={c.id} value={String(c.id)}>
                {c.name}
              </option>
            ))}
          </select>
        </div>
        <div className="mb-3">
          <label className="form-label">Ürün adı *</label>
          <input className="form-control" value={name} onChange={(e) => setName(e.target.value)} required />
        </div>
        <div className="mb-3">
          <label className="form-label">Açıklama</label>
          <textarea
            className="form-control"
            rows={3}
            value={description}
            onChange={(e) => setDescription(e.target.value)}
          />
        </div>
        <div className="row g-3 mb-3">
          <div className="col-md-4">
            <label className="form-label">Stok *</label>
            <input
              type="number"
              min={0}
              className="form-control"
              value={stock}
              onChange={(e) => setStock(e.target.value)}
              required
            />
          </div>
          <div className="col-md-4">
            <label className="form-label">Durum</label>
            <select className="form-select" value={status} onChange={(e) => setStatus(e.target.value)}>
              <option value={String(ProductStatus.Active)}>Aktif</option>
              <option value={String(ProductStatus.Inactive)}>Pasif</option>
              <option value={String(ProductStatus.Draft)}>Taslak</option>
            </select>
          </div>
          <div className="col-md-4">
            <label className="form-label">Güncel fiyat</label>
            <div className="form-control bg-body-secondary">{currentPriceLabel}</div>
          </div>
        </div>
        <div className="mb-3">
          <label className="form-label">Yeni fiyat (opsiyonel)</label>
          <input
            type="number"
            step="0.01"
            min="0"
            className="form-control"
            placeholder="Boş bırakılırsa fiyat değişmez"
            value={newPrice}
            onChange={(e) => setNewPrice(e.target.value)}
          />
        </div>
        <div className="mb-3">
          <label className="form-label">Görsel URL</label>
          <input type="url" className="form-control" value={imageUrl} onChange={(e) => setImageUrl(e.target.value)} />
          {productImageSrc(imageUrl) && (
            <div className="mt-2 d-inline-block">
              <img
                src={productImageSrc(imageUrl)}
                alt="Önizleme"
                className="rounded border"
                style={{ maxHeight: 140, maxWidth: '100%', objectFit: 'cover' }}
                referrerPolicy="no-referrer"
              />
            </div>
          )}
        </div>

        {features.length > 0 && (
          <div className="mb-3">
            <label className="form-label">Ürün özellikleri</label>
            {features.map((f) => (
              <div key={f.id} className="input-group mb-2">
                <span className="input-group-text" style={{ minWidth: '8rem' }}>
                  {f.name}
                </span>
                <input
                  type="text"
                  className="form-control"
                  value={featureInputs[f.id] ?? ''}
                  onChange={(e) =>
                    setFeatureInputs((prev) => ({
                      ...prev,
                      [f.id]: e.target.value,
                    }))
                  }
                />
              </div>
            ))}
          </div>
        )}

        <button type="submit" className="btn btn-primary" disabled={saving}>
          {saving ? 'Kaydediliyor…' : 'Güncelle'}
        </button>
      </form>
    </div>
  );
};

export default ProductEdit;
