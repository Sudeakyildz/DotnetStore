export type LoginResponse = {
  accessToken: string;
  tokenType: string;
  expiresInSeconds: number;
  email: string;
  userName: string;
  role: string;
};

export type MeResponse = {
  id: number;
  email: string;
  userName: string;
  role: string;
  firstLoginAt: string | null;
  lastLoginAt: string | null;
  createdAt: string;
};

export type AuditLogListItemDto = {
  id: number;
  userEmail: string;
  userName: string;
  action: string;
  details: string | null;
  createdAtUtc: string;
};

export type UserListItemDto = {
  id: number;
  email: string;
  userName: string;
  role: string;
  firstLoginAt: string | null;
  lastLoginAt: string | null;
  createdAt: string;
  updatedAt: string;
};

export type CategoryDto = {
  id: number;
  name: string;
  description: string | null;
  imageUrl: string | null;
  slug: string | null;
  createdByUserId?: number | null;
  updatedByUserId?: number | null;
  createdAt: string;
  updatedAt: string;
};

/** API: FeatureDataType enum sayı değeri (0–5) */
export type FeatureDto = {
  id: number;
  name: string;
  dataType: number;
  createdByUserId?: number | null;
  updatedByUserId?: number | null;
  createdAt: string;
  updatedAt: string;
};

export type ProductFeatureValueDto = {
  featureId: number;
  featureName: string;
  value: string;
};

/** ProductStatus: 1 Active, 2 Inactive, 3 Draft */
export type ProductListItemDto = {
  id: number;
  categoryId: number;
  categoryName: string | null;
  name: string;
  stock: number;
  status: number;
  activePrice: number | null;
  imageUrl: string | null;
};

export type ProductDetailDto = {
  id: number;
  categoryId: number;
  categoryName: string | null;
  name: string;
  description: string | null;
  stock: number;
  status: number;
  imageUrl: string | null;
  activePrice: number | null;
  featureValues: ProductFeatureValueDto[];
  createdByUserId?: number | null;
  updatedByUserId?: number | null;
  createdAt: string;
  updatedAt: string;
};

export type OrderListItemDto = {
  id: number;
  customerUserId: number;
  customerEmail: string | null;
  customerName: string | null;
  status: number;
  statusLabel: string;
  itemCount: number;
  total: number;
  createdAt: string;
  updatedAt: string;
  note: string | null;
};

export type OrderItemRowDto = {
  productId: number;
  productName: string | null;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
};

export type OrderDetailDto = {
  id: number;
  customerUserId: number;
  customerEmail: string | null;
  customerName: string | null;
  status: number;
  statusLabel: string;
  note: string | null;
  items: OrderItemRowDto[];
  total: number;
  createdByUserId: number | null;
  updatedByUserId: number | null;
  createdAt: string;
  updatedAt: string;
};
