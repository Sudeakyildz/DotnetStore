import { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { apiFetch, readJson } from '../api/client';
import type { CategoryDto, FeatureDto, ProductListItemDto } from '../api/types';

const Dashboard = () => {
  const [counts, setCounts] = useState<{ categories: number; products: number; features: number } | null>(null);
  const [loading, setLoading] = useState(true);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const [cRes, pRes, fRes] = await Promise.all([
        apiFetch('/api/Categories'),
        apiFetch('/api/Products'),
        apiFetch('/api/Features'),
      ]);
      const categories = cRes.ok ? await readJson<CategoryDto[]>(cRes) : [];
      const products = pRes.ok ? await readJson<ProductListItemDto[]>(pRes) : [];
      const features = fRes.ok ? await readJson<FeatureDto[]>(fRes) : [];
      setCounts({
        categories: categories.length,
        products: products.length,
        features: features.length,
      });
    } catch {
      setCounts({ categories: 0, products: 0, features: 0 });
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void load();
  }, [load]);

  return (
    <div className="dashboard-page">
      <header className="dashboard-hero mb-4 mb-md-5">
        <h1 className="dashboard-hero-title">Kontrol paneli</h1>
        <p className="dashboard-hero-meta text-muted small mb-0">
          {new Date().toLocaleDateString('tr-TR', {
            weekday: 'long',
            day: 'numeric',
            month: 'long',
            year: 'numeric',
          })}
        </p>
      </header>

      <div className="row g-3 g-md-4 mb-4 mb-md-5">
        <div className="col-sm-4">
          <div className="stat-card stat-card--categories p-4 h-100">
            <div className="stat-card-icon stat-card-icon--categories" aria-hidden>
              <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <path d="M4 6h7v7H4V6zM13 6h7v4h-7V6zM13 12h7v6h-7v-6zM4 15h7v3H4v-3z" />
              </svg>
            </div>
            <div className="stat-label mb-1">Ürün grupları</div>
            <div className="stat-value">{loading ? '…' : counts?.categories ?? 0}</div>
          </div>
        </div>
        <div className="col-sm-4">
          <div className="stat-card stat-card--products p-4 h-100">
            <div className="stat-card-icon stat-card-icon--products" aria-hidden>
              <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <path d="M6 2L3 6v14a2 2 0 002 2h14a2 2 0 002-2V6l-3-4H6zM3 6h18M16 10a4 4 0 01-8 0" />
              </svg>
            </div>
            <div className="stat-label mb-1">Ürünler</div>
            <div className="stat-value">{loading ? '…' : counts?.products ?? 0}</div>
          </div>
        </div>
        <div className="col-sm-4">
          <div className="stat-card stat-card--features p-4 h-100">
            <div className="stat-card-icon stat-card-icon--features" aria-hidden>
              <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <path d="M8 6h13M8 12h13M8 18h13M3 6h.01M3 12h.01M3 18h.01" strokeLinecap="round" />
              </svg>
            </div>
            <div className="stat-label mb-1">Özellik tanımları</div>
            <div className="stat-value">{loading ? '…' : counts?.features ?? 0}</div>
          </div>
        </div>
      </div>

      <div className="row g-3">
        <div className="col-md-4">
          <Link to="/categories" className="text-decoration-none dash-tile h-100 d-block">
            <div className="dash-tile-inner h-100">
              <span className="dash-tile-label">Katalog</span>
              <h2 className="dash-tile-title">Ürün grupları</h2>
              <p className="dash-tile-desc">Kategori oluşturma ve düzenleme</p>
              <span className="dash-tile-action">
                Aç
                <span className="dash-tile-chevron" aria-hidden>
                  →
                </span>
              </span>
            </div>
          </Link>
        </div>
        <div className="col-md-4">
          <Link to="/products" className="text-decoration-none dash-tile h-100 d-block">
            <div className="dash-tile-inner dash-tile-inner--accent h-100">
              <span className="dash-tile-label">Stok</span>
              <h2 className="dash-tile-title">Ürünler</h2>
              <p className="dash-tile-desc">Liste, arama ve fiyat yönetimi</p>
              <span className="dash-tile-action">
                Aç
                <span className="dash-tile-chevron" aria-hidden>
                  →
                </span>
              </span>
            </div>
          </Link>
        </div>
        <div className="col-md-4">
          <Link to="/features" className="text-decoration-none dash-tile h-100 d-block">
            <div className="dash-tile-inner h-100">
              <span className="dash-tile-label">Şema</span>
              <h2 className="dash-tile-title">Ürün özellikleri</h2>
              <p className="dash-tile-desc">Renk, beden ve veri tipleri</p>
              <span className="dash-tile-action">
                Aç
                <span className="dash-tile-chevron" aria-hidden>
                  →
                </span>
              </span>
            </div>
          </Link>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;
