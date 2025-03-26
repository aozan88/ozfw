using AutoMapper;
using Ortak.Application.DtoModels;
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
