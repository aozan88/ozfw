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
        Task<ActionResult<BaseResponseDto<ResponseDtoType>>> GetByIdAsync(BaseGetByIdDto baseGetByIdDto);
        Task<ActionResult<BaseResponseDto<ResponseDtoType>>> GetAllAsync(BaseRequestDto<RequestDtoType> baseRequestDto);

        // Create Operations
        Task<ActionResult<BaseResponseDto<ResponseDtoType>>> CreateAsync(BaseRequestDto<RequestDtoType> baseRequestDto);
        Task<ActionResult<BaseResponseDto<ResponseDtoType>>> CreateManyAsync(List<DtoType> dtoList);

        // Update Operations
        Task<ActionResult<BaseResponseDto<ResponseDtoType>>> UpdateAsync(BaseRequestDto<RequestDtoType> baseRequestDto);
        Task<ActionResult<BaseResponseDto<ResponseDtoType>>> UpdateManyAsync(List<DtoType> dtoList);

        // Delete Operations
        Task<ActionResult<BaseResponseDto<ResponseDtoType>>> DeleteAsync(BaseGetByIdDto baseGetByIdDto);
        Task<ActionResult<BaseResponseDto<ResponseDtoType>>> DeleteByFilterAsync(BaseRequestDto<RequestDtoType> baseRequestDto);
    }
}
