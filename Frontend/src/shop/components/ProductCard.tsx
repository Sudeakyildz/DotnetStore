import { Link, useNavigate } from 'react-router-dom';
import type { StoreProduct } from '../../api/shopTypes';
import { formatMoney } from '../../lib/format';
import { productImageSrc } from '../../lib/productImage';
import { useShop } from '../ShopContext';

export const ProductImage = ({ src, name, className }: { src: string | null; name: string; className?: string }) => {
  const url = productImageSrc(src);
  return url ? (
    <img src={url} alt={name} loading="lazy" className={className} />
  ) : (
    <div className={`product-placeholder ${className ?? ''}`} aria-label={name}>
      {name.trim().charAt(0).toUpperCase()}
    </div>
  );
};

const ProductCard = ({ p }: { p: StoreProduct }) => {
  const { isCustomer, profile, addToCart, toggleFavorite, isFavorite, notify } = useShop();
  const navigate = useNavigate();
  const out = p.stock < 1;

  const needLogin = () => {
    notify('Devam etmek için giriş yapın.', 'info');
    navigate('/giris', { state: { from: '/urunler' } });
  };

  return (
    <div className="product-card">
      <button
        type="button"
        className={`fav-btn ${isFavorite(p.id) ? 'active' : ''}`}
        aria-label="Favorilere ekle/çıkar"
        onClick={() => (profile ? void toggleFavorite(p.id) : needLogin())}
      >
        {isFavorite(p.id) ? '♥' : '♡'}
      </button>
      <Link to={`/urun/${p.id}`} className="product-card-img">
        <ProductImage src={p.imageUrl} name={p.name} />
        {out && <span className="badge text-bg-dark product-badge">Tükendi</span>}
        {!out && p.stock <= 3 && <span className="badge text-bg-warning product-badge">Son {p.stock} ürün</span>}
      </Link>
      <div className="product-card-body">
        <div className="small text-muted">{p.categoryName}</div>
        <Link to={`/urun/${p.id}`} className="product-card-title">{p.name}</Link>
        <div className="d-flex justify-content-between align-items-center mt-auto pt-2">
          <strong className="product-price">{formatMoney(p.price)}</strong>
          <button
            type="button"
            className="btn btn-brand btn-sm"
            disabled={out}
            onClick={() => (isCustomer ? void addToCart(p.id) : profile ? notify('Sepet yalnızca müşteri hesaplarında kullanılır.', 'info') : needLogin())}
          >
            Sepete ekle
          </button>
        </div>
      </div>
    </div>
  );
};

export default ProductCard;
