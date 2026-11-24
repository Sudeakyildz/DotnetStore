import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { apiJson } from '../../api/client';
import type { PagedResult, StoreCategory, StoreProduct } from '../../api/shopTypes';
import ProductCard from '../components/ProductCard';

const Home = () => {
  const [products, setProducts] = useState<StoreProduct[] | null>(null);
  const [categories, setCategories] = useState<StoreCategory[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    void (async () => {
      try {
        const [p, c] = await Promise.all([
          apiJson<PagedResult<StoreProduct>>('/api/store/products?pageSize=8&sort=newest'),
          apiJson<StoreCategory[]>('/api/store/categories'),
        ]);
        setProducts(p.items);
        setCategories(c.filter((x) => x.productCount > 0));
      } catch (e) {
        setError(e instanceof Error ? e.message : 'Ürünler yüklenemedi.');
        setProducts([]);
      }
    })();
  }, []);

  return (
    <>
      <section className="shop-hero">
        <div className="container">
          <div className="row align-items-center">
            <div className="col-lg-7">
              <span className="badge text-bg-light mb-3">Yeni sezon</span>
              <h1>İhtiyacın olan her şey, tek yerde.</h1>
              <p className="lead">
                Elektronikten giyime seçili ürünler, hızlı teslimat ve güvenli ödeme. 500 ₺ üzeri alışverişlerde
                kargo bizden.
              </p>
              <Link to="/urunler" className="btn btn-light btn-lg me-2">Alışverişe başla</Link>
              <Link to="/kayit" className="btn btn-outline-light btn-lg">Üye ol</Link>
            </div>
          </div>
        </div>
      </section>

      <div className="container py-5">
        {categories.length > 0 && (
          <section className="mb-5">
            <h2 className="section-title">Kategoriler</h2>
            <div className="row g-3">
              {categories.map((c) => (
                <div className="col-6 col-md-4 col-lg-3" key={c.id}>
                  <Link to={`/urunler?categoryId=${c.id}`} className="category-tile">
                    <strong>{c.name}</strong>
                    <small>{c.productCount} ürün</small>
                  </Link>
                </div>
              ))}
            </div>
          </section>
        )}

        <section>
          <div className="d-flex justify-content-between align-items-end mb-3">
            <h2 className="section-title m-0">Yeni gelenler</h2>
            <Link to="/urunler">Tümünü gör →</Link>
          </div>
          {error && <div className="alert alert-danger">{error}</div>}
          {products === null ? (
            <div className="text-muted">Yükleniyor…</div>
          ) : products.length === 0 && !error ? (
            <div className="text-muted">Henüz satışta ürün yok.</div>
          ) : (
            <div className="row g-3">
              {products.map((p) => (
                <div className="col-6 col-md-4 col-lg-3" key={p.id}>
                  <ProductCard p={p} />
                </div>
              ))}
            </div>
          )}
        </section>

        <section className="row g-3 mt-5 text-center">
          {[
            ['🚚', 'Hızlı kargo', 'Siparişin 1-3 iş gününde kapında'],
            ['🔒', 'Güvenli ödeme', 'Kart bilgilerin sunucumuzda saklanmaz'],
            ['↩️', 'Kolay iptal', 'Kargoya verilmeden iptal et, ücret iade edilsin'],
          ].map(([icon, t, d]) => (
            <div className="col-md-4" key={t}>
              <div className="feature-box">
                <div className="fs-2" aria-hidden>{icon}</div>
                <strong>{t}</strong>
                <div className="small text-muted">{d}</div>
              </div>
            </div>
          ))}
        </section>
      </div>
    </>
  );
};

export default Home;
