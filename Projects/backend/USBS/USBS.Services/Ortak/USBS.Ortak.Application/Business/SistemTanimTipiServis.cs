using Ortak.Application.Contract;
using AutoMapper;
using USBS.Common.Domain;
using USBS.Common.Dtos.OrtakDtos;

namespace Ortak.Application.Business
{
    public class SistemTanimTipiServis : ISistemTanimTipiServis
    {
        private readonly ISistemTanimTipiRepository _repository;
        private readonly IMapper _mapper;

        public SistemTanimTipiServis(ISistemTanimTipiRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SistemTanimTipiDto>> GetAllActiveSistemTanimTipisAsync()
        {
            var SistemTanimTipis = await _repository.GetAktifTanimTipiListAsync();
            return _mapper.Map<IEnumerable<SistemTanimTipiDto>>(SistemTanimTipis);
        }

        public async Task<SistemTanimTipiDto> AddSistemTanimTipiAsync(SistemTanimTipiDto SistemTanimTipiDto)
        {
            try
            {
                var SistemTanimTipi = _mapper.Map<SistemTanimTipi>(SistemTanimTipiDto);
                await _repository.AddAsync(SistemTanimTipi);
            }
            catch (Exception ex)
            {
                throw;
            }
            return SistemTanimTipiDto;
        }

        public async Task<SistemTanimTipiDto> UpdateSistemTanimTipiAsync(Guid id, SistemTanimTipiDto SistemTanimTipiDto)
        {
            var SistemTanimTipi = await _repository.GetByIdAsync(id);
            if (SistemTanimTipi == null)
            {
                return SistemTanimTipiDto;
            }

            _mapper.Map(SistemTanimTipiDto, SistemTanimTipi);
            await _repository.UpdateAsync(SistemTanimTipi);
            return SistemTanimTipiDto;
        }

        public async Task<bool> DeleteSistemTanimTipiAsync(Guid id)
        {
            var SistemTanimTipi = await _repository.GetByIdAsync(id);
            if (SistemTanimTipi == null) return false;
            SistemTanimTipi.Durum = -1;
            await _repository.UpdateAsync(SistemTanimTipi);
            return true;
        }

        public async Task<SistemTanimTipiDto> GetByIdAsync(Guid id)
        {
            var SistemTanimTipi = await _repository.GetByIdAsync(id);
            return _mapper.Map<SistemTanimTipiDto>(SistemTanimTipi);
        }
    }
}
