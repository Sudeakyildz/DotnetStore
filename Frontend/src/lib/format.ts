const money = new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' });

export const formatMoney = (n: number | null | undefined): string => (n == null ? '—' : money.format(n));

export const formatDate = (iso: string): string => {
  try {
    return new Date(iso).toLocaleString('tr-TR', { dateStyle: 'medium', timeStyle: 'short' });
  } catch {
    return iso;
  }
};
