using OzFw.ScribanCodeGen.Model;

namespace OzFw.ScribanCodeGen.Readers
{
    /// <summary>
    /// Diyagram/CSV'den gelen adları (urun, ust_kategori_id, URUN, UstKategoriId) C# adına çevirir.
    /// C# adından türetilen snake_case orijinal DB adını vermiyorsa orijinal ad tablo/kolon adı olarak saklanır.
    /// </summary>
    public readonly record struct ExternalName(string Name, string? DbName)
    {
        public static ExternalName From(string raw)
        {
            raw = raw.Trim().Trim('"', '`', '\'');
            var name = Naming.ToPascal(raw);
            if (name == raw)
                return new ExternalName(name, null);

            var dbName = raw.Any(char.IsUpper) ? raw.ToLowerInvariant() : raw;
            return new ExternalName(name, Naming.ToSnake(name) == dbName ? null : dbName);
        }
    }
}
