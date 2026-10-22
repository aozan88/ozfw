using System.Text.Json;
using FluentValidation;
using KonFw.ServiceBase;
using KonFwBaseHelpers;
using KonFwBaseModels;
using KonFwBaseModels.Enums;

namespace USBS.Tests.Framework;

public class QueryableExtensionsTests
{
    private class Kategori
    {
        public string Ad { get; set; } = "";
    }

    private class Kalem
    {
        public string Kod { get; set; } = "";
    }

    private class Urun
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Ad { get; set; } = "";
        public decimal Fiyat { get; set; }
        public int? Stok { get; set; }
        public DurumEnum Durum { get; set; } = DurumEnum.Aktif;
        public DateTime Tarih { get; set; }
        public Kategori Kategori { get; set; } = new();
        public List<Kalem> Kalemler { get; set; } = new();
    }

    private class UrunDto : BaseDto
    {
        [EntityField("Ad")]
        public string UrunAdi { get; set; } = "";

        [EntityField("Kategori.Ad")]
        public string KategoriAdi { get; set; } = "";
    }

    private static IQueryable<Urun> Data() => new List<Urun>
    {
        new() { Ad = "Elma", Fiyat = 10, Stok = 5, Tarih = new DateTime(2026, 1, 1), Kategori = new() { Ad = "Meyve" }, Kalemler = { new() { Kod = "A1" } } },
        new() { Ad = "Armut", Fiyat = 20, Stok = null, Tarih = new DateTime(2026, 2, 1), Kategori = new() { Ad = "Meyve" } },
        new() { Ad = "Ispanak", Fiyat = 30, Stok = 0, Durum = DurumEnum.Pasif, Tarih = new DateTime(2026, 3, 1), Kategori = new() { Ad = "Sebze" }, Kalemler = { new() { Kod = "B2" } } },
        new() { Ad = "İncir", Fiyat = 40, Stok = 2, Tarih = new DateTime(2026, 4, 1), Kategori = new() { Ad = "Meyve" } },
    }.AsQueryable();

    private static SearchFilter Leaf(string field, string op, object? value) => new() { Field = field, Operator = op, Value = value };

    // İstemci JSON'u System.Text.Json ile geldiğinde Value bir JsonElement olur
    private static object Json(string raw) => JsonDocument.Parse(raw).RootElement.Clone();

    [Fact]
    public async Task Filtresiz_istek_sayfalar_ve_sayilari_doner()
    {
        var result = await Data().ApplyQuery(new BaseRequestDto<Urun> { Page = 2, Size = 3 });

        Assert.True(result.IsSuccess);
        Assert.Equal(4, result.TotalTableCount);
        Assert.Equal(4, result.TotalRecordCount);
        Assert.Single(result.ObjectValue!);
        Assert.Equal(1, result.Count);
    }

    [Fact]
    public async Task Size_eksi_bir_tum_kayitlari_doner()
    {
        var result = await Data().ApplyQuery(new BaseRequestDto<Urun> { Size = -1 });
        Assert.Equal(4, result.ObjectValue!.Count());
    }

    [Theory]
    [InlineData("eq", "10", 1)]
    [InlineData("neq", "10", 3)]
    [InlineData("gt", "20", 2)]
    [InlineData("gte", "20", 3)]
    [InlineData("lt", "20", 1)]
    [InlineData("lte", "20", 2)]
    public async Task Sayisal_operatorler(string op, string raw, int expected)
    {
        var result = await Data().ApplyQuery(new BaseRequestDto<Urun> { Size = -1, Filter = Leaf("Fiyat", op, Json(raw)) });
        Assert.Equal(expected, result.TotalRecordCount);
    }

    [Fact]
    public async Task Metin_aramasi_buyuk_kucuk_harf_ve_turkce_karakter_duyarsiz()
    {
        var result = await Data().ApplyQuery(new BaseRequestDto<Urun> { Size = -1, Filter = Leaf("ad", "contains", Json("\"İNC\"")) });
        Assert.Equal("İncir", Assert.Single(result.ObjectValue!).Ad);
    }

    [Fact]
    public async Task Ic_ice_mantiksal_agac()
    {
        // (Fiyat >= 30 OR Ad startswith "el") AND Durum = Aktif
        var filter = new SearchFilter
        {
            Logic = "and",
            Filters = new()
            {
                new SearchFilter { Logic = "or", Filters = new() { Leaf("Fiyat", "gte", 30), Leaf("Ad", "startswith", "el") } },
                Leaf("Durum", "eq", Json("\"Aktif\""))
            }
        };

        var result = await Data().ApplyQuery(new BaseRequestDto<Urun> { Size = -1, Filter = filter });
        Assert.Equal(new[] { "Elma", "İncir" }, result.ObjectValue!.Select(u => u.Ad).OrderBy(a => a));
    }

    [Fact]
    public async Task In_between_ve_null_operatorleri()
    {
        var inResult = await Data().ApplyQuery(new BaseRequestDto<Urun> { Size = -1, Filter = Leaf("Ad", "in", Json("[\"elma\",\"ARMUT\"]")) });
        Assert.Equal(2, inResult.TotalRecordCount);

        var between = await Data().ApplyQuery(new BaseRequestDto<Urun> { Size = -1, Filter = Leaf("Tarih", "between", Json("[\"2026-01-15\",\"2026-03-15\"]")) });
        Assert.Equal(2, between.TotalRecordCount);

        var isNull = await Data().ApplyQuery(new BaseRequestDto<Urun> { Size = -1, Filter = Leaf("Stok", "isnull", null) });
        Assert.Equal("Armut", Assert.Single(isNull.ObjectValue!).Ad);
    }

    [Fact]
    public async Task Koleksiyon_ve_ic_ice_navigation_yolu()
    {
        var any = await Data().ApplyQuery(new BaseRequestDto<Urun> { Size = -1, Filter = Leaf("Kalemler.Kod", "eq", "b2") });
        Assert.Equal("Ispanak", Assert.Single(any.ObjectValue!).Ad);

        var nested = await Data().ApplyQuery(new BaseRequestDto<Urun> { Size = -1, Filter = Leaf("Kategori.Ad", "eq", "sebze") });
        Assert.Single(nested.ObjectValue!);
    }

    [Fact]
    public async Task Dto_alan_adlari_EntityField_ile_entitye_eslenir()
    {
        var request = new BaseRequestDto<UrunDto>
        {
            Size = -1,
            Filter = Leaf("kategoriAdi", "eq", "Meyve"),
            Sort = new[] { new Sort { Field = "urunAdi", Dir = SortType.DESC } }
        };

        var result = await Data().ApplyQuery(request);
        Assert.Equal(new[] { "İncir", "Elma", "Armut" }, result.ObjectValue!.Select(u => u.Ad));
    }

    [Fact]
    public async Task Coklu_siralama()
    {
        var result = await Data().ApplyQuery(new BaseRequestDto<Urun>
        {
            Size = -1,
            Sort = new[] { new Sort { Field = "Kategori.Ad" }, new Sort { Field = "Fiyat", Dir = SortType.DESC } }
        });
        Assert.Equal(new[] { "İncir", "Armut", "Elma", "Ispanak" }, result.ObjectValue!.Select(u => u.Ad));
    }

    [Fact]
    public async Task Bilinmeyen_alan_ve_operator_anlasilir_hata_verir()
    {
        var field = await Assert.ThrowsAsync<QueryFieldException>(() =>
            Data().ApplyQuery(new BaseRequestDto<Urun> { Filter = Leaf("Yok", "eq", 1) }));
        Assert.Contains("Yok", field.Message);

        await Assert.ThrowsAsync<QueryFieldException>(() =>
            Data().ApplyQuery(new BaseRequestDto<Urun> { Filter = Leaf("Fiyat", "benzer", 1) }));
    }

    [Fact]
    public async Task Operator_olmadan_alan_verilirse_hata_ve_bos_dugum_yok_sayilir()
    {
        await Assert.ThrowsAsync<QueryFieldException>(() =>
            Data().ApplyQuery(new BaseRequestDto<Urun> { Filter = new SearchFilter { Field = "Ad" } }));

        var empty = await Data().ApplyQuery(new BaseRequestDto<Urun> { Size = -1, Filter = new SearchFilter { Logic = "and", Filters = new() } });
        Assert.Equal(4, empty.TotalRecordCount);
    }
}

public class BaseRequestDtoValidationTests
{
    private class Model
    {
        public string? Ad { get; set; }
    }

    private class ModelValidator : AbstractValidator<Model>
    {
        public ModelValidator() => RuleFor(x => x.Ad).NotEmpty();
    }

    private static async Task<List<string>> Validate(BaseRequestDto<Model> request, params string[] ruleSets)
    {
        var validator = new BaseRequestDtoValidation<Model>(new ModelValidator());
        var result = await validator.ValidateAsync(request, o => o.IncludeRuleSets(ruleSets).IncludeRulesNotInRuleSet());
        return result.Errors.Select(e => e.PropertyName).ToList();
    }

    [Fact]
    public async Task Common_ClientId_ister()
    {
        var errors = await Validate(new BaseRequestDto<Model>(), BaseRequestDtoValidation<Model>.Common);
        Assert.Contains("ClientId", errors);
    }

    [Fact]
    public async Task GetAll_sayfa_ve_boyut_kurallari()
    {
        var errors = await Validate(new BaseRequestDto<Model> { ClientId = Guid.NewGuid(), Page = 0, Size = 0 },
            BaseRequestDtoValidation<Model>.Common, BaseRequestDtoValidation<Model>.GetAll);
        Assert.Contains("Page", errors);
        Assert.Contains("Size", errors);
    }

    [Fact]
    public async Task CreateOrUpdate_payload_ve_ic_validatoru_calistirir()
    {
        var missing = await Validate(new BaseRequestDto<Model> { ClientId = Guid.NewGuid() }, BaseRequestDtoValidation<Model>.CreateOrUpdate);
        Assert.Contains("Payload", missing);

        var inner = await Validate(new BaseRequestDto<Model> { ClientId = Guid.NewGuid(), Payload = new Model() }, BaseRequestDtoValidation<Model>.CreateOrUpdate);
        Assert.Contains("Payload.Ad", inner);

        var ok = await Validate(new BaseRequestDto<Model> { ClientId = Guid.NewGuid(), Payload = new Model { Ad = "x" } },
            BaseRequestDtoValidation<Model>.Common, BaseRequestDtoValidation<Model>.CreateOrUpdate);
        Assert.Empty(ok);
    }

    [Fact]
    public async Task GetById_validasyonu()
    {
        var validator = new BaseGetByIdDtoValidation();
        var result = await validator.ValidateAsync(new BaseGetByIdDto(), o => o.IncludeRuleSets(BaseGetByIdDtoValidation.Common, BaseGetByIdDtoValidation.GetById));
        Assert.Equal(new[] { "ClientId", "Payload" }, result.Errors.Select(e => e.PropertyName).OrderBy(p => p));
    }
}
