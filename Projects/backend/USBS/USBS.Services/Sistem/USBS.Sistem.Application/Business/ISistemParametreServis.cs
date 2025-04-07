using Sistem.Application.DtoModels;

namespace Sistem.Application.Business
{
    public interface ISistemParametreServis
    {
        Task<SistemParametreDto> AddSistemParametreAsync(SistemParametreDto SistemParametreDto);
        Task<bool> DeleteSistemParametreAsync(Guid id);
        Task<IEnumerable<SistemParametreDto>> GetAllActiveSistemParametresAsync();
        Task<SistemParametreDto> GetByIdAsync(Guid id);
        Task<SistemParametreDto> UpdateSistemParametreAsync(Guid id, SistemParametreDto SistemParametreDto);
    }
}