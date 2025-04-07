using AutoMapper;
using Sistem.Application.DtoModels;
using USBS.Common.Domain;

namespace Sistem.Application.Mappings
{
    public class DtoMappingProfile : Profile
    {
        public DtoMappingProfile()
        {
            CreateMap<SistemParametre, SistemParametreDto>().ReverseMap();
        }
    }
}
