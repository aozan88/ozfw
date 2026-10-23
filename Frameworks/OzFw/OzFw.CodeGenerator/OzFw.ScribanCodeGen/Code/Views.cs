using OzFw.ScribanCodeGen.Model;
using OzFw.ScribanCodeGen.Sql;

namespace OzFw.ScribanCodeGen.Code
{
    /// <summary>Şablonlara verilen, hesaplanmış değerleri içeren modül görünümü.</summary>
    public sealed class ModuleView
    {
        public required string Name { get; init; }
        public required string NamespaceRoot { get; init; }
        public required string DbSchema { get; init; }
        public required string ProjectPrefix { get; init; }
        public required int HttpsPort { get; init; }
        public required int HttpPort { get; init; }
        public required ModulePaths Paths { get; init; }
        public required List<EntityView> Entities { get; init; }
        public required List<BaseColumnView> BaseColumns { get; init; }

        // Namespace'ler (mevcut Ortak/Sistem modülleriyle aynı düzen)
        public string DomainNs => $"{NamespaceRoot}.Common.Domain";
        public string DtosNs => $"{NamespaceRoot}.Common.Dtos.{Name}Dtos";
        public string ContractNs => $"{Name}.Application.Contract";
        public string BusinessNs => $"{Name}.Application.Business";
        public string ValidationsNs => $"{Name}.Application.Validations";
        public string AppRegistrationNs => $"{Name}.Application.Registration";
        public string MappingsNs => $"{NamespaceRoot}.{Name}.Application.Mappings";
        public string PersistenceNs => $"{Name}.Data.Persistence";
        public string ConfigurationsNs => $"{Name}.Data.Persistence.Configurations";
        public string RepositoriesNs => $"{Name}.Data.Repositories";
        public string DataRegistrationNs => $"{Name}.Data.Registration";
        public string ControllersNs => $"{Name}.API.Controllers";
        public string ApiNs => $"{Name}.API";

        public static ModuleView From(CodeGenModel model, int apiPort)
        {
            var root = model.NamespaceRoot!;
            return new ModuleView
            {
                Name = model.Module,
                NamespaceRoot = root,
                DbSchema = model.DbSchema!,
                ProjectPrefix = $"{root}.{model.Module}",
                HttpsPort = apiPort,
                HttpPort = apiPort + 1,
                Paths = ModulePaths.For(root, model.Module),
                Entities = model.Entities.Select(e => EntityView.From(model, e)).ToList(),
                BaseColumns = BaseEntityFields.All.Select(b => new BaseColumnView(b.Name, b.Column, b.SqlType, b.Nullable)).ToList()
            };
        }
    }

    public sealed record BaseColumnView(string Name, string Column, string ColumnType, bool Nullable);

    /// <summary>Repo köküne göre yollar (Projects/backend/{root}/...).</summary>
    public sealed class ModulePaths
    {
        public required string Domain { get; init; }
        public required string Dtos { get; init; }
        public required string Application { get; init; }
        public required string Data { get; init; }
        public required string Api { get; init; }
        public required string Solution { get; init; }
        public required string SolutionFolder { get; init; }

        /// <summary>Modül projelerinden repo köküne göreli yol (csproj referansları için).</summary>
        public string ToRepoRoot => @"..\..\..\..\..\..";

        public static ModulePaths For(string root, string module)
        {
            var backend = $"Projects/backend/{root}";
            var services = $"{backend}/{root}.Services/{module}";
            return new ModulePaths
            {
                Domain = $"{backend}/{root}.Common/{root}.Common.Domain",
                Dtos = $"{backend}/{root}.Common.Dtos",
                Application = $"{services}/{root}.{module}.Application",
                Data = $"{services}/{root}.{module}.Data",
                Api = $"{services}/{root}.{module}.API",
                Solution = $"{backend}/{root}.sln",
                SolutionFolder = $"Services/{module}"
            };
        }
    }

    public sealed class EntityView
    {
        public required string Name { get; init; }
        public required string Table { get; init; }
        public required string DbSetName { get; init; }
        public string? Description { get; init; }
        public required string PkName { get; init; }
        public required List<FieldView> Fields { get; init; }
        public required List<RelationView> Relations { get; init; }

        /// <summary>DTO'da adı farklı olan alanlar (Mapster eşlemesi gerektirir).</summary>
        public List<FieldView> DtoNamedFields => Fields.Where(f => !string.IsNullOrEmpty(f.DtoName)).ToList();
        public bool HasDtoNames => DtoNamedFields.Count > 0;

        public static EntityView From(CodeGenModel model, EntityModel e) => new()
        {
            Name = e.Name,
            Table = e.Table!,
            DbSetName = e.DbSetName!,
            Description = Xml(e.Description),
            PkName = PostgresDdlGenerator.ConstraintName("pk", e.Table!),
            Fields = e.Fields.Select(FieldView.From).ToList(),
            Relations = e.Relations.Select(r => RelationView.From(e, r)).ToList()
        };

        internal static string? Xml(string? text) =>
            text?.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
    }

    public sealed class FieldView
    {
        public required string Name { get; init; }
        public required string Column { get; init; }
        public required string ColumnType { get; init; }
        public required bool Nullable { get; init; }
        /// <summary>Entity'deki C# tipi (nullable işaretiyle).</summary>
        public required string EntityType { get; init; }
        /// <summary>DTO'daki C# tipi (enum'lar int olarak taşınır).</summary>
        public required string DtoType { get; init; }
        /// <summary>Nullable olmayan string için " = string.Empty;" gibi başlangıç değeri, yoksa boş.</summary>
        public required string Initializer { get; init; }
        public required bool IsString { get; init; }
        public required bool IsGuid { get; init; }
        public required bool IsDecimal { get; init; }
        public int? MaxLength { get; init; }
        public int? Precision { get; init; }
        public int? Scale { get; init; }
        public string? DtoName { get; init; }
        /// <summary>DTO'daki property adı: dtoName verilmişse o, yoksa Name.</summary>
        public string DtoPropertyName => string.IsNullOrEmpty(DtoName) ? Name : DtoName;
        public string? Description { get; init; }

        public static FieldView From(FieldModel f)
        {
            var isEnum = TypeMap.IsEnum(f.Type);
            var clr = isEnum ? f.Type[TypeMap.EnumPrefix.Length..] : f.Type;
            var dtoClr = isEnum ? "int" : clr;
            var isRef = f.Type is TypeMap.String or TypeMap.Bytes;
            string Suffix(string t) => f.Nullable ? t + "?" : t;

            return new FieldView
            {
                Name = f.Name,
                Column = f.Column!,
                ColumnType = TypeMap.ToPostgres(f),
                Nullable = f.Nullable,
                EntityType = Suffix(clr),
                DtoType = Suffix(dtoClr),
                Initializer = !f.Nullable && isRef ? (f.Type == TypeMap.String ? " = string.Empty;" : " = Array.Empty<byte>();") : "",
                IsString = f.Type == TypeMap.String,
                IsGuid = f.Type == TypeMap.Guid,
                IsDecimal = f.Type == TypeMap.Decimal,
                MaxLength = f.MaxLength,
                Precision = f.Precision,
                Scale = f.Scale,
                DtoName = f.DtoName,
                Description = EntityView.Xml(f.Description)
            };
        }
    }

    public sealed class RelationView
    {
        public required string Name { get; init; }
        public required string Target { get; init; }
        public required string FkField { get; init; }
        public required bool Required { get; init; }
        public required string DeleteBehavior { get; init; }
        public required string ConstraintName { get; init; }
        public required string IndexName { get; init; }

        public static RelationView From(EntityModel e, RelationModel r)
        {
            var column = e.Fields.First(f => f.Name == r.FkField).Column!;
            return new RelationView
            {
                Name = r.Name!,
                Target = r.Target,
                FkField = r.FkField!,
                Required = r.Required == true,
                DeleteBehavior = r.OnDelete switch
                {
                    OnDeleteActions.Cascade => "Cascade",
                    OnDeleteActions.SetNull => "SetNull",
                    OnDeleteActions.NoAction => "NoAction",
                    _ => "Restrict"
                },
                ConstraintName = PostgresDdlGenerator.ConstraintName("fk", e.Table!, column),
                IndexName = PostgresDdlGenerator.ConstraintName("ix", e.Table!, column)
            };
        }
    }
}
