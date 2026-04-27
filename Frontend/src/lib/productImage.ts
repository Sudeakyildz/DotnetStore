const apiBase = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.replace(/\/$/, '') ?? '';

/**
 * Ürün/kategori görseli: tam (http/https) URL, veya API'deki `wwwroot` altındaki yol.
 * Veritabanına örnek: `/images/urun-1.jpg` (dosya: `wwwroot/images/urun-1.jpg`) veya harici resim linki.
 */
export function productImageSrc(imageUrl: string | null | undefined): string | undefined {
  const u = imageUrl?.trim();
  if (!u) return undefined;
  if (u.startsWith('http://') || u.startsWith('https://')) return u;
  if (u.startsWith('/')) {
    if (apiBase) return `${apiBase}${u}`;
    return u;
  }
  return undefined;
}
