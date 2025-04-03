using AutoMapper;
using USBS.Common.Dtos.OrtakDtos;
using USBS.Common.Domain;

namespace Ortak.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Kod, KodDto>().ReverseMap();
        }
    }
}
