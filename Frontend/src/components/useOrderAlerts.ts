import { useCallback, useEffect, useRef, useState } from 'react';
import { apiJson } from '../api/client';

type Summary = {
  pendingCount: number;
  latestOrderId: number;
  latest: { id: number; customerName: string | null; total: number; createdAt: string }[];
};

export type OrderAlert = { id: number; customerName: string | null; total: number };

const STORAGE_KEY = 'admin_last_seen_order_id';
const POLL_MS = 10_000;

function beep() {
  try {
    const Ctx = window.AudioContext ?? (window as unknown as { webkitAudioContext: typeof AudioContext }).webkitAudioContext;
    const ctx = new Ctx();
    const osc = ctx.createOscillator();
    const gain = ctx.createGain();
    osc.connect(gain);
    gain.connect(ctx.destination);
    osc.frequency.value = 880;
    gain.gain.value = 0.05;
    osc.start();
    osc.stop(ctx.currentTime + 0.18);
  } catch {
    /* ses engellenmiş olabilir */
  }
}

/**
 * Yeni müşteri siparişlerini yoklar (10 sn). Sadece Admin için etkinleştirin.
 * Panel kapalıyken gelen siparişler de bir sonraki açılışta bildirim olarak gösterilir.
 */
export function useOrderAlerts(enabled: boolean) {
  const [pendingCount, setPendingCount] = useState(0);
  const [alerts, setAlerts] = useState<OrderAlert[]>([]);
  const baseline = useRef<number | null>(null);

  useEffect(() => {
    if (!enabled) return;
    try {
      const v = localStorage.getItem(STORAGE_KEY);
      baseline.current = v ? Number(v) : null;
    } catch {
      baseline.current = null;
    }

    let stopped = false;
    const tick = async () => {
      try {
        const s = await apiJson<Summary>('/api/Orders/summary');
        if (stopped) return;
        setPendingCount(s.pendingCount);

        if (baseline.current === null) {
          baseline.current = s.latestOrderId; // ilk kurulumda eski siparişler için bildirim yağdırma
        } else {
          const fresh = s.latest.filter((o) => o.id > (baseline.current ?? 0));
          if (fresh.length > 0) {
            setAlerts((a) => [
              ...fresh.map((o) => ({ id: o.id, customerName: o.customerName, total: o.total })),
              ...a,
            ].slice(0, 5));
            beep();
          }
          baseline.current = Math.max(baseline.current, s.latestOrderId);
        }
        try {
          localStorage.setItem(STORAGE_KEY, String(baseline.current));
        } catch {
          /* yoksay */
        }
      } catch {
        /* geçici hata: sonraki turda tekrar */
      }
    };

    void tick();
    const t = window.setInterval(() => void tick(), POLL_MS);
    return () => {
      stopped = true;
      window.clearInterval(t);
    };
  }, [enabled]);

  useEffect(() => {
    const base = 'DotnetStore';
    document.title = pendingCount > 0 ? `(${pendingCount}) Yeni sipariş · ${base}` : base;
    return () => {
      document.title = base;
    };
  }, [pendingCount]);

  const dismiss = useCallback((id: number) => setAlerts((a) => a.filter((x) => x.id !== id)), []);
  const dismissAll = useCallback(() => setAlerts([]), []);

  return { pendingCount, alerts, dismiss, dismissAll };
}
