import { useCallback, useEffect, useState } from 'react';
import { apiFetch, parseErrorMessage, readJson } from '../api/client';
import type { OrderDetailDto, OrderListItemDto, UserListItemDto } from '../api/types';
import { ORDER_STATUS_OPTIONS, OrderStatus } from '../lib/orderStatus';

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

  const load = useCallback(async () => {
    setLoading(true);
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

  const onStatusRowChange = async (o: OrderListItemDto, next: number) => {
    if (o.status === next) return;
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
      setRows((prev) =>
        prev
          ? prev.map((r) =>
              r.id === o.id
                ? {
                    ...r,
                    status: next,
                    statusLabel: ORDER_STATUS_OPTIONS.find((x) => x.value === next)?.label ?? r.statusLabel,
                    updatedAt: new Date().toISOString(),
                  }
                : r
            )
          : prev
      );
      if (detail && detail.id === o.id) {
        void openDetail(o.id);
      }
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

  return (
    <div>
      <div className="d-flex flex-wrap align-items-baseline justify-content-between gap-2 mb-3">
        <h1 className="h3 mb-0">Müşteri siparişleri (durum)</h1>
        <button type="button" className="btn btn-sm btn-admin-primary" onClick={() => setFormOpen(true)}>
          Yeni kayıt
        </button>
      </div>
      <p className="text-muted small mb-3">
        Aşağıdaki aşamalar yalnız <strong>admin</strong> hesabıyla listelenir ve yönetilir. Durum, müşteri
        (kullanıcı) adına ileri/geri sürülebilir. Bu panel harici, gerçek bir mağazada beğeniler ve sepet
        ayrı servislerde olur; burada durum alanı, müşteriye atanan tüm aşamayı (beğeniden kargoya kadar) tek
        hatta toplar.
      </p>
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
      ) : (
        <div className="table-responsive card shadow-sm">
          <table className="table table-sm table-striped mb-0 align-middle">
            <thead className="table-light">
              <tr>
                <th>Id</th>
                <th>Müşteri</th>
                <th>Durum (admin değiştirir)</th>
                <th>Satır / tutar</th>
                <th>Not</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {(rows ?? []).map((o) => (
                <tr key={o.id}>
                  <td>{o.id}</td>
                  <td>
                    <div className="small">{o.customerEmail ?? '—'}</div>
                    <div className="text-muted" style={{ fontSize: '0.75rem' }}>
                      KullanıcıId: {o.customerUserId}
                    </div>
                  </td>
                  <td style={{ minWidth: 200 }}>
                    <select
                      className="form-select form-select-sm"
                      disabled={savingId === o.id}
                      value={o.status}
                      onChange={(e) => void onStatusRowChange(o, parseInt(e.target.value, 10))}
                    >
                      {ORDER_STATUS_OPTIONS.map((opt) => (
                        <option key={opt.value} value={opt.value}>
                          {opt.label}
                        </option>
                      ))}
                    </select>
                  </td>
                  <td>
                    {o.itemCount} sat · {trMoney(o.total)}
                    <div className="text-muted" style={{ fontSize: '0.7rem' }}>
                      {formatDt(o.updatedAt)}
                    </div>
                  </td>
                  <td className="small text-truncate" style={{ maxWidth: 140 }} title={o.note ?? undefined}>
                    {o.note || '—'}
                  </td>
                  <td>
                    <button type="button" className="btn btn-link btn-sm p-0" onClick={() => void openDetail(o.id)}>
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
        <div className="mt-3 p-3 border rounded bg-body-secondary">
          <div className="d-flex justify-content-between align-items-center mb-2">
            <h2 className="h6 m-0">Sipariş #{detail.id} — {detail.statusLabel}</h2>
            <button className="btn btn-sm btn-outline-dark" type="button" onClick={() => setDetail(null)}>
              Kapat
            </button>
          </div>
          <p className="small mb-2 text-muted">Toplam: {trMoney(detail.total)}</p>
          <div className="table-responsive small">
            <table className="table table-sm mb-0">
              <thead>
                <tr>
                  <th>Ürün</th>
                  <th>Adet</th>
                  <th>Birim fiyat</th>
                  <th>Toplam</th>
                </tr>
              </thead>
              <tbody>
                {detail.items.map((it, k) => (
                  <tr key={k}>
                    <td>
                      #{it.productId} {it.productName ? `· ${it.productName}` : ''}
                    </td>
                    <td>{it.quantity}</td>
                    <td>{trMoney(it.unitPrice)}</td>
                    <td>{trMoney(it.lineTotal)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
};

export default AdminOrders;
