# DotnetStore

ASP.NET Core + React ile geliştirilen **e-ticaret platformu**: müşteri vitrini (katalog, sepet, favoriler, ödeme, sipariş takibi) ve rol tabanlı **yönetim paneli**.

| Katman | Teknoloji |
|--------|-----------|
| API | ASP.NET Core (net8 hedefi), JWT, FluentValidation, EF Core, rate limiting |
| Veri | SQL Server (LocalDB önerilir) |
| Arayüz | React 19, Vite, Bootstrap 5 (vitrin: `/`, panel: `/yonetim`) |

**Kaynak kod:** [github.com/Sudeakyildz/DotnetStore](https://github.com/Sudeakyildz/DotnetStore)

## Özellikler

**Müşteri vitrini**
- Katalog: arama, kategori, fiyat aralığı, stokta olanlar, sıralama, sayfalama
- Ürün detayı, benzer ürünler, favoriler
- Sepet (stok sınırlı), kargo hesabı (500 ₺ ve üzeri ücretsiz)
- Ödeme (sağlayıcı soyutlaması, şu an **sahte/mock** sağlayıcı), teslimat bilgisi
- Siparişlerim, sipariş takibi, kargoya verilmeden iptal (otomatik iade + stok geri)
- Üyelik / giriş / şifre değiştirme

**Yönetim paneli** (`/yonetim`): kategori, ürün, özellik, fiyat geçmişi, siparişler (durum kuralları, ödeme ve teslimat bilgisi), kullanıcılar, işlem kayıtları. Rol bazlı yetki: Admin, StaffPrices, StaffFeatures, StaffCategories.

**Sağlamlık**
- Stok tek SQL UPDATE ile atomik düşer (eşzamanlı siparişte eksiye inmez); ödeme başarısızsa sipariş ve stok geri alınır
- Sipariş durum geçişleri kurallıdır (Sipariş alındı → Kargoda → Teslim; iptalde stok + ödeme iadesi)
- Giriş/kayıt uçlarında rate limit; kart verisi saklanmaz/loglanmaz (yalnızca marka + son 4 hane)
- Tarihler UTC (`Z` ile) döner

## Gereksinimler

- .NET SDK (8 veya üzeri; proje `RollForward=LatestMajor` ile yeni çalışma zamanlarında da çalışır)
- [Node.js 20+](https://nodejs.org/)
- SQL Server **LocalDB** veya tam SQL Server

## Hızlı başlangıç

```powershell
.\start-dev.ps1
```

veya elle:

```powershell
# API (http://localhost:5198/swagger)
cd backend\DotnetStore.Api
dotnet run

# Arayüz (http://localhost:5173)
cd Frontend
npm ci
npm run dev
```

Geliştirmede API açılışında migration + örnek veri (seed) otomatik çalışır.

### Demo hesaplar (yalnızca Development seed)

| Rol | E-posta | Şifre |
|-----|---------|-------|
| Admin | `admin@dotnetstore.local` | `Admin123!` |
| Fiyat / Özellik / Kategori sorumlusu | `fiyat@…`, `ozellik@…`, `kategori@dotnetstore.local` | `Fiyat123!`, `Ozellik123!`, `Kategori123!` |
| Müşteri | `musteri.ayse@staj.local` | `Musteri!12345` |

Hesapları `backend/DotnetStore.Api/appsettings.Development.json` içinden değiştirebilirsiniz (bu dosya `.gitignore`'dadır; örneği `appsettings.Development.json.example`).

### Test ödemesi

Ödeme ekranında gerçek para çekilmez. Test kartları: `4242 4242 4242 4242` (başarılı), `4000 0000 0000 0002` (reddedilir), `4000 0000 0000 9995` (yetersiz bakiye). Gelecek bir son kullanma tarihi ve 3 haneli CVV girin.

**Gerçek ödeme için:** `Payments/IPaymentProvider.cs` arayüzünü iyzico / PayTR / Stripe gibi bir sağlayıcı için uygulayın ve `Program.cs` içindeki `MockPaymentProvider` kaydını değiştirin. Kart bilgisini asla kendi sunucunuzda saklamayın; mümkünse sağlayıcının barındırılan ödeme formunu / 3D Secure akışını kullanın.

## Yapılandırma

| Ayar | Açıklama |
|------|----------|
| `Jwt:SigningKey` | **Zorunlu**, en az 32 karakter. Repoda yoktur. Geliştirmede `appsettings.Development.json`'dadır; üretimde `Jwt__SigningKey` ortam değişkeni veya [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) kullanın |
| `ConnectionStrings:DefaultConnection` | SQL Server bağlantısı |
| `Cors:Origins` | Üretimde izin verilen arayüz adresleri (dizi). Boşsa Development dışında çapraz-origin kapalıdır |
| `Store:FreeShippingThreshold` / `Store:ShippingFee` | Ücretsiz kargo eşiği ve kargo ücreti |
| `Frontend/.env` | `VITE_API_BASE_URL` (isteğe bağlı; `.env.example`) |

## Veritabanı

```powershell
cd backend\DotnetStore.Api
dotnet ef database update --project ..\..\db\StajDb\StajDb.csproj
```

## Testler

```powershell
dotnet test StajProje.sln
```

## Proje yapısı

```
backend/DotnetStore.Api   → Web API (controller / service / payments)
db/StajDb                 → EF Core modelleri ve migration'lar
Frontend                  → React: src/shop (vitrin), src/Pages (yönetim)
tests/DotnetStore.Tests   → xUnit testleri
```

## Lisans

Eğitim / staj amaçlıdır; ticari kullanım için ek izin gerekir.
