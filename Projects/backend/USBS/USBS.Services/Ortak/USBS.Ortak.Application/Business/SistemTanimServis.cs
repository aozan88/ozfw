using Ortak.Application.Contract;
using AutoMapper;
using USBS.Common.Domain;
using USBS.Common.Dtos.OrtakDtos;
using KonFwBaseModels;
using KonFw.ServiceBase;


namespace Ortak.Application.Business
{
    public class SistemTanimServis : ISistemTanimServis
    {
        private readonly ISistemTanimRepository _repository;
        private readonly IMapper _mapper;

       
        public SistemTanimServis(ISistemTanimRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<SistemTanimDto> GetByIdAsync(Guid id)
        {
            var SistemTanim = await _repository.GetByIdAsync(id);
            var SistemTanimDto = _mapper.Map<SistemTanimDto>(SistemTanim);

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
                    ObjectValue = new[] { _mapper.Map<SistemTanimDto>(entity) }
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
            var dtoList = _mapper.Map<IEnumerable<SistemTanimDto>>(entityList);

            return new BaseResponseDto<SistemTanimDto>
            {
                ObjectValue = dtoList,
                IsSuccess = true
            };
        }

        public async Task<BaseResponseDto<List<SistemTanimDto>>> GetAllAsync(BaseRequestDto<SistemTanimDto> baseRequestDto)
        {
            var allEntities = await _repository.GetAllAsync();
            var dtoList = _mapper.Map<List<SistemTanimDto>>(allEntities);

            return new BaseResponseDto<List<SistemTanimDto>>
            {
                ObjectValue = new []{ dtoList },
                IsSuccess = true
            };
        }

        public async Task<List<SistemTanimDto>> GetAllAsync()
        {
            var allEntities = await _repository.GetAllAsync();
            return _mapper.Map<List<SistemTanimDto>>(allEntities);
        }

        public async Task<BaseResponseDto<SistemTanimDto>> CreateAsync(BaseRequestDto<SistemTanimDto> baseRequestDto)
        {
            var entity = _mapper.Map<SistemTanim>(baseRequestDto.Payload);
            var addedEntity = await _repository.AddAsync(entity);

            var responseDto = _mapper.Map<SistemTanimDto>(addedEntity);

            return new BaseResponseDto<SistemTanimDto>
            {
                ObjectValue = new[] { responseDto },
                IsSuccess = true
            };
        }

        public async Task<HashSet<SistemTanimDto>> CreateManyAsync(HashSet<SistemTanimDto> dtoList)
        {
            var entityList = _mapper.Map<List<SistemTanim>>(dtoList);
            var resultSet = new HashSet<SistemTanimDto>();

            foreach (var entity in entityList)
            {
                var addedEntity = await _repository.AddAsync(entity);
                resultSet.Add(_mapper.Map<SistemTanimDto>(addedEntity));
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

            _mapper.Map(dto, entity);
            await _repository.UpdateAsync(entity);

            var updatedDto = _mapper.Map<SistemTanimDto>(entity);
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
                    _mapper.Map(dto, entity);
                    await _repository.UpdateAsync(entity);
                    resultSet.Add(_mapper.Map<SistemTanimDto>(entity));
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
