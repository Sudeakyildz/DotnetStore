import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { apiFetch, readJson, parseErrorMessage } from '../../api/client';
import type { CategoryDto } from '../../api/types';

const CategoryEdit = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [imageUrl, setImageUrl] = useState('');
  const [slug, setSlug] = useState('');
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      if (!id) return;
      setError(null);
      setLoading(true);
      try {
        const res = await apiFetch(`/api/Categories/${id}`);
        if (res.status === 404) {
          navigate('/categories', { replace: true });
          return;
        }
        if (!res.ok) {
          setError(await parseErrorMessage(res));
          return;
        }
        const c = await readJson<CategoryDto>(res);
        setName(c.name);
        setDescription(c.description ?? '');
        setImageUrl(c.imageUrl ?? '');
        setSlug(c.slug ?? '');
      } catch {
        setError('Veri yüklenemedi.');
      } finally {
        setLoading(false);
      }
    };
    void load();
  }, [id, navigate]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!id) return;
    setError(null);
    setSaving(true);
    try {
      const res = await apiFetch(`/api/Categories/${id}`, {
        method: 'PUT',
        body: JSON.stringify({
          name,
          description: description || null,
          imageUrl: imageUrl || null,
          slug: slug || null,
        }),
      });
      if (!res.ok) {
        setError(await parseErrorMessage(res));
        return;
      }
      navigate('/categories');
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
        <Link to="/categories" className="small text-decoration-none">
          ← Grup listesine dön
        </Link>
      </div>
      <h1 className="h3 mb-4">Ürün grubu güncelleme</h1>
      {error && <div className="alert alert-danger">{error}</div>}
      <form onSubmit={handleSubmit} className="card shadow-sm p-4" style={{ maxWidth: '520px' }}>
        <div className="mb-3">
          <label className="form-label">Grup adı *</label>
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
        <div className="mb-3">
          <label className="form-label">Görsel URL</label>
          <input type="url" className="form-control" value={imageUrl} onChange={(e) => setImageUrl(e.target.value)} />
        </div>
        <div className="mb-3">
          <label className="form-label">Slug</label>
          <input className="form-control" value={slug} onChange={(e) => setSlug(e.target.value)} />
        </div>
        <button type="submit" className="btn btn-primary" disabled={saving}>
          {saving ? 'Kaydediliyor…' : 'Güncelle'}
        </button>
      </form>
    </div>
  );
};

export default CategoryEdit;
