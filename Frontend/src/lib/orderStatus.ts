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
