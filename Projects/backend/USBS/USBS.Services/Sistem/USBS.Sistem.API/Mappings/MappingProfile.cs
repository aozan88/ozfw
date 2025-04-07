using AutoMapper;
using Sistem.API.ViewModels;
using Sistem.Application.DtoModels;

namespace Sistem.API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<SistemParametreVM, SistemParametreDto>().ReverseMap();
        }
    }
}
