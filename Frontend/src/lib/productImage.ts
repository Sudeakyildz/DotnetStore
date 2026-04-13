/** Ürün görseli: tam URL veya boş. Göreli yol ileride /uploads ile genişletilebilir. */
export function productImageSrc(imageUrl: string | null | undefined): string | undefined {
  const u = imageUrl?.trim();
  if (!u) return undefined;
  if (u.startsWith('http://') || u.startsWith('https://')) return u;
  return undefined;
}
