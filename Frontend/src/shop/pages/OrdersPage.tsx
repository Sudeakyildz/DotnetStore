import { useCallback, useEffect, useState } from 'react';
import { Link, useLocation, useParams } from 'react-router-dom';
import { apiJson } from '../../api/client';
import type { OrderDetailDto, OrderListItemDto } from '../../api/types';
import { formatDate, formatMoney } from '../../lib/format';
import { OrderStatus } from '../../lib/orderStatus';
import { useConfirm } from '../../components/useConfirm';
import { ProductImage } from '../components/ProductCard';
import { statusBadge } from '../orderUi';
import { useShop } from '../ShopContext';

export const OrderList = () => {
  const [orders, setOrders] = useState<OrderListItemDto[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    void (async () => {
      try {
        const all = await apiJson<OrderListItemDto[]>('/api/my/orders');
        // Eski "beğeni/sepet" demo satırlarını gösterme; yalnızca gerçek siparişler.
        setOrders(all.filter((o) => o.status >= OrderStatus.SiparisAlindi));
      } catch (e) {
        setError(e instanceof Error ? e.message : 'Siparişler yüklenemedi.');
      }
    })();
  }, []);

  return (
    <div className="container py-4">
      <h1 className="h3 mb-3">Siparişlerim</h1>
      {error && <div className="alert alert-danger">{error}</div>}
      {orders === null && !error && <div className="text-muted">Yükleniyor…</div>}
      {orders?.length === 0 && (
        <div className="empty-state">
          <div className="fs-1" aria-hidden>📦</div>
          <strong>Henüz siparişin yok</strong>
          <Link to="/urunler" className="btn btn-brand mt-3">Alışverişe başla</Link>
        </div>
      )}
      {orders?.map((o) => (
        <Link key={o.id} to={`/hesabim/siparisler/${o.id}`} className="order-row">
          <div className="order-thumb"><ProductImage src={o.firstImageUrl ?? null} name={o.itemsPreview ?? 'Sipariş'} /></div>
          <div className="flex-grow-1">
            <div className="fw-semibold">Sipariş #{o.id}</div>
            <div className="small text-muted text-truncate">{o.itemsPreview}</div>
            <div className="small text-muted">{formatDate(o.createdAt)}</div>
          </div>
          <div className="text-end">
            <span className={`badge ${statusBadge(o.status)}`}>{o.statusLabel}</span>
            <div className="fw-bold mt-1">{formatMoney(o.total)}</div>
          </div>
        </Link>
      ))}
    </div>
  );
};

const STEPS = [
  { s: OrderStatus.SiparisAlindi, label: 'Sipariş alındı' },
  { s: OrderStatus.KargoyaVerildi, label: 'Kargoya verildi' },
  { s: OrderStatus.TeslimEdildi, label: 'Teslim edildi' },
];

export const OrderDetail = () => {
  const { id } = useParams();
  const location = useLocation();
  const { notify } = useShop();
  const { confirm, dialog } = useConfirm();
  const [o, setO] = useState<OrderDetailDto | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const justOrdered = (location.state as { justOrdered?: boolean } | null)?.justOrdered;

  const load = useCallback(async () => {
    try {
      setO(await apiJson<OrderDetailDto>(`/api/my/orders/${id}`));
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Sipariş yüklenemedi.');
    }
  }, [id]);

  useEffect(() => {
    void load();
  }, [load]);

  const cancel = async () => {
    if (!(await confirm('Siparişi iptal etmek istediğinize emin misiniz? Ödemeniz iade edilecek.', { confirmLabel: 'Siparişi iptal et' }))) return;
    setBusy(true);
    try {
      await apiJson<void>(`/api/my/orders/${id}/cancel`, { method: 'POST' });
      notify('Siparişiniz iptal edildi, ödemeniz iade edildi.');
      await load();
    } catch (e) {
      notify(e instanceof Error ? e.message : 'İptal edilemedi.', 'danger');
    } finally {
      setBusy(false);
    }
  };

  if (error) return <div className="container py-5"><div className="alert alert-warning">{error}</div></div>;
  if (!o) return <div className="container py-5 text-muted">Yükleniyor…</div>;

  const cancelled = o.status === OrderStatus.IptalEdildi;
  const stepIndex = STEPS.findIndex((x) => x.s === o.status);

  return (
    <div className="container py-4">
      {dialog}
      {justOrdered && (
        <div className="alert alert-success">
          <strong>Teşekkürler!</strong> Siparişin alındı ve ödemen onaylandı. Sipariş numaran: <strong>#{o.id}</strong>
        </div>
      )}
      <div className="d-flex flex-wrap justify-content-between align-items-center mb-3 gap-2">
        <div>
          <Link to="/hesabim/siparisler" className="small">← Siparişlerim</Link>
          <h1 className="h3 m-0">Sipariş #{o.id}</h1>
          <small className="text-muted">{formatDate(o.createdAt)}</small>
        </div>
        <span className={`badge fs-6 ${statusBadge(o.status)}`}>{o.statusLabel}</span>
      </div>

      {!cancelled && stepIndex >= 0 && (
        <div className="order-steps mb-4">
          {STEPS.map((st, i) => (
            <div key={st.s} className={`order-step ${i <= stepIndex ? 'done' : ''}`}>
              <span className="dot">{i <= stepIndex ? '✓' : i + 1}</span>
              <span>{st.label}</span>
            </div>
          ))}
        </div>
      )}

      <div className="row g-4">
        <div className="col-lg-8">
          {o.items.map((i) => (
            <div className="cart-line" key={i.productId}>
              <Link to={`/urun/${i.productId}`} className="cart-line-img"><ProductImage src={i.imageUrl ?? null} name={i.productName ?? 'Ürün'} /></Link>
              <div className="flex-grow-1">
                <div className="fw-semibold">{i.productName}</div>
                <div className="small text-muted">{i.quantity} × {formatMoney(i.unitPrice)}</div>
              </div>
              <strong>{formatMoney(i.lineTotal)}</strong>
            </div>
          ))}
        </div>
        <aside className="col-lg-4">
          <div className="summary-box mb-3">
            <h2 className="h6">Ödeme</h2>
            <div className="d-flex justify-content-between"><span>Kargo</span><span>{o.shippingFee ? formatMoney(o.shippingFee) : 'Ücretsiz'}</span></div>
            <div className="d-flex justify-content-between fw-bold"><span>Toplam</span><span>{formatMoney(o.total)}</span></div>
            <div className="small text-muted mt-2">{o.paymentStatusLabel}{o.paymentCard ? ` · ${o.paymentCard}` : ''}</div>
          </div>
          {o.shippingAddress && (
            <div className="summary-box mb-3">
              <h2 className="h6">Teslimat</h2>
              <div>{o.shippingFullName}</div>
              <div className="small text-muted">{o.shippingPhone}</div>
              <div className="small">{o.shippingAddress}, {o.shippingCity}</div>
              {o.trackingNumber && <div className="small mt-2">Kargo takip no: <strong>{o.trackingNumber}</strong></div>}
            </div>
          )}
          {o.canCancel && (
            <button type="button" className="btn btn-outline-danger w-100" disabled={busy} onClick={() => void cancel()}>
              Siparişi iptal et
            </button>
          )}
        </aside>
      </div>
    </div>
  );
};
