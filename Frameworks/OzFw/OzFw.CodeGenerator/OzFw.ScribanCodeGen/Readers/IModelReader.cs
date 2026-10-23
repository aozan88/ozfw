using OzFw.ScribanCodeGen.Model;

namespace OzFw.ScribanCodeGen.Readers
{
    /// <summary>Bir girdi formatını ara modele (CodeGenModel) dönüştürür. Normalizasyon/doğrulama okuyucunun işi değildir.</summary>
    public interface IModelReader
    {
        /// <summary>Okuyucunun desteklediği dosya uzantıları (noktalı, küçük harf).</summary>
        IReadOnlyList<string> Extensions { get; }

        CodeGenModel Read(string content);
    }

    public sealed class ModelReadException : Exception
    {
        public ModelReadException(string message) : base(message) { }

        public static ModelReadException At(int line, string message) => new($"Satır {line}: {message}");
    }

    public static class ModelReaders
    {
        public static readonly IReadOnlyList<IModelReader> All = new IModelReader[]
        {
            new JsonModelReader(),
            new CsvModelReader(),
            new DbmlModelReader(),
            new MermaidModelReader()
        };

        public static IModelReader ForPath(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return All.FirstOrDefault(r => r.Extensions.Contains(ext))
                ?? throw new ModelReadException($"Desteklenmeyen model dosyası uzantısı '{ext}'. Desteklenenler: {string.Join(", ", All.SelectMany(r => r.Extensions))}.");
        }

        /// <summary>Dosyayı okur, isteğe bağlı modül/şema geçersiz kılmalarını uygular ve modeli normalize eder.</summary>
        public static CodeGenModel Load(string path, string? module = null, string? dbSchema = null, string? namespaceRoot = null)
        {
            var model = ForPath(path).Read(File.ReadAllText(path));
            if (!string.IsNullOrWhiteSpace(module)) model.Module = module;
            if (!string.IsNullOrWhiteSpace(dbSchema)) model.DbSchema = dbSchema;
            if (!string.IsNullOrWhiteSpace(namespaceRoot)) model.NamespaceRoot = namespaceRoot;
            return ModelNormalizer.Normalize(model);
        }
    }
}
