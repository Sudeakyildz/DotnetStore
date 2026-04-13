import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { apiFetch, parseErrorMessage } from '../../api/client';
import { FeatureDataType, featureDataTypeLabel } from '../../lib/featureDataType';

const dataTypes = [
  FeatureDataType.None,
  FeatureDataType.String,
  FeatureDataType.Int,
  FeatureDataType.Decimal,
  FeatureDataType.Date,
  FeatureDataType.Bool,
] as const;

const FeatureCreate = () => {
  const navigate = useNavigate();
  const [name, setName] = useState('');
  const [dataType, setDataType] = useState(String(FeatureDataType.String));
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      const res = await apiFetch('/api/Features', {
        method: 'POST',
        body: JSON.stringify({
          name,
          dataType: Number(dataType),
        }),
      });
      if (!res.ok) {
        setError(await parseErrorMessage(res));
        return;
      }
      navigate('/features');
    } catch {
      setError('Kayıt oluşturulamadı.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <div className="mb-3">
        <Link to="/features" className="small text-decoration-none">
          ← Özellik listesine dön
        </Link>
      </div>
      <h1 className="h3 mb-4">Ürün özelliği ekle</h1>
      {error && <div className="alert alert-danger">{error}</div>}
      <form onSubmit={handleSubmit} className="card shadow-sm p-4" style={{ maxWidth: '480px' }}>
        <div className="mb-3">
          <label className="form-label">Özellik adı *</label>
          <input className="form-control" value={name} onChange={(e) => setName(e.target.value)} required />
        </div>
        <div className="mb-3">
          <label className="form-label">Veri tipi</label>
          <select className="form-select" value={dataType} onChange={(e) => setDataType(e.target.value)}>
            {dataTypes.map((d) => (
              <option key={d} value={String(d)}>
                {featureDataTypeLabel(d)}
              </option>
            ))}
          </select>
        </div>
        <button type="submit" className="btn btn-primary" disabled={loading}>
          {loading ? 'Kaydediliyor…' : 'Kaydet'}
        </button>
      </form>
    </div>
  );
};

export default FeatureCreate;
