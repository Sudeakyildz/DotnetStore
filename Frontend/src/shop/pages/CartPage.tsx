import { Link, useNavigate } from 'react-router-dom';
import { formatMoney } from '../../lib/format';
import { ProductImage } from '../components/ProductCard';
import { useShop } from '../ShopContext';

const CartPage = () => {
  const { profile, isCustomer, cart, setQuantity, removeFromCart } = useShop();
  const navigate = useNavigate();

  if (!profile)
    return (
      <div className="container py-5">
        <div className="empty-state">
          <div className="fs-1" aria-hidden>🛒</div>
          <strong>Sepetini görmek için giriş yap</strong>
          <Link to="/giris" state={{ from: '/sepet' }} className="btn btn-brand mt-3">Giriş yap</Link>
        </div>
      </div>
    );
  if (!isCustomer)
    return (
      <div className="container py-5 text-center text-muted">
        Yönetici hesaplarıyla alışveriş yapılamaz. Müşteri hesabıyla giriş yapın.
      </div>
    );
  if (!cart) return <div className="container py-5 text-muted">Yükleniyor…</div>;

  if (cart.items.length === 0)
    return (
      <div className="container py-5">
        <div className="empty-state">
          <div className="fs-1" aria-hidden>🛒</div>
          <strong>Sepetin boş</strong>
          <Link to="/urunler" className="btn btn-brand mt-3">Alışverişe başla</Link>
        </div>
      </div>
    );

  const hasUnavailable = cart.items.some((i) => !i.available || i.quantity > i.stock);
  const remaining = Math.max(0, cart.freeShippingThreshold - cart.subtotal);

  return (
    <div className="container py-4">
      <h1 className="h3 mb-3">Sepetim ({cart.itemCount} ürün)</h1>
      <div className="row g-4">
        <div className="col-lg-8">
          {cart.items.map((i) => (
            <div className={`cart-line ${!i.available ? 'unavailable' : ''}`} key={i.productId}>
              <Link to={`/urun/${i.productId}`} className="cart-line-img">
                <ProductImage src={i.imageUrl} name={i.name} />
              </Link>
              <div className="flex-grow-1">
                <Link to={`/urun/${i.productId}`} className="fw-semibold text-decoration-none text-dark">{i.name}</Link>
                {!i.available && <div className="small text-danger">Bu ürün şu an satın alınamıyor.</div>}
                {i.available && i.quantity > i.stock && (
                  <div className="small text-danger">Yetersiz stok (kalan: {i.stock}).</div>
                )}
                <div className="small text-muted">{formatMoney(i.unitPrice)} / adet</div>
                <div className="d-flex align-items-center gap-2 mt-2">
                  <div className="btn-group btn-group-sm" role="group" aria-label="Adet">
                    <button type="button" className="btn btn-outline-secondary" aria-label="Azalt"
                      onClick={() => void setQuantity(i.productId, i.quantity - 1)}>−</button>
                    <span className="btn btn-light disabled">{i.quantity}</span>
                    <button type="button" className="btn btn-outline-secondary" aria-label="Artır"
                      disabled={i.quantity >= Math.min(20, i.stock)}
                      onClick={() => void setQuantity(i.productId, i.quantity + 1)}>+</button>
                  </div>
                  <button type="button" className="btn btn-link btn-sm text-danger"
                    onClick={() => void removeFromCart(i.productId)}>Kaldır</button>
                </div>
              </div>
              <strong>{formatMoney(i.lineTotal)}</strong>
            </div>
          ))}
        </div>

        <aside className="col-lg-4">
          <div className="summary-box">
            <h2 className="h5">Sipariş özeti</h2>
            <div className="d-flex justify-content-between"><span>Ara toplam</span><span>{formatMoney(cart.subtotal)}</span></div>
            <div className="d-flex justify-content-between">
              <span>Kargo</span>
              <span>{cart.shippingFee === 0 ? 'Ücretsiz' : formatMoney(cart.shippingFee)}</span>
            </div>
            {remaining > 0 && (
              <div className="small text-success mt-1">
                {formatMoney(remaining)} daha ekle, kargo ücretsiz olsun!
              </div>
            )}
            <hr />
            <div className="d-flex justify-content-between fs-5 fw-bold"><span>Toplam</span><span>{formatMoney(cart.total)}</span></div>
            <button
              type="button"
              className="btn btn-brand w-100 mt-3"
              disabled={hasUnavailable}
              onClick={() => navigate('/odeme')}
            >
              Ödemeye geç
            </button>
            {hasUnavailable && (
              <div className="small text-danger mt-2">Devam etmek için uygun olmayan ürünleri sepetten çıkarın.</div>
            )}
          </div>
        </aside>
      </div>
    </div>
  );
};

export default CartPage;
