import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { apiFetch, readJson, parseErrorMessage } from '../../api/client';
import type { CategoryDto } from '../../api/types';

const CategoryCreate = () => {
  const navigate = useNavigate();
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [imageUrl, setImageUrl] = useState('');
  const [slug, setSlug] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      const res = await apiFetch('/api/Categories', {
        method: 'POST',
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
      const created = await readJson<CategoryDto>(res);
      navigate(`/categories/edit/${created.id}`);
    } catch {
      setError('Kayıt oluşturulamadı.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <div className="mb-3">
        <Link to="/categories" className="small text-decoration-none">
          ← Grup listesine dön
        </Link>
      </div>
      <h1 className="h3 mb-4">Ürün grubu kayıt</h1>
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
          <input
            type="url"
            className="form-control"
            placeholder="https://..."
            value={imageUrl}
            onChange={(e) => setImageUrl(e.target.value)}
          />
        </div>
        <div className="mb-3">
          <label className="form-label">Slug</label>
          <input
            className="form-control"
            placeholder="Boş bırakılırsa addan üretilir"
            value={slug}
            onChange={(e) => setSlug(e.target.value)}
          />
        </div>
        <button type="submit" className="btn btn-primary" disabled={loading}>
          {loading ? 'Kaydediliyor…' : 'Kaydet'}
        </button>
      </form>
    </div>
  );
};

export default CategoryCreate;
