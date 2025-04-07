using USBS.Common.Dtos.OrtakDtos;

namespace Ortak.Application.Business
{
    public interface ISistemTanimServis
    {
        Task<SistemTanimDto> AddSistemTanimAsync(SistemTanimDto SistemTanimDto);
        Task<bool> DeleteSistemTanimAsync(Guid id);
        Task<IEnumerable<SistemTanimDto>> GetAllActiveSistemTanimsAsync();
        Task<SistemTanimDto> GetByIdAsync(Guid id);
        Task<SistemTanimDto> UpdateSistemTanimAsync(Guid id, SistemTanimDto SistemTanimDto);
    }
}