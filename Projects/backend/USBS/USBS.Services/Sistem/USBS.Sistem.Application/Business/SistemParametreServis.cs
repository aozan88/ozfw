using Sistem.Application.Contract;
using Sistem.Application.DtoModels;
using AutoMapper;
using USBS.Common.Domain;

namespace Sistem.Application.Business
{
    public class SistemParametreServis
    {
        private readonly ISistemParametreRepository _repository;
        private readonly IMapper _mapper;

        public SistemParametreServis(ISistemParametreRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SistemParametreDto>> GetAllActiveSistemParametresAsync()
        {
            var SistemParametres = await _repository.GetActiveSistemParametresAsync();
            return _mapper.Map<IEnumerable<SistemParametreDto>>(SistemParametres);
        }

        public async Task<SistemParametreDto> AddSistemParametreAsync(SistemParametreDto SistemParametreDto)
        {
            var SistemParametre = _mapper.Map<SistemParametre>(SistemParametreDto);
            await _repository.AddAsync(SistemParametre);
            return SistemParametreDto;
        }

        public async Task<SistemParametreDto> UpdateSistemParametreAsync(Guid id, SistemParametreDto SistemParametreDto)
        {
            var SistemParametre = await _repository.GetByIdAsync(id);
            if (SistemParametre == null)
            {
                return null;
            }

            _mapper.Map(SistemParametreDto, SistemParametre);
            await _repository.UpdateAsync(SistemParametre);
            return SistemParametreDto;
        }

        public async Task<bool> DeleteSistemParametreAsync(Guid id)
        {
            var SistemParametre = await _repository.GetByIdAsync(id);
            if (SistemParametre == null) return false;
            SistemParametre.Durum = -1;
            await _repository.UpdateAsync(SistemParametre);
            return true;
        }

        public async Task<SistemParametreDto> GetByIdAsync(Guid id)
        {
            var SistemParametre = await _repository.GetByIdAsync(id);
            return _mapper.Map<SistemParametreDto>(SistemParametre);
        }
    }
}
