import { OrderStatus } from '../lib/orderStatus';

export const statusBadge = (status: number) => {
  switch (status) {
    case OrderStatus.SiparisAlindi: return 'text-bg-primary';
    case OrderStatus.KargoyaVerildi: return 'text-bg-info';
    case OrderStatus.TeslimEdildi: return 'text-bg-success';
    case OrderStatus.IptalEdildi: return 'text-bg-secondary';
    default: return 'text-bg-light';
  }
};
