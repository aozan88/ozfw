using OzFw.ScribanCodeGen.Cli;
using OzFw.ScribanCodeGen.Model;
using OzFw.ScribanCodeGen.Readers;
using OzFw.ScribanCodeGen.Sql;

namespace USBS.Tests.CodeGen;

public class CodeGenModelTests
{
    private static readonly string SamplesDir = FindSamples();

    private static string FindSamples()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "samples", "codegen")))
            dir = dir.Parent;
        return dir == null ? throw new DirectoryNotFoundException("samples/codegen bulunamadı") : Path.Combine(dir.FullName, "samples", "codegen");
    }

    private static CodeGenModel Parse<TReader>(string content) where TReader : IModelReader, new() =>
        ModelNormalizer.Normalize(new TReader().Read(content));

    private static string Ddl(CodeGenModel model) =>
        string.Join("\n---\n", PostgresDdlGenerator.Generate(model).Select(f => f.RelativePath + "\n" + f.Content));

    [Theory]
    [InlineData("ornek.csv")]
    [InlineData("ornek.dbml")]
    [InlineData("ornek.mmd")]
    public void Tum_formatlar_json_ile_ayni_model_ve_DDLi_uretir(string file)
    {
        var expected = ModelReaders.Load(Path.Combine(SamplesDir, "ornek.json"));
        var actual = ModelReaders.Load(Path.Combine(SamplesDir, file));

        Assert.Equal(JsonModelReader.Write(expected), JsonModelReader.Write(actual));
        Assert.Equal(Ddl(expected), Ddl(actual));
    }

    [Fact]
    public void Ornek_model_beklenen_DDLi_uretir()
    {
        var files = PostgresDdlGenerator.Generate(ModelReaders.Load(Path.Combine(SamplesDir, "ornek.json")));
        Assert.Equal(new[] { "sql/ornek/_all.sql", "sql/ornek/kategori.sql", "sql/ornek/urun.sql" }, files.Select(f => f.RelativePath));

        var all = files[0].Content;
        Assert.True(all.IndexOf("\"ornek\".\"kategori\" (", StringComparison.Ordinal) < all.IndexOf("\"ornek\".\"urun\" (", StringComparison.Ordinal),
            "Referans verilen tablo önce oluşturulmalı");
        Assert.Contains("\"fiyat\" numeric(18,2) NOT NULL", all);
        Assert.Contains("\"barkod\" varchar(50) NULL", all);
        Assert.Contains("\"gecerlilik_tarihi\" date NULL", all);
        Assert.Contains("CONSTRAINT \"fk_urun_kategori_id\" FOREIGN KEY (\"kategori_id\") REFERENCES \"ornek\".\"kategori\" (\"id\") ON DELETE RESTRICT", all);
        Assert.Contains("CONSTRAINT \"fk_kategori_ust_kategori_id\" FOREIGN KEY (\"ust_kategori_id\") REFERENCES \"ornek\".\"kategori\"", all);
        Assert.Contains("CREATE INDEX IF NOT EXISTS \"ix_urun_kategori_id\"", all);
        Assert.Contains("\"durum\" integer NOT NULL DEFAULT 1", all);
        Assert.Contains("COMMENT ON COLUMN \"ornek\".\"urun\".\"ad\" IS 'Ürün adı';", all);
        Assert.DoesNotContain("DO $$", all);
    }

    [Fact]
    public void Dongusel_FK_tablolardan_sonra_idempotent_eklenir()
    {
        var model = Parse<JsonModelReader>("""
            { "module": "Dongu", "entities": [
              { "name": "A", "fields": [ { "name": "BId", "type": "Guid", "nullable": true } ], "relations": [ { "target": "B", "fkField": "BId", "onDelete": "setnull" } ] },
              { "name": "B", "fields": [ { "name": "AId", "type": "Guid" } ], "relations": [ { "target": "A" } ] }
            ] }
            """);

        var all = PostgresDdlGenerator.Generate(model)[0].Content;
        Assert.Contains("CREATE SCHEMA IF NOT EXISTS \"dongu\";", all);
        Assert.Contains("DO $$", all);
        Assert.Contains("EXCEPTION WHEN duplicate_object THEN NULL;", all);
        Assert.Contains("ON DELETE SET NULL", all);
        Assert.Single(all.Split("ADD CONSTRAINT").Skip(1));
    }

    [Fact]
    public void Girdi_sirasi_ne_olursa_olsun_once_referans_verilen_tablo_olusur()
    {
        var model = Parse<JsonModelReader>("""
            { "module": "Sira", "entities": [
              { "name": "Cocuk", "relations": [ { "target": "Ebeveyn" } ] },
              { "name": "Ebeveyn", "fields": [ { "name": "Ad", "type": "text" } ] }
            ] }
            """);

        var all = PostgresDdlGenerator.Generate(model)[0].Content;
        Assert.True(all.IndexOf("\"ebeveyn\" (", StringComparison.Ordinal) < all.IndexOf("\"cocuk\" (", StringComparison.Ordinal));
        // FK alanı modelde yoksa otomatik eklenir (zorunlu)
        Assert.Contains("\"ebeveyn_id\" uuid NOT NULL", all);
    }

    [Fact]
    public void Dogrulama_hatalari_tek_seferde_toplanir()
    {
        var ex = Assert.Throws<ModelValidationException>(() => Parse<JsonModelReader>("""
            { "module": "Hata", "entities": [
              { "name": "X", "fields": [
                  { "name": "Ad", "type": "metin" },
                  { "name": "Kod", "type": "string" },
                  { "name": "kod", "type": "string" },
                  { "name": "Durum", "type": "int" },
                  { "name": "Tutar", "type": "decimal", "precision": 5, "scale": 7 },
                  { "name": "YId", "type": "Guid" } ],
                "relations": [
                  { "target": "Yok" },
                  { "target": "X", "fkField": "YId", "required": false },
                  { "target": "X", "fkField": "ZId", "onDelete": "setnull" } ] }
            ] }
            """));

        Assert.Contains(ex.Errors, e => e.Contains("bilinmeyen tip") && e.Contains("metin"));
        Assert.Contains(ex.Errors, e => e.Contains("birden fazla tanımlanmış") && e.Contains("Kod", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(ex.Errors, e => e.Contains("X.Durum") && e.Contains("BaseEntity"));
        Assert.Contains(ex.Errors, e => e.Contains("scale"));
        Assert.Contains(ex.Errors, e => e.Contains("İlişki hedefi bulunamadı") && e.Contains("Yok"));
        Assert.Contains(ex.Errors, e => e.Contains("required=false") && e.Contains("YId"));
        Assert.Contains(ex.Errors, e => e.Contains("setnull"));
    }

    [Theory]
    [InlineData("TanimTipiId", "tanim_tipi_id", "TanimTipiId")]
    [InlineData("HTMLSayfa", "html_sayfa", "HtmlSayfa")]
    [InlineData("Adres2Satir", "adres2_satir", "Adres2Satir")]
    [InlineData("ŞehirKodu", "şehir_kodu", "ŞehirKodu")]
    [InlineData("İlKodu", "il_kodu", "IlKodu")] // snake'te İ/I ayrımı kaybolur
    [InlineData("UstKategoriId", "ust_kategori_id", "UstKategoriId")]
    public void Snake_case(string pascal, string snake, string backToPascal)
    {
        Assert.Equal(snake, Naming.ToSnake(pascal));
        Assert.Equal(backToPascal, Naming.ToPascal(snake));
    }

    [Fact]
    public void Uzun_constraint_adi_63_bayta_kisaltilir_ve_benzersiz_kalir()
    {
        var a = PostgresDdlGenerator.ConstraintName("fk", new string('a', 50), "kolon_bir");
        var b = PostgresDdlGenerator.ConstraintName("fk", new string('a', 50), "kolon_iki");
        Assert.True(System.Text.Encoding.UTF8.GetByteCount(a) <= 63);
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Csv_noktali_virgul_ve_tirnak_destekler_snake_adlari_cevirir()
    {
        var model = Parse<CsvModelReader>("""
            # module: Deneme
            entity;field;type;nullable;length;description
            urun_grubu;grup_adi;varchar;hayır;80;"Açıklama; noktalı virgül ve ""tırnak"" içerir"
            urun_grubu;id;uuid;;;
            """);

        var entity = Assert.Single(model.Entities);
        Assert.Equal("UrunGrubu", entity.Name);
        Assert.Equal("urun_grubu", entity.Table);
        var field = Assert.Single(entity.Fields);
        Assert.Equal(("GrupAdi", "grup_adi", "string", 80, false), (field.Name, field.Column, field.Type, field.MaxLength!.Value, field.Nullable));
        Assert.Equal("Açıklama; noktalı virgül ve \"tırnak\" içerir", field.Description);
    }

    [Fact]
    public void Mermaid_belirsiz_FK_icin_aciklayici_hata_verir()
    {
        var ex = Assert.Throws<ModelReadException>(() => new MermaidModelReader().Read("""
            erDiagram
                A ||--o{ B : "iliski"
                A {
                    string ad
                }
                B {
                    uuid bir_id FK
                    uuid iki_id FK
                }
            """));
        Assert.Contains("FK kolonu belirlenemedi", ex.Message);
    }

    [Fact]
    public void Dbml_cokacok_iliski_desteklenmez()
    {
        var ex = Assert.Throws<ModelReadException>(() => new DbmlModelReader().Read("""
            Table a { id uuid [pk] }
            Table b { id uuid [pk] }
            Ref: a.id <> b.id
            """));
        Assert.Contains("Çoka-çok", ex.Message);
    }

    [Fact]
    public void Cli_dry_run_dosya_yazmaz_ve_hata_kodlari()
    {
        var root = Path.Combine(Path.GetTempPath(), "ozfw-codegen-" + Guid.NewGuid().ToString("N"));
        try
        {
            var output = new StringWriter();
            var code = CodeGenCli.Run(new[] { "--model", Path.Combine(SamplesDir, "ornek.dbml"), "--root", root, "--dry-run" }, output, new StringWriter());
            Assert.Equal(CodeGenCli.Ok, code);
            Assert.Contains("[yeni] sql/ornek/_all.sql", output.ToString());
            Assert.False(Directory.Exists(root));

            Assert.Equal(CodeGenCli.Ok, CodeGenCli.Run(new[] { "--model", Path.Combine(SamplesDir, "ornek.dbml"), "--root", root }, new StringWriter(), new StringWriter()));
            Assert.True(File.Exists(Path.Combine(root, "sql", "ornek", "urun.sql")));

            var second = new StringWriter();
            CodeGenCli.Run(new[] { "--model", Path.Combine(SamplesDir, "ornek.mmd"), "--root", root }, second, new StringWriter());
            Assert.Contains("[değişmedi] sql/ornek/_all.sql", second.ToString());

            Assert.Equal(CodeGenCli.UsageError, CodeGenCli.Run(new[] { "--bilinmeyen" }, new StringWriter(), new StringWriter()));
            Assert.Equal(CodeGenCli.UsageError, CodeGenCli.Run(new[] { "--model", Path.Combine(SamplesDir, "ornek.json"), "--only", "entity" }, new StringWriter(), new StringWriter()));

            var bad = Path.Combine(root, "bozuk.json");
            File.WriteAllText(bad, """{ "module": "", "entities": [] }""");
            var err = new StringWriter();
            Assert.Equal(CodeGenCli.ModelError, CodeGenCli.Run(new[] { "--model", bad, "--root", root }, new StringWriter(), err));
            Assert.Contains("Modül adı", err.ToString());
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }
}
