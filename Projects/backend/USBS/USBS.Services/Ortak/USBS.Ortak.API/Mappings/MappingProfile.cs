using AutoMapper;
using USBS.Common.Dtos.OrtakDtos;
using Ortak.API.ViewModels;

namespace Ortak.API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<SistemTanimVM, SistemTanimDto>().ReverseMap();
            CreateMap<SistemTanimTipiVM, SistemTanimTipiDto>().ReverseMap();
        }
    }
}
