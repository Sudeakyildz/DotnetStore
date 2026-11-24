export const OrderStatus = {
  BegeniyeEklendi: 1,
  Sepette: 2,
  SiparisAlindi: 3,
  KargoyaVerildi: 4,
  TeslimEdildi: 5,
  IptalEdildi: 6,
} as const;

export const ORDER_STATUS_OPTIONS: { value: number; label: string }[] = [
  { value: 1, label: 'Beğeniye eklendi' },
  { value: 2, label: 'Sepette' },
  { value: 3, label: 'Sipariş alındı' },
  { value: 4, label: 'Kargoya verildi' },
  { value: 5, label: 'Teslim edildi' },
  { value: 6, label: 'İptal edildi' },
];

/** Sunucudaki geçiş kurallarıyla aynı (OrderStatusHelper.IsAllowedTransition). */
export function allowedStatusOptions(current: number): { value: number; label: string }[] {
  const allowed = (to: number): boolean => {
    if (to === current) return true;
    switch (current) {
      case OrderStatus.BegeniyeEklendi:
      case OrderStatus.Sepette:
        return true;
      case OrderStatus.SiparisAlindi:
        return to === OrderStatus.KargoyaVerildi || to === OrderStatus.IptalEdildi;
      case OrderStatus.KargoyaVerildi:
        return to === OrderStatus.TeslimEdildi || to === OrderStatus.IptalEdildi;
      default:
        return false;
    }
  };
  return ORDER_STATUS_OPTIONS.filter((o) => allowed(o.value));
}
