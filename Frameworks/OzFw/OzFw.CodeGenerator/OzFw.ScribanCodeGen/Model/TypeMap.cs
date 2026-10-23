using System.Text.RegularExpressions;

namespace OzFw.ScribanCodeGen.Model
{
    /// <summary>Girdideki tip yazımı (C# ya da SQL/diyagram takma adı) → kanonik tip.</summary>
    public sealed record ParsedType(string Canonical, int? Length, int? Precision, int? Scale, bool NullableSuffix);

    public static class TypeMap
    {
        public const string String = "string";
        public const string Int = "int";
        public const string Long = "long";
        public const string Short = "short";
        public const string Decimal = "decimal";
        public const string Double = "double";
        public const string Bool = "bool";
        public const string Guid = "Guid";
        public const string DateTime = "DateTime";
        public const string DateOnly = "DateOnly";
        public const string Geometry = "Geometry";
        public const string Bytes = "byte[]";
        public const string EnumPrefix = "enum:";

        public static readonly IReadOnlyList<string> Canonicals = new[]
        {
            String, Int, Long, Short, Decimal, Double, Bool, Guid, DateTime, DateOnly, Geometry, Bytes, EnumPrefix + "<Ad>"
        };

        private static readonly Dictionary<string, string> Aliases = new(StringComparer.OrdinalIgnoreCase)
        {
            ["string"] = String, ["text"] = String, ["varchar"] = String, ["charactervarying"] = String,
            ["char"] = String, ["character"] = String, ["nvarchar"] = String, ["nchar"] = String, ["citext"] = String,

            ["int"] = Int, ["integer"] = Int, ["int4"] = Int, ["int32"] = Int, ["serial"] = Int,
            ["long"] = Long, ["bigint"] = Long, ["int8"] = Long, ["int64"] = Long, ["bigserial"] = Long,
            ["short"] = Short, ["smallint"] = Short, ["int2"] = Short, ["int16"] = Short,

            ["decimal"] = Decimal, ["numeric"] = Decimal, ["money"] = Decimal,
            ["double"] = Double, ["float"] = Double, ["float8"] = Double, ["doubleprecision"] = Double, ["real"] = Double, ["float4"] = Double,

            ["bool"] = Bool, ["boolean"] = Bool, ["bit"] = Bool,
            ["guid"] = Guid, ["uuid"] = Guid, ["uniqueidentifier"] = Guid,

            ["datetime"] = DateTime, ["timestamp"] = DateTime, ["timestamptz"] = DateTime,
            ["timestampwithouttimezone"] = DateTime, ["timestampwithtimezone"] = DateTime, ["datetime2"] = DateTime,
            ["dateonly"] = DateOnly, ["date"] = DateOnly,

            ["geometry"] = Geometry, ["point"] = Geometry, ["polygon"] = Geometry, ["multipolygon"] = Geometry, ["linestring"] = Geometry,
            ["byte[]"] = Bytes, ["bytes"] = Bytes, ["bytea"] = Bytes, ["binary"] = Bytes, ["varbinary"] = Bytes, ["blob"] = Bytes,
        };

        private static readonly Regex TypeRegex = new(@"^\s*(?<name>[\p{L}_][\p{L}\p{Nd}_ \[\]]*?)\s*(\(\s*(?<a>\d+)\s*(,\s*(?<b>\d+)\s*)?\))?\s*(?<q>\?)?\s*$", RegexOptions.Compiled);

        /// <summary>Tanınmayan tipte null döner.</summary>
        public static ParsedType? Parse(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            var text = raw.Trim();
            if (text.StartsWith(EnumPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var enumName = text[EnumPrefix.Length..].Trim().TrimEnd('?');
                return Naming.IsIdentifier(enumName) ? new ParsedType(EnumPrefix + enumName, null, null, null, text.EndsWith('?')) : null;
            }

            var m = TypeRegex.Match(text);
            if (!m.Success)
                return null;

            var key = m.Groups["name"].Value.Replace(" ", "");
            if (!Aliases.TryGetValue(key, out var canonical))
                return null;

            int? a = m.Groups["a"].Success ? int.Parse(m.Groups["a"].Value) : null;
            int? b = m.Groups["b"].Success ? int.Parse(m.Groups["b"].Value) : null;
            var nullableSuffix = m.Groups["q"].Success;

            return canonical switch
            {
                String => new ParsedType(canonical, a, null, null, nullableSuffix),
                Decimal => new ParsedType(canonical, null, a, b ?? (a.HasValue ? 0 : null), nullableSuffix),
                _ => new ParsedType(canonical, null, null, null, nullableSuffix)
            };
        }

        public static bool IsEnum(string canonical) => canonical.StartsWith(EnumPrefix, StringComparison.Ordinal);

        public static string ToPostgres(FieldModel field) => field.Type switch
        {
            String => field.MaxLength.HasValue ? $"varchar({field.MaxLength})" : "text",
            Int => "integer",
            Long => "bigint",
            Short => "smallint",
            Decimal => field.Precision.HasValue ? $"numeric({field.Precision},{field.Scale ?? 0})" : "numeric",
            Double => "double precision",
            Bool => "boolean",
            Guid => "uuid",
            DateTime => "timestamp",
            DateOnly => "date",
            Geometry => "geometry",
            Bytes => "bytea",
            var t when IsEnum(t) => "integer",
            var t => throw new InvalidOperationException($"Desteklenmeyen tip: {t}")
        };
    }
}
