using Microsoft.EntityFrameworkCore;
using KonFw.BaseInfrastructure;
using USBS.Common.Domain;
using Sistem.Application.Contract;
using Sistem.Data.Persistence;

namespace Sistem.Data.Repositories
{
    public class SistemParametreRepository : GenericRepository<SistemParametre>, ISistemParametreRepository
    {
        private readonly SistemDbContext _context;

        public SistemParametreRepository(SistemDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SistemParametre>> GetActiveSistemParametresAsync()
        {
            return await _context.SistemParametres.Where(k => k.Durum == 1).ToListAsync();
        }
    }
}
