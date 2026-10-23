using System.Text.Json.Serialization;

namespace OzFw.ScribanCodeGen.Model
{
    /// <summary>
    /// Kod üreticinin ara modeli. JSON/CSV/DBML/Mermaid okuyucuları bu modele dönüştürür;
    /// ModelNormalizer varsayılanları doldurur ve modeli doğrular. Şema: docs/codegen-model.schema.json
    /// </summary>
    public sealed class CodeGenModel
    {
        /// <summary>Modül adı (PascalCase), ör. "Ornek". Klasör ve namespace adlarında kullanılır.</summary>
        public string Module { get; set; } = "";

        /// <summary>PostgreSQL şeması. Verilmezse modül adının snake_case hali.</summary>
        public string? DbSchema { get; set; }

        /// <summary>Üretilecek C# kodunun kök namespace'i. Verilmezse "USBS".</summary>
        public string? NamespaceRoot { get; set; }

        public List<EntityModel> Entities { get; set; } = new();
    }

    public sealed class EntityModel
    {
        /// <summary>Entity sınıf adı (PascalCase).</summary>
        public string Name { get; set; } = "";

        /// <summary>Tablo adı. Verilmezse Name'in snake_case hali.</summary>
        public string? Table { get; set; }

        /// <summary>DbContext'teki DbSet adı. Verilmezse Name + "s".</summary>
        public string? DbSetName { get; set; }

        public string? Description { get; set; }

        /// <summary>
        /// Entity'ye özgü alanlar. BaseEntity alanları (Id, OlusturanKisi, OlusturulmaTarihi, DegistirenKisi,
        /// DegistirilmeTarihi, Versiyon, Durum, LanguageTag) her entity'ye otomatik eklenir, burada tanımlanmaz.
        /// </summary>
        public List<FieldModel> Fields { get; set; } = new();

        public List<RelationModel> Relations { get; set; } = new();
    }

    public sealed class FieldModel
    {
        /// <summary>Property adı (PascalCase).</summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Tip: string, int, long, short, decimal, double, bool, Guid, DateTime, DateOnly, Geometry, byte[], enum:EnumAdi.
        /// SQL/diyagram takma adları da kabul edilir (varchar(100), uuid, numeric(18,2), timestamp, bytea ...).
        /// </summary>
        public string Type { get; set; } = "";

        public bool Nullable { get; set; }
        public int? MaxLength { get; set; }
        public int? Precision { get; set; }
        public int? Scale { get; set; }

        /// <summary>Kolon adı. Verilmezse Name'in snake_case hali.</summary>
        public string? Column { get; set; }

        /// <summary>DTO tarafındaki ad farklıysa (entity'de [DtoField] üretilir).</summary>
        public string? DtoName { get; set; }

        public string? Description { get; set; }
    }

    public sealed class RelationModel
    {
        /// <summary>Navigation property adı. Verilmezse FkField'dan "Id" soneki atılarak türetilir.</summary>
        public string? Name { get; set; }

        /// <summary>Hedef entity adı.</summary>
        public string Target { get; set; } = "";

        /// <summary>Bu entity'deki FK alanı (Guid). Modelde yoksa otomatik eklenir. Verilmezse Target + "Id".</summary>
        public string? FkField { get; set; }

        /// <summary>
        /// Zorunlu ilişki mi? FK alanı modelde tanımlıysa onun Nullable değerinden türetilir;
        /// açıkça verilip FK alanıyla çelişirse doğrulama hatası olur.
        /// </summary>
        public bool? Required { get; set; }

        /// <summary>restrict (varsayılan), cascade, setnull, noaction</summary>
        public string? OnDelete { get; set; }
    }

    public static class OnDeleteActions
    {
        public const string Restrict = "restrict";
        public const string Cascade = "cascade";
        public const string SetNull = "setnull";
        public const string NoAction = "noaction";

        public static string? Normalize(string? value) =>
            value?.Trim().ToLowerInvariant().Replace(" ", "").Replace("_", "") switch
            {
                null or "" => Restrict,
                "restrict" => Restrict,
                "cascade" => Cascade,
                "setnull" => SetNull,
                "noaction" => NoAction,
                _ => null
            };

        public static string ToSql(string action) => action switch
        {
            Cascade => "CASCADE",
            SetNull => "SET NULL",
            NoAction => "NO ACTION",
            _ => "RESTRICT"
        };
    }

    /// <summary>Her entity'ye BaseEntity'den gelen alanlar (USBS.Common.Domain.BaseEntity ile aynı).</summary>
    public static class BaseEntityFields
    {
        public sealed record BaseColumn(string Name, string Column, string SqlType, bool Nullable, string? Default);

        public static readonly IReadOnlyList<BaseColumn> All = new[]
        {
            new BaseColumn("Id", "id", "uuid", false, null),
            new BaseColumn("OlusturanKisi", "olusturan_kisi", "text", true, null),
            new BaseColumn("OlusturulmaTarihi", "olusturulma_tarihi", "timestamp", false, null),
            new BaseColumn("DegistirenKisi", "degistiren_kisi", "text", true, null),
            new BaseColumn("DegistirilmeTarihi", "degistirilme_tarihi", "timestamp", false, null),
            new BaseColumn("Versiyon", "versiyon", "integer", false, "0"),
            new BaseColumn("Durum", "durum", "integer", false, "1"),
            new BaseColumn("LanguageTag", "language_tag", "text", true, null),
        };

        /// <summary>Ad PascalCase ya da snake_case verilebilir.</summary>
        public static bool IsBase(string name) =>
            All.Any(b => string.Equals(b.Name, name, StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(b.Column, name, StringComparison.OrdinalIgnoreCase));
    }
}
