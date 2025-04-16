using Ortak.Application.Contract;
using Mapster;
using USBS.Common.Domain;
using USBS.Common.Dtos.OrtakDtos;
using KonFwBaseModels;
using KonFw.ServiceBase;


namespace Ortak.Application.Business
{
    public class SistemTanimServis : ISistemTanimServis
    {
        private readonly ISistemTanimRepository _repository;


        public SistemTanimServis(ISistemTanimRepository repository)
        {
            _repository = repository;
        }

        public async Task<SistemTanimDto> GetByIdAsync(Guid id)
        {
            var SistemTanim = await _repository.GetByIdAsync(id);
            var SistemTanimDto = SistemTanim.Adapt<SistemTanimDto>();

            return SistemTanimDto;
        }

        public async Task<BaseResponseDto<SistemTanimDto>> DeleteByFilterAsync(BaseRequestDto<SistemTanimDto> baseRequestDto)
        {
            // Filtreye göre silme işlemi burada örnek olarak sadece ID alındı
            var dto = baseRequestDto.Payload;
            var entity = await _repository.GetByIdAsync(dto.Id);
            if (entity != null)
            {
                entity.Durum = -1;
                await _repository.UpdateAsync(entity);

                return new BaseResponseDto<SistemTanimDto>
                {
                    IsSuccess = true,
                    ObjectValue = new[] { entity.Adapt<SistemTanimDto>() }
                };
            }

            return new BaseResponseDto<SistemTanimDto>
            {
                IsSuccess = false
            };
        }

        public async Task<BaseResponseDto<SistemTanimDto>> GetByFilterAsync(BaseRequestDto<SistemTanimDto> baseRequestDto)
        {

            var entityList = await _repository.GetAktifTanimListAsync();
            var dtoList = entityList.Adapt<IEnumerable<SistemTanimDto>>();

            return new BaseResponseDto<SistemTanimDto>
            {
                ObjectValue = dtoList,
                IsSuccess = true
            };
        }

        public async Task<BaseResponseDto<List<SistemTanimDto>>> GetAllAsync(BaseRequestDto<SistemTanimDto> baseRequestDto)
        {
            var allEntities = await _repository.GetAllAsync();
            var dtoList = allEntities.Adapt<List<SistemTanimDto>>();

            return new BaseResponseDto<List<SistemTanimDto>>
            {
                ObjectValue = new[] { dtoList },
                IsSuccess = true
            };
        }

        public async Task<List<SistemTanimDto>> GetAllAsync()
        {
            var allEntities = await _repository.GetAllAsync();
            return allEntities.Adapt<List<SistemTanimDto>>();
        }

        public async Task<BaseResponseDto<SistemTanimDto>> CreateAsync(BaseRequestDto<SistemTanimDto> baseRequestDto)
        {
            var entity = baseRequestDto.Payload.Adapt<SistemTanim>();
            var addedEntity = await _repository.AddAsync(entity);

            var responseDto = addedEntity.Adapt<SistemTanimDto>();

            return new BaseResponseDto<SistemTanimDto>
            {
                ObjectValue = new[] { responseDto },
                IsSuccess = true
            };
        }

        public async Task<HashSet<SistemTanimDto>> CreateManyAsync(HashSet<SistemTanimDto> dtoList)
        {
            var entityList = dtoList.Adapt<List<SistemTanim>>();
            var resultSet = new HashSet<SistemTanimDto>();

            foreach (var entity in entityList)
            {
                var addedEntity = await _repository.AddAsync(entity);
                resultSet.Add(addedEntity.Adapt<SistemTanimDto>());
            }

            return resultSet;
        }

        public async Task<BaseResponseDto<SistemTanimDto>> UpdateAsync(BaseRequestDto<SistemTanimDto> baseRequestDto)
        {
            var dto = baseRequestDto.Payload;
            var entity = await _repository.GetByIdAsync(dto.Id);

            if (entity == null)
            {
                return new BaseResponseDto<SistemTanimDto>
                {
                    IsSuccess = false,
                    Mesaj = "Güncellenecek kayıt bulunamadı."
                };
            }

            dto.Adapt(entity);
            await _repository.UpdateAsync(entity);

            var updatedDto = entity.Adapt<SistemTanimDto>();
            return new BaseResponseDto<SistemTanimDto>
            {
                ObjectValue = new[] { updatedDto },
                IsSuccess = true
            };
        }

        public async Task<HashSet<SistemTanimDto>> UpdateManyAsync(HashSet<SistemTanimDto> dtoList)
        {
            var resultSet = new HashSet<SistemTanimDto>();

            foreach (var dto in dtoList)
            {
                var entity = await _repository.GetByIdAsync(dto.Id);
                if (entity != null)
                {
                    dto.Adapt(entity);
                    await _repository.UpdateAsync(entity);
                    resultSet.Add(entity.Adapt<SistemTanimDto>());
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
            }
        }
    }
}
