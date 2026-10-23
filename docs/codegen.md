# OzFw kod üretici: model ve SQL DDL

`Frameworks/OzFw/OzFw.CodeGenerator/OzFw.ScribanCodeGen` bir veri modelini okur, doğrular ve PostgreSQL DDL üretir.
Migration kullanılmaz: üretilen script review edilip DB'ye elle uygulanır. C# katman şablonları (entity, DTO,
repository, servis, controller) bir sonraki aşamada aynı modelden üretilecek.

## Çalıştırma

```bash
dotnet run --project Frameworks/OzFw/OzFw.CodeGenerator/OzFw.ScribanCodeGen -- --model samples/codegen/ornek.dbml --dry-run
```

| Seçenek | Açıklama |
|---|---|
| `--model <dosya>` | `.json` (ara model), `.csv`, `.dbml` (dbdiagram.io), `.mmd` / `.mermaid` (erDiagram) |
| `--root <klasör>` | Çıktı kökü (varsayılan: geçerli klasör). SQL `<root>/sql/<modul>/` altına yazılır |
| `--only sql` | Üretilecek hedefler (şimdilik yalnızca `sql`) |
| `--dry-run` | Hiçbir dosya yazmaz, ne yazılacağını listeler (`yeni` / `güncellenecek` / `değişmedi`) |
| `--module`, `--schema`, `--namespace` | Model dosyasındaki değerleri geçersiz kılar |
| `--export-model <dosya.json>` | Normalize edilmiş modeli JSON olarak yazar (diyagram/CSV → ara model) |

Çıkış kodları: `0` başarılı, `1` model okunamadı ya da doğrulanamadı (tüm hatalar listelenir), `2` hatalı kullanım.
Argümansız çalıştırılırsa eski (sabit listeli) şablon akışı çalışır.

## Girdi formatları

Aynı modelin dört formatı `samples/codegen/` altında; dördü de birebir aynı ara modeli ve DDL'i üretir (testle doğrulanır).

- **JSON**: ara modelin kendisi, şema `docs/codegen-model.schema.json`.
- **CSV**: her satır bir alan. Başlık `entity,field,type,nullable,length,precision,scale,fk_target,description`
  (isteğe bağlı `column`, `dto_name`, `on_delete`). Ayraç `,` ya da `;` (Excel TR). Modül `# module: Ornek` satırıyla verilir.
- **DBML**: `Table şema.tablo { ... }`, kolon ayarları (`not null`, `note`, `ref: > t.id`), `Ref:` satırları
  (`[delete: cascade]`), `Enum` blokları. DBML'de kolonlar varsayılan olarak nullable'dır. `indexes` blokları şimdilik atlanır.
- **Mermaid erDiagram**: `tip ad [PK|FK] "yorum"`. Alanlar varsayılan olarak zorunludur. Yorumda `;` ile ayrılmış
  yönergeler: `nullable`, `maxLength: 100`, `precision: 18,2`, `ref: Hedef`, `onDelete: cascade`, kalan metin açıklamadır.
  FK'nin zorunluluğu kardinaliteden gelir (`||` zorunlu, `|o` isteğe bağlı). İlişki etiketi FK kolonunun adı olabilir.

Diyagram ve CSV'de `snake_case` / `UPPER` adlar PascalCase'e çevrilir (`ust_kategori_id` → `UstKategoriId`).
BaseEntity kolonları (`id`, `olusturulma_tarihi` ...) yazılabilir, okuyucu bunları atlar. İlişkiler yalnızca
hedefin `id` kolonuna olabilir; çoka-çok ilişki için ara entity tanımlanmalıdır.

## Üretilen DDL

- `sql/<modul>/_all.sql`: şema + tüm tablolar, FK bağımlılık sırasıyla (önce referans verilenler).
- `sql/<modul>/<tablo>.sql`: tek tablo; referans verdiği tablolar önceden var olmalı.
- Her tabloda BaseEntity kolonları, `pk_<tablo>`, FK constraint'leri (`fk_<tablo>_<kolon>`) ve FK index'leri
  (`ix_<tablo>_<kolon>`); açıklamalar `COMMENT ON` olarak yazılır.
- Script idempotenttir (`IF NOT EXISTS`); döngüsel FK'ler tablolardan sonra `DO $$ ... duplicate_object` bloklarıyla eklenir.
- `DateTime` → `timestamp` (DbContext'lerde `Npgsql.EnableLegacyTimestampBehavior` açık), enum → `integer`.
