using Sistem.Application.Contract;
using Sistem.Application.DtoModels;
using AutoMapper;
using USBS.Common.Domain;

namespace Sistem.Application.Business
{
    public class SistemConfigBusinessService
    {
        private readonly ISistemConfigRepository _repository;
        private readonly IMapper _mapper;

        public SistemConfigBusinessService(ISistemConfigRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SistemConfigDto>> GetAllActiveSistemConfigsAsync()
        {
            var SistemConfigs = await _repository.GetActiveSistemConfigsAsync();
            return _mapper.Map<IEnumerable<SistemConfigDto>>(SistemConfigs);
        }

        public async Task<SistemConfigDto> AddSistemConfigAsync(SistemConfigDto SistemConfigDto)
        {
            var SistemConfig = _mapper.Map<SistemConfig>(SistemConfigDto);
            await _repository.AddAsync(SistemConfig);
            return SistemConfigDto;
        }

        public async Task<SistemConfigDto> UpdateSistemConfigAsync(Guid id, SistemConfigDto SistemConfigDto)
        {
            var SistemConfig = await _repository.GetByIdAsync(id);
            if (SistemConfig == null)
            {
                return null;
            }

            _mapper.Map(SistemConfigDto, SistemConfig);
            await _repository.UpdateAsync(SistemConfig);
            return SistemConfigDto;
        }

        public async Task<bool> DeleteSistemConfigAsync(Guid id)
        {
            var SistemConfig = await _repository.GetByIdAsync(id);
            if (SistemConfig == null) return false;
            SistemConfig.Durum = -1;
            await _repository.UpdateAsync(SistemConfig);
            return true;
        }

        public async Task<SistemConfigDto> GetByIdAsync(Guid id)
        {
            var SistemConfig = await _repository.GetByIdAsync(id);
            return _mapper.Map<SistemConfigDto>(SistemConfig);
        }
    }
}
