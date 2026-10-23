using System.Text;
using OzFw.ScribanCodeGen.Model;
using OzFw.ScribanCodeGen.Readers;
using OzFw.ScribanCodeGen.Sql;

namespace OzFw.ScribanCodeGen.Cli
{
    /// <summary>
    /// dotnet run --project Frameworks/OzFw/OzFw.CodeGenerator/OzFw.ScribanCodeGen -- --model &lt;dosya&gt; [seçenekler]
    /// </summary>
    public static class CodeGenCli
    {
        public const int Ok = 0;
        public const int ModelError = 1;
        public const int UsageError = 2;

        private static readonly string[] Targets = { "sql" };

        private const string Usage = """
            Kullanım:
              OzFw.ScribanCodeGen --model <dosya> [--root <klasör>] [--only sql] [--dry-run]
                                  [--module <Ad>] [--schema <şema>] [--namespace <Kök>]
                                  [--export-model <dosya.json>]

              --model         Model dosyası: .json (ara model), .csv, .dbml (dbdiagram.io), .mmd/.mermaid (erDiagram)
              --root          Çıktı kök klasörü (varsayılan: geçerli klasör). SQL: <root>/sql/<modul>/
              --only          Üretilecek hedefler, virgülle (şimdilik: sql). Varsayılan: hepsi
              --dry-run       Dosya yazmaz; nelerin yazılacağını listeler
              --module/--schema/--namespace  Model dosyasındaki değerleri geçersiz kılar
              --export-model  Normalize edilmiş modeli JSON olarak yazar (diyagram/CSV → ara model dönüşümü)

            Model şeması: docs/codegen-model.schema.json
            """;

        public static int Run(string[] args, TextWriter? output = null, TextWriter? error = null)
        {
            if (output == null || error == null)
                Console.OutputEncoding = Encoding.UTF8;
            output ??= Console.Out;
            error ??= Console.Error;

            Dictionary<string, string?> options;
            try
            {
                options = ParseArgs(args);
            }
            catch (ArgumentException ex)
            {
                error.WriteLine(ex.Message);
                error.WriteLine(Usage);
                return UsageError;
            }

            if (options.ContainsKey("help") || !options.TryGetValue("model", out var modelPath) || string.IsNullOrWhiteSpace(modelPath))
            {
                (options.ContainsKey("help") ? output : error).WriteLine(Usage);
                return options.ContainsKey("help") ? Ok : UsageError;
            }

            var only = (options.GetValueOrDefault("only") ?? string.Join(",", Targets))
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(t => t.ToLowerInvariant()).ToList();
            var unknown = only.Except(Targets).ToList();
            if (unknown.Count > 0)
            {
                error.WriteLine($"Bilinmeyen hedef: {string.Join(", ", unknown)}. Desteklenenler: {string.Join(", ", Targets)}.");
                return UsageError;
            }

            if (!File.Exists(modelPath))
            {
                error.WriteLine($"Model dosyası bulunamadı: {modelPath}");
                return UsageError;
            }

            CodeGenModel model;
            try
            {
                model = ModelReaders.Load(modelPath, options.GetValueOrDefault("module"), options.GetValueOrDefault("schema"), options.GetValueOrDefault("namespace"));
            }
            catch (ModelReadException ex)
            {
                error.WriteLine($"{Path.GetFileName(modelPath)}: {ex.Message}");
                return ModelError;
            }
            catch (ModelValidationException ex)
            {
                error.WriteLine($"{Path.GetFileName(modelPath)}: {ex.Message}");
                return ModelError;
            }

            var dryRun = options.ContainsKey("dry-run");
            var root = Path.GetFullPath(options.GetValueOrDefault("root") ?? Directory.GetCurrentDirectory());
            output.WriteLine($"Model: {model.Module} ({model.Entities.Count} entity, şema '{model.DbSchema}') ← {Path.GetFileName(modelPath)}");

            var files = new List<GeneratedFile>();
            if (only.Contains("sql"))
                files.AddRange(PostgresDdlGenerator.Generate(model));

            if (options.TryGetValue("export-model", out var exportPath) && !string.IsNullOrWhiteSpace(exportPath))
                files.Add(new GeneratedFile(Path.GetRelativePath(root, Path.GetFullPath(exportPath)), JsonModelReader.Write(model) + "\n"));

            foreach (var file in files)
            {
                var fullPath = Path.Combine(root, file.RelativePath);
                var existing = File.Exists(fullPath) ? File.ReadAllText(fullPath) : null;
                var status = existing == null ? "yeni" : existing == file.Content ? "değişmedi" : "güncellenecek";

                if (!dryRun && status != "değişmedi")
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
                    File.WriteAllText(fullPath, file.Content, new UTF8Encoding(false));
                }

                output.WriteLine($"  [{(dryRun ? status : status == "güncellenecek" ? "güncellendi" : status)}] {file.RelativePath.Replace('\\', '/')}");
            }

            if (dryRun)
                output.WriteLine("Dry-run: hiçbir dosya yazılmadı.");
            return Ok;
        }

        private static Dictionary<string, string?> ParseArgs(string[] args)
        {
            var flags = new HashSet<string> { "dry-run", "help" };
            var valued = new HashSet<string> { "model", "root", "only", "module", "schema", "namespace", "export-model" };
            var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg is "-h" or "/?") { result["help"] = null; continue; }
                if (!arg.StartsWith("--"))
                    throw new ArgumentException($"Beklenmeyen argüman: '{arg}'.");

                var key = arg[2..];
                string? value = null;
                var eq = key.IndexOf('=');
                if (eq >= 0) { value = key[(eq + 1)..]; key = key[..eq]; }

                if (flags.Contains(key)) { result[key] = null; continue; }
                if (!valued.Contains(key))
                    throw new ArgumentException($"Bilinmeyen seçenek: '--{key}'.");
                if (value == null)
                {
                    if (i + 1 >= args.Length || args[i + 1].StartsWith("--"))
                        throw new ArgumentException($"'--{key}' bir değer bekliyor.");
                    value = args[++i];
                }
                result[key] = value;
            }
            return result;
        }
    }
}
