using System.Text;
using OzFw.ScribanCodeGen.Model;

namespace OzFw.ScribanCodeGen.Readers
{
    /// <summary>
    /// Her satır bir alan. Başlık: entity,field,type,nullable,length,precision,scale,fk_target,description
    /// (ek olarak isteğe bağlı: column, dto_name, on_delete). Ayraç virgül ya da noktalı virgül (Excel TR) olabilir.
    /// Modül bilgisi yorum satırıyla verilir: "# module: Ornek", "# dbSchema: ornek", "# namespaceRoot: USBS".
    /// BaseEntity kolonları (id, olusturan_kisi ...) satır olarak gelirse atlanır.
    /// </summary>
    public sealed class CsvModelReader : IModelReader
    {
        private static readonly string[] RequiredColumns = { "entity", "field", "type" };

        public IReadOnlyList<string> Extensions { get; } = new[] { ".csv" };

        public CodeGenModel Read(string content)
        {
            var model = new CodeGenModel();
            var entities = new Dictionary<string, EntityModel>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, int>? header = null;
            char delimiter = ',';

            var lines = content.Replace("\r\n", "\n").Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                var lineNo = i + 1;
                var line = lines[i].Trim().TrimStart('﻿');
                if (line.Length == 0)
                    continue;

                if (line.StartsWith('#'))
                {
                    ApplyDirective(model, line.TrimStart('#'));
                    continue;
                }

                if (header == null)
                {
                    delimiter = line.Count(c => c == ';') > line.Count(c => c == ',') ? ';' : ',';
                    var names = Split(line, delimiter, lineNo);
                    header = names
                        .Select((n, idx) => (Name: n.Trim().ToLowerInvariant().Replace(" ", "_"), idx))
                        .GroupBy(x => x.Name).ToDictionary(g => g.Key, g => g.First().idx);
                    var missing = RequiredColumns.Where(c => !header.ContainsKey(c)).ToList();
                    if (missing.Count > 0)
                        throw ModelReadException.At(lineNo, $"CSV başlığında zorunlu kolon(lar) eksik: {string.Join(", ", missing)}.");
                    continue;
                }

                var cells = Split(line, delimiter, lineNo);
                string? Cell(string name) =>
                    header.TryGetValue(name, out var idx) && idx < cells.Count && !string.IsNullOrWhiteSpace(cells[idx]) ? cells[idx].Trim() : null;

                var entityName = ExternalName.From(Cell("entity") ?? throw ModelReadException.At(lineNo, "entity boş."));
                var rawField = Cell("field") ?? throw ModelReadException.At(lineNo, "field boş.");

                if (!entities.TryGetValue(entityName.Name, out var entity))
                {
                    entity = new EntityModel { Name = entityName.Name, Table = entityName.DbName };
                    entities.Add(entityName.Name, entity);
                    model.Entities.Add(entity);
                }

                if (BaseEntityFields.IsBase(rawField))
                    continue;

                var fieldName = ExternalName.From(rawField);
                entity.Fields.Add(new FieldModel
                {
                    Name = fieldName.Name,
                    Type = Cell("type") ?? throw ModelReadException.At(lineNo, "type boş."),
                    Nullable = ParseBool(Cell("nullable"), lineNo),
                    MaxLength = ParseInt(Cell("length") ?? Cell("max_length"), lineNo),
                    Precision = ParseInt(Cell("precision"), lineNo),
                    Scale = ParseInt(Cell("scale"), lineNo),
                    Column = Cell("column") ?? fieldName.DbName,
                    DtoName = Cell("dto_name"),
                    Description = Cell("description")
                });

                var target = Cell("fk_target");
                if (target != null)
                {
                    entity.Relations.Add(new RelationModel
                    {
                        Target = ExternalName.From(target).Name,
                        FkField = fieldName.Name,
                        OnDelete = Cell("on_delete")
                    });
                }
            }

            if (header == null)
                throw new ModelReadException("CSV başlık satırı bulunamadı.");
            return model;
        }

        internal static void ApplyDirective(CodeGenModel model, string directive)
        {
            var idx = directive.IndexOf(':');
            if (idx < 0)
                return;
            var key = directive[..idx].Trim().ToLowerInvariant();
            var value = directive[(idx + 1)..].Trim();
            switch (key)
            {
                case "module": model.Module = value; break;
                case "dbschema": case "schema": model.DbSchema = value; break;
                case "namespaceroot": case "namespace": model.NamespaceRoot = value; break;
            }
        }

        private static bool ParseBool(string? value, int lineNo) => value?.Trim().ToLowerInvariant() switch
        {
            null or "" or "false" or "0" or "no" or "hayır" or "hayir" or "h" or "not null" => false,
            "true" or "1" or "yes" or "evet" or "e" or "null" or "nullable" => true,
            var v => throw ModelReadException.At(lineNo, $"nullable değeri anlaşılamadı: '{v}' (true/false, evet/hayır)."),
        };

        private static int? ParseInt(string? value, int lineNo)
        {
            if (value == null)
                return null;
            return int.TryParse(value, out var n) ? n : throw ModelReadException.At(lineNo, $"Sayı bekleniyor: '{value}'.");
        }

        /// <summary>RFC 4180 tarzı: tırnak içindeki ayraç ve "" kaçışı desteklenir.</summary>
        private static List<string> Split(string line, char delimiter, int lineNo)
        {
            var cells = new List<string>();
            var sb = new StringBuilder();
            var inQuotes = false;
            for (var i = 0; i < line.Length; i++)
            {
                var c = line[i];
                if (inQuotes)
                {
                    if (c == '"' && i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; }
                    else if (c == '"') inQuotes = false;
                    else sb.Append(c);
                }
                else if (c == '"') inQuotes = true;
                else if (c == delimiter) { cells.Add(sb.ToString()); sb.Clear(); }
                else sb.Append(c);
            }
            if (inQuotes)
                throw ModelReadException.At(lineNo, "Kapanmamış tırnak.");
            cells.Add(sb.ToString());
            return cells;
        }
    }
}
