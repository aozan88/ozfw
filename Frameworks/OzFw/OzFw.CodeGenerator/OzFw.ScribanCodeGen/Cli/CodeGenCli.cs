using System.Text;
using OzFw.ScribanCodeGen.Code;
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

        private static readonly string[] Targets = { "sql", "code" };

        private const string Usage = """
            Kullanım:
              OzFw.ScribanCodeGen --model <dosya> [--root <klasör>] [--only sql,code] [--dry-run] [--force]
                                  [--module <Ad>] [--schema <şema>] [--namespace <Kök>] [--api-port <port>]
                                  [--no-sln] [--export-model <dosya.json>]

              --model         Model dosyası: .json (ara model), .csv, .dbml (dbdiagram.io), .mmd/.mermaid (erDiagram)
              --root          Repo kökü (varsayılan: geçerli klasör). SQL: <root>/sql/<modul>/,
                              C#: <root>/Projects/backend/<NamespaceRoot>/... (Ortak/Sistem ile aynı düzen)
              --only          Üretilecek hedefler, virgülle: sql, code. Varsayılan: hepsi
              --dry-run       Dosya yazmaz; nelerin yazılacağını listeler
              --force         auto-generated başlığı olmayan .g.cs/.sql dosyalarının da üzerine yazar
              --api-port      Yeni modülün HTTPS portu (HTTP = port+1). Varsayılan: 24300
              --no-sln        Yeni oluşturulan projeleri sln'e eklemez
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

            var force = options.ContainsKey("force");
            var apiPort = CSharpLayerGenerator.DefaultApiPort;
            if (options.TryGetValue("api-port", out var portText) && (!int.TryParse(portText, out apiPort) || apiPort is < 1 or > 65534))
            {
                error.WriteLine($"Geçersiz --api-port: '{portText}'.");
                return UsageError;
            }

            var files = new List<GeneratedFile>();
            try
            {
                if (only.Contains("sql"))
                    files.AddRange(PostgresDdlGenerator.Generate(model));
                if (only.Contains("code"))
                    files.AddRange(CSharpLayerGenerator.Generate(model, apiPort));
            }
            catch (ModelValidationException ex)
            {
                error.WriteLine($"{Path.GetFileName(modelPath)}: {ex.Message}");
                return ModelError;
            }

            if (options.TryGetValue("export-model", out var exportPath) && !string.IsNullOrWhiteSpace(exportPath))
                files.Add(new GeneratedFile(Path.GetRelativePath(root, Path.GetFullPath(exportPath)), JsonModelReader.Write(model) + "\n"));

            var createdProjects = new List<string>();
            var skipped = 0;
            foreach (var file in files)
            {
                var fullPath = Path.Combine(root, file.RelativePath);
                var existing = File.Exists(fullPath) ? File.ReadAllText(fullPath) : null;

                string status;
                if (existing == null)
                    status = "yeni";
                else if (file.Scaffold)
                    status = "korundu";
                else if (existing == file.Content)
                    status = "değişmedi";
                else if (!force && !IsGenerated(existing))
                {
                    status = "atlandı: auto-generated başlığı yok, elle düzenlenmiş olabilir (--force ile üzerine yazılır)";
                    skipped++;
                }
                else
                    status = dryRun ? "güncellenecek" : "güncellendi";

                if (!dryRun && (status == "yeni" || status == "güncellendi"))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
                    File.WriteAllText(fullPath, file.Content, new UTF8Encoding(false));
                }
                if (status == "yeni" && file.Scaffold && file.RelativePath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                    createdProjects.Add(fullPath);

                output.WriteLine($"  [{status}] {file.RelativePath.Replace('\\', '/')}");
            }

            if (createdProjects.Count > 0)
            {
                var paths = ModulePaths.For(model.NamespaceRoot!, model.Module);
                var sln = Path.Combine(root, paths.Solution);
                if (options.ContainsKey("no-sln") || !File.Exists(sln))
                    output.WriteLine($"Yeni projeler sln'e eklenmedi. Elle: dotnet sln \"{paths.Solution}\" add --solution-folder {paths.SolutionFolder} <csproj>");
                else if (dryRun)
                    output.WriteLine($"  [sln'e eklenecek] {createdProjects.Count} proje → {paths.Solution} ({paths.SolutionFolder})");
                else
                    AddToSolution(sln, paths.SolutionFolder, createdProjects, output, error);
            }

            if (skipped > 0)
                error.WriteLine($"Uyarı: {skipped} dosya atlandı (yukarıdaki 'atlandı' satırları).");
            if (dryRun)
                output.WriteLine("Dry-run: hiçbir dosya yazılmadı.");
            return Ok;
        }

        /// <summary>Dosyanın ilk satırlarında auto-generated işareti var mı (generator'ın ürettiği ve elle dokunulmamış dosya).</summary>
        private static bool IsGenerated(string content) =>
            content.AsSpan(0, Math.Min(content.Length, 300)).Contains("<auto-generated>", StringComparison.Ordinal);

        private static void AddToSolution(string sln, string solutionFolder, List<string> projects, TextWriter output, TextWriter error)
        {
            try
            {
                var slnDir = Path.GetDirectoryName(sln)!;
                var bytes = File.ReadAllBytes(sln);
                var hasBom = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
                var (content, added) = SolutionFile.AddProjects(
                    File.ReadAllText(sln), solutionFolder, projects.Select(p => Path.GetRelativePath(slnDir, p)));
                if (added > 0)
                    File.WriteAllText(sln, content, new UTF8Encoding(hasBom));
                output.WriteLine($"  [sln] {added} proje {Path.GetFileName(sln)} içine eklendi ({solutionFolder}).");
            }
            catch (Exception ex)
            {
                error.WriteLine($"Projeler sln'e eklenemedi: {ex.Message}");
            }
        }

        private static Dictionary<string, string?> ParseArgs(string[] args)
        {
            var flags = new HashSet<string> { "dry-run", "help", "force", "no-sln" };
            var valued = new HashSet<string> { "model", "root", "only", "module", "schema", "namespace", "export-model", "api-port" };
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
