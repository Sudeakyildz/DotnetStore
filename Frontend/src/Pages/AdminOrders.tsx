import { useCallback, useEffect, useMemo, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useConfirm } from '../components/useConfirm';
import { apiFetch, parseErrorMessage, readJson } from '../api/client';
import type { OrderDetailDto, OrderListItemDto, UserListItemDto } from '../api/types';
import { ORDER_STATUS_OPTIONS, OrderStatus, allowedStatusOptions } from '../lib/orderStatus';

const formatDt = (iso: string) => {
  try {
    return new Date(iso).toLocaleString('tr-TR');
  } catch {
    return iso;
  }
};

const trMoney = (n: number) =>
  new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(n);

const AdminOrders = () => {
  const [rows, setRows] = useState<OrderListItemDto[] | null>(null);
  const [users, setUsers] = useState<UserListItemDto[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [detail, setDetail] = useState<OrderDetailDto | null>(null);
  const [savingId, setSavingId] = useState<number | null>(null);
  const [formOpen, setFormOpen] = useState(false);
  const [createErr, setCreateErr] = useState<string | null>(null);
  const [newCustomerId, setNewCustomerId] = useState<number | ''>('');
  const [newStatus, setNewStatus] = useState<number>(OrderStatus.SiparisAlindi);
  const [newNote, setNewNote] = useState('');
  const [lineRows, setLineRows] = useState<{ productId: string; quantity: string }[]>([
    { productId: '', quantity: '1' },
  ]);
  const [createBusy, setCreateBusy] = useState(false);

  const { confirm, dialog } = useConfirm();
  const [params, setParams] = useSearchParams();
  const statusFilter = params.get('status') ?? '';

  const load = useCallback(async (silent = false) => {
    if (!silent) setLoading(true);
    setError(null);
    try {
      const [res, ures] = await Promise.all([
        apiFetch('/api/Orders'),
        apiFetch('/api/Users/musteri'),
      ]);
      if (!res.ok) {
        setError(await parseErrorMessage(res));
        setRows([]);
        return;
      }
      setRows(await readJson<OrderListItemDto[]>(res));
      if (ures.ok) {
        setUsers(await readJson<UserListItemDto[]>(ures));
      } else {
        setUsers([]);
      }
    } catch {
      setError('Liste yüklenemedi.');
      setRows([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void load();
    // Yeni müşteri siparişleri kendiliğinden görünsün
    const t = window.setInterval(() => void load(true), 20000);
    return () => window.clearInterval(t);
  }, [load]);

  const openDetail = async (id: number) => {
    setError(null);
    const res = await apiFetch(`/api/Orders/${id}`);
    if (!res.ok) {
      setError(await parseErrorMessage(res));
      return;
    }
    setDetail(await readJson<OrderDetailDto>(res));
  };

  const changeStatus = async (o: { id: number; status: number }, next: number) => {
    if (o.status === next) return;
    if (next === OrderStatus.IptalEdildi) {
      const ok = await confirm(
        `Sipariş #${o.id} iptal edilsin mi? Stok geri eklenir ve ödeme müşteriye iade edilir.`,
        { confirmLabel: 'Siparişi iptal et' },
      );
      if (!ok) return;
    }
    setSavingId(o.id);
    setError(null);
    try {
      const res = await apiFetch(`/api/Orders/${o.id}/status`, {
        method: 'PUT',
        body: JSON.stringify({ status: next }),
      });
      if (!res.ok) {
        setError(await parseErrorMessage(res));
        return;
      }
      await load(true);
      if (detail && detail.id === o.id) await openDetail(o.id);
    } finally {
      setSavingId(null);
    }
  };

  const addLine = () => setLineRows((r) => [...r, { productId: '', quantity: '1' }]);
  const changeLine = (i: number, f: 'productId' | 'quantity', v: string) => {
    setLineRows((rows) => rows.map((row, j) => (j === i ? { ...row, [f]: v } : row)));
  };

  const createOrder = async (e: React.FormEvent) => {
    e.preventDefault();
    setCreateErr(null);
    const idNum = newCustomerId === '' ? 0 : Number(newCustomerId);
    if (idNum < 1) {
      setCreateErr('Müşteri seçin (kullanıcı id).');
      return;
    }
    const items: { productId: number; quantity: number }[] = [];
    for (const l of lineRows) {
      const pid = parseInt(l.productId, 10);
      const q = parseInt(l.quantity, 10);
      if (Number.isNaN(pid) || pid < 1) continue;
      if (Number.isNaN(q) || q < 1) {
        setCreateErr('Miktar geçerli değil.');
        return;
      }
      items.push({ productId: pid, quantity: q });
    }
    if (items.length === 0) {
      setCreateErr('En az bir satır: ürün no ve adet girin.');
      return;
    }
    setCreateBusy(true);
    try {
      const res = await apiFetch('/api/Orders', {
        method: 'POST',
        body: JSON.stringify({
          customerUserId: idNum,
          items,
          status: newStatus,
          note: newNote.trim() || null,
        }),
      });
      if (!res.ok) {
        setCreateErr(await parseErrorMessage(res));
        return;
      }
      setFormOpen(false);
      setNewNote('');
      setLineRows([{ productId: '', quantity: '1' }]);
      setNewStatus(OrderStatus.SiparisAlindi);
      await load();
    } catch {
      setCreateErr('Kayıt isteği başarısız.');
    } finally {
      setCreateBusy(false);
    }
  };

  const counts = useMemo(() => {
    const c: Record<string, number> = {};
    for (const o of rows ?? []) c[o.status] = (c[o.status] ?? 0) + 1;
    return c;
  }, [rows]);

  const visible = (rows ?? []).filter((o) => !statusFilter || String(o.status) === statusFilter);
  const tabs = [
    { key: '', label: 'Tümü', count: rows?.length ?? 0 },
    { key: String(OrderStatus.SiparisAlindi), label: 'Yeni', count: counts[OrderStatus.SiparisAlindi] ?? 0 },
    { key: String(OrderStatus.KargoyaVerildi), label: 'Kargoda', count: counts[OrderStatus.KargoyaVerildi] ?? 0 },
    { key: String(OrderStatus.TeslimEdildi), label: 'Teslim edildi', count: counts[OrderStatus.TeslimEdildi] ?? 0 },
    { key: String(OrderStatus.IptalEdildi), label: 'İptal', count: counts[OrderStatus.IptalEdildi] ?? 0 },
  ];

  const nextOptions = (status: number) => allowedStatusOptions(status).filter((o) => o.value !== status);

  return (
    <div>
      {dialog}
      <div className="d-flex flex-wrap align-items-baseline justify-content-between gap-2 mb-3">
        <h1 className="h3 mb-0">Siparişler</h1>
        <div className="d-flex gap-2">
          <button type="button" className="btn btn-sm btn-outline-secondary" onClick={() => void load()}>
            Yenile
          </button>
          <button type="button" className="btn btn-sm btn-admin-primary" onClick={() => setFormOpen(true)}>
            Manuel sipariş
          </button>
        </div>
      </div>

      <ul className="nav nav-pills mb-3 gap-1">
        {tabs.map((t) => (
          <li className="nav-item" key={t.key}>
            <button
              type="button"
              className={`nav-link py-1 px-3 ${statusFilter === t.key ? 'active' : 'bg-white border'}`}
              onClick={() => setParams(t.key ? { status: t.key } : {})}
            >
              {t.label} <span className="badge text-bg-light ms-1">{t.count}</span>
            </button>
          </li>
        ))}
      </ul>

      {error && <div className="alert alert-danger py-2">{error}</div>}

      {formOpen && (
        <div className="card shadow-sm border-0 mb-4">
          <div className="card-body">
            <h2 className="h6">Yeni sipariş (admin)</h2>
            {createErr && <div className="alert alert-warning py-2 small">{createErr}</div>}
            <form onSubmit={createOrder} className="small">
              <div className="row g-2 align-items-end mb-2">
                <div className="col-md-4">
                  <label className="form-label text-muted">Sipariş veren (müşteri)</label>
                  <p className="form-text text-muted mb-0" style={{ fontSize: '0.7rem' }}>
                    Yalnızca Musteri rolü; admin ve personel (fiyat/özellik vb.) bu listede yok.
                  </p>
                  <select
                    className="form-select form-select-sm"
                    value={newCustomerId}
                    onChange={(e) =>
                      setNewCustomerId(e.target.value === '' ? '' : parseInt(e.target.value, 10))
                    }
                    required
                  >
                    <option value="">Seçin…</option>
                    {(users ?? []).map((u) => (
                      <option key={u.id} value={u.id}>
                        {u.email} (id: {u.id})
                      </option>
                    ))}
                  </select>
                </div>
                <div className="col-md-2">
                  <label className="form-label text-muted">Başlangıç durumu</label>
                  <select
                    className="form-select form-select-sm"
                    value={newStatus}
                    onChange={(e) => setNewStatus(parseInt(e.target.value, 10))}
                  >
                    {ORDER_STATUS_OPTIONS.map((o) => (
                      <option key={o.value} value={o.value}>
                        {o.label}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="col-md-6">
                  <label className="form-label text-muted">Not (isteğe bağlı)</label>
                  <input
                    className="form-control form-control-sm"
                    value={newNote}
                    onChange={(e) => setNewNote(e.target.value)}
                  />
                </div>
              </div>
              <p className="text-muted" style={{ fontSize: '0.8rem' }}>
                Ürün satırları (ürün id + adet, birim fiyat o an veritabanındaki açık fiyattır):
              </p>
              {lineRows.map((l, i) => (
                <div key={i} className="d-flex flex-wrap gap-2 mb-2">
                  <input
                    className="form-control form-control-sm"
                    style={{ maxWidth: 120 }}
                    placeholder="Ürün no"
                    value={l.productId}
                    onChange={(e) => changeLine(i, 'productId', e.target.value)}
                  />
                  <input
                    className="form-control form-control-sm"
                    style={{ maxWidth: 100 }}
                    placeholder="Adet"
                    value={l.quantity}
                    onChange={(e) => changeLine(i, 'quantity', e.target.value)}
                  />
                  {i === lineRows.length - 1 && (
                    <button type="button" className="btn btn-sm btn-outline-secondary" onClick={addLine}>
                      + Satır
                    </button>
                  )}
                </div>
              ))}
              <div className="d-flex gap-2 mt-2">
                <button className="btn btn-sm btn-admin-primary" type="submit" disabled={createBusy}>
                  {createBusy ? 'Gönderiliyor…' : 'Oluştur'}
                </button>
                <button
                  className="btn btn-sm btn-outline-secondary"
                  type="button"
                  onClick={() => {
                    setFormOpen(false);
                    setCreateErr(null);
                  }}
                >
                  Vazgeç
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {loading ? (
        <p className="text-muted">Yükleniyor…</p>
      ) : visible.length === 0 ? (
        <div className="card shadow-sm"><div className="card-body text-muted">Bu filtrede sipariş yok.</div></div>
      ) : (
        <div className="table-responsive card shadow-sm">
          <table className="table table-hover mb-0 align-middle">
            <thead className="table-light">
              <tr>
                <th>No</th>
                <th>Müşteri</th>
                <th>Tarih</th>
                <th>Tutar</th>
                <th>Ödeme</th>
                <th>Durum</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {visible.map((o) => (
                <tr key={o.id} className={o.status === OrderStatus.SiparisAlindi ? 'table-warning' : ''}>
                  <td className="fw-semibold">#{o.id}</td>
                  <td>
                    <div className="small">{o.customerName ?? '—'}</div>
                    <div className="text-muted" style={{ fontSize: '0.75rem' }}>{o.customerEmail ?? ''}</div>
                  </td>
                  <td className="small">{formatDt(o.createdAt)}</td>
                  <td>
                    {trMoney(o.total)}
                    <div className="text-muted" style={{ fontSize: '0.7rem' }}>{o.itemCount} kalem</div>
                  </td>
                  <td className="small">{o.paymentStatusLabel || '—'}</td>
                  <td style={{ minWidth: 190 }}>
                    <select
                      className="form-select form-select-sm"
                      disabled={savingId === o.id}
                      value={o.status}
                      onChange={(e) => void changeStatus(o, parseInt(e.target.value, 10))}
                    >
                      {allowedStatusOptions(o.status).map((opt) => (
                        <option key={opt.value} value={opt.value}>{opt.label}</option>
                      ))}
                    </select>
                  </td>
                  <td className="text-end">
                    <button type="button" className="btn btn-sm btn-outline-primary" onClick={() => void openDetail(o.id)}>
                      Detay
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {detail && (
        <>
          <div
            style={{ position: 'fixed', inset: 0, background: 'rgba(15,23,42,.4)', zIndex: 2600 }}
            onClick={() => setDetail(null)}
          />
          <aside
            role="dialog"
            aria-label={`Sipariş #${detail.id} detayı`}
            style={{
              position: 'fixed', top: 0, right: 0, bottom: 0, width: 'min(520px, 100vw)', background: '#fff',
              zIndex: 2700, boxShadow: '-8px 0 30px rgba(0,0,0,.2)', overflowY: 'auto',
            }}
          >
            <div className="p-3 border-bottom d-flex justify-content-between align-items-start">
              <div>
                <h2 className="h5 m-0">Sipariş #{detail.id}</h2>
                <div className="text-muted small">{formatDt(detail.createdAt)}</div>
              </div>
              <button className="btn-close" type="button" aria-label="Kapat" onClick={() => setDetail(null)} />
            </div>

            <div className="p-3">
              <div className="d-flex align-items-center gap-2 mb-3">
                <span className="badge text-bg-primary fs-6">{detail.statusLabel}</span>
                {detail.paymentStatusLabel && <span className="badge text-bg-light border">{detail.paymentStatusLabel}</span>}
              </div>

              {nextOptions(detail.status).length > 0 && (
                <div className="mb-3">
                  <div className="small text-muted mb-1">Durumu güncelle</div>
                  <div className="d-flex flex-wrap gap-2">
                    {nextOptions(detail.status).map((opt) => (
                      <button
                        key={opt.value}
                        type="button"
                        disabled={savingId === detail.id}
                        className={`btn btn-sm ${opt.value === OrderStatus.IptalEdildi ? 'btn-outline-danger' : 'btn-admin-primary'}`}
                        onClick={() => void changeStatus(detail, opt.value)}
                      >
                        {opt.value === OrderStatus.IptalEdildi ? 'İptal et & iade' : `→ ${opt.label}`}
                      </button>
                    ))}
                  </div>
                </div>
              )}

              <h3 className="h6">Müşteri</h3>
              <p className="small mb-3">
                {detail.customerName ?? '—'}<br />
                <span className="text-muted">{detail.customerEmail}</span>
              </p>

              {detail.shippingAddress && (
                <>
                  <h3 className="h6">Teslimat</h3>
                  <p className="small mb-3">
                    {detail.shippingFullName} · {detail.shippingPhone}<br />
                    {detail.shippingAddress}, {detail.shippingCity}
                    {detail.trackingNumber && (<><br />Kargo takip no: <strong>{detail.trackingNumber}</strong></>)}
                  </p>
                </>
              )}

              {detail.note && (
                <>
                  <h3 className="h6">Sipariş notu</h3>
                  <p className="small mb-3">{detail.note}</p>
                </>
              )}

              <h3 className="h6">Ürünler</h3>
              <table className="table table-sm small">
                <thead>
                  <tr><th>Ürün</th><th className="text-end">Adet</th><th className="text-end">Birim</th><th className="text-end">Toplam</th></tr>
                </thead>
                <tbody>
                  {detail.items.map((it, k) => (
                    <tr key={k}>
                      <td>{it.productName ?? `#${it.productId}`}</td>
                      <td className="text-end">{it.quantity}</td>
                      <td className="text-end">{trMoney(it.unitPrice)}</td>
                      <td className="text-end">{trMoney(it.lineTotal)}</td>
                    </tr>
                  ))}
                </tbody>
                <tfoot>
                  {detail.shippingFee ? (
                    <tr><td colSpan={3} className="text-end">Kargo</td><td className="text-end">{trMoney(detail.shippingFee)}</td></tr>
                  ) : null}
                  <tr className="fw-bold"><td colSpan={3} className="text-end">Genel toplam</td><td className="text-end">{trMoney(detail.total)}</td></tr>
                </tfoot>
              </table>
              {detail.paymentCard && <div className="small text-muted">Ödeme: {detail.paymentCard}</div>}
            </div>
          </aside>
        </>
      )}
    </div>
  );
};

export default AdminOrders;
