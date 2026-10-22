using OzFw.BaseInfrastructure;
using OzFwBaseModels;
using Mapster;

namespace OzFw.ServiceBase
{
    /// <summary>
    /// IBaseServiceInterface'in Mapster tabanlı genel CRUD uygulaması.
    /// DTO ile entity alan adları aynıysa ek eşleme gerekmez; farklıysa MapToDto/MapToEntity override edilir
    /// ya da Mapster TypeAdapterConfig tanımlanır.
    /// </summary>
    public abstract class BaseService<TEntity, TDto> : IBaseServiceInterface<TDto, TDto, TDto>
        where TEntity : class, IEntity
        where TDto : BaseDto
    {
        protected readonly IRepository<TEntity> Repository;

        protected BaseService(IRepository<TEntity> repository)
        {
            Repository = repository;
        }

        protected virtual TDto MapToDto(TEntity entity) => entity.Adapt<TDto>();
        protected virtual TEntity MapToEntity(TDto dto) => dto.Adapt<TEntity>();
        protected virtual void MapToEntity(TDto dto, TEntity entity) => dto.Adapt(entity);

        protected static BaseResponseDto<TDto> Success(IEnumerable<TDto> items, string? mesaj = null) => new()
        {
            IsSuccess = true,
            Mesaj = mesaj ?? string.Empty,
            ObjectValue = items,
            Count = items.Count()
        };

        protected static BaseResponseDto<TDto> Fail(string mesaj) => new()
        {
            IsSuccess = false,
            Mesaj = mesaj
        };

        public virtual async Task<BaseResponseDto<TDto>> GetByIdAsync(Guid uid)
        {
            var entity = await Repository.GetByIdAsync(uid);
            if (entity == null || entity is ISoftDeletable { Durum: OzFwBaseModels.Enums.DurumEnum.Silindi })
                return Fail("Kayıt bulunamadı.");
            return Success(new[] { MapToDto(entity) });
        }

        public virtual Task<BaseResponseDto<TDto>> GetByFilterAsync(BaseRequestDto<TDto> baseRequestDto)
        {
            return GetAllAsync(baseRequestDto);
        }

        public virtual async Task<BaseResponseDto<TDto>> GetAllAsync(BaseRequestDto<TDto> baseRequestDto, bool useSplitQuery = false, bool includeDeleted = false)
        {
            var result = await Repository.GetAllAsync(baseRequestDto, useSplitQuery, includeDeleted);
            return new BaseResponseDto<TDto>
            {
                IsSuccess = result.IsSuccess,
                Mesaj = result.Mesaj,
                PageNumber = result.PageNumber,
                Size = result.Size,
                Count = result.Count,
                TotalRecordCount = result.TotalRecordCount,
                TotalTableCount = result.TotalTableCount,
                ClientId = result.ClientId,
                SortList = result.SortList,
                ObjectValue = result.ObjectValue?.Select(MapToDto).ToList() ?? new List<TDto>()
            };
        }

        public virtual async Task<List<TDto>> GetAllAsync()
        {
            var entities = await Repository.GetAllAsync();
            return entities.Select(MapToDto).ToList();
        }

        public virtual async Task<BaseResponseDto<TDto>> CreateAsync(BaseRequestDto<TDto> baseRequestDto)
        {
            if (baseRequestDto.Payload == null)
                return Fail("Payload boş olamaz.");

            var entity = MapToEntity(baseRequestDto.Payload);
            if (entity.Id == Guid.Empty)
                entity.Id = Guid.NewGuid();

            var added = await Repository.AddAsync(entity);
            return Success(new[] { MapToDto(added) });
        }

        public virtual async Task<BaseResponseDto<TDto>> CreateManyAsync(List<TDto> dtoList)
        {
            var entities = dtoList.Select(MapToEntity).ToList();
            foreach (var entity in entities.Where(e => e.Id == Guid.Empty))
                entity.Id = Guid.NewGuid();

            await Repository.AddManyAsync(entities);
            return Success(entities.Select(MapToDto).ToList());
        }

        public virtual async Task<BaseResponseDto<TDto>> UpdateAsync(BaseRequestDto<TDto> baseRequestDto)
        {
            var dto = baseRequestDto.Payload;
            if (dto == null)
                return Fail("Payload boş olamaz.");

            var entity = await Repository.GetByIdAsync(dto.Id);
            if (entity == null)
                return Fail("Güncellenecek kayıt bulunamadı.");

            MapToEntity(dto, entity);
            await Repository.UpdateAsync(entity);
            return Success(new[] { MapToDto(entity) });
        }

        public virtual async Task<BaseResponseDto<TDto>> UpdateManyAsync(List<TDto> dtoList)
        {
            var updated = new List<TEntity>();
            var notFound = new List<Guid>();

            foreach (var dto in dtoList)
            {
                var entity = await Repository.GetByIdAsync(dto.Id);
                if (entity == null)
                {
                    notFound.Add(dto.Id);
                    continue;
                }
                MapToEntity(dto, entity);
                updated.Add(entity);
            }

            if (updated.Count > 0)
                await Repository.UpdateManyAsync(updated);

            var response = Success(updated.Select(MapToDto).ToList());
            if (notFound.Count > 0)
                response.Mesaj = $"{notFound.Count} kayıt bulunamadı: {string.Join(", ", notFound)}";
            return response;
        }

        public virtual async Task<BaseResponseDto<TDto>> DeleteAsync(Guid uid)
        {
            var deleted = await Repository.DeleteAsync(uid);
            return deleted
                ? Success(Array.Empty<TDto>(), "Kayıt silindi.")
                : Fail("Silinecek kayıt bulunamadı.");
        }

        public virtual async Task<BaseResponseDto<TDto>> DeleteByFilterAsync(BaseRequestDto<TDto> baseRequestDto)
        {
            // Filtresiz toplu silmeye izin verilmez
            if (baseRequestDto.Filter == null)
                return Fail("Toplu silme için filtre zorunludur.");

            var matches = await Repository.GetAllAsync(new BaseRequestDto<TDto>
            {
                ClientId = baseRequestDto.ClientId,
                Filter = baseRequestDto.Filter,
                Page = 1,
                Size = -1
            });

            var ids = matches.ObjectValue?.Select(e => e.Id).ToList() ?? new List<Guid>();
            if (ids.Count == 0)
                return Success(Array.Empty<TDto>(), "Filtreye uyan kayıt yok.");

            var count = await Repository.DeleteByFilterAsync(e => ids.Contains(e.Id));
            return Success(Array.Empty<TDto>(), $"{count} kayıt silindi.");
        }
    }
}
