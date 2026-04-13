import { useCallback, useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { apiFetch, readJson, parseErrorMessage } from '../../api/client';
import type { CategoryDto, FeatureDto } from '../../api/types';
import { ProductStatus } from '../../lib/productStatus';
import { featureDataTypeLabel } from '../../lib/featureDataType';
import { productImageSrc } from '../../lib/productImage';

type FeatureInput = { featureId: number; value: string };

const ProductCreate = () => {
  const navigate = useNavigate();
  const [categories, setCategories] = useState<CategoryDto[]>([]);
  const [features, setFeatures] = useState<FeatureDto[]>([]);
  const [categoryId, setCategoryId] = useState('');
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [stock, setStock] = useState('0');
  const [status, setStatus] = useState(String(ProductStatus.Active));
  const [imageUrl, setImageUrl] = useState('');
  const [price, setPrice] = useState('');
  const [featureInputs, setFeatureInputs] = useState<Record<number, string>>({});
  const [loadingMeta, setLoadingMeta] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadMeta = useCallback(async () => {
    setLoadingMeta(true);
    setError(null);
    try {
      const [cRes, fRes] = await Promise.all([apiFetch('/api/Categories'), apiFetch('/api/Features')]);
      if (cRes.ok) setCategories(await readJson<CategoryDto[]>(cRes));
      if (fRes.ok) setFeatures(await readJson<FeatureDto[]>(fRes));
    } catch {
      setError('Grup veya özellik listesi yüklenemedi.');
    } finally {
      setLoadingMeta(false);
    }
  }, []);

  useEffect(() => {
    void loadMeta();
  }, [loadMeta]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!categoryId) {
      setError('Ürün grubu seçin.');
      return;
    }
    const priceNum = Number(price);
    if (Number.isNaN(priceNum) || priceNum < 0) {
      setError('Geçerli bir fiyat girin.');
      return;
    }

    const featureValues: FeatureInput[] = [];
    for (const f of features) {
      const v = (featureInputs[f.id] ?? '').trim();
      if (v) featureValues.push({ featureId: f.id, value: v });
    }

    setError(null);
    setSaving(true);
    try {
      const res = await apiFetch('/api/Products', {
        method: 'POST',
        body: JSON.stringify({
          categoryId: Number(categoryId),
          name,
          description: description || null,
          stock: Number(stock) || 0,
          status: Number(status),
          imageUrl: imageUrl || null,
          price: priceNum,
          featureValues: featureValues.length ? featureValues : null,
        }),
      });
      if (!res.ok) {
        setError(await parseErrorMessage(res));
        return;
      }
      navigate('/products');
    } catch {
      setError('Ürün kaydedilemedi.');
    } finally {
      setSaving(false);
    }
  };

  if (loadingMeta) {
    return <p className="text-muted">Form hazırlanıyor…</p>;
  }

  return (
    <div>
      <div className="mb-3">
        <Link to="/products" className="small text-decoration-none">
          ← Ürün listesine dön
        </Link>
      </div>
      <h1 className="h3 mb-4">Ürün kayıt</h1>
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
            <option value="">Seçin…</option>
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
            <label className="form-label">Liste fiyatı *</label>
            <input
              type="number"
              step="0.01"
              min="0"
              className="form-control"
              value={price}
              onChange={(e) => setPrice(e.target.value)}
              required
            />
          </div>
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
            <label className="form-label">Ürün özellikleri (opsiyonel)</label>
            {features.map((f) => (
              <div key={f.id} className="input-group mb-2">
                <span className="input-group-text" style={{ minWidth: '8rem' }}>
                  {f.name}
                  <small className="text-muted ms-1">({featureDataTypeLabel(f.dataType)})</small>
                </span>
                <input
                  type="text"
                  className="form-control"
                  placeholder="Değer"
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
          {saving ? 'Kaydediliyor…' : 'Kaydet'}
        </button>
      </form>
    </div>
  );
};

export default ProductCreate;
