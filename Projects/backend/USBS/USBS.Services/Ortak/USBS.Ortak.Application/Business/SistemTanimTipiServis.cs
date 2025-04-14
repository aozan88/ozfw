using Ortak.Application.Contract;
using AutoMapper;
using USBS.Common.Domain;
using USBS.Common.Dtos.OrtakDtos;
using KonFwBaseModels;
using USBS.Ortak.API.Consumers;
using KonFwEventBus;

namespace Ortak.Application.Business
{
    public class SistemTanimTipiServis : ISistemTanimTipiServis
    {
        private readonly ISistemTanimTipiRepository _repository;
        private readonly IMapper _mapper;
        private readonly IEventBus _eventBus;

        public SistemTanimTipiServis(ISistemTanimTipiRepository repository, IMapper mapper, IEventBus eventBus)
        {
            _repository = repository;
            _mapper = mapper;
            _eventBus = eventBus;
        }

        public async Task<BaseResponseDto<SistemTanimTipiDto>> GetAllActiveSistemTanimTipisAsync()
        {
            var entityList = await _repository.GetAktifTanimTipiListAsync();

            if (entityList == null)
            {
                return new BaseResponseDto<SistemTanimTipiDto>
                {
                    IsSuccess = false,
                    Mesaj = "Hiçbir aktif sistem tanımı bulunamadı."
                };
            }

            var dtoList = _mapper.Map<IEnumerable<SistemTanimTipiDto>>(entityList);

            return new BaseResponseDto<SistemTanimTipiDto>
            {
                ObjectValue = dtoList,
                IsSuccess = true
            };
        }

        public async Task<BaseResponseDto<SistemTanimTipiDto>> AddSistemTanimTipiAsync(SistemTanimTipiDto SistemTanimTipiDto)
        {

            var entity = _mapper.Map<SistemTanimTipi>(SistemTanimTipiDto);

            var addedEntity = await _repository.AddAsync(entity);
            if (addedEntity == null)
            {
                return new BaseResponseDto<SistemTanimTipiDto>
                {
                    IsSuccess = false,
                    Mesaj = "Veritabanına kayıt eklenemedi."
                };
            }

            var responseDto = _mapper.Map<SistemTanimTipiDto>(addedEntity);

            return new BaseResponseDto<SistemTanimTipiDto>
            {
                ObjectValue = new List<SistemTanimTipiDto> { responseDto },
                IsSuccess = true
            };
        }

        public async Task<BaseResponseDto<SistemTanimTipiDto>> UpdateSistemTanimTipiAsync(SistemTanimTipiDto SistemTanimTipiDto)
        {
            var entity = await _repository.GetByIdAsync(SistemTanimTipiDto.Id);
            if (entity == null)
            {
                return new BaseResponseDto<SistemTanimTipiDto>
                {
                    IsSuccess = false,
                    Mesaj = $"Entity with ID {SistemTanimTipiDto.Id} not found."
                };
            }

            _mapper.Map(SistemTanimTipiDto, entity);
             await _repository.UpdateAsync(entity);

            var updatedDto = _mapper.Map<SistemTanimTipiDto>(entity);

            return new BaseResponseDto<SistemTanimTipiDto>
            {
                ObjectValue = new[] { updatedDto },
                IsSuccess = true
            };
        }

        public async Task<BaseResponseDto<SistemTanimTipiDto>> DeleteSistemTanimTipiAsync(Guid id)
        {
            var SistemTanimTipi = await _repository.GetByIdAsync(id);

            if (SistemTanimTipi == null)
            {
                return new BaseResponseDto<SistemTanimTipiDto>
                {
                    IsSuccess = false,
                    ObjectValue = null
                };
            }

            SistemTanimTipi.Durum = -1;
            await _repository.UpdateAsync(SistemTanimTipi);

            

            var @event = new SistemTanimTipiSilindiEvent
            {
                SistemTanimTipiId = SistemTanimTipi.Id,
                YeniDurum = SistemTanimTipi.Durum
            };

            await _eventBus.PublishAsync(@event);

            return new BaseResponseDto<SistemTanimTipiDto>
            {
                IsSuccess = true,
                ObjectValue = new List<SistemTanimTipiDto>()
            };
        }

        public async Task<BaseResponseDto<SistemTanimTipiDto>> GetByIdAsync(Guid id)
        {
            var SistemTanimTipi = await _repository.GetByIdAsync(id);
            var SistemTanimTipiDto = _mapper.Map<SistemTanimTipiDto>(SistemTanimTipi);

            if (SistemTanimTipiDto == null)
            {
                return new BaseResponseDto<SistemTanimTipiDto>
                {
                    IsSuccess = false,
                    ObjectValue = new List<SistemTanimTipiDto>()
                };
            }

            return new BaseResponseDto<SistemTanimTipiDto>
            {
                ObjectValue = new[] { SistemTanimTipiDto },
                IsSuccess = true
            };
        }
    }
}
