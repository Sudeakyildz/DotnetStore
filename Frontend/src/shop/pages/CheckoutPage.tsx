import { useState } from 'react';
import { Link, Navigate, useNavigate } from 'react-router-dom';
import { apiJson } from '../../api/client';
import type { OrderDetailDto } from '../../api/types';
import { formatMoney } from '../../lib/format';
import { useShop } from '../ShopContext';

const formatCardNumber = (v: string) =>
  v.replace(/\D/g, '').slice(0, 19).replace(/(.{4})/g, '$1 ').trim();

const formatExpiry = (v: string) => {
  const d = v.replace(/\D/g, '').slice(0, 4);
  return d.length > 2 ? `${d.slice(0, 2)}/${d.slice(2)}` : d;
};

const CheckoutPage = () => {
  const { cart, refreshCart, notify, profile } = useShop();
  const navigate = useNavigate();
  const [form, setForm] = useState({
    fullName: profile?.userName ?? '',
    phone: '',
    city: '',
    address: '',
    note: '',
    holder: '',
    number: '',
    expiry: '',
    cvv: '',
  });
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [done, setDone] = useState(false);

  if (!cart) return <div className="container py-5 text-muted">Yükleniyor…</div>;
  if (cart.items.length === 0 && !done) return <Navigate to="/sepet" replace />;

  const set = (k: keyof typeof form) => (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) =>
    setForm((f) => ({ ...f, [k]: e.target.value }));

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    const [mm, yy] = form.expiry.split('/');
    const expMonth = Number(mm);
    const expYear = Number(yy) < 100 ? 2000 + Number(yy) : Number(yy);
    if (!expMonth || !yy) {
      setError('Son kullanma tarihini AA/YY biçiminde girin.');
      return;
    }
    setBusy(true);
    try {
      const order = await apiJson<OrderDetailDto>('/api/my/checkout', {
        method: 'POST',
        body: JSON.stringify({
          shippingFullName: form.fullName,
          shippingPhone: form.phone,
          shippingCity: form.city,
          shippingAddress: form.address,
          note: form.note || null,
          card: {
            holderName: form.holder,
            number: form.number.replace(/\s/g, ''),
            expMonth,
            expYear,
            cvv: form.cvv,
          },
        }),
      });
      setDone(true); // sepet boşalınca /sepet'e yönlendirme yapılmasın
      notify('Siparişin alındı, teşekkürler!');
      navigate(`/hesabim/siparisler/${order.id}`, { replace: true, state: { justOrdered: true } });
      void refreshCart();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Ödeme başarısız.');
      await refreshCart();
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="container py-4">
      <h1 className="h3 mb-3">Ödeme</h1>
      <form onSubmit={(e) => void submit(e)} className="row g-4">
        <div className="col-lg-8">
          <section className="summary-box mb-3">
            <h2 className="h5">Teslimat bilgileri</h2>
            <div className="row g-3">
              <div className="col-md-6">
                <label className="form-label" htmlFor="fn">Ad soyad</label>
                <input id="fn" className="form-control" required maxLength={150} autoComplete="name"
                  value={form.fullName} onChange={set('fullName')} />
              </div>
              <div className="col-md-6">
                <label className="form-label" htmlFor="ph">Telefon</label>
                <input id="ph" className="form-control" required type="tel" autoComplete="tel" placeholder="05xx xxx xx xx"
                  value={form.phone} onChange={set('phone')} />
              </div>
              <div className="col-md-4">
                <label className="form-label" htmlFor="ct">Şehir</label>
                <input id="ct" className="form-control" required maxLength={100} autoComplete="address-level1"
                  value={form.city} onChange={set('city')} />
              </div>
              <div className="col-md-8">
                <label className="form-label" htmlFor="ad">Adres</label>
                <textarea id="ad" className="form-control" required minLength={10} maxLength={500} rows={2}
                  autoComplete="street-address" value={form.address} onChange={set('address')} />
              </div>
              <div className="col-12">
                <label className="form-label" htmlFor="nt">Sipariş notu (isteğe bağlı)</label>
                <input id="nt" className="form-control" maxLength={500} value={form.note} onChange={set('note')} />
              </div>
            </div>
          </section>

          <section className="summary-box">
            <h2 className="h5">Kart bilgileri</h2>
            <div className="alert alert-info small">
              <strong>Demo ödeme:</strong> gerçek para çekilmez. Başarılı: <code>4242 4242 4242 4242</code> ·
              Reddedilen: <code>4000 0000 0000 0002</code> · Bakiye yetersiz: <code>4000 0000 0000 9995</code>.
              Gelecekteki tarih ve herhangi bir 3 haneli CVV girin.
            </div>
            <div className="row g-3">
              <div className="col-12">
                <label className="form-label" htmlFor="ch">Kart üzerindeki isim</label>
                <input id="ch" className="form-control" required autoComplete="cc-name" value={form.holder} onChange={set('holder')} />
              </div>
              <div className="col-12">
                <label className="form-label" htmlFor="cn">Kart numarası</label>
                <input id="cn" className="form-control" required inputMode="numeric" autoComplete="cc-number"
                  placeholder="0000 0000 0000 0000" value={form.number}
                  onChange={(e) => setForm((f) => ({ ...f, number: formatCardNumber(e.target.value) }))} />
              </div>
              <div className="col-6">
                <label className="form-label" htmlFor="ce">Son kullanma (AA/YY)</label>
                <input id="ce" className="form-control" required inputMode="numeric" autoComplete="cc-exp" placeholder="12/30"
                  value={form.expiry} onChange={(e) => setForm((f) => ({ ...f, expiry: formatExpiry(e.target.value) }))} />
              </div>
              <div className="col-6">
                <label className="form-label" htmlFor="cv">CVV</label>
                <input id="cv" className="form-control" required inputMode="numeric" autoComplete="cc-csc" maxLength={4}
                  value={form.cvv} onChange={(e) => setForm((f) => ({ ...f, cvv: e.target.value.replace(/\D/g, '') }))} />
              </div>
            </div>
          </section>
        </div>

        <aside className="col-lg-4">
          <div className="summary-box">
            <h2 className="h5">Sipariş özeti</h2>
            {cart.items.map((i) => (
              <div key={i.productId} className="d-flex justify-content-between small mb-1">
                <span>{i.quantity} × {i.name}</span>
                <span>{formatMoney(i.lineTotal)}</span>
              </div>
            ))}
            <hr />
            <div className="d-flex justify-content-between"><span>Ara toplam</span><span>{formatMoney(cart.subtotal)}</span></div>
            <div className="d-flex justify-content-between">
              <span>Kargo</span><span>{cart.shippingFee === 0 ? 'Ücretsiz' : formatMoney(cart.shippingFee)}</span>
            </div>
            <hr />
            <div className="d-flex justify-content-between fs-5 fw-bold"><span>Toplam</span><span>{formatMoney(cart.total)}</span></div>
            {error && <div className="alert alert-danger py-2 small mt-3 mb-0" role="alert">{error}</div>}
            <button type="submit" className="btn btn-brand w-100 mt-3" disabled={busy}>
              {busy ? 'İşleniyor…' : `${formatMoney(cart.total)} öde`}
            </button>
            <Link to="/sepet" className="btn btn-link w-100 mt-1">← Sepete dön</Link>
          </div>
        </aside>
      </form>
    </div>
  );
};

export default CheckoutPage;
