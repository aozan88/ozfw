using KonFwBaseModels;
using USBS.Common.Domain;
using USBS.Common.Dtos.OrtakDtos;

namespace Ortak.Application.Business
{
    public interface ISistemTanimServis
    {
        Task<BaseResponseDto<SistemTanimDto>> AddSistemTanimAsync(SistemTanimDto SistemTanimDto);
        Task<BaseResponseDto<SistemTanimDto>> DeleteSistemTanimAsync(Guid id);
        Task<BaseResponseDto<SistemTanimDto>> GetAllActiveSistemTanimsAsync();
        Task<BaseResponseDto<SistemTanimDto>> GetByIdAsync(Guid id);
        Task<BaseResponseDto<SistemTanimDto>> UpdateSistemTanimAsync(SistemTanimDto SistemTanimDto);
    }
}