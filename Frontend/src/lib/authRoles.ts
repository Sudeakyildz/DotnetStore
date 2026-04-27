export const AuthRoles = {
  Admin: 'Admin',
  StaffPrices: 'StaffPrices',
  StaffFeatures: 'StaffFeatures',
  StaffCategories: 'StaffCategories',
  /** Gelecekteki müşteri (storefront); panel dışı */
  Musteri: 'Musteri',
} as const;

export type AuthRoleName = (typeof AuthRoles)[keyof typeof AuthRoles];
