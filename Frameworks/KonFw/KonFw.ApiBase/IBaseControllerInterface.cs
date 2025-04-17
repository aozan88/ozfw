using KonFwBaseModels;
using Microsoft.AspNetCore.Mvc;

namespace KonFw.ApiBase
{
    public interface IBaseControllerInterface<DtoType, RequestDtoType, ResponseDtoType>
        where DtoType : BaseDto
    {
        // Response creation for reusability.
        Task<ActionResult<T>> CreateResponseAsync<T>(T responseObject, int status);

        // Get Operations
        Task<ActionResult<DtoType>> GetByIdAsync(Guid uid);
        Task<ActionResult<BaseResponseDto<ResponseDtoType>>> GetByFilterAsync(BaseRequestDto<RequestDtoType> baseRequestDto);
        Task<ActionResult<List<DtoType>>> GetAllAsync();

        // Create Operations
        Task<ActionResult<BaseResponseDto<ResponseDtoType>>> CreateAsync(BaseRequestDto<RequestDtoType> baseRequestDto);
        Task<ActionResult<HashSet<DtoType>>> CreateManyAsync(HashSet<DtoType> dtoList);

        // Update Operations
        Task<ActionResult<BaseResponseDto<ResponseDtoType>>> UpdateAsync(BaseRequestDto<RequestDtoType> baseRequestDto);
        Task<ActionResult<HashSet<DtoType>>> UpdateManyAsync(HashSet<DtoType> dtoList);

        // Delete Operations
        Task<ActionResult> DeleteAsync(Guid uid);
        Task<ActionResult<BaseResponseDto<ResponseDtoType>>> DeleteByFilterAsync(BaseRequestDto<RequestDtoType> baseRequestDto);
    }
}
