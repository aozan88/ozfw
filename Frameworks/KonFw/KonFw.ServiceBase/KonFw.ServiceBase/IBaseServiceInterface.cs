using KonFwBaseModels;

namespace KonFw.ServiceBase
{
    public interface IBaseServiceInterface<DtoType, RequestDtoType, ResponseDtoType>
        where DtoType : BaseDto
    {
        // Get Operations
        Task<BaseResponseDto<ResponseDtoType>> GetByIdAsync(Guid uid);
        Task<BaseResponseDto<ResponseDtoType>> GetByFilterAsync(BaseRequestDto<RequestDtoType> baseRequestDto);
        Task<BaseResponseDto<ResponseDtoType>> GetAllAsync(BaseRequestDto<RequestDtoType> baseRequestDto, bool useSplitQuery = false, bool includeDeleted = false);
        Task<List<DtoType>> GetAllAsync();

        // Create Operations
        Task<BaseResponseDto<ResponseDtoType>> CreateAsync(BaseRequestDto<RequestDtoType> baseRequestDto);
        Task<BaseResponseDto<ResponseDtoType>> CreateManyAsync(List<DtoType> dtoList);

        // Update Operations
        Task<BaseResponseDto<ResponseDtoType>> UpdateAsync(BaseRequestDto<RequestDtoType> baseRequestDto);
        Task<BaseResponseDto<ResponseDtoType>> UpdateManyAsync(List<DtoType> dtoList);

        // Delete Operations (ISoftDeletable entity'lerde soft-delete)
        Task<BaseResponseDto<ResponseDtoType>> DeleteAsync(Guid uid);
        Task<BaseResponseDto<ResponseDtoType>> DeleteByFilterAsync(BaseRequestDto<RequestDtoType> baseRequestDto);
    }
}
