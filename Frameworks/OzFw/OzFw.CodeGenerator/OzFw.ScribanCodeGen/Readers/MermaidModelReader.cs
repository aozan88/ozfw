using System.Text.RegularExpressions;
using OzFw.ScribanCodeGen.Model;

namespace OzFw.ScribanCodeGen.Readers
{
    /// <summary>
    /// Mermaid erDiagram okuyucu.
    /// Entity bloğu:  URUN { uuid kategori_id FK "ref: Kategori" }
    /// Attribute:     tip ad [PK|FK|UK[, ...]] ["yorum"]
    /// Yorum yönergeleri ';' ile ayrılır, kalan metin açıklamadır:
    ///   nullable | not null, maxLength: 100, precision: 18,2, ref: Hedef, onDelete: cascade, column: ad, dto: ad
    /// Mermaid tiplerinde virgül olamadığından ondalıklar "decimal" + "precision: 18,2" ile verilir
    /// (varchar(100) gibi parantezli tipler de kabul edilir).
    /// Alanlar varsayılan olarak zorunludur (not null). FK alanında yönerge yoksa nullable bilgisi ilişki
    /// kardinalitesinden alınır: "||" zorunlu, "|o" isteğe bağlı.
    /// İlişki: A ||--o{ B : "etiket"  → FK "çok" tarafta (B). Etiket FK kolonunun adıysa o kolon kullanılır.
    /// Yönergeler: "%% module: Ornek", "%% dbSchema: ornek", "%% namespaceRoot: USBS".
    /// </summary>
    public sealed class MermaidModelReader : IModelReader
    {
        private static readonly Regex DirectiveRegex = new(@"^%%\s*(module|dbschema|schema|namespaceroot|namespace)\s*:(.*)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex EntityOpenRegex = new(@"^""?([\p{L}\p{Nd}_-]+)""?(?:\s*\[\s*""?[^\]]*""?\s*\])?\s*\{$", RegexOptions.Compiled);
        private static readonly Regex AttributeRegex = new(@"^([\p{L}_][\p{L}\p{Nd}_\-\[\]\(\),]*)\s+([\p{L}_][\p{L}\p{Nd}_\-]*)((?:\s+(?:PK|FK|UK)(?:\s*,\s*(?:PK|FK|UK))*)?)\s*(?:""(.*)"")?$", RegexOptions.Compiled);
        private static readonly Regex RelationRegex = new(@"^""?([\p{L}\p{Nd}_-]+)""?\s+(\|o|\|\||\}o|\}\|)(--|\.\.)(o\||\|\||o\{|\|\{)\s+""?([\p{L}\p{Nd}_-]+)""?\s*(?::\s*(.*))?$", RegexOptions.Compiled);

        public IReadOnlyList<string> Extensions { get; } = new[] { ".mmd", ".mermaid" };

        private sealed class EntityInfo
        {
            public required EntityModel Entity { get; init; }
            public Dictionary<string, (FieldModel Field, bool IsFk, bool ExplicitNullable, string? RefTarget, string? OnDelete)> Attributes { get; } = new(StringComparer.OrdinalIgnoreCase);
        }

        private sealed record PendingRelation(int Line, string Left, string LeftCard, string RightCard, string Right, string? Label);

        public CodeGenModel Read(string content)
        {
            var model = new CodeGenModel();
            var entities = new Dictionary<string, EntityInfo>(StringComparer.OrdinalIgnoreCase);
            var relations = new List<PendingRelation>();
            EntityInfo? current = null;
            var sawHeader = false;

            var lines = content.Replace("\r\n", "\n").Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                var lineNo = i + 1;
                var line = lines[i].Trim().TrimStart('﻿');
                if (line.Length == 0 || line.StartsWith("```"))
                    continue;

                if (line.StartsWith("%%"))
                {
                    var d = DirectiveRegex.Match(line);
                    if (d.Success)
                        CsvModelReader.ApplyDirective(model, $"{d.Groups[1].Value}:{d.Groups[2].Value}");
                    continue;
                }

                if (!sawHeader)
                {
                    if (!line.Equals("erDiagram", StringComparison.Ordinal))
                        throw ModelReadException.At(lineNo, "Mermaid dosyası 'erDiagram' ile başlamalı.");
                    sawHeader = true;
                    continue;
                }

                if (current != null)
                {
                    if (line == "}") { current = null; continue; }
                    ReadAttribute(current, line, lineNo);
                    continue;
                }

                Match m;
                if ((m = EntityOpenRegex.Match(line)).Success)
                {
                    current = GetOrAdd(model, entities, m.Groups[1].Value);
                }
                else if ((m = RelationRegex.Match(line)).Success)
                {
                    GetOrAdd(model, entities, m.Groups[1].Value);
                    GetOrAdd(model, entities, m.Groups[5].Value);
                    var label = m.Groups[6].Success ? m.Groups[6].Value.Trim().Trim('"').Trim() : null;
                    relations.Add(new PendingRelation(lineNo, m.Groups[1].Value, m.Groups[2].Value, m.Groups[4].Value, m.Groups[5].Value, label));
                }
                else
                {
                    throw ModelReadException.At(lineNo, $"Anlaşılamayan Mermaid satırı: '{line}'.");
                }
            }

            if (current != null)
                throw new ModelReadException("Mermaid: kapanmamış entity bloğu ('}' eksik).");

            var linked = new HashSet<(EntityInfo, string)>();
            foreach (var r in relations)
                ApplyRelation(r, entities, linked);

            // İlişki satırı olmadan "ref: X" ile işaretlenmiş FK'ler
            foreach (var info in entities.Values.Distinct())
            {
                foreach (var (column, attr) in info.Attributes)
                {
                    if (attr.RefTarget == null || linked.Contains((info, column)))
                        continue;
                    var target = Resolve(entities, attr.RefTarget, 0);
                    info.Entity.Relations.Add(new RelationModel { Target = target.Entity.Name, FkField = attr.Field.Name, OnDelete = attr.OnDelete });
                }
            }

            return model;
        }

        private static EntityInfo GetOrAdd(CodeGenModel model, Dictionary<string, EntityInfo> entities, string raw)
        {
            if (entities.TryGetValue(raw, out var existing))
                return existing;

            var name = ExternalName.From(raw);
            var info = entities.Values.FirstOrDefault(e => e.Entity.Name == name.Name);
            if (info == null)
            {
                info = new EntityInfo { Entity = new EntityModel { Name = name.Name, Table = name.DbName } };
                model.Entities.Add(info.Entity);
            }
            entities[raw] = info;
            return info;
        }

        private static EntityInfo Resolve(Dictionary<string, EntityInfo> entities, string raw, int line)
        {
            if (entities.TryGetValue(raw, out var info))
                return info;
            var pascal = ExternalName.From(raw).Name;
            return entities.Values.FirstOrDefault(e => e.Entity.Name == pascal)
                   ?? throw ModelReadException.At(line, $"Entity bulunamadı: '{raw}'.");
        }

        private static void ReadAttribute(EntityInfo info, string line, int lineNo)
        {
            var m = AttributeRegex.Match(line);
            if (!m.Success)
                throw ModelReadException.At(lineNo, $"Attribute tanımı anlaşılamadı: '{line}'.");

            var rawType = m.Groups[1].Value;
            var rawName = m.Groups[2].Value;
            var keys = m.Groups[3].Value;
            var comment = m.Groups[4].Success ? m.Groups[4].Value : null;

            if (BaseEntityFields.IsBase(rawName))
                return;
            if (Regex.IsMatch(keys, @"\bPK\b"))
                throw ModelReadException.At(lineNo, $"'{rawName}': yalnızca BaseEntity'nin 'id' alanı PK olabilir.");

            var name = ExternalName.From(rawName);
            var field = new FieldModel { Name = name.Name, Column = name.DbName, Type = rawType };
            bool? nullable = null;
            string? refTarget = null, onDelete = null;
            var description = new List<string>();

            foreach (var part in (comment ?? "").Split(';').Select(p => p.Trim()).Where(p => p.Length > 0))
            {
                var lower = part.ToLowerInvariant();
                if (lower is "nullable" or "null") { nullable = true; continue; }
                if (lower is "not null" or "required" or "zorunlu") { nullable = false; continue; }

                var idx = part.IndexOf(':');
                var key = idx > 0 ? part[..idx].Trim().ToLowerInvariant() : null;
                var value = idx > 0 ? part[(idx + 1)..].Trim() : null;
                switch (key)
                {
                    case "maxlength" or "length":
                        field.MaxLength = ParseInt(value, lineNo);
                        break;
                    case "precision":
                        var ps = value!.Split(',', StringSplitOptions.TrimEntries);
                        field.Precision = ParseInt(ps[0], lineNo);
                        if (ps.Length > 1) field.Scale = ParseInt(ps[1], lineNo);
                        break;
                    case "ref":
                        refTarget = value;
                        break;
                    case "ondelete":
                        onDelete = value;
                        break;
                    case "column":
                        field.Column = value;
                        break;
                    case "dto":
                        field.DtoName = value;
                        break;
                    case "desc" or "description" or "açıklama" or "aciklama":
                        description.Add(value!);
                        break;
                    default:
                        description.Add(part);
                        break;
                }
            }

            field.Nullable = nullable ?? false;
            field.Description = description.Count > 0 ? string.Join("; ", description) : null;

            info.Entity.Fields.Add(field);
            info.Attributes[rawName] = (field, Regex.IsMatch(keys, @"\bFK\b") || refTarget != null, nullable.HasValue, refTarget, onDelete);
        }

        private static void ApplyRelation(PendingRelation r, Dictionary<string, EntityInfo> entities, HashSet<(EntityInfo, string)> linked)
        {
            var left = Resolve(entities, r.Left, r.Line);
            var right = Resolve(entities, r.Right, r.Line);

            var rightMany = r.RightCard.EndsWith('{');
            var leftMany = r.LeftCard.StartsWith('}');
            if (rightMany && leftMany)
                throw ModelReadException.At(r.Line, "Çoka-çok ilişki desteklenmiyor; ara entity tanımlayın.");

            // FK "çok" taraftadır; bire-bir ilişkide sağ taraf FK taşır
            var (holder, target, targetCard) = leftMany ? (left, right, r.RightCard) : (right, left, r.LeftCard);

            var candidates = holder.Attributes
                .Where(a => a.Value.IsFk && !linked.Contains((holder, a.Key)))
                .ToList();

            KeyValuePair<string, (FieldModel Field, bool IsFk, bool ExplicitNullable, string? RefTarget, string? OnDelete)> fk;
            if (r.Label != null && holder.Attributes.TryGetValue(r.Label, out var byLabel))
            {
                fk = new(r.Label, byLabel);
            }
            else
            {
                var targetName = target.Entity.Name;
                var matching = candidates.Where(a =>
                    (a.Value.RefTarget != null && ExternalName.From(a.Value.RefTarget).Name == targetName) ||
                    string.Equals(a.Value.Field.Name, targetName + "Id", StringComparison.Ordinal)).ToList();
                if (matching.Count != 1)
                    throw ModelReadException.At(r.Line,
                        $"'{holder.Entity.Name}' içinde '{targetName}' ilişkisi için FK kolonu belirlenemedi " +
                        $"({matching.Count} aday). Kolonu FK + \"ref: {targetName}\" ile işaretleyin ya da ilişki etiketine kolon adını yazın.");
                fk = matching[0];
            }

            linked.Add((holder, fk.Key));
            if (!fk.Value.ExplicitNullable)
                fk.Value.Field.Nullable = targetCard is "|o" or "o|";

            holder.Entity.Relations.Add(new RelationModel
            {
                Target = target.Entity.Name,
                FkField = fk.Value.Field.Name,
                OnDelete = fk.Value.OnDelete
            });
        }

        private static int? ParseInt(string? value, int lineNo) =>
            int.TryParse(value, out var n) ? n : throw ModelReadException.At(lineNo, $"Sayı bekleniyor: '{value}'.");
    }
}
