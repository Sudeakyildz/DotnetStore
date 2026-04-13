export type LoginResponse = {
  accessToken: string;
  tokenType: string;
  expiresInSeconds: number;
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
