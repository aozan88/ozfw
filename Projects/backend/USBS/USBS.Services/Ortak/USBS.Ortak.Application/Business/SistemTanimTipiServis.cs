using Ortak.Application.Contract;
using Mapster;
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
        private readonly IEventBus _eventBus;

        public SistemTanimTipiServis(ISistemTanimTipiRepository repository, IEventBus eventBus)
        {
            _repository = repository;
            _eventBus = eventBus;
        }

        public async Task<SistemTanimTipiDto> GetByIdAsync(Guid id)
        {
            var SistemTanimTipi = await _repository.GetByIdAsync(id);
            var SistemTanimTipiDto = SistemTanimTipi.Adapt<SistemTanimTipiDto>();

            return SistemTanimTipiDto;
        }

        public async Task<HashSet<SistemTanimTipiDto>> UpdateManyAsync(HashSet<SistemTanimTipiDto> dtoList)
        {
            var resultSet = new HashSet<SistemTanimTipiDto>();

            foreach (var dto in dtoList)
            {
                var entity = await _repository.GetByIdAsync(dto.Id);
                if (entity != null)
                {
                    dto.Adapt(entity);
                    await _repository.UpdateAsync(entity);
                    resultSet.Add(entity.Adapt<SistemTanimTipiDto>());
                }
            }

            return resultSet;
        }


        public async Task<BaseResponseDto<SistemTanimTipiDto>> DeleteByFilterAsync(BaseRequestDto<SistemTanimTipiDto> baseRequestDto)
        {
            // Filtreye göre silme işlemi burada örnek olarak sadece ID alındı
            var dto = baseRequestDto.Payload;
            var entity = await _repository.GetByIdAsync(dto.Id);
            if (entity != null)
            {
                entity.Durum = -1;
                await _repository.UpdateAsync(entity);

                return new BaseResponseDto<SistemTanimTipiDto>
                {
                    IsSuccess = true,
                    ObjectValue = new[] { entity.Adapt<SistemTanimTipiDto>() }
                };
            }

            return new BaseResponseDto<SistemTanimTipiDto>
            {
                IsSuccess = false
            };
        }

        public async Task<BaseResponseDto<SistemTanimTipiDto>> GetByFilterAsync(BaseRequestDto<SistemTanimTipiDto> baseRequestDto)
        {

            var entityList = await _repository.GetAktifTanimTipiListAsync();
            var dtoList = entityList.Adapt<IEnumerable<SistemTanimTipiDto>>();

            return new BaseResponseDto<SistemTanimTipiDto>
            {
                ObjectValue = dtoList,
                IsSuccess = true
            };
        }

        public async Task<BaseResponseDto<List<SistemTanimTipiDto>>> GetAllAsync(BaseRequestDto<SistemTanimTipiDto> baseRequestDto)
        {
            var allEntities = await _repository.GetAllAsync();
            var dtoList = allEntities.Adapt<List<SistemTanimTipiDto>>();

            return new BaseResponseDto<List<SistemTanimTipiDto>>
            {
                ObjectValue = new[] { dtoList },
                IsSuccess = true
            };
        }
        public async Task<List<SistemTanimTipiDto>> GetAllAsync()
        {
            var allEntities = await _repository.GetAllAsync();
            return allEntities.Adapt<List<SistemTanimTipiDto>>();
        }

        public async Task<BaseResponseDto<SistemTanimTipiDto>> CreateAsync(BaseRequestDto<SistemTanimTipiDto> baseRequestDto)
        {
            var entity = baseRequestDto.Payload.Adapt<SistemTanimTipi>();
            var addedEntity = await _repository.AddAsync(entity);

            var responseDto = addedEntity.Adapt<SistemTanimTipiDto>();

            return new BaseResponseDto<SistemTanimTipiDto>
            {
                ObjectValue = new[] { responseDto },
                IsSuccess = true
            };
        }

        public async Task<HashSet<SistemTanimTipiDto>> CreateManyAsync(HashSet<SistemTanimTipiDto> dtoList)
        {
            var entityList = dtoList.Adapt<List<SistemTanimTipi>>();
            var resultSet = new HashSet<SistemTanimTipiDto>();

            foreach (var entity in entityList)
            {
                var addedEntity = await _repository.AddAsync(entity);
                resultSet.Add(addedEntity.Adapt<SistemTanimTipiDto>());
            }

            return resultSet;
        }

        public async Task<BaseResponseDto<SistemTanimTipiDto>> UpdateAsync(BaseRequestDto<SistemTanimTipiDto> baseRequestDto)
        {
            var dto = baseRequestDto.Payload;
            var entity = await _repository.GetByIdAsync(dto.Id);

            if (entity == null)
            {
                return new BaseResponseDto<SistemTanimTipiDto>
                {
                    IsSuccess = false,
                    Mesaj = "Güncellenecek kayıt bulunamadı."
                };
            }

            dto.Adapt(entity);
            await _repository.UpdateAsync(entity);

            var updatedDto = entity.Adapt<SistemTanimTipiDto>();
            return new BaseResponseDto<SistemTanimTipiDto>
            {
                ObjectValue = new[] { updatedDto },
                IsSuccess = true
            };
        }

        public async Task<HashSet<SistemTanimTipiDto>> UpdateAsync(HashSet<SistemTanimTipiDto> dtoList)
        {
            var resultSet = new HashSet<SistemTanimTipiDto>();

            foreach (var dto in dtoList)
            {
                var entity = await _repository.GetByIdAsync(dto.Id);
                if (entity != null)
                {
                    dto.Adapt(entity);
                    await _repository.UpdateAsync(entity);
                    resultSet.Add(entity.Adapt<SistemTanimTipiDto>());
                }
            }

            return resultSet;
        }

        public async Task DeleteAsync(Guid uid)
        {
            var entity = await _repository.GetByIdAsync(uid);
            if (entity != null)
            {
                entity.Durum = -1;

                await _repository.UpdateAsync(entity);

                var @event = new SistemTanimTipiSilindiEvent
                {
                    SistemTanimTipiId = entity.Id,
                    YeniDurum = entity.Durum
                };

                await _eventBus.PublishAsync(@event);
            }
        }
    }
}
