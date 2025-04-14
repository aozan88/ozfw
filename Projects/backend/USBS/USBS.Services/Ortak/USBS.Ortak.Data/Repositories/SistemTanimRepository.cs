using Microsoft.EntityFrameworkCore;
using KonFw.BaseInfrastructure;
using USBS.Common.Domain;
using Ortak.Application.Contract;
using Ortak.Data.Persistence;


namespace Ortak.Data.Repositories
{
    public class SistemTanimRepository : GenericRepository<SistemTanim>, ISistemTanimRepository
    {
        private readonly OrtakDbContext _context;

        public SistemTanimRepository(OrtakDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SistemTanim>> GetAktifTanimListAsync()
        {
            return await _context.SistemTanims.Where(t => t.Durum == 1).ToListAsync();
        }

        public async Task<IEnumerable<SistemTanim>> GetAktifTanimByTanimTipiIdAsync(Guid tanimTipiId)
        {
            return await _context.SistemTanims.Where(t => t.Durum == 1 && t.TanimTipiId == tanimTipiId).ToListAsync();
        }
    }
}
