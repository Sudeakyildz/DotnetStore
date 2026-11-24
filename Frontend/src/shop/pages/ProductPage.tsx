import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { apiJson } from '../../api/client';
import type { StoreProductDetail } from '../../api/shopTypes';
import { formatMoney } from '../../lib/format';
import ProductCard, { ProductImage } from '../components/ProductCard';
import { useShop } from '../ShopContext';

const ProductView = ({ id }: { id: string | undefined }) => {
  const navigate = useNavigate();
  const { isCustomer, profile, addToCart, toggleFavorite, isFavorite, notify } = useShop();
  const [p, setP] = useState<StoreProductDetail | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [qty, setQty] = useState(1);

  useEffect(() => {
    void (async () => {
      try {
        setP(await apiJson<StoreProductDetail>(`/api/store/products/${id}`));
      } catch (e) {
        setError(e instanceof Error ? e.message : 'Ürün yüklenemedi.');
      }
    })();
  }, [id]);

  if (error)
    return (
      <div className="container py-5 text-center">
        <div className="alert alert-warning d-inline-block">{error}</div>
        <div><Link to="/urunler">Ürünlere dön</Link></div>
      </div>
    );
  if (!p) return <div className="container py-5 text-muted">Yükleniyor…</div>;

  const out = p.stock < 1;
  const needLogin = () => {
    notify('Devam etmek için giriş yapın.', 'info');
    navigate('/giris', { state: { from: `/urun/${p.id}` } });
  };
  const maxQty = Math.min(20, p.stock);

  const add = async (goCart: boolean) => {
    if (!profile) return needLogin();
    if (!isCustomer) return notify('Sepet yalnızca müşteri hesaplarında kullanılır.', 'info');
    const ok = await addToCart(p.id, qty);
    if (ok && goCart) navigate('/sepet');
  };

  return (
    <div className="container py-4">
      <nav className="small mb-3">
        <Link to="/">Ana sayfa</Link> / <Link to="/urunler">Ürünler</Link>
        {p.categoryName && <> / <Link to={`/urunler?categoryId=${p.categoryId}`}>{p.categoryName}</Link></>}
      </nav>

      <div className="row g-4">
        <div className="col-md-6">
          <div className="detail-img"><ProductImage src={p.imageUrl} name={p.name} /></div>
        </div>
        <div className="col-md-6">
          <h1 className="h3">{p.name}</h1>
          <div className="product-price fs-3 my-3">{formatMoney(p.price)}</div>

          <div className="mb-3">
            {out ? (
              <span className="badge text-bg-dark">Tükendi</span>
            ) : p.stock <= 5 ? (
              <span className="badge text-bg-warning">Son {p.stock} ürün!</span>
            ) : (
              <span className="badge text-bg-success">Stokta</span>
            )}
          </div>

          {p.description && <p className="text-secondary">{p.description}</p>}

          {!out && (
            <div className="d-flex align-items-center gap-2 mb-3">
              <label htmlFor="qty" className="form-label m-0">Adet</label>
              <input id="qty" type="number" className="form-control" style={{ width: 90 }} min={1} max={maxQty}
                value={qty} onChange={(e) => setQty(Math.min(maxQty, Math.max(1, Number(e.target.value) || 1)))} />
            </div>
          )}

          <div className="d-flex flex-wrap gap-2">
            <button className="btn btn-brand btn-lg" disabled={out} onClick={() => void add(false)} type="button">
              Sepete ekle
            </button>
            <button className="btn btn-dark btn-lg" disabled={out} onClick={() => void add(true)} type="button">
              Hemen al
            </button>
            <button
              className={`btn btn-lg ${isFavorite(p.id) ? 'btn-danger' : 'btn-outline-secondary'}`}
              type="button"
              onClick={() => (profile ? void toggleFavorite(p.id) : needLogin())}
            >
              {isFavorite(p.id) ? '♥ Favorilerde' : '♡ Favorilere ekle'}
            </button>
          </div>

          {p.features.length > 0 && (
            <table className="table table-sm mt-4">
              <tbody>
                {p.features.map((f) => (
                  <tr key={f.name}>
                    <th className="w-50 fw-normal text-muted">{f.name}</th>
                    <td>{f.value}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </div>

      {p.related.length > 0 && (
        <section className="mt-5">
          <h2 className="section-title">Benzer ürünler</h2>
          <div className="row g-3">
            {p.related.map((r) => (
              <div className="col-6 col-md-3" key={r.id}><ProductCard p={r} /></div>
            ))}
          </div>
        </section>
      )}
    </div>
  );
};

// id değişince bileşen sıfırlansın (yükleme/hata/adet durumu temiz başlar)
const ProductPage = () => {
  const { id } = useParams();
  return <ProductView key={id} id={id} />;
};

export default ProductPage;
