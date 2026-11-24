export type PagedResult<T> = { items: T[]; total: number; page: number; pageSize: number };

export type StoreProduct = {
  id: number;
  name: string;
  categoryId: number;
  categoryName: string | null;
  price: number;
  imageUrl: string | null;
  stock: number;
};

export type StoreProductDetail = {
  id: number;
  name: string;
  description: string | null;
  categoryId: number;
  categoryName: string | null;
  price: number;
  imageUrl: string | null;
  stock: number;
  features: { name: string; value: string }[];
  related: StoreProduct[];
};

export type StoreCategory = {
  id: number;
  name: string;
  slug: string | null;
  description: string | null;
  imageUrl: string | null;
  productCount: number;
};

export type CartItem = {
  productId: number;
  name: string;
  imageUrl: string | null;
  unitPrice: number;
  quantity: number;
  stock: number;
  lineTotal: number;
  available: boolean;
};

export type Cart = {
  items: CartItem[];
  itemCount: number;
  subtotal: number;
  shippingFee: number;
  total: number;
  freeShippingThreshold: number;
};

export type CheckoutPayload = {
  shippingFullName: string;
  shippingPhone: string;
  shippingCity: string;
  shippingAddress: string;
  note: string | null;
  card: { holderName: string; number: string; expMonth: number; expYear: number; cvv: string };
};
