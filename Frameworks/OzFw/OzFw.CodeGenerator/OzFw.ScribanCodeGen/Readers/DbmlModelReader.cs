using System.Text;
using System.Text.RegularExpressions;
using OzFw.ScribanCodeGen.Model;

namespace OzFw.ScribanCodeGen.Readers
{
    /// <summary>
    /// dbdiagram.io DBML okuyucu. Desteklenenler: Project (modül adı), Table [şema.]tablo [as alias] { ... },
    /// kolon ayarları (pk, not null, null, note, ref, default, unique), tablo Note'u, Enum blokları,
    /// tek satır ve blok Ref tanımları (&gt; &lt; -; delete: cascade|restrict|set null|no action), indexes (atlanır).
    /// DBML'de kolonlar varsayılan olarak nullable'dır; "not null" ya da "pk" ile zorunlu olur.
    /// Yönergeler: "// module: Ornek", "// dbSchema: ornek", "// namespaceRoot: USBS".
    /// BaseEntity kolonları (id, olusturan_kisi ...) atlanır; ilişkiler yalnızca hedefin id kolonuna olabilir.
    /// </summary>
    public sealed class DbmlModelReader : IModelReader
    {
        private static readonly Regex DirectiveRegex = new(@"^\s*//\s*(module|dbschema|schema|namespaceroot|namespace)\s*:(.*)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ProjectRegex = new(@"^Project\s+""?([\p{L}\p{Nd}_]+)""?\s*\{$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex TableRegex = new(@"^Table\s+(?:""?([\p{L}\p{Nd}_]+)""?\.)?""?([\p{L}\p{Nd}_]+)""?(?:\s+as\s+""?([\p{L}\p{Nd}_]+)""?)?\s*(?:\[[^\]]*\])?\s*\{$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex EnumRegex = new(@"^Enum\s+(?:""?([\p{L}\p{Nd}_]+)""?\.)?""?([\p{L}\p{Nd}_]+)""?\s*\{$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex RefLineRegex = new(@"^Ref\s*[\p{L}\p{Nd}_""]*\s*:\s*(.+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex RefBlockRegex = new(@"^Ref\s*[\p{L}\p{Nd}_""]*\s*\{$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex RefBodyRegex = new(@"^(\S+)\s*(<>|<|>|-)\s*(\S+?)\s*(?:\[(.*)\])?$", RegexOptions.Compiled);
        private static readonly Regex ColumnRegex = new(@"^""?([\p{L}\p{Nd}_]+)""?\s+(.+?)\s*(?:\[(.*)\])?$", RegexOptions.Compiled);
        private static readonly Regex NoteRegex = new(@"^note\s*:\s*(.+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public IReadOnlyList<string> Extensions { get; } = new[] { ".dbml" };

        private sealed record PendingRef(int Line, string Left, string Op, string Right, string? Settings);

        private sealed class TableInfo
        {
            public required EntityModel Entity { get; init; }
            public required string DbName { get; init; }
            public Dictionary<string, FieldModel> Columns { get; } = new(StringComparer.OrdinalIgnoreCase);
        }

        public CodeGenModel Read(string content)
        {
            var model = new CodeGenModel();
            var tables = new Dictionary<string, TableInfo>(StringComparer.OrdinalIgnoreCase);
            var enums = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var refs = new List<PendingRef>();
            string? projectName = null;

            var lines = Preprocess(content, model);
            TableInfo? currentTable = null;
            var mode = Mode.None;
            var depth = 0;

            foreach (var (lineNo, line) in lines)
            {
                if (mode == Mode.Skip)
                {
                    depth += line.Count(c => c == '{') - line.Count(c => c == '}');
                    if (depth <= 0) mode = currentTable != null ? Mode.Table : Mode.None;
                    continue;
                }

                if (mode == Mode.Table)
                {
                    if (line == "}") { currentTable = null; mode = Mode.None; continue; }
                    if (Regex.IsMatch(line, @"^indexes\s*\{", RegexOptions.IgnoreCase)) { mode = Mode.Skip; depth = 1; continue; }

                    var note = NoteRegex.Match(line);
                    if (note.Success) { currentTable!.Entity.Description = Unquote(note.Groups[1].Value); continue; }

                    ReadColumn(currentTable!, line, lineNo, enums, refs);
                    continue;
                }

                if (mode is Mode.Enum or Mode.Project)
                {
                    if (line == "}") mode = Mode.None;
                    continue;
                }

                if (mode == Mode.RefBlock)
                {
                    if (line == "}") { mode = Mode.None; continue; }
                    refs.Add(ParseRef(line, lineNo));
                    continue;
                }

                Match m;
                if ((m = ProjectRegex.Match(line)).Success) { projectName = m.Groups[1].Value; mode = Mode.Project; }
                else if ((m = TableRegex.Match(line)).Success) { currentTable = AddTable(model, tables, m, lineNo); mode = Mode.Table; }
                else if ((m = EnumRegex.Match(line)).Success) { enums.Add(m.Groups[2].Value); mode = Mode.Enum; }
                else if (RefBlockRegex.IsMatch(line)) mode = Mode.RefBlock;
                else if ((m = RefLineRegex.Match(line)).Success) refs.Add(ParseRef(m.Groups[1].Value.Trim(), lineNo));
                else if (Regex.IsMatch(line, @"^TableGroup\b.*\{$", RegexOptions.IgnoreCase)) { mode = Mode.Skip; depth = 1; }
                else throw ModelReadException.At(lineNo, $"Anlaşılamayan DBML satırı: '{line}'.");
            }

            if (mode != Mode.None)
                throw new ModelReadException("DBML: kapanmamış blok ('}' eksik).");

            if (string.IsNullOrWhiteSpace(model.Module) && projectName != null)
                model.Module = Naming.ToPascal(projectName);

            foreach (var r in refs)
                ApplyRef(r, tables);

            return model;
        }

        private enum Mode { None, Table, Enum, Project, RefBlock, Skip }

        private static TableInfo AddTable(CodeGenModel model, Dictionary<string, TableInfo> tables, Match m, int lineNo)
        {
            var schema = m.Groups[1].Success ? m.Groups[1].Value : null;
            var rawTable = m.Groups[2].Value;
            if (schema != null)
            {
                if (model.DbSchema != null && !string.Equals(model.DbSchema, schema, StringComparison.Ordinal))
                    throw ModelReadException.At(lineNo, $"Birden fazla şema kullanılmış ('{model.DbSchema}', '{schema}'); bir modül tek şemada olmalı.");
                model.DbSchema = schema;
            }

            var name = ExternalName.From(rawTable);
            var entity = new EntityModel { Name = name.Name, Table = name.DbName };
            var info = new TableInfo { Entity = entity, DbName = rawTable };

            if (!tables.TryAdd(rawTable, info))
                throw ModelReadException.At(lineNo, $"Tablo birden fazla tanımlanmış: '{rawTable}'.");
            if (schema != null) tables.TryAdd($"{schema}.{rawTable}", info);
            if (m.Groups[3].Success) tables.TryAdd(m.Groups[3].Value, info);

            model.Entities.Add(entity);
            return info;
        }

        private static void ReadColumn(TableInfo table, string line, int lineNo, HashSet<string> enums, List<PendingRef> refs)
        {
            var m = ColumnRegex.Match(line);
            if (!m.Success)
                throw ModelReadException.At(lineNo, $"Kolon tanımı anlaşılamadı: '{line}'.");

            var rawColumn = m.Groups[1].Value;
            var rawType = Unquote(m.Groups[2].Value);
            var settings = SplitSettings(m.Groups[3].Success ? m.Groups[3].Value : "");

            var isPk = settings.Any(s => s.Equals("pk", StringComparison.OrdinalIgnoreCase) || s.Equals("primary key", StringComparison.OrdinalIgnoreCase));
            if (BaseEntityFields.IsBase(rawColumn))
                return;
            if (isPk)
                throw ModelReadException.At(lineNo, $"'{table.DbName}.{rawColumn}': yalnızca BaseEntity'nin 'id' kolonu PK olabilir.");

            var name = ExternalName.From(rawColumn);
            var field = new FieldModel
            {
                Name = name.Name,
                Column = name.DbName,
                Type = enums.Contains(rawType) ? TypeMap.EnumPrefix + Naming.ToPascal(rawType) : rawType,
                Nullable = !settings.Any(s => s.Equals("not null", StringComparison.OrdinalIgnoreCase))
            };

            foreach (var s in settings)
            {
                var kv = SplitKeyValue(s);
                if (kv == null) continue;
                switch (kv.Value.Key)
                {
                    case "note":
                        field.Description = Unquote(kv.Value.Value);
                        break;
                    case "ref":
                        // "ref: > tablo.id" — bu kolon FK
                        var rm = Regex.Match(kv.Value.Value, @"^(<>|<|>|-)\s*(\S+)$");
                        if (!rm.Success)
                            throw ModelReadException.At(lineNo, $"ref ayarı anlaşılamadı: '{s}'.");
                        refs.Add(new PendingRef(lineNo, $"{table.DbName}.{rawColumn}", rm.Groups[1].Value, rm.Groups[2].Value, null));
                        break;
                }
            }

            table.Entity.Fields.Add(field);
            table.Columns[rawColumn] = field;
        }

        private static PendingRef ParseRef(string body, int lineNo)
        {
            var m = RefBodyRegex.Match(body);
            if (!m.Success)
                throw ModelReadException.At(lineNo, $"Ref tanımı anlaşılamadı: '{body}'.");
            return new PendingRef(lineNo, m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value, m.Groups[4].Success ? m.Groups[4].Value : null);
        }

        private static void ApplyRef(PendingRef r, Dictionary<string, TableInfo> tables)
        {
            if (r.Op == "<>")
                throw ModelReadException.At(r.Line, "Çoka-çok (<>) ilişki desteklenmiyor; ara tablo tanımlayın.");

            // ">" ve "-": sol taraf FK; "<": sağ taraf FK
            var (fkEnd, pkEnd) = r.Op == "<" ? (r.Right, r.Left) : (r.Left, r.Right);
            var (fkTable, fkColumn) = Endpoint(fkEnd, tables, r.Line);
            var (pkTable, pkColumn) = Endpoint(pkEnd, tables, r.Line);

            if (!pkColumn.Equals("id", StringComparison.OrdinalIgnoreCase))
                throw ModelReadException.At(r.Line, $"İlişki yalnızca hedef tablonun 'id' kolonuna olabilir ({pkEnd}).");
            if (!fkTable.Columns.TryGetValue(fkColumn, out var fkField))
                throw ModelReadException.At(r.Line, $"FK kolonu bulunamadı: '{fkEnd}'.");

            string? onDelete = null;
            foreach (var s in SplitSettings(r.Settings ?? ""))
            {
                var kv = SplitKeyValue(s);
                if (kv?.Key == "delete") onDelete = kv.Value.Value;
            }

            fkTable.Entity.Relations.Add(new RelationModel
            {
                Target = pkTable.Entity.Name,
                FkField = fkField.Name,
                OnDelete = onDelete
            });
        }

        private static (TableInfo Table, string Column) Endpoint(string endpoint, Dictionary<string, TableInfo> tables, int line)
        {
            var parts = endpoint.Replace("\"", "").Split('.');
            if (parts.Length < 2)
                throw ModelReadException.At(line, $"Ref ucu 'tablo.kolon' biçiminde olmalı: '{endpoint}'.");
            var column = parts[^1];
            var tableKey = string.Join('.', parts[..^1]);
            if (!tables.TryGetValue(tableKey, out var table) && !tables.TryGetValue(parts[^2], out table))
                throw ModelReadException.At(line, $"Ref'teki tablo bulunamadı: '{tableKey}'.");
            return (table, column);
        }

        /// <summary>Yorumları atar, yönergeleri uygular, '{' ile biten satırları ayırır; (satır no, metin) döner.</summary>
        private static List<(int, string)> Preprocess(string content, CodeGenModel model)
        {
            var result = new List<(int, string)>();
            var text = content.Replace("\r\n", "\n");

            // Blok yorumlar ve üç tırnaklı notlar
            text = Regex.Replace(text, @"/\*.*?\*/", m => new string('\n', m.Value.Count(c => c == '\n')), RegexOptions.Singleline);
            text = Regex.Replace(text, @"'''(.*?)'''", m => "'" + m.Groups[1].Value.Replace("\n", " ").Replace("'", "\\'") + "'" + new string('\n', m.Value.Count(c => c == '\n')), RegexOptions.Singleline);

            var lines = text.Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                var raw = lines[i].TrimStart('﻿');
                var d = DirectiveRegex.Match(raw);
                if (d.Success)
                {
                    CsvModelReader.ApplyDirective(model, $"{d.Groups[1].Value}:{d.Groups[2].Value}");
                    continue;
                }

                var line = StripLineComment(raw).Trim();
                if (line.Length == 0)
                    continue;

                // "Table x { id uuid }" gibi tek satırda açılıp kapanan blokları satırlara böl
                var open = line.IndexOf('{');
                if (open >= 0 && open < line.Length - 1 && !line.StartsWith("Ref", StringComparison.OrdinalIgnoreCase))
                {
                    result.Add((i + 1, line[..(open + 1)].Trim()));
                    var rest = line[(open + 1)..].Trim();
                    var close = rest.EndsWith('}');
                    if (close) rest = rest[..^1].Trim();
                    if (rest.Length > 0) result.Add((i + 1, rest));
                    if (close) result.Add((i + 1, "}"));
                    continue;
                }
                result.Add((i + 1, line));
            }
            return result;
        }

        private static string StripLineComment(string line)
        {
            var inQuote = '\0';
            for (var i = 0; i < line.Length - 1; i++)
            {
                var c = line[i];
                if (inQuote != '\0') { if (c == inQuote && line[i - 1] != '\\') inQuote = '\0'; continue; }
                if (c is '\'' or '"' or '`') { inQuote = c; continue; }
                if (c == '/' && line[i + 1] == '/') return line[..i];
            }
            return line;
        }

        private static List<string> SplitSettings(string settings)
        {
            var result = new List<string>();
            var sb = new StringBuilder();
            var inQuote = '\0';
            var parens = 0;
            foreach (var c in settings)
            {
                if (inQuote != '\0') { sb.Append(c); if (c == inQuote) inQuote = '\0'; continue; }
                if (c is '\'' or '"' or '`') { inQuote = c; sb.Append(c); continue; }
                if (c == '(') parens++;
                if (c == ')') parens--;
                if (c == ',' && parens == 0) { result.Add(sb.ToString().Trim()); sb.Clear(); continue; }
                sb.Append(c);
            }
            if (sb.Length > 0) result.Add(sb.ToString().Trim());
            return result.Where(s => s.Length > 0).ToList();
        }

        private static (string Key, string Value)? SplitKeyValue(string setting)
        {
            var idx = setting.IndexOf(':');
            return idx < 0 ? null : (setting[..idx].Trim().ToLowerInvariant(), setting[(idx + 1)..].Trim());
        }

        private static string Unquote(string value)
        {
            value = value.Trim();
            if (value.Length >= 2 && (value[0] is '\'' or '"' or '`') && value[^1] == value[0])
                value = value[1..^1];
            return value.Replace("\\'", "'");
        }
    }
}
