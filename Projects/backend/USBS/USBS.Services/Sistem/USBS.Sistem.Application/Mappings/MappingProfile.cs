using AutoMapper;
using Sistem.Application.DtoModels;
using USBS.Common.Domain;

namespace Sistem.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<SistemParametre, SistemParametreDto>().ReverseMap();
        }
    }
}
