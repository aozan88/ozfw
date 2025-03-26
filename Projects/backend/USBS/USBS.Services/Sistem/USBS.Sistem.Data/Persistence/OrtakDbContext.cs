using Microsoft.EntityFrameworkCore;
using USBS.Common.DbContexts;
using USBS.Common.Domain;

namespace Sistem.Data.Persistence
{
    public class SistemDbContext : AppDbContext
    {
        public SistemDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<SistemConfig> SistemConfigs { get; set; }
    }
}
