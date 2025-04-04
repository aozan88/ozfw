using KonFw.BaseInfrastructure;
using Microsoft.EntityFrameworkCore;
using USBS.Common.Domain;

namespace Sistem.Data.Persistence
{
    public class SistemDbContext : KonFwDbContext
    {
        public SistemDbContext(DbContextOptions<SistemDbContext> options) : base(options) { }

        public DbSet<SistemConfig> SistemConfigs { get; set; }
    }
}
