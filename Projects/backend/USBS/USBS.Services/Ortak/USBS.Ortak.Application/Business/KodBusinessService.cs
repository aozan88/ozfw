using Ortak.Application.Contract;
using Ortak.Application.DtoModels;
using AutoMapper;
using USBS.Common.Domain;

namespace Ortak.Application.Business
{
    public class KodBusinessService
    {
        private readonly IKodRepository _repository;
        private readonly IMapper _mapper;

        public KodBusinessService(IKodRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<KodDto>> GetAllActiveKodsAsync()
        {
            var Kods = await _repository.GetActiveKodsAsync();
            return _mapper.Map<IEnumerable<KodDto>>(Kods);
        }

        public async Task<KodDto> AddKodAsync(KodDto KodDto)
        {
            var Kod = _mapper.Map<Kod>(KodDto);
            await _repository.AddAsync(Kod);
            return KodDto;
        }

        public async Task<KodDto> UpdateKodAsync(Guid id, KodDto KodDto)
        {
            var Kod = await _repository.GetByIdAsync(id);
            if (Kod == null)
            {
                return null;
            }

            _mapper.Map(KodDto, Kod);
            await _repository.UpdateAsync(Kod);
            return KodDto;
        }

        public async Task<bool> DeleteKodAsync(Guid id)
        {
            var Kod = await _repository.GetByIdAsync(id);
            if (Kod == null) return false;
            Kod.Durum = -1;
            await _repository.UpdateAsync(Kod);
            return true;
        }

        public async Task<KodDto> GetByIdAsync(Guid id)
        {
            var Kod = await _repository.GetByIdAsync(id);
            return _mapper.Map<KodDto>(Kod);
        }
    }
}
