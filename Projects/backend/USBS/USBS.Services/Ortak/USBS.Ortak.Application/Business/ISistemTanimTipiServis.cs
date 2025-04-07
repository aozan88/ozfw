using USBS.Common.Dtos.OrtakDtos;

namespace Ortak.Application.Business
{
    public interface ISistemTanimTipiServis
    {
        Task<SistemTanimTipiDto> AddSistemTanimTipiAsync(SistemTanimTipiDto SistemTanimTipiDto);
        Task<bool> DeleteSistemTanimTipiAsync(Guid id);
        Task<IEnumerable<SistemTanimTipiDto>> GetAllActiveSistemTanimTipisAsync();
        Task<SistemTanimTipiDto> GetByIdAsync(Guid id);
        Task<SistemTanimTipiDto> UpdateSistemTanimTipiAsync(Guid id, SistemTanimTipiDto SistemTanimTipiDto);
    }
}