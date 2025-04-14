using KonFwBaseModels;

namespace KonFw.ServiceBase
{
    public interface IBaseServiceInterface<DtoType, RequestDtoType, ResponseDtoType>
        where DtoType : BaseDto
    {
        // Get Operations
        Task<DtoType> GetByIdAsync(Guid uid);
        Task<BaseResponseDto<ResponseDtoType>> GetByFilterAsync(BaseRequestDto<RequestDtoType> baseRequestDto);
        Task<BaseResponseDto<List<ResponseDtoType>>> GetAllAsync(BaseRequestDto<RequestDtoType> baseRequestDto);
        Task<List<DtoType>> GetAllAsync();

        // Create Operations
        Task<BaseResponseDto<ResponseDtoType>> CreateAsync(BaseRequestDto<RequestDtoType> baseRequestDto);
        Task<HashSet<DtoType>> CreateAsync(HashSet<DtoType> dtoList);

        // Update Operations
        Task<BaseResponseDto<ResponseDtoType>> UpdateAsync(BaseRequestDto<RequestDtoType> baseRequestDto);
        Task<HashSet<DtoType>> UpdateAsync(HashSet<DtoType> dtoList);

        // Delete Operations
        Task DeleteAsync(Guid uid);
        Task<BaseResponseDto<ResponseDtoType>> DeleteByFilterAsync(BaseRequestDto<RequestDtoType> baseRequestDto);
    }
}
