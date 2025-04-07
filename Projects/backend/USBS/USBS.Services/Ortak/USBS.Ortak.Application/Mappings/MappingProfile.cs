using AutoMapper;
using USBS.Common.Dtos.OrtakDtos;
using USBS.Common.Domain;

namespace Ortak.Application.Mappings
{
    public class DtoMappingProfile : Profile
    {
        public DtoMappingProfile()
        {
            CreateMap<SistemTanim, SistemTanimDto>().ReverseMap();
            CreateMap<SistemTanimTipi, SistemTanimTipiDto>().ReverseMap();
        }
    }
}
