# DotnetStore

Staj kapsamında geliştirilen **e-ticaret yönetim paneli** ve **REST API** projesi.

| Katman | Teknoloji |
|--------|-----------|
| API | ASP.NET Core 9, JWT, FluentValidation, EF Core |
| Veri | SQL Server (LocalDB önerilir) |
| Panel | React 19, Vite 8, Bootstrap 5 |

**Kaynak kod:** [github.com/Sudeakyildz/DotnetStore](https://github.com/Sudeakyildz/DotnetStore)

## Gereksinimler

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- SQL Server **LocalDB** veya tam SQL Server

## Hızlı başlangıç

### API

```powershell
cd backend\DotnetStore.Api
dotnet run
```

Varsayılan HTTP profili: Swagger için [http://localhost:5198/swagger](http://localhost:5198/swagger)

### Yönetim paneli

```powershell
cd Frontend
npm ci
npm run dev
```

Panel: [http://localhost:5173](http://localhost:5173) — geliştirmede `/api` istekleri Vite proxy ile API’ye yönlendirilir (`vite.config.ts`).

### İkisini birlikte (Windows)

Depo kökünde:

```powershell
.\start-dev.ps1
```

## Yapılandırma

- Bağlantı dizesi ve geliştirme JWT: `backend/DotnetStore.Api/appsettings.json`
- İsteğe bağlı: `Frontend/.env.example` dosyasını `.env` olarak kopyalayın (ör. farklı API adresi için `VITE_API_BASE_URL`).

**Üretim:** JWT imza anahtarını ve bağlantı bilgilerini repoda tutmayın; [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) veya ortam değişkenleri kullanın.

## Veritabanı

```powershell
cd backend\DotnetStore.Api
dotnet ef database update --project ..\..\db\StajDb\StajDb.csproj
```

Geliştirme ortamında API açılışında migration + seed çalışabilir (bkz. `Program.cs`).

## Giriş (seed)

Geliştirme seeder’ı: **admin** / **admin123** (değiştirmek için `Infrastructure/DatabaseSeeder.cs`).

## Proje yapısı

```
backend/DotnetStore.Api   → Web API
db/StajDb                 → EF Core modeller ve migration’lar
Frontend                  → React yönetim arayüzü
```

## Lisans

Eğitim / staj amaçlıdır; ticari kullanım için ek izin gerekir.
