using AutoMapper;
using KonFwBaseModels;
using Sistem.API.ViewModels;
using Sistem.Application.DtoModels;
using USBS.Common.Domain;
using USBS.Common.Dtos.OrtakDtos;

namespace Sistem.API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<SistemParametreVM, SistemParametreDto>().ReverseMap();
            CreateMap<SistemParametreDto, SistemParametre>().ReverseMap();

            CreateMap<BaseRequestDto<SistemParametreVM>, BaseRequestDto<SistemParametreDto>>().ReverseMap();
        }
    }
}
