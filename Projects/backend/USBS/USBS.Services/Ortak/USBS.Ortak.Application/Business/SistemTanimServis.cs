using Ortak.Application.Contract;
using AutoMapper;
using USBS.Common.Domain;
using USBS.Common.Dtos.OrtakDtos;
using KonFwBaseModels;


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

        public async Task<BaseResponseDto<SistemTanimDto>> GetAllActiveSistemTanimsAsync()
        {
            var entityList = await _repository.GetAktifTanimListAsync();

            if (entityList == null)
            {
                return new BaseResponseDto<SistemTanimDto>
                {
                    IsSuccess = false,
                    Mesaj = "Hiçbir aktif sistem tanımı bulunamadı."
                };
            }

            var dtoList = _mapper.Map<IEnumerable<SistemTanimDto>>(entityList);

            return new BaseResponseDto<SistemTanimDto>
            {
                ObjectValue = dtoList,
                IsSuccess = true
            };
        }

        public async Task<BaseResponseDto<SistemTanimDto>> AddSistemTanimAsync(SistemTanimDto SistemTanimDto)
        {
            var entity = _mapper.Map<SistemTanim>(SistemTanimDto);

            var addedEntity = await _repository.AddAsync(entity);
            if (addedEntity == null)
            {
                return new BaseResponseDto<SistemTanimDto>
                {
                    IsSuccess = false,
                    Mesaj = "Veritabanına kayıt eklenemedi."
                };
            }

            var responseDto = _mapper.Map<SistemTanimDto>(addedEntity);

            return new BaseResponseDto<SistemTanimDto>
            {
                ObjectValue = new List<SistemTanimDto> { responseDto },
                IsSuccess = true
            };
        }

        public async Task<BaseResponseDto<SistemTanimDto>> UpdateSistemTanimAsync(SistemTanimDto SistemTanimDto)
        {
            var entity = await _repository.GetByIdAsync(SistemTanimDto.Id);
            if (entity == null)
            {
                return new BaseResponseDto<SistemTanimDto>
                {
                    IsSuccess = false,
                    Mesaj = $"Entity with ID {SistemTanimDto.Id} not found."
                };
            }

            _mapper.Map(SistemTanimDto, entity);
            await _repository.UpdateAsync(entity);

            var updatedDto = _mapper.Map<SistemTanimDto>(entity);

            return new BaseResponseDto<SistemTanimDto>
            {
                ObjectValue = new[] { updatedDto },
                IsSuccess = true
            };
        }

        public async Task<BaseResponseDto<SistemTanimDto>> DeleteSistemTanimAsync(Guid id)
        {
            var sistemTanim = await _repository.GetByIdAsync(id);

            if (sistemTanim == null)
            {
                return new BaseResponseDto<SistemTanimDto>
                {
                    IsSuccess = false,
                    ObjectValue = null
                };
            }

            sistemTanim.Durum = -1;
            await _repository.UpdateAsync(sistemTanim);

            return new BaseResponseDto<SistemTanimDto>
            {
                IsSuccess = true,
                ObjectValue = new List<SistemTanimDto>() 
            };
        }

        public async Task<BaseResponseDto<SistemTanimDto>> GetByIdAsync(Guid id)
        {
            var SistemTanim = await _repository.GetByIdAsync(id);
            var SistemTanimDto = _mapper.Map<SistemTanimDto>(SistemTanim);

            if (SistemTanimDto == null)
            {
                return new BaseResponseDto<SistemTanimDto>
                {
                    IsSuccess = false,
                    ObjectValue = new List<SistemTanimDto>()
                };
            }

            return new BaseResponseDto<SistemTanimDto>
            {
                ObjectValue = new[] { SistemTanimDto },
                IsSuccess = true
            };
        }
    }
}
