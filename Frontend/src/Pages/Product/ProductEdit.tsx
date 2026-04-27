import { useCallback, useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { apiFetch, readJson, parseErrorMessage, getStoredAuthProfile } from '../../api/client';
import type { CategoryDto, FeatureDto, ProductDetailDto } from '../../api/types';
import { ProductStatus } from '../../lib/productStatus';
import { productImageSrc } from '../../lib/productImage';
import { AuthRoles } from '../../lib/authRoles';

const ProductEdit = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const role = getStoredAuthProfile()?.role ?? '';
  const isAdmin = role === AuthRoles.Admin;
  const isPriceOnly = role === AuthRoles.StaffPrices;
  const isFeatureOnly = role === AuthRoles.StaffFeatures;

  const [categories, setCategories] = useState<CategoryDto[]>([]);
  const [features, setFeatures] = useState<FeatureDto[]>([]);
  const [categoryId, setCategoryId] = useState('');
  const [categoryLabel, setCategoryLabel] = useState('');
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
      const pRes = await apiFetch(`/api/Products/${id}`);
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
      setCategoryLabel(p.categoryName ?? '—');
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

      const needCategories = isAdmin;
      const needFeatures = isAdmin || isFeatureOnly;
      const [cRes, fRes] = await Promise.all([
        needCategories ? apiFetch('/api/Categories') : Promise.resolve(null as Response | null),
        needFeatures ? apiFetch('/api/Features') : Promise.resolve(null as Response | null),
      ]);
      if (cRes?.ok) setCategories(await readJson<CategoryDto[]>(cRes));
      if (fRes?.ok) setFeatures(await readJson<FeatureDto[]>(fRes));
    } catch {
      setError('Veri yüklenemedi.');
    } finally {
      setLoading(false);
    }
  }, [id, navigate, isAdmin, isFeatureOnly]);

  useEffect(() => {
    void load();
  }, [load]);

  useEffect(() => {
    if (role === AuthRoles.StaffCategories) {
      navigate('/categories', { replace: true });
    }
  }, [role, navigate]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!id) return;

    setError(null);
    setSaving(true);
    try {
      if (isPriceOnly) {
        if (newPrice.trim() === '') {
          setError('Yeni fiyat girmelisiniz.');
          setSaving(false);
          return;
        }
        const n = Number(newPrice);
        if (Number.isNaN(n) || n < 0) {
          setError('Yeni fiyat geçersiz.');
          setSaving(false);
          return;
        }
        const res = await apiFetch(`/api/Products/${id}/price`, {
          method: 'PUT',
          body: JSON.stringify({ newPrice: n }),
        });
        if (!res.ok) {
          setError(await parseErrorMessage(res));
          return;
        }
        navigate('/products');
        return;
      }

      if (isFeatureOnly) {
        const featureValues = features
          .map((f) => ({
            featureId: f.id,
            value: (featureInputs[f.id] ?? '').trim(),
          }))
          .filter((x) => x.value.length > 0);

        const res = await apiFetch(`/api/Products/${id}/feature-values`, {
          method: 'PUT',
          body: JSON.stringify({ featureValues }),
        });
        if (!res.ok) {
          setError(await parseErrorMessage(res));
          return;
        }
        navigate('/products');
        return;
      }

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
          setSaving(false);
          return;
        }
        newPriceVal = n;
      }

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

  const readOnlyBasics = isPriceOnly || isFeatureOnly;
  const priceSection = isAdmin || isPriceOnly;
  const featureSection = isAdmin || isFeatureOnly;

  return (
    <div>
      <div className="mb-3">
        <Link to="/products" className="small text-decoration-none">
          ← Ürün listesine dön
        </Link>
      </div>
      <h1 className="h3 mb-4">Ürün güncelleme</h1>
      {(isAdmin || isPriceOnly) && id && (
        <div className="alert alert-light border py-2 small mb-3 d-flex flex-wrap align-items-center gap-2">
          <span className="text-muted">Sadece fiyat değişecekse:</span>
          <Link to={`/products/${id}/fiyat`} className="btn btn-sm btn-warning text-dark">
            Fiyat güncelleme sayfası
          </Link>
        </div>
      )}
      {readOnlyBasics && (
        <div className="alert alert-info small py-2">
          {isPriceOnly && 'Bu hesap yalnızca fiyat güncelleyebilir; diğer alanlar salt okunur.'}
          {isFeatureOnly && 'Bu hesap yalnızca ürün özellik değerlerini güncelleyebilir; diğer alanlar salt okunur.'}
        </div>
      )}
      {error && <div className="alert alert-danger">{error}</div>}
      <form onSubmit={handleSubmit} className="card shadow-sm p-4" style={{ maxWidth: '640px' }}>
        <div className="mb-3">
          <label className="form-label">Ürün grubu *</label>
          {readOnlyBasics ? (
            <div className="form-control bg-body-secondary">{categoryLabel}</div>
          ) : (
            <select
              className="form-select"
              required={isAdmin}
              value={categoryId}
              onChange={(e) => setCategoryId(e.target.value)}
            >
              {categories.map((c) => (
                <option key={c.id} value={String(c.id)}>
                  {c.name}
                </option>
              ))}
            </select>
          )}
        </div>
        <div className="mb-3">
          <label className="form-label">Ürün adı *</label>
          <input
            className="form-control"
            value={name}
            onChange={(e) => setName(e.target.value)}
            required={isAdmin}
            disabled={readOnlyBasics}
          />
        </div>
        <div className="mb-3">
          <label className="form-label">Açıklama</label>
          <textarea
            className="form-control"
            rows={3}
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            disabled={readOnlyBasics}
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
              required={isAdmin}
              disabled={readOnlyBasics}
            />
          </div>
          <div className="col-md-4">
            <label className="form-label">Durum</label>
            <select
              className="form-select"
              value={status}
              onChange={(e) => setStatus(e.target.value)}
              disabled={readOnlyBasics}
            >
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
        {priceSection && (
          <div className="mb-3">
            <label className="form-label">{isPriceOnly ? 'Yeni fiyat *' : 'Yeni fiyat (opsiyonel)'}</label>
            <input
              type="number"
              step="0.01"
              min="0"
              className="form-control"
              placeholder={isAdmin ? 'Boş bırakılırsa fiyat değişmez' : ''}
              value={newPrice}
              onChange={(e) => setNewPrice(e.target.value)}
            />
          </div>
        )}
        <div className="mb-3">
          <label className="form-label">Görsel URL</label>
          <input
            type="url"
            className="form-control"
            value={imageUrl}
            onChange={(e) => setImageUrl(e.target.value)}
            disabled={readOnlyBasics}
          />
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

        {featureSection && features.length > 0 && (
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
                  disabled={isPriceOnly}
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
