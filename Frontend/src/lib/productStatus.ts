/** Backend ProductStatus enum */
export const ProductStatus = {
  Active: 1,
  Inactive: 2,
  Draft: 3,
} as const;

export function productStatusLabel(code: number): string {
  switch (code) {
    case ProductStatus.Active:
      return 'Aktif';
    case ProductStatus.Inactive:
      return 'Pasif';
    case ProductStatus.Draft:
      return 'Taslak';
    default:
      return String(code);
  }
}
