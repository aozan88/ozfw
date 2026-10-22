using Mapster;
using Sistem.Application.DtoModels;
using USBS.Common.Domain;

namespace USBS.Sistem.Application.Mappings
{
    public static class MapsterConfig
    {
        /// <summary>
        /// Aynı isimli alanlar Mapster tarafından otomatik eşlenir; burada yalnızca adı farklı olanlar tanımlanır.
        /// Filtre/sıralama tarafı için aynı eşleme DTO üzerindeki [EntityField] ile verilir.
        /// </summary>
        public static void RegisterMappings()
        {
            TypeAdapterConfig<SistemParametre, SistemParametreDto>.NewConfig()
                .Map(dest => dest.SistemParametreAdi, src => src.ParametreAdi)
                .Map(dest => dest.SistemParametreTipiId, src => src.ParametreTipiId)
                .Map(dest => dest.UstSistemParametreId, src => src.UstParametreId);

            TypeAdapterConfig<SistemParametreDto, SistemParametre>.NewConfig()
                .Map(dest => dest.ParametreAdi, src => src.SistemParametreAdi)
                .Map(dest => dest.ParametreTipiId, src => src.SistemParametreTipiId)
                .Map(dest => dest.UstParametreId, src => src.UstSistemParametreId ?? Guid.Empty);
        }
    }
}
