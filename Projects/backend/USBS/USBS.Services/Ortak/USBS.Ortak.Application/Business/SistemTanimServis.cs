using Ortak.Application.Contract;
using AutoMapper;
using USBS.Common.Domain;
using USBS.Common.Dtos.OrtakDtos;

namespace Ortak.Application.Business
{
    public class SistemTanimServis : ISistemTanimServis
    {
        private readonly ISistemTanimRepository _repository;
        private readonly IMapper _mapper;

        public SistemTanimServis(ISistemTanimRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SistemTanimDto>> GetAllActiveSistemTanimsAsync()
        {
            var SistemTanims = await _repository.GetAktifTanimListAsync();
            return _mapper.Map<IEnumerable<SistemTanimDto>>(SistemTanims);
        }

        public async Task<SistemTanimDto> AddSistemTanimAsync(SistemTanimDto SistemTanimDto)
        {
            var SistemTanim = _mapper.Map<SistemTanim>(SistemTanimDto);
            await _repository.AddAsync(SistemTanim);
            return SistemTanimDto;
        }

        public async Task<SistemTanimDto> UpdateSistemTanimAsync(Guid id, SistemTanimDto SistemTanimDto)
        {
            var SistemTanim = await _repository.GetByIdAsync(id);
            if (SistemTanim == null)
            {
                return SistemTanimDto;
            }

            _mapper.Map(SistemTanimDto, SistemTanim);
            await _repository.UpdateAsync(SistemTanim);
            return SistemTanimDto;
        }

        public async Task<bool> DeleteSistemTanimAsync(Guid id)
        {
            var SistemTanim = await _repository.GetByIdAsync(id);
            if (SistemTanim == null) return false;
            SistemTanim.Durum = -1;
            await _repository.UpdateAsync(SistemTanim);
            return true;
        }

        public async Task<SistemTanimDto> GetByIdAsync(Guid id)
        {
            var SistemTanim = await _repository.GetByIdAsync(id);
            return _mapper.Map<SistemTanimDto>(SistemTanim);
        }
    }
}
