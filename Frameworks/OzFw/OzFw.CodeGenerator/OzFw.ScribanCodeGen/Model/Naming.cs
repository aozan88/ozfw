using System.Text;
using System.Text.RegularExpressions;

namespace OzFw.ScribanCodeGen.Model
{
    public static class Naming
    {
        private static readonly Regex IdentifierRegex = new(@"^[\p{L}_][\p{L}\p{Nd}_]*$", RegexOptions.Compiled);

        public static bool IsIdentifier(string? value) => !string.IsNullOrEmpty(value) && IdentifierRegex.IsMatch(value);

        /// <summary>
        /// PascalCase → snake_case. Türkçe karakterler korunur (Ş → ş); I/İ her zaman 'i' olur
        /// ki "TanimTipiId" → "tanim_tipi_id" (tr-TR'deki 'ı' dönüşümü yapılmaz).
        /// Kısaltmalar tek kelime sayılır: "HTMLSayfa" → "html_sayfa".
        /// </summary>
        public static string ToSnake(string value)
        {
            var sb = new StringBuilder(value.Length + 8);
            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (c is '-' or ' ' or '.')
                {
                    AppendUnderscore(sb);
                    continue;
                }

                if (char.IsUpper(c))
                {
                    var prev = i > 0 ? value[i - 1] : '\0';
                    var next = i + 1 < value.Length ? value[i + 1] : '\0';
                    if (i > 0 && (char.IsLower(prev) || char.IsDigit(prev) || (char.IsUpper(prev) && char.IsLower(next))))
                        AppendUnderscore(sb);
                    sb.Append(Lower(c));
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString().Trim('_');
        }

        /// <summary>
        /// snake_case / UPPER_SNAKE / kebab-case → PascalCase. Zaten PascalCase olan değer korunur.
        /// </summary>
        public static string ToPascal(string value)
        {
            var parts = value.Split(new[] { '_', '-', ' ', '.' }, StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder(value.Length);
            foreach (var part in parts)
            {
                var allUpper = part.All(ch => !char.IsLetter(ch) || char.IsUpper(ch));
                var allLower = part.All(ch => !char.IsLetter(ch) || char.IsLower(ch));
                var body = allUpper || allLower ? new string(part.Skip(1).Select(Lower).ToArray()) : part[1..];
                sb.Append(char.ToUpperInvariant(part[0])).Append(body);
            }
            return sb.ToString();
        }

        private static char Lower(char c) => c == 'İ' ? 'i' : char.ToLowerInvariant(c);

        private static void AppendUnderscore(StringBuilder sb)
        {
            if (sb.Length > 0 && sb[^1] != '_')
                sb.Append('_');
        }
    }
}
