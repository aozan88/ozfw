# AppHost — tek noktadan sistem yönetimi: analiz ve tasarım

> Durum: analiz (Ekim 2026). Bu belge C seçeneğinin (AppHost'un yeniden tasarımı) kararlarını, gereksinimlerini,
> hedef mimarisini ve uygulama planını içerir. Uygulama bu belge onaylandıktan sonra başlar.

## Özet

OzFW'nin AppHost'u, erişimi ve yapılandırması hazır bir ortama **tek bir yapılandırmayla** bağlanıp temel ve genel
uyumlu bir mikroservis sistemini (çekirdek servisler + modelden üretilen modüller) ayağa kaldıran, gözlemleyen ve
dağıtım çıktısı üreten bir araç olacak. Tasarımın merkezinde **altyapı profili** var: hangi bileşenin (DB, kuyruk,
önbellek, log, telemetri) hangi adreste olduğunu ya da yerelde konteyner olarak açılacağını tek yerde tanımlar.
Profil iki farklı çalıştırıcıya beslenir ve hangisinin kullanılacağı yapılandırmayla seçilir:

- **OzFw Host** — kendi çalıştırıcımız: servisleri ayrı süreçler olarak başlatır, ayarlarını dağıtır, çekirdek
  servisler sağlıklı olmadan diğerlerini başlatmaz, uzaktaki (paylaşılan dev) çekirdeğe bağlanabilir.
- **Aspire AppHost** — aynı profilden Aspire kaynakları üretir; dashboard, konteyner yönetimi ve publish
  yeteneklerinden yararlanır.

Servis kodu hiçbir çalıştırıcıyı bilmez; yalnızca standart yapılandırma anahtarlarını okur (1. aşamada başlatıldı).
Gözlemlenebilirlik OTLP üzerinden çalıştırıcıdan bağımsızdır; Aspire Dashboard tek başına (standalone), Grafana,
Quickwit ya da ELK seçilebilir.

### Yol haritası

| # | Aşama | Durum |
|---|---|---|
| 1 | **Temizlik** — ServiceDefaults'un modüllerden ayrılması, altyapının yapılandırmadan seçilmesi (`EventBus:Provider`, `Cache:Provider`, `Logging:Provider`), tüm API'lerde `AddServiceDefaults()` | ✅ `6ed930a` |
| 2 | **Analiz** — bu belge | ✅ |
| 3 | **Bileşenlerin / ortamın kurulması** — en azından yerelde; demo için public erişimli ortam ([Bileşen matrisi](#5-bileşen-matrisi-ve-ortam-kurulumu)) | Sırada |
| 4 | **C implementasyonu** — altyapı profili, OzFw Host, Aspire adaptörü, generator entegrasyonu, gözlemlenebilirlik, dağıtım çıktısı ([Plan](#6-uygulama-planı)) | |
| 5 | **Genel standart iyileştirmeleri** ([Liste](#7-genel-standart-iyileştirmeleri)) | |

3 ve 4 kısmen paralel yürüyebilir: profil modeli ve OzFw Host'un çekirdek davranışı (C1–C2) bileşenler kurulmadan
`inmemory`/`memory` sağlayıcılarla geliştirilip test edilebilir.

---

## 1. Amaç ve bağlam

OzFW, hangi bileşenlerin kullanılacağının ve hatta sunucunun ne olacağının belli olmadığı bir dönemde, geliştirmenin
durmaması için ortaya çıktı. Elimizdeki tek ortam ekibin dev ortamıydı. Hedefler:

- Servislerin **ortam ve bileşen değişiminden bağımsız** çalışması (ister RabbitMQ ister Kafka, ister ELK ister
  Grafana, istenen DB). Sonradan ortam değiştirmek bu sayede zor olmadı.
- Code generator ile yeni servislerin hızla çoğaltılması (sonradan FE code-gen de eklendi, başka bir projede kullanıldı).
- AppHost ile **tek noktadan** sisteme bağlanıp temel bir mikroservis bütününü ayağa kaldırmak.

### Aspire'ın neden seçilmediği (2025 başı)

- Sunucunun tüm bileşenleri hazır değildi.
- Esnek değildi: kendi servis katmanını geliştirmeye ya da Aspire'da olmayan bir altyapı projesini sisteme eklemeye
  karşı, kendi framework bileşenlerini kullandırmaya zorluyordu.
- Kubernetes ile ortak kullanım, Microsoft dışı bir sunucuda çalıştırma, portları değiştirme, Quickwit gibi bir
  bileşeni kullanma zor ve tutarsızdı.
- Buna karşın **dashboard'u** sistemin geneline bakmak için çok iyiydi; kendi API'lerine ve framework'ün altyapı
  bileşenlerine göre özelleştirebilmek isteniyordu.

### Çekirdek servisler

Sistem bütün olarak çalıştırıldığında **Ortak** ve **Sistem** API'leri olmazsa olmazdır: işleyişin merkezindedirler;
onlar ayakta değilse diğer servislerin çalışıyor olmasının anlamı yoktur. Yeni bir API geliştiren geliştirici de
Ortak ve Sistem'e ihtiyaç duyar; bunları yerelde kaldırabilir ya da **paylaşılan dev ortamındaki** Ortak/Sistem'e
bağlanarak çalışır — böylece tüm geliştiricilerin geliştirme ortamı, ayarları ve verisi aynı olur.

### Gereksinimler

| # | Gereksinim |
|---|---|
| R1 | Altyapı **tek noktada** tanımlanır (profil); her bileşen isteğe bağlıdır ve sağlayıcısı seçilebilir |
| R2 | Çalıştırıcı **yapılandırmayla seçilir**: OzFw Host ya da Aspire. Aspire bir seçenektir, zorunluluk değildir |
| R3 | **Çekirdek servis kapısı**: Ortak ve Sistem sağlıklı olmadan diğer modüller başlatılmaz; çekirdek düşerse sistem "kullanılamaz" sayılır |
| R4 | **Uzak çekirdek**: geliştirici modunda Ortak/Sistem yerelde başlatılmaz, paylaşılan dev ortamındaki adreslerine bağlanılır |
| R5 | Servis kodu altyapıyı ve çalıştırıcıyı bilmez; yalnızca standart yapılandırma anahtarlarını okur |
| R6 | Sistemin geneli tek ekrandan görülür; görünüm framework bileşenlerine göre (event bus, cache, repository) özelleştirilebilir; arka uç (Aspire Dashboard, Grafana, Quickwit, ELK) seçilebilir |
| R7 | Dağıtım çıktısı (docker-compose, ileride Kubernetes) aynı profilden üretilir; portlar ve sunucu türü (Microsoft dışı dahil) kontrol edilebilir |
| R8 | Generator ile üretilen yeni modül, elle adım olmadan sisteme (çalıştırıcılar ve dağıtım çıktısı) dahil olur |
| R9 | Çok sayıda proje geliştirme deneyimini hantallaştırmamalı (IDE yükleme, build süresi) |
| R10 | USBS'teki örnek kullanımlar (RabbitMQ consumer, background worker, controller test tabanı, xunit/Moq) korunur ve framework'e taşınır |

---

## 2. Mevcut durum

### AppHost bugün

`USBS.AppHost` bir orkestratör değil; Ortak ve Sistem API'lerini proje referansıyla tek bir `WebApplication`
içinde birleştiren bir **süreç içi (in-process) birleştirici**. Aspire paketi yok.

| Bulgu | Etki | Yer |
|---|---|---|
| İki modül de `SwaggerDoc("v1")` ekliyor | Birleşik modda her istek 500 | `SistemApiProject.cs`, `OrtakApiProject.cs` |
| Session, Swagger, logging kayıtları her modülde tekrar | Çift kayıt, beklenmeyen davranış | `*ApiProject.cs` |
| Modüller aynı `DefaultConnection`'ı okuyor | Modül başına farklı DB verilemez (tek DB kararıyla şimdilik sorun değil) | appsettings |
| Yalnızca Ortak'ın middleware'i uygulanıyor | Sistem'e özgü middleware çalışmaz | `USBS.AppHost/Program.cs` |
| `ScalarMiddleware` her isteğe yanıt yazıyor, `ApiServiceProject` kullanılmıyor | Ölü/zararlı kod | `USBS.AppHost/` |
| `docker-compose.yaml` yolları ve servis adları güncel değil; `Deployment/` yer tutucu | Dağıtım çıktısı yok | `USBS.AppHost/`, `Deployment/` |
| ~~ServiceDefaults Ortak modülüne bağımlı~~ | ~~Her modül Ortak'ı taşıyordu~~ | ✅ 1. aşamada giderildi |
| ~~RabbitMQ ve Kafka birlikte kaydediliyor~~ | ~~Son kayıt kazanıyordu~~ | ✅ `EventBus:Provider` |
| ~~`AddServiceDefaults()` hiçbir API'de çağrılmıyor~~ | ~~OTel/health yok~~ | ✅ Ortak/Sistem/Ornek'te açık, `/health` `/alive` çalışıyor |

### Bileşenlerin olgunluğu

| Bileşen | Durum | Not |
|---|---|---|
| RabbitMQ (`OzFwRabbitMq`) | Gerçek istemci (RabbitMQ.Client 7) | Her `PublishAsync` yeni bağlantı+kanal açıyor ve kapatmıyor → bağlantı sızıntısı; bağlantı paylaşımı gerekli |
| Kafka (`OzFwKafka`) | **Sahte** | `PublishAsync`/`SubscribeAsync` yalnızca konsola yazıyor; Confluent.Kafka ile gerçek uygulama gerekli |
| Redis (`OzFwRedisCaching`) | **Sahte** | Konsola yazıyor; StackExchange.Redis ile gerçek uygulama gerekli |
| In-memory event bus / memory cache | ✅ Gerçek (1. aşama) | Yerel geliştirme ve testler için varsayılan |
| Serilog + Elasticsearch sink | Çalışıyor | `Serilog.Sinks.Elasticsearch` bakımı bırakılmış bir paket; `Elastic.Serilog.Sinks`'e geçiş değerlendirilmeli |
| Graylog / ELK `IKonLogger` | Çalışıyor (özel HTTP istemcisi) | OTLP'ye geçişle sadeleşebilir |
| OpenTelemetry (`ServiceDefaults`) | ✅ Açık | Exporter yalnızca `OTEL_EXPORTER_OTLP_ENDPOINT` verilince devreye girer |
| Veritabanı | Yalnızca PostgreSQL | `UseNpgsql` sabit (elle yazılan ve üretilen Data kayıtlarında); DDL üreticisi PostgreSQL lehçesi |

### USBS'teki çalışma modeli (referans)

- Her API kendi profiliyle tek başına çalışır (`RunningProfile`), ayrıca birleşik AppHost modu vardır.
- docker-compose: servis başına image, `ASPNETCORE_ENVIRONMENT=Docker`, `RunningProfile`, sabit portlar
  (24100/24200/24300 ve +1), `/health` üzerinden healthcheck, ortak bridge ağı.
- Test: modül başına `API.Test` ve `Application.Test` projeleri (xunit, Moq), `BaseControllerTestBase`.
- Örnekler: `SistemTanimTipiSilindiEventHandler` (RabbitMQ consumer), `MyBackgroundWorker` (hosted service).

---

## 3. Aspire: 2025 başındaki kısıtlar ve bugün

Aşağıdaki maddeler Aspire 9.x'in bilinen yeteneklerine dayanır; kullanılacak sürümde (9.5 ya da 13.x) **doğrulanmalıdır**.

| 2025 başındaki kısıt | Bugünkü durum | Tasarımdaki karşılığı |
|---|---|---|
| Kendi bileşenlerini kullandırıyor | Aspire client entegrasyonları zorunlu değil. `AddConnectionString`/`AddParameter` ile verilen değerler servise düz yapılandırma (`ConnectionStrings__x`, ortam değişkeni) olarak geçer; servis kendi `OzFwRabbitMq`/`OzFwRedis` bileşenini kullanmaya devam eder | Servisler yalnızca OzFW yapılandırma sözleşmesini okur; Aspire paketleri yalnızca Aspire adaptör projesinde bulunur |
| Aspire'da olmayan altyapı | `AddContainer(...)` ile herhangi bir image (Quickwit vb.); `AddConnectionString` ile var olan herhangi bir sunucu | Profilde `mode: container` ise image, `external` ise adres |
| Port kontrolü | Endpoint'ler sabit port alabilir, proxy kapatılabilir (`WithHttpEndpoint(port, isProxied: false)`) | Profilde modül portu; OzFw Host'ta doğrudan |
| Kubernetes / Microsoft dışı sunucu | Docker Compose ve Kubernetes publisher'ları 9.x'te sunuldu (kısmen preview) | Dağıtım çıktısını **kendi** generator'ımız üretir; Aspire publisher bir alternatiftir |
| Dashboard yalnızca AppHost ile | **Aspire Dashboard tek başına (standalone) konteyner olarak çalışır** ve herhangi bir OTLP kaynağını gösterir | Dashboard her iki çalıştırıcıda da kullanılabilir; Aspire'a bağlı değil |

Sonuç: Aspire bugün bir **seçenek** olarak eklenebilecek kadar esnek, ama tasarımı ona bağlamak için yeterli sebep yok.
Profil tek doğruluk kaynağı; Aspire ve OzFw Host bu profilin iki adaptörü.

---

## 4. Hedef mimari

```
                   ozfw.infra.json (altyapı profili)        ozfw.modules.json (generator'ın tuttuğu modül kaydı)
                                   \                         /
                                    \                       /
                     ┌───────────────┴───────────────────────┴───────────────┐
                     │  OzFw.Hosting (profil modeli, doğrulama, config üretimi) │
                     └───────┬──────────────────────┬──────────────────┬─────┘
                             │                      │                  │
                     OzFw Host (süreçler)    Aspire AppHost      Dağıtım üreticisi
                     çekirdek kapısı,        (Aspire.AppHost.Sdk) docker-compose (+ ileride k8s)
                     uzak çekirdek           kaynaklar + projeler
                             │                      │                  │
                             └──────────► servisler (yalnızca standart config anahtarlarını okur)
                                                    │
                                              OTLP ─┴─► OTel Collector ─► Aspire Dashboard | Grafana (LGTM) | Quickwit | ELK
```

### 4.1 Altyapı profili

Tek dosya; ortam başına (local, dev, demo) ayrı profil ya da `ozfw.infra.<ortam>.json` katmanı. Gizli değerler
profilde değil, ortam değişkeni / user-secrets referansı olarak tutulur.

```json
{
  "host": { "mode": "ozfw" },
  "core": { "mode": "local", "remote": { "ortak": "https://dev.example/ortak", "sistem": "https://dev.example/sistem" } },
  "database": { "provider": "postgres", "mode": "external", "connectionString": "${secret:DB_CONNECTION}" },
  "eventBus": { "provider": "rabbitmq", "mode": "container" },
  "cache": { "provider": "memory" },
  "logging": { "provider": "none" },
  "telemetry": { "otlpEndpoint": "http://localhost:4317", "dashboard": "aspire" }
}
```

- `host.mode`: `ozfw` | `aspire` (ileride `inprocess`).
- `core.mode`: `local` (Ortak/Sistem başlatılır) | `remote` (başlatılmaz, adresleri modüllere verilir).
- Her bileşen: `provider` + `mode` (`external` adres / `container` yerel image / `none`).
- Profil doğrulaması: bilinmeyen sağlayıcı, eksik adres, `container` modunda Docker yoksa anlaşılır hata (generator'daki
  doğrulama yaklaşımıyla aynı: tüm hatalar tek seferde).

### 4.2 Yapılandırma sözleşmesi (servislerin okuduğu anahtarlar)

| Anahtar | Değer | Durum |
|---|---|---|
| `ConnectionStrings:DefaultConnection`, `Database:Provider` | bağlantı, `postgres`/`mssql`/`mysql` | Provider anahtarı yeni |
| `EventBus:Provider`, `RabbitMqEventBus:*`, `KafkaEventBus:*` | | ✅ 1. aşama |
| `Cache:Provider`, `Redis:*` | | ✅ 1. aşama |
| `Logging:Provider`, `ElasticConfiguration:*`, `GraylogConfiguration:*` | `graylog`/`elk`/`none` (+ `otlp`, `quickwit`) | Kısmen |
| `OTEL_EXPORTER_OTLP_ENDPOINT`, `OTEL_SERVICE_NAME` | | ✅ ServiceDefaults okuyor |
| `Services:ortak:https:0`, `Services:sistem:https:0` | çekirdek adresleri (service discovery biçimi) | Yeni; modüller çekirdeğe `https+http://ortak` ile erişir |

Çalıştırıcılar bu anahtarları ortam değişkeni olarak (`EventBus__Provider=rabbitmq`) üretir; servis tek başına
çalıştırıldığında aynı değerler appsettings'ten gelir. Böylece "tek başına", "OzFw Host" ve "Aspire" modlarında
servis kodu ve davranışı aynıdır.

### 4.3 OzFw Host (kendi çalıştırıcımız)

- Profili okur, modül kaydından servisleri bulur, her biri için ortam değişkenlerini hesaplar.
- Başlatma sırası: altyapı (konteyner modundakiler) → **çekirdek (Ortak, Sistem) → `/health` Healthy olana kadar
  bekleme (zaman aşımı ile)** → diğer modüller.
- Çekirdek kapısı çalışma sırasında da izlenir; çekirdek düşerse durum "kullanılamaz" olarak raporlanır (otomatik
  yeniden başlatma ileride).
- `core.mode = remote`: Ortak/Sistem başlatılmaz; uzak adreslerin `/health`'i kontrol edilir, adresler modüllere verilir.
  Geliştirici yalnızca kendi modülünü çalıştırır.
- Konsolda servis başına renkli log önekleri, durum özeti; asıl gözlem OTLP/dashboard üzerinden.
- `USBS.AppHost` bu rolü üstlenir; bugünkü süreç içi birleştirme kodu ayrı ve isteğe bağlı bir "modüler monolit"
  moduna taşınır (aşağıda).

### 4.4 Aspire adaptörü

- Ayrı proje (`USBS.AppHost.Aspire`, `Aspire.AppHost.Sdk`): Aspire SDK proje semantiğini değiştirdiği için OzFw Host
  ile aynı projede çalışma zamanı anahtarıyla seçilemez. "Yapılandırmayla seçim", `host.mode`'a göre doğru giriş
  noktasını çalıştıran ortak bir komutla sağlanır (ör. `ozfw up`).
- Profilden kaynaklar: `external` → `AddConnectionString`/`AddParameter`; `container` → `AddPostgres`, `AddRabbitMQ`,
  `AddRedis`, `AddKafka`, diğerleri `AddContainer`.
- Modüller `AddProject<...>().WithReference(...)`, çekirdek kapısı `WaitFor(ortak).WaitFor(sistem)`.
- Aspire paketleri yalnızca bu projede; servislere sızmaz.

### 4.5 Gözlemlenebilirlik ve dashboard

- Tüm servisler OTLP gönderir (ServiceDefaults hazır). Tek giriş noktası **OpenTelemetry Collector**; profile göre
  arka uçlara dağıtır: Aspire Dashboard (standalone), Grafana LGTM (Loki/Tempo/Prometheus), Quickwit (log/trace arama),
  Elasticsearch. Böylece "ister ELK ister Grafana ister Quickwit" telemetri tarafında da yapılandırma meselesi olur.
- **Framework'e özgü görünüm**: OzFW bileşenleri kendi `ActivitySource` ve `Meter`'larını yayınlar —
  event bus (publish/consume span'leri, kuyruk adı, süre, hata), cache (hit/miss, süre), repository/ApplyQuery
  (sorgu süresi, dönen kayıt), BaseController (uç bazında istek/doğrulama hatası). Dashboard'larda modül ve bileşen
  bazında filtrelenebilir. Grafana için bu metriklere göre hazır dashboard JSON'ları framework ile birlikte gelir.
- Aspire modunda Aspire'ın kendi dashboard'u da aynı telemetriyi gösterir.

### 4.6 Dağıtım çıktısı

- Aynı profil ve modül kaydından **docker-compose** üretimi (generator'ın Scriban altyapısıyla): servis başına image,
  ortam değişkenleri, healthcheck, `depends_on: condition: service_healthy` ile çekirdek kapısı, compose
  `profiles` ile isteğe bağlı altyapı bileşenleri. Portlar ve hedef sunucu tamamen bizim kontrolümüzde.
- Kubernetes (Helm/kustomize) ikinci adım; Aspire modunda Aspire publisher alternatif olarak kullanılabilir.
- Gizli değerler compose'a yazılmaz; `.env` / secret referansı.

### 4.7 Generator entegrasyonu

- Generator yeni bir modül ürettiğinde `ozfw.modules.json`'a ekler: ad, proje yolu, port, `dependsOn: [ortak, sistem]`.
- OzFw Host ve dağıtım üreticisi bu kaydı okur; Aspire adaptörü için `Modules.g.cs` (AddProject satırları) ve
  csproj referansı üretilir. Port ataması otomatik (çakışma kontrolüyle), `--api-port` ile ezilebilir.

### 4.8 Modüler monolit modu (isteğe bağlı, sonra)

Bugünkü süreç içi birleştirme, küçük kurulumlar için tek paket dağıtım olarak değerlidir. Yapılacaksa bir modül
sözleşmesiyle (`IOzFwModule`: servis kaydı, endpoint eşleme, middleware), modül başına route öneki (`/ortak`, `/sistem`;
USBS'te `UsePathBase` ile denenmişti) ve modül başına Swagger dokümanıyla; ortak servisler bir kez kaydedilir.

### 4.9 Proje sayısı ve "hantallık"

Performans değil geliştirme deneyimi meselesi: USBS.sln şu an 43 proje; her yeni modül 3 (+ test projeleri) ekler.

- **Solution filter (`.slnf`)**: senaryo başına (çekirdek + kendi modülüm) — en ucuz kazanç.
- **Framework'ü NuGet paketine çevirmek** (iç feed): modül çözümleri yalnızca kendi projelerini içerir, framework
  derlenmez; sürümleme netleşir. OzFW'nin "framework" iddiasıyla da uyumlu.
- Çok küçük projeleri birleştirmek (ör. Notification altındaki üç ayar sınıfı, `OzFwBaseUtils`).
- `Directory.Build.props` ve `Directory.Packages.props` (merkezi paket sürümleri) — ayrıca bu konuşmadaki paket
  güncellemeleri gibi işleri tek dosyaya indirir.

---

## 5. Bileşen matrisi ve ortam kurulumu

Hedef: her bileşen OzFW'de **isteğe bağlı** kullanılabilir olsun. "FW işi" sütunu, bileşenin profile bağlanabilmesi
için framework'te yapılması gerekenleri gösterir.

| Bileşen | OzFW'de bugün | FW işi | Yerel kurulum (önerilen image) | Öncelik |
|---|---|---|---|---|
| PostgreSQL | ✅ Tek desteklenen DB | — | `postgres` (+ PostGIS gerekirse `postgis/postgis`) | Yüksek |
| MSSQL | ❌ | `Database:Provider`, `UseSqlServer`, generator DDL lehçesi, legacy timestamp ayarının sağlayıcıya göre yönetimi | `mcr.microsoft.com/mssql/server` | Orta |
| MySQL | ❌ | Aynı; EF sağlayıcısı (Pomelo) .NET 9 / EF 9 uyumluluğu doğrulanmalı | `mysql` | Orta |
| RabbitMQ | ✅ Gerçek istemci | Bağlantı/kanal paylaşımı, yeniden bağlanma, consumer örneği | `rabbitmq:*-management` | Yüksek |
| Kafka | ⚠ Sahte | Confluent.Kafka ile gerçek publish/consume, topic/grup yönetimi | `apache/kafka` (KRaft, ZooKeeper'sız) | Orta |
| Redis | ⚠ Sahte | StackExchange.Redis ile `IKonCache` | `redis` | Orta |
| ELK | ✅ Serilog sink (isteğe bağlı) | `Elastic.Serilog.Sinks`'e geçiş ya da OTLP üzerinden | `elasticsearch` + `kibana` (bellek ihtiyacı yüksek) | Düşük |
| Graylog | ✅ `IKonLogger` | OTLP/GELF değerlendirmesi | `graylog` (+ OpenSearch, MongoDB) | Düşük |
| Quickwit | ❌ | OTLP log/trace alımı üzerinden (Collector) | `quickwit/quickwit` | Orta |
| Grafana | ❌ | OTLP + hazır dashboard JSON'ları | `grafana/otel-lgtm` (Grafana + Loki + Tempo + Prometheus tek image) | Yüksek |
| OpenTelemetry | ✅ SDK servislerde | Collector yapılandırması, framework ActivitySource/Meter'ları | `otel/opentelemetry-collector-contrib` | Yüksek |
| Aspire Dashboard | ❌ | Yalnızca yapılandırma (OTLP hedefi) | `mcr.microsoft.com/dotnet/aspire-dashboard` | Yüksek |

### Yerel ortam

- Tek bir `infra/docker-compose.yml`, her bileşen bir compose **profile**'ı: `docker compose --profile postgres
  --profile rabbitmq --profile observability up`. Böylece "isteğe bağlı bileşen" ilkesi ortam tarafında da korunur.
- Image sürümleri kurulum sırasında sabitlenir (latest kullanılmaz).
- Bu makinede şu an Docker çalışmıyor; ortam kurulumu adımının ön koşulu.
- Önerilen başlangıç seti (hafif): PostgreSQL, RabbitMQ, Redis, OTel Collector + Aspire Dashboard; ardından
  Grafana LGTM, Kafka, Quickwit; en son ELK, MSSQL, MySQL.

### Demo (public erişimli) ortam

- Yönetim arayüzleri (RabbitMQ management, Kibana, Grafana, Aspire Dashboard, Quickwit UI) **doğrudan açılmaz**;
  TLS'li bir reverse proxy arkasında, kimlik doğrulamasıyla.
- Varsayılan şifreler değiştirilir; tüm gizli değerler ortam değişkeni / secret store'dan gelir, repoya girmez.
- Veri portları (5432, 5672, 9092, 6379, 9200, 4317) yalnızca uygulama ağına açılır, dışarıya kapalı.
- Kaynak planı: Elasticsearch ve Kafka bellek açısından en ağır bileşenler; demo için tek düğüm, sınırlı heap.
- Demo verisi için generator'ın `Ornek` modülü ve örnek SQL seed'leri kullanılabilir.

---

## 6. Uygulama planı

Süreler kaba tahmindir; ortam kurulumunun (3. aşama) hazır olmasına bağlıdır.

| Adım | İçerik | Bitti sayılması için |
|---|---|---|
| C1 | `OzFw.Hosting`: profil modeli, doğrulama, profil → ortam değişkeni/yapılandırma üretimi; modül kaydı | Birim testleri: her sağlayıcı/mod için üretilen anahtarlar; hatalı profil tüm hatalarla reddedilir |
| C2 | OzFw Host: süreç başlatma, çekirdek kapısı, uzak çekirdek, durum özeti; `USBS.AppHost` bu role geçer, süreç içi kod ayrılır | Ortak+Sistem+Ornek tek komutla kalkar; Ortak kapalıyken Ornek başlamaz; `core.mode=remote` ile yalnızca Ornek kalkar |
| C3 | Aspire adaptörü (`USBS.AppHost.Aspire`) | Aynı profille Aspire dashboard'da üç servis; `WaitFor` ile çekirdek kapısı |
| C4 | Generator: modül kaydı, Aspire `Modules.g.cs`, otomatik port | Yeni modül üretildiğinde iki çalıştırıcıda da ek adımsız görünür |
| C5 | Gözlemlenebilirlik: Collector yapılandırması, framework ActivitySource/Meter'ları, Grafana dashboard'ları | Event bus/cache/repository span ve metrikleri dashboard'da modül bazında |
| C6 | Dağıtım: profil + modül kaydından docker-compose | `docker compose up` ile sistem kalkar; çekirdek healthcheck'e göre sıralanır |

Toplam kabaca 1,5–2 hafta (ortam hazırsa). C1–C2 bileşenler kurulmadan `inmemory`/`memory` sağlayıcılarla geliştirilebilir.

---

## 7. Genel standart iyileştirmeleri

- Kafka ve Redis için gerçek istemciler; RabbitMQ bağlantı paylaşımı ve yeniden bağlanma.
- `Database:Provider` soyutlaması (PostgreSQL/MSSQL/MySQL) ve generator'da DDL lehçeleri.
- Gizli bilgiler: appsettings'teki şifrelerin user-secrets / ortam değişkenine taşınması ve repodaki geçmiş değerlerin
  değiştirilmesi.
- `Directory.Build.props`, `Directory.Packages.props`; framework'ün NuGet paketlerine çevrilmesi; `.slnf`'ler.
- USBS'teki test altyapısının (modül başına API/Application test projeleri, `BaseControllerTestBase`, Moq) framework'e
  taşınması; generator'ın modül başına test iskeleti üretmesi.
- RabbitMQ consumer ve background worker örneklerinin şablonlaştırılması (generator seçeneği).
- Ölü kodun temizlenmesi: `ScalarMiddleware`, `ApiServiceProject`, sln dışı `DTOS`/`USBS.Dtos`/`OzFwSeriLog` kopyaları.
- Modül başına Swagger dokümanı ve route öneki (modüler monolit modu için de gerekli).

---

## 8. Açık sorular

1. Çekirdek kapısında zaman aşımı ve davranış: çekirdek hiç kalkmazsa diğer modüller hiç başlatılmasın mı, yoksa
   "bozuk" durumda mı başlasın?
2. Uzak çekirdek ile yerel modül aynı DB'yi mi kullanacak (paylaşılan dev DB), yoksa modül yerel DB'ye mi bağlanacak?
3. `ozfw` komutu: generator CLI genişletilerek mi (`ozfw generate`, `ozfw up`, `ozfw compose`), yoksa ayrı bir araç mı?
   Öneri: tek araç — profil, generator ve çalıştırıcı aynı modeli paylaşıyor.
4. Demo ortamı hangi sunucuda/bulutta olacak (Kubernetes adımının önceliğini belirler)?
5. Aspire sürümü: 9.5 (mevcut .NET 9 SDK ile) mı, 13.x (muhtemelen .NET 10 SDK gerektirir; doğrulanmalı) mı?

## 9. Riskler

| Risk | Azaltma |
|---|---|
| İki çalıştırıcının davranışının ayrışması | Tek profil modeli ve tek yapılandırma üretimi (`OzFw.Hosting`); her iki adaptör için aynı senaryo testleri |
| Aspire sürüm değişikliklerinin (API/SDK) adaptörü kırması | Aspire yalnızca adaptör projesinde; sürüm sabitlenir |
| Yerel ortamın ağırlığı (ELK, Kafka) | Compose profile'larıyla seçmeli kurulum; hafif başlangıç seti |
| Kafka/Redis sahte uygulamalarının gerçek sanılması | README ve bu belgede açıkça işaretli; gerçek istemciler iyileştirme listesinde |
| Gizli bilgilerin repoda olması | Ortam kurulumu sırasında taşınması ve değiştirilmesi |
