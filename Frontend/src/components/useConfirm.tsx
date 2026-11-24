import { useCallback, useRef, useState } from 'react';
import type { ReactNode } from 'react';

type Pending = { message: string; confirmLabel: string; danger: boolean };

/**
 * window.confirm yerine sayfa içi onay penceresi (gömülü tarayıcılarda confirm() engellenebiliyor).
 * Kullanım: const { confirm, dialog } = useConfirm(); if (!(await confirm('Emin misiniz?'))) return; ... {dialog}
 */
export function useConfirm(): {
  confirm: (message: string, opts?: { confirmLabel?: string; danger?: boolean }) => Promise<boolean>;
  dialog: ReactNode;
} {
  const [pending, setPending] = useState<Pending | null>(null);
  const resolver = useRef<((v: boolean) => void) | null>(null);

  const confirm = useCallback(
    (message: string, opts?: { confirmLabel?: string; danger?: boolean }) =>
      new Promise<boolean>((resolve) => {
        resolver.current = resolve;
        setPending({ message, confirmLabel: opts?.confirmLabel ?? 'Evet', danger: opts?.danger ?? true });
      }),
    [],
  );

  const close = (v: boolean) => {
    resolver.current?.(v);
    resolver.current = null;
    setPending(null);
  };

  const dialog = pending ? (
    <div
      role="dialog"
      aria-modal="true"
      style={{ position: 'fixed', inset: 0, background: 'rgba(15,23,42,.5)', zIndex: 3000, display: 'grid', placeItems: 'center', padding: 16 }}
      onClick={() => close(false)}
    >
      <div className="card shadow" style={{ maxWidth: 440, width: '100%' }} onClick={(e) => e.stopPropagation()}>
        <div className="card-body">
          <p className="mb-3">{pending.message}</p>
          <div className="d-flex justify-content-end gap-2">
            <button type="button" className="btn btn-outline-secondary btn-sm" onClick={() => close(false)}>
              Vazgeç
            </button>
            <button
              type="button"
              className={`btn btn-sm ${pending.danger ? 'btn-danger' : 'btn-primary'}`}
              autoFocus
              onClick={() => close(true)}
            >
              {pending.confirmLabel}
            </button>
          </div>
        </div>
      </div>
    </div>
  ) : null;

  return { confirm, dialog };
}
