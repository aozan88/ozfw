using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Ornek.Data.Persistence;
using OzFw.ScribanCodeGen.Code;
using OzFw.ScribanCodeGen.Readers;
using OzFw.ScribanCodeGen.Sql;

namespace USBS.Tests.CodeGen;

/// <summary>
/// samples/codegen/ornek.* modelinden üretilmiş Ornek modülü (Projects/backend/USBS/USBS.Services/Ornek) üzerinde:
/// EF'in ürettiği şema ile generator'ın SQL DDL'i aynı mı, repo'daki üretilmiş dosyalar güncel mi.
/// </summary>
public class GeneratedModuleTests
{
    private sealed class NoHttpContext : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; }
    }

    private static readonly string RepoRoot = FindRepoRoot();

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "samples", "codegen")))
            dir = dir.Parent;
        return dir?.FullName ?? throw new DirectoryNotFoundException("Repo kökü bulunamadı");
    }

    private static string EfCreateScript()
    {
        var options = new DbContextOptionsBuilder<OrnekDbContext>()
            .UseNpgsql("Host=localhost;Database=yok") // GenerateCreateScript bağlantı açmaz
            .Options;
        using var context = new OrnekDbContext(options, new NoHttpContext());
        return context.Database.GenerateCreateScript();
    }

    private static string OurDdl() =>
        PostgresDdlGenerator.Generate(ModelReaders.Load(Path.Combine(RepoRoot, "samples", "codegen", "ornek.json")))[0].Content;

    /// <summary>
    /// CREATE TABLE gövdelerinden (tablo, kolon, tip, null) kümesi; kısıt ve index tanımları.
    /// Yazım farkları normalize edilir: tırnaklar atılır (EF gerekmedikçe tırnaklamaz), NULL belirtilmemişse NULL sayılır,
    /// DEFAULT'lar yok sayılır (EF her zaman değer gönderir), IF NOT EXISTS atılır.
    /// </summary>
    private static (SortedSet<string> Columns, SortedSet<string> Constraints, SortedSet<string> Indexes) Shape(string sql)
    {
        sql = sql.Replace("\r\n", "\n").Replace("\"", "");
        var columns = new SortedSet<string>(StringComparer.Ordinal);
        var constraints = new SortedSet<string>(StringComparer.Ordinal);
        var indexes = new SortedSet<string>(StringComparer.Ordinal);

        foreach (Match table in Regex.Matches(sql, @"CREATE TABLE (?:IF NOT EXISTS )?(?<name>[\w.]+) \((?<body>.*?)\n\);", RegexOptions.Singleline))
        {
            var name = table.Groups["name"].Value;
            foreach (var raw in table.Groups["body"].Value.Split('\n'))
            {
                var line = raw.Trim().TrimEnd(',');
                var c = Regex.Match(line, @"^CONSTRAINT (?<cname>\w+) (?<def>.+)$");
                if (c.Success)
                {
                    constraints.Add($"{name} {c.Groups["cname"].Value} {c.Groups["def"].Value}");
                    continue;
                }
                var col = Regex.Match(line, @"^(?<col>\w+) (?<type>.+?)(?: (?<null>NOT NULL|NULL))?(?: DEFAULT .+)?$");
                if (col.Success)
                    columns.Add($"{name}.{col.Groups["col"].Value} {col.Groups["type"].Value} {(col.Groups["null"].Success ? col.Groups["null"].Value : "NULL")}");
            }
        }

        foreach (Match ix in Regex.Matches(sql, @"CREATE INDEX (?:IF NOT EXISTS )?(?<ix>\w+) ON (?<t>[\w.]+) \((?<c>[^)]+)\)"))
            indexes.Add($"{ix.Groups["t"].Value} {ix.Groups["ix"].Value} ({ix.Groups["c"].Value})");

        return (columns, constraints, indexes);
    }

    [Fact]
    public void EF_semasi_generator_DDL_ile_ayni()
    {
        var script = EfCreateScript();
        var ef = Shape(script);
        var ours = Shape(OurDdl());

        Assert.True(ef.Columns.Count > 0, "EF script'i ayrıştırılamadı:\n" + script);
        // 2 tablo x (8 BaseEntity + 3/6 alan); 2 PK + 2 FK; 2 FK index — karşılaştırma boş kümelerle geçmesin
        Assert.Equal(25, ef.Columns.Count);
        Assert.Equal(4, ef.Constraints.Count);
        Assert.Equal(2, ef.Indexes.Count);
        Assert.Equal(string.Join("\n", ours.Columns), string.Join("\n", ef.Columns));
        Assert.Equal(string.Join("\n", ours.Constraints), string.Join("\n", ef.Constraints));
        Assert.Equal(string.Join("\n", ours.Indexes), string.Join("\n", ef.Indexes));
    }

    [Fact]
    public void Repodaki_uretilmis_dosyalar_ve_sql_guncel()
    {
        var model = ModelReaders.Load(Path.Combine(RepoRoot, "samples", "codegen", "ornek.json"));
        var files = CSharpLayerGenerator.Generate(model).Where(f => !f.Scaffold)
            .Concat(PostgresDdlGenerator.Generate(model));

        var stale = files
            .Where(f => !File.Exists(Path.Combine(RepoRoot, f.RelativePath)) ||
                        File.ReadAllText(Path.Combine(RepoRoot, f.RelativePath)).Replace("\r\n", "\n") != f.Content)
            .Select(f => f.RelativePath)
            .ToList();

        Assert.True(stale.Count == 0,
            "Model ile repo'daki üretilmiş dosyalar farklı; generator'ı yeniden çalıştırın:\n" + string.Join("\n", stale));
    }

    [Fact]
    public void Sln_ekleme_mevcut_klasoru_kullanir_ve_tekrarlanmaz()
    {
        var sln = File.ReadAllText(Path.Combine(RepoRoot, "Projects", "backend", "USBS", "USBS.sln"));
        var (again, added) = SolutionFile.AddProjects(sln, "Services/Ornek", new[] { @"USBS.Services\Ornek\USBS.Ornek.API\USBS.Ornek.API.csproj" });
        Assert.Equal(0, added);
        Assert.Equal(sln, again);

        var guids = new Queue<Guid>(Enumerable.Range(1, 10).Select(i => new Guid($"00000000-0000-0000-0000-{i:D12}")));
        var (updated, count) = SolutionFile.AddProjects(sln, "Services/Yeni", new[] { @"USBS.Services\Yeni\USBS.Yeni.API\USBS.Yeni.API.csproj" }, guids.Dequeue);
        Assert.Equal(1, count);
        Assert.Single(Regex.Matches(updated, @"= ""Services"", ""Services"""));
        Assert.Contains("{00000000-0000-0000-0000-000000000002}.Release|Any CPU.Build.0 = Release|Any CPU", updated);
        Assert.DoesNotContain("|x64", updated);
    }
}
