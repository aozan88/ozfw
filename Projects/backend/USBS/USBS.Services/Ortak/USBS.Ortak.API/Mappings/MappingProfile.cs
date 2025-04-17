using AutoMapper;
using USBS.Common.Dtos.OrtakDtos;
using Ortak.API.ViewModels;
using USBS.Common.Domain;

namespace Ortak.API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<SistemTanimVM, SistemTanimDto>().ReverseMap();
            CreateMap<SistemTanimDto, SistemTanim>().ReverseMap();

            CreateMap<SistemTanimTipiVM, SistemTanimTipiDto>().ReverseMap();
            CreateMap<SistemTanimTipiDto, SistemTanimTipi>().ReverseMap();

            CreateMap<BaseRequestDto<SistemTanimVM>, BaseRequestDto<SistemTanimDto>>().ReverseMap();
            CreateMap<BaseRequestDto<SistemTanimTipiVM>, BaseRequestDto<SistemTanimTipiDto>>().ReverseMap();



        }
    }
}
