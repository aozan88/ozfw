using OzFw.BaseInfrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using USBS.Common.Domain;

namespace Sistem.Data.Persistence
{
    public class SistemDbContext : OzFwDbContext
    {
        private readonly IHttpContextAccessor _context;
        public SistemDbContext(DbContextOptions<SistemDbContext> options, IHttpContextAccessor context) : base(options, context)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            _context = context;
        }

        public DbSet<SistemParametre> SistemParametres { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<SistemParametre>().ToTable("Sistem_Parametre");
        }
    }
}
