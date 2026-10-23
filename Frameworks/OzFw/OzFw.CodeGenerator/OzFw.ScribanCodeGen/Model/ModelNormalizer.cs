namespace OzFw.ScribanCodeGen.Model
{
    public sealed class ModelValidationException : Exception
    {
        public IReadOnlyList<string> Errors { get; }

        public ModelValidationException(IReadOnlyList<string> errors)
            : base("Model doğrulanamadı:" + Environment.NewLine + string.Join(Environment.NewLine, errors.Select(e => "  - " + e)))
        {
            Errors = errors;
        }
    }

    /// <summary>
    /// Ham modeli (okuyucunun ürettiği) üretime hazır hale getirir: varsayılan adları doldurur,
    /// tipleri kanonik hale çevirir, eksik FK alanlarını ekler ve tüm hataları toplayıp tek seferde bildirir.
    /// Sonuç deterministiktir: aynı içerikteki modeller hangi formattan gelirse gelsin aynı çıktıyı verir.
    /// </summary>
    public static class ModelNormalizer
    {
        public const string DefaultNamespaceRoot = "USBS";

        public static CodeGenModel Normalize(CodeGenModel model)
        {
            var errors = new List<string>();

            model.Module = model.Module?.Trim() ?? "";
            if (!Naming.IsIdentifier(model.Module))
                errors.Add($"Modül adı geçersiz ya da boş: '{model.Module}'.");

            model.DbSchema = string.IsNullOrWhiteSpace(model.DbSchema) ? Naming.ToSnake(model.Module) : model.DbSchema.Trim();
            model.NamespaceRoot = string.IsNullOrWhiteSpace(model.NamespaceRoot) ? DefaultNamespaceRoot : model.NamespaceRoot.Trim();

            if (model.Entities.Count == 0)
                errors.Add("Modelde hiç entity yok.");

            foreach (var group in model.Entities.GroupBy(e => e.Name, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1))
                errors.Add($"Entity birden fazla tanımlanmış: '{group.Key}'.");

            var entityNames = new HashSet<string>(model.Entities.Select(e => e.Name), StringComparer.Ordinal);

            foreach (var entity in model.Entities)
            {
                NormalizeEntity(entity, errors);
            }

            // İlişkiler alanlar normalize edildikten sonra işlenir (FK alanı tipi kontrolü için)
            foreach (var entity in model.Entities)
            {
                NormalizeRelations(entity, entityNames, errors);
            }

            foreach (var group in model.Entities.GroupBy(e => e.Table, StringComparer.Ordinal).Where(g => g.Count() > 1))
                errors.Add($"Aynı tablo adına sahip entity'ler: {string.Join(", ", group.Select(e => e.Name))} → '{group.Key}'.");

            if (errors.Count > 0)
                throw new ModelValidationException(errors);

            // Deterministik sıra: entity'ler ada göre; alan ve ilişki sırası tanım sırasıdır
            model.Entities = model.Entities.OrderBy(e => e.Name, StringComparer.Ordinal).ToList();
            return model;
        }

        private static void NormalizeEntity(EntityModel entity, List<string> errors)
        {
            entity.Name = entity.Name?.Trim() ?? "";
            var where = $"'{entity.Name}'";
            if (!Naming.IsIdentifier(entity.Name))
                errors.Add($"Entity adı geçersiz: {where}.");

            entity.Table = string.IsNullOrWhiteSpace(entity.Table) ? Naming.ToSnake(entity.Name) : entity.Table.Trim();
            entity.DbSetName = string.IsNullOrWhiteSpace(entity.DbSetName) ? entity.Name + "s" : entity.DbSetName.Trim();
            entity.Description = string.IsNullOrWhiteSpace(entity.Description) ? null : entity.Description.Trim();

            foreach (var field in entity.Fields)
            {
                field.Name = field.Name?.Trim() ?? "";
                var fieldWhere = $"{entity.Name}.{field.Name}";

                if (!Naming.IsIdentifier(field.Name))
                    errors.Add($"Alan adı geçersiz: '{fieldWhere}'.");
                if (BaseEntityFields.IsBase(field.Name) || (field.Column != null && BaseEntityFields.IsBase(field.Column)))
                    errors.Add($"'{fieldWhere}' BaseEntity'den gelen bir alanla çakışıyor; bu alanlar otomatik eklenir, modelde tanımlanmaz.");

                var parsed = TypeMap.Parse(field.Type);
                if (parsed == null)
                {
                    errors.Add($"'{fieldWhere}' için bilinmeyen tip: '{field.Type}'. Desteklenenler: {string.Join(", ", TypeMap.Canonicals)}.");
                }
                else
                {
                    field.Type = parsed.Canonical;
                    field.MaxLength ??= parsed.Length;
                    field.Precision ??= parsed.Precision;
                    field.Scale ??= parsed.Scale;
                    if (parsed.NullableSuffix)
                        field.Nullable = true;

                    if (field.Type != TypeMap.String && field.MaxLength.HasValue)
                        errors.Add($"'{fieldWhere}': maxLength yalnızca string alanlarda kullanılabilir.");
                    if (field.Type != TypeMap.Decimal && (field.Precision.HasValue || field.Scale.HasValue))
                        errors.Add($"'{fieldWhere}': precision/scale yalnızca decimal alanlarda kullanılabilir.");
                }

                if (field.MaxLength is <= 0)
                    errors.Add($"'{fieldWhere}': maxLength pozitif olmalı.");
                if (field.Precision is <= 0 or > 1000)
                    errors.Add($"'{fieldWhere}': precision 1-1000 arasında olmalı.");
                if (field.Scale is < 0 || (field.Precision.HasValue && field.Scale > field.Precision))
                    errors.Add($"'{fieldWhere}': scale 0 ile precision arasında olmalı.");
                if (field.Scale.HasValue && !field.Precision.HasValue)
                    errors.Add($"'{fieldWhere}': scale verildiyse precision da verilmeli.");
                if (field.Precision.HasValue)
                    field.Scale ??= 0;
                if (field.DtoName != null && !Naming.IsIdentifier(field.DtoName.Trim()))
                    errors.Add($"'{fieldWhere}': dtoName geçersiz '{field.DtoName}'.");

                field.Column = string.IsNullOrWhiteSpace(field.Column) ? Naming.ToSnake(field.Name) : field.Column.Trim();
                field.DtoName = string.IsNullOrWhiteSpace(field.DtoName) ? null : field.DtoName.Trim();
                field.Description = string.IsNullOrWhiteSpace(field.Description) ? null : field.Description.Trim();
            }

            foreach (var group in entity.Fields.GroupBy(f => f.Name, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1))
                errors.Add($"'{entity.Name}' içinde alan birden fazla tanımlanmış: '{group.Key}'.");
            foreach (var group in entity.Fields.GroupBy(f => f.Column, StringComparer.Ordinal).Where(g => g.Count() > 1 && g.Select(f => f.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1))
                errors.Add($"'{entity.Name}' içinde aynı kolon adına düşen alanlar: {string.Join(", ", group.Select(f => f.Name))} → '{group.Key}'.");
        }

        private static void NormalizeRelations(EntityModel entity, HashSet<string> entityNames, List<string> errors)
        {
            foreach (var relation in entity.Relations)
            {
                relation.Target = relation.Target?.Trim() ?? "";
                var where = $"{entity.Name} → {relation.Target}";

                if (!entityNames.Contains(relation.Target))
                {
                    errors.Add($"İlişki hedefi bulunamadı: '{where}'.");
                    continue;
                }

                relation.FkField = string.IsNullOrWhiteSpace(relation.FkField)
                    ? (string.IsNullOrWhiteSpace(relation.Name) ? relation.Target : relation.Name.Trim()) + "Id"
                    : relation.FkField.Trim();

                var onDelete = OnDeleteActions.Normalize(relation.OnDelete);
                if (onDelete == null)
                    errors.Add($"'{where}': geçersiz onDelete '{relation.OnDelete}' (restrict, cascade, setnull, noaction).");
                relation.OnDelete = onDelete ?? OnDeleteActions.Restrict;

                var fk = entity.Fields.FirstOrDefault(f => string.Equals(f.Name, relation.FkField, StringComparison.OrdinalIgnoreCase));
                if (fk == null)
                {
                    fk = new FieldModel
                    {
                        Name = relation.FkField,
                        Type = TypeMap.Guid,
                        Nullable = relation.Required == false,
                        Column = Naming.ToSnake(relation.FkField)
                    };
                    entity.Fields.Add(fk);
                }
                else
                {
                    relation.FkField = fk.Name;
                    if (fk.Type != TypeMap.Guid)
                        errors.Add($"'{entity.Name}.{fk.Name}' FK alanı Guid olmalı (şu an: {fk.Type}).");
                    if (relation.Required.HasValue && relation.Required.Value == fk.Nullable)
                        errors.Add($"'{where}': required={relation.Required.Value.ToString().ToLowerInvariant()} ama FK alanı '{fk.Name}' nullable={fk.Nullable.ToString().ToLowerInvariant()}.");
                }
                relation.Required = !fk.Nullable;

                if (relation.OnDelete == OnDeleteActions.SetNull && relation.Required == true)
                    errors.Add($"'{where}': onDelete=setnull zorunlu (nullable olmayan) FK ile kullanılamaz.");

                relation.Name = string.IsNullOrWhiteSpace(relation.Name)
                    ? (relation.FkField.EndsWith("Id", StringComparison.Ordinal) && relation.FkField.Length > 2 ? relation.FkField[..^2] : relation.FkField + "Ref")
                    : relation.Name.Trim();

                if (!Naming.IsIdentifier(relation.Name))
                    errors.Add($"'{where}': navigation adı geçersiz '{relation.Name}'.");
                if (entity.Fields.Any(f => string.Equals(f.Name, relation.Name, StringComparison.OrdinalIgnoreCase)))
                    errors.Add($"'{where}': navigation adı '{relation.Name}' bir alanla çakışıyor.");
            }

            // Formatlar ilişkileri farklı sırada verebilir (Mermaid/DBML'de ayrı satırlar); FK alanının sırası esas alınır
            entity.Relations = entity.Relations
                .OrderBy(r => entity.Fields.FindIndex(f => string.Equals(f.Name, r.FkField, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            foreach (var group in entity.Relations.GroupBy(r => r.FkField, StringComparer.OrdinalIgnoreCase).Where(g => g.Key != null && g.Count() > 1))
                errors.Add($"'{entity.Name}.{group.Key}' için birden fazla ilişki tanımlanmış.");
            foreach (var group in entity.Relations.GroupBy(r => r.Name, StringComparer.OrdinalIgnoreCase).Where(g => g.Key != null && g.Count() > 1))
                errors.Add($"'{entity.Name}' içinde aynı navigation adı birden fazla: '{group.Key}'.");
        }
    }
}
