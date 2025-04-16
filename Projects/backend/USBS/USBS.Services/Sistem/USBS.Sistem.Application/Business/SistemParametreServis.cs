using Sistem.Application.Contract;
using Sistem.Application.DtoModels;
using Mapster;
using USBS.Common.Domain;

namespace Sistem.Application.Business
{
    public class SistemParametreServis : ISistemParametreServis
    {
        private readonly ISistemParametreRepository _repository;

        public SistemParametreServis(ISistemParametreRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<SistemParametreDto>> GetAllActiveSistemParametresAsync()
        {
            var SistemParametres = await _repository.GetActiveSistemParametresAsync();
            return SistemParametres.Adapt<IEnumerable<SistemParametreDto>>();
        }

        public async Task<SistemParametreDto> AddSistemParametreAsync(SistemParametreDto SistemParametreDto)
        {
            var SistemParametre = SistemParametreDto.Adapt<SistemParametre>();
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

            SistemParametreDto.Adapt(SistemParametre);
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
            return SistemParametre.Adapt<SistemParametreDto>();
        }
    }
}
