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
            var SistemParametres = await _repository.GetActiveSistemParametresAsync();
            return SistemParametres.Adapt<IEnumerable<SistemParametreDto>>();
            var SistemParametre = await _repository.GetByIdAsync(id);
            var SistemParametreDto = _mapper.Map<SistemParametreDto>(SistemParametre);

            return SistemParametreDto;
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
                    ObjectValue = new[] { _mapper.Map<SistemParametreDto>(entity) }
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
            var dtoList = _mapper.Map<IEnumerable<SistemParametreDto>>(entityList);

            return new BaseResponseDto<SistemParametreDto>
            {
                ObjectValue = dtoList,
                IsSuccess = true
            };
        }

        public async Task<BaseResponseDto<List<SistemParametreDto>>> GetAllAsync(BaseRequestDto<SistemParametreDto> baseRequestDto)
        {
            var allEntities = await _repository.GetAllAsync();
            var dtoList = _mapper.Map<List<SistemParametreDto>>(allEntities);

            return new BaseResponseDto<List<SistemParametreDto>>
            {
                ObjectValue = new[] { dtoList },
                IsSuccess = true
            };
        }

        public async Task<List<SistemParametreDto>> GetAllAsync()
        {
            var allEntities = await _repository.GetAllAsync();
            return _mapper.Map<List<SistemParametreDto>>(allEntities);
        }

        public async Task<BaseResponseDto<SistemParametreDto>> CreateAsync(BaseRequestDto<SistemParametreDto> baseRequestDto)
        {
            var entity = _mapper.Map<SistemParametre>(baseRequestDto.Payload);
            var addedEntity = await _repository.AddAsync(entity);

            var responseDto = _mapper.Map<SistemParametreDto>(addedEntity);

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
        public async Task<HashSet<SistemParametreDto>> CreateManyAsync(HashSet<SistemParametreDto> dtoList)
        {
            var entityList = _mapper.Map<List<SistemParametre>>(dtoList);
            var resultSet = new HashSet<SistemParametreDto>();

            foreach (var entity in entityList)
            {
                var addedEntity = await _repository.AddAsync(entity);
                resultSet.Add(_mapper.Map<SistemParametreDto>(addedEntity));
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

            _mapper.Map(dto, entity);
            await _repository.UpdateAsync(entity);

            var updatedDto = _mapper.Map<SistemParametreDto>(entity);
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
                    _mapper.Map(dto, entity);
                    await _repository.UpdateAsync(entity);
                    resultSet.Add(_mapper.Map<SistemParametreDto>(entity));
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
