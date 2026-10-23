# OzFW

**.NET 9 için modüler mikroservis altyapısı ve model tabanlı kod üretici.**

OzFW iki problemi birlikte çözmeyi hedefler:

1. **Altyapı seçimini koddan ayırmak.** Mesaj kuyruğu (RabbitMQ ya da Kafka), loglama (ELK, Graylog ya da
   yalnızca konsol/OTLP), önbellek (Redis) ve veritabanı yapılandırmayla seçilir; servis kodu bunlardan habersizdir.
2. **Tekrarlayan katman kodunu yazmamak.** Analiz ekibinden gelen veri modeli (JSON, CSV ya da dbdiagram/Mermaid
   diyagramı) tek komutla PostgreSQL tablolarına (DDL) ve entity → EF eşlemesi → DTO → repository → servis →
   validation → REST API zincirinin tamamına dönüşür.

Sonuç: her bileşenin bir örneğinin bulunduğu, yeni servislerin üretilerek çoğaltıldığı bir mikroservis iskeleti.
Repo içindeki **USBS** uygulaması bu altyapının örnek kullanımıdır.

> **Durum:** Aktif geliştirme. Çekirdek katmanlar, kod üretici ve örnek modüller çalışır ve testlidir;
> bazı bileşenler henüz iskelet halindedir (aşağıdaki tabloya bakın). AppHost'un tüm servisleri tek noktadan
> yönetme modu yeniden tasarlanmayı bekliyor.

---

## İçindekiler

- [Hızlı başlangıç](#hızlı-başlangıç)
- [Modelden servis üretmek](#modelden-servis-üretmek)
- [Repo yapısı](#repo-yapısı)
- [Framework bileşenleri](#framework-bileşenleri)
- [API sözleşmesi](#api-sözleşmesi)
- [Altyapı seçimi](#altyapı-seçimi)
- [Yol haritası](#yol-haritası)

---

## Hızlı başlangıç

**Gereksinimler:** .NET 9 SDK, PostgreSQL (servisleri veriyle çalıştırmak için). RabbitMQ/Kafka, Redis ve
ELK/Graylog isteğe bağlıdır.

```bash
git clone https://github.com/aozan88/ozfw.git
cd ozfw/Projects/backend/USBS
dotnet build USBS.sln
dotnet test USBS.sln
```

Her API kendi profiliyle bağımsız çalışır:

```bash
cd USBS.Services/Ornek/USBS.Ornek.API
dotnet run --launch-profile Ornek.API-Development
# Swagger: https://localhost:24300/swagger
```

Bağlantı dizesi `appsettings.Development.json` içindeki `ConnectionStrings:DefaultConnection` anahtarındadır.
Gerçek şifreleri repoya yazmayın; `dotnet user-secrets` ya da ortam değişkeni (`ConnectionStrings__DefaultConnection`) kullanın.

| Servis | Profil | Port (HTTPS) |
|---|---|---|
| Ortak.API | `Ortak.API-Development` | 24100 |
| Sistem.API | `Sistem.API-Development` | 24200 |
| Ornek.API (üretilmiş) | `Ornek.API-Development` | 24300 |

---

## Modelden servis üretmek

Kod üretici `Frameworks/OzFw/OzFw.CodeGenerator/OzFw.ScribanCodeGen` altındadır. Ayrıntılı belge: [docs/codegen.md](docs/codegen.md).

**1. Modeli yazın** (ya da analiz ekibinin diyagramını/CSV'sini olduğu gibi kullanın). Aynı modelin dört biçimi
[`samples/codegen/`](samples/codegen) altında; dördü birebir aynı çıktıyı verir.

```dbml
// module: Ornek
Table ornek.urun {
  id uuid [pk]
  ad varchar(150) [not null, note: 'Ürün adı']
  fiyat decimal(18,2) [not null]
  stok int
  kategori_id uuid [not null, ref: > ornek.kategori.id]
}
```

**2. Üretin** (repo kökünden):

```bash
dotnet run --project Frameworks/OzFw/OzFw.CodeGenerator/OzFw.ScribanCodeGen -- --model samples/codegen/ornek.dbml --dry-run
dotnet run --project Frameworks/OzFw/OzFw.CodeGenerator/OzFw.ScribanCodeGen -- --model samples/codegen/ornek.dbml
```

**3. Çıktı:**

- `sql/<modul>/_all.sql` — bağımlılık sırasıyla tablolar, FK'ler, index'ler, açıklamalar. Migration yok:
  script review edilip veritabanına uygulanır; birden çok kez çalıştırılabilir.
- `Projects/backend/USBS/...` — entity, EF eşlemesi, DTO, repository, servis, validation, controller, DI kayıtları
  ve ilk çalıştırmada modülün API/Application/Data projeleri (sln'e eklenir).

`*.g.cs` dosyaları her üretimde yenilenir; tüm sınıflar `partial` olduğundan özel iş kuralları ayrı dosyalara
yazılır ve korunur. Testler, EF'in beklediği şemanın üretilen SQL ile kolon/tip/kısıt bazında aynı olduğunu doğrular.

---

## Repo yapısı

```
Frameworks/OzFw/                Yeniden kullanılabilir framework (NuGet'e çıkarılabilir katman)
  OzFw.BaseEssentials/            İstek/yanıt modelleri, filtre motoru (ApplyQuery), yardımcılar
  OzFw.BaseInfrastructure/        GenericRepository, soft-delete, audit'li DbContext
  OzFw.ServiceBase/               BaseService (Mapster'lı CRUD), RuleSet validation
  OzFw.ApiBase/                   BaseController (8 standart POST uç)
  OzFw.EventManagement/           IEventBus + RabbitMQ / Kafka
  OzFw.Logging/                   Serilog, ELK, Graylog
  OzFw.Caching/                   IKonCache + Redis
  OzFw.CodeGenerator/             Model → SQL DDL + C# katmanları
  ...                             Notification, FileManagement, Localization, Configuration (iskelet)
Projects/backend/USBS/          Örnek uygulama
  USBS.Common/                    Ortak domain (entity'ler) ve altyapı
  USBS.Common.Dtos/               DTO'lar
  USBS.Components/                Framework bileşenlerinin uygulamaya bağlanması (provider'lar)
  USBS.ServiceDefaults/           Ortak kayıtlar: loglama, olay altyapısı, cache, OpenTelemetry
  USBS.Services/<Modül>/          Her modül: .API / .Application / .Data
    Ortak, Sistem                   Elle yazılmış örnekler
    Ornek                           Modelden üretilmiş örnek
  USBS.AppHost/                   Servisleri tek noktadan çalıştırma (yeniden tasarlanacak)
  USBS.Tests/                     Framework, kod üretici ve EF/DDL tutarlılık testleri
docs/                           Kod üretici belgesi ve model şeması
samples/codegen/                Aynı modelin JSON / CSV / DBML / Mermaid biçimleri
sql/                            Üretilmiş DDL script'leri
```

Her modül aynı katman düzenini izler:

```
<Modül>.API            Controllers (BaseController), Program, Extensions
<Modül>.Application    Business (servisler), Contract (repository arayüzleri), Validations, Mappings, Registration
<Modül>.Data           Persistence (DbContext, EF eşlemeleri), Repositories, Registration
```

---

## Framework bileşenleri

| Bileşen | Durum | Açıklama |
|---|---|---|
| `OzFwBaseModels` | ✅ | `BaseRequestDto<T>`, `BaseResponseDto<T>`, `BaseGetByIdDto`, filtre/sıralama modelleri, `DurumEnum`, `[DtoField]` / `[EntityField]` |
| `OzFwBaseHelpers` | ✅ | `ApplyQuery`: iç içe and/or filtre ağacı, koleksiyonlarda Any/All, `in` / `between` / `isnull`, çoklu sıralama, sayfalama; EF'te parametreli sorgu üretir |
| `OzFwBaseInfrastructure` | ✅ | `GenericRepository<T>` (filtreli listeleme, toplu ekle/güncelle/sil), `ISoftDeletable` ile soft-delete, oluşturan/değiştiren bilgisini dolduran `OzFwDbContext` |
| `OzFw.ServiceBase` | ✅ | `BaseService<TEntity, TDto>`: tüm CRUD hazır, override ile özelleştirilir; RuleSet'li istek validation'ları |
| `OzFw.ApiBase` | ✅ | `BaseController`: getById, getAll, create, manyCreate, update, manyUpdate, deleteByFilter, delete; `AddOzFwControllers()` |
| `OzFw.CodeGenerator` | ✅ | JSON / CSV / DBML / Mermaid → PostgreSQL DDL + C# katmanları |
| `OzFw.EventManagement` | ✅ | `IEventBus`; RabbitMQ ve Kafka uygulamaları |
| `OzFw.Logging` | ✅ | Serilog yapılandırması, Elasticsearch (isteğe bağlı), Graylog |
| `OzFw.Caching` | ✅ | `IKonCache`, Redis uygulaması |
| `OzFw.Notification` | 🚧 | Mail, SMS, SignalR — yalnızca ayar sınıfları |
| `OzFw.FileManagement`, `OzFw.Localization`, `OzFw.Configuration` | 🚧 | İskelet |
| `OzFw.Security`, `OzFw.Reporting`, `OzFw.Containerization` | 📝 | Planlandı |

---

## API sözleşmesi

Tüm uçlar `POST api/<Entity>/<işlem>` biçimindedir ve aynı zarfı kullanır.

**Listeleme** — `POST api/Urun/getAll`

```json
{
  "clientId": "5f0c...",
  "page": 1,
  "size": 20,
  "filter": {
    "logic": "and",
    "filters": [
      { "field": "fiyat", "operator": "gte", "value": 100 },
      { "logic": "or", "filters": [
          { "field": "ad", "operator": "contains", "value": "elma" },
          { "field": "kategori.ad", "operator": "eq", "value": "Meyve" } ] }
    ]
  },
  "sort": [ { "field": "ad", "dir": "ASC" } ]
}
```

Operatörler: `eq`, `neq`, `lt`, `lte`, `gt`, `gte`, `contains`, `doesnotcontain`, `startswith`, `endswith`,
`in`, `between`, `isnull`, `isnotnull`. Metin karşılaştırmaları büyük/küçük harf duyarsızdır. `size: -1` tüm kayıtları döner.

**Ekleme / güncelleme** — `POST api/Urun/create`, `.../update`: `{ "clientId": "...", "payload": { ... } }`
**Tekil / silme** — `POST api/Urun/getById`, `.../delete`: `{ "clientId": "...", "payload": "<id>" }`

**Yanıt**

```json
{ "isSuccess": true, "mesaj": "", "pageNumber": 1, "size": 20, "count": 20,
  "totalRecordCount": 134, "totalTableCount": 900, "objectValue": [ ... ] }
```

Doğrulama hataları `400` ile `fieldNames` listesinde döner (ör. `"Payload.Ad: 'Ad' boş olmamalı."`); silme
işlemleri soft-delete'tir (`durum = -1`) ve silinmiş kayıtlar sorgulara gelmez.

---

## Altyapı seçimi

| Konu | Nasıl seçilir |
|---|---|
| Olay altyapısı | `EventBus:Provider` = `inmemory` (varsayılan, broker gerektirmez) / `rabbitmq` / `kafka` / `none`. Ayarlar `RabbitMqEventBus` / `KafkaEventBus` bölümlerinde. Servisler yalnızca `IEventBus`'a bağlıdır (`AddOzFwEventBus`) |
| Önbellek | `Cache:Provider` = `memory` (varsayılan) / `redis` (`Redis:ConnectionString`). Servisler `IKonCache`'e bağlıdır (`AddOzFwCache`) |
| Loglama | `Logging:Provider` = `graylog` (varsayılan) / `elk` / `none`. `ElasticConfiguration:Uri` boşsa Elasticsearch'e yazılmaz |
| Veritabanı | `ConnectionStrings:DefaultConnection` (PostgreSQL, Npgsql) |
| Gözlemlenebilirlik | Her API `AddServiceDefaults()` çağırır: OpenTelemetry (trace/metric/log), health check, service discovery, HTTP resilience. `OTEL_EXPORTER_OTLP_ENDPOINT` verilirse OTLP'ye aktarır. Development'ta `/health` ve `/alive` uçları açıktır |

> Kafka ve Redis uygulamaları şu an iskelet düzeyindedir (gerçek istemci bağlantısı yok); RabbitMQ gerçek istemcidir.
> Ayrıntı ve yol haritası: [docs/apphost-analiz.md](docs/apphost-analiz.md).

---

## Yol haritası

- **AppHost / tek noktadan yönetim:** hazır altyapıya (DB, kuyruk, cache, log) tek bir yapılandırmayla bağlanıp
  modelden üretilen servisleri birlikte ayağa kaldırma ve dağıtım çıktısı (docker-compose/Kubernetes) üretme.
- Olay altyapısı ve loglama için tek bir `Provider` ayarıyla seçim; consumer/worker örnekleri.
- Gizli bilgilerin (bağlantı dizeleri) user-secrets / ortam değişkenlerine taşınması.
- Kod üretici: Geometry alanlar, ters navigation'lar, benzersizlik index'leri, draw.io girdisi.
- Security (Keycloak/JWT), raporlama ve dosya yönetimi bileşenleri.
