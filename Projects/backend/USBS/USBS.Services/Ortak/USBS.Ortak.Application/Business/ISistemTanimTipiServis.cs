using KonFwBaseModels;
using USBS.Common.Dtos.OrtakDtos;

namespace Ortak.Application.Business
{
    public interface ISistemTanimTipiServis
    {
        Task<BaseResponseDto<SistemTanimTipiDto>> AddSistemTanimTipiAsync(SistemTanimTipiDto SistemTanimTipiDto);
        Task<BaseResponseDto<SistemTanimTipiDto>> DeleteSistemTanimTipiAsync(Guid id);
        Task<BaseResponseDto<SistemTanimTipiDto>> GetAllActiveSistemTanimTipisAsync();
        Task<BaseResponseDto<SistemTanimTipiDto>> GetByIdAsync(Guid id);
        Task<BaseResponseDto<SistemTanimTipiDto>> UpdateSistemTanimTipiAsync(SistemTanimTipiDto SistemTanimTipiDto);
    }
}