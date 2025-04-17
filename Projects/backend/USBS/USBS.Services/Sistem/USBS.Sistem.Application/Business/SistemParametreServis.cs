using Sistem.Application.Contract;
using Sistem.Application.DtoModels;
using Mapster;
using USBS.Common.Domain;
using KonFwBaseModels;
using USBS.Common.Dtos.OrtakDtos;

namespace Sistem.Application.Business
{
    public class SistemParametreServis : ISistemParametreServis
    {
        private readonly ISistemParametreRepository _repository;

        public SistemParametreServis(ISistemParametreRepository repository)
        {
            _repository = repository;
        }

        public async Task<SistemParametreDto> GetByIdAsync(Guid id)
        {
            var SistemParametre = await _repository.GetByIdAsync(id);
            var sistemParametreDto = SistemParametre.Adapt<SistemParametreDto>();

            return sistemParametreDto;
        }


        public async Task<BaseResponseDto<SistemParametreDto>> DeleteByFilterAsync(BaseRequestDto<SistemParametreDto> baseRequestDto)
        {
            // Filtreye göre silme işlemi burada örnek olarak sadece ID alındı
            var dto = baseRequestDto.Payload;
            var entity = await _repository.GetByIdAsync(dto.Id);
            if (entity != null)
            {
                entity.Durum = -1;
                await _repository.UpdateAsync(entity);

                return new BaseResponseDto<SistemParametreDto>
                {
                    IsSuccess = true,
                    ObjectValue = new[] { entity.Adapt<SistemParametreDto>() }
                };
            }

            return new BaseResponseDto<SistemParametreDto>
            {
                IsSuccess = false
            };
        }

        public async Task<BaseResponseDto<SistemParametreDto>> GetByFilterAsync(BaseRequestDto<SistemParametreDto> baseRequestDto)
        {

            var entityList = await _repository.GetActiveSistemParametresAsync();
            var dtoList = entityList.Adapt<IEnumerable<SistemParametreDto>>();

            return new BaseResponseDto<SistemParametreDto>
            {
                ObjectValue = dtoList,
                IsSuccess = true
            };
        }

        public async Task<BaseResponseDto<List<SistemParametreDto>>> GetAllAsync(BaseRequestDto<SistemParametreDto> baseRequestDto)
        {
            var allEntities = await _repository.GetAllAsync();
            var dtoList = allEntities.Adapt<List<SistemParametreDto>>();

            return new BaseResponseDto<List<SistemParametreDto>>
            {
                ObjectValue = new[] { dtoList },
                IsSuccess = true
            };
        }

        public async Task<List<SistemParametreDto>> GetAllAsync()
        {
            var allEntities = await _repository.GetAllAsync(); 

            return allEntities.Adapt<List<SistemParametreDto>>();
        }

        public async Task<BaseResponseDto<SistemParametreDto>> CreateAsync(BaseRequestDto<SistemParametreDto> baseRequestDto)
        {
            var entity = baseRequestDto.Payload.Adapt<SistemParametre>();
            var addedEntity = await _repository.AddAsync(entity);
            var responseDto = addedEntity.Adapt<SistemParametreDto>();

            return new BaseResponseDto<SistemParametreDto>
            {
                ObjectValue = new[] { responseDto },
                IsSuccess = true
            };
        }

        public async Task<SistemParametreDto> AddSistemParametreAsync(SistemParametreDto SistemParametreDto)
        {
            var SistemParametre = SistemParametreDto.Adapt<SistemParametre>();
            await _repository.AddAsync(SistemParametre);
            return SistemParametreDto;
        }
        public async Task<HashSet<SistemParametreDto>> CreateManyAsync(HashSet<SistemParametreDto> dtoList)
        {
            var entityList = dtoList.Adapt<List<SistemParametre>>();
            var resultSet = new HashSet<SistemParametreDto>();

            foreach (var entity in entityList)
            {
                var addedEntity = await _repository.AddAsync(entity);
                resultSet.Add(addedEntity.Adapt<SistemParametreDto>());
            }

            return resultSet;
        }

        public async Task<BaseResponseDto<SistemParametreDto>> UpdateAsync(BaseRequestDto<SistemParametreDto> baseRequestDto)
        {
            var dto = baseRequestDto.Payload;
            var entity = await _repository.GetByIdAsync(dto.Id);

            if (entity == null)
            {
                return new BaseResponseDto<SistemParametreDto>
                {
                    IsSuccess = false,
                    Mesaj = "Güncellenecek kayıt bulunamadı."
                };
            }

            dto.Adapt(entity);
            await _repository.UpdateAsync(entity);

            var updatedDto = entity.Adapt<SistemParametreDto>();
            return new BaseResponseDto<SistemParametreDto>
            {
                ObjectValue = new[] { updatedDto },
                IsSuccess = true
            };
        }

        public async Task<HashSet<SistemParametreDto>> UpdateManyAsync(HashSet<SistemParametreDto> dtoList)
        {
            var resultSet = new HashSet<SistemParametreDto>();

            foreach (var dto in dtoList)
            {
                var entity = await _repository.GetByIdAsync(dto.Id);
                if (entity != null)
                {
                    dto.Adapt(entity);
                    await _repository.UpdateAsync(entity);
                    resultSet.Add(entity.Adapt<SistemParametreDto>());
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
