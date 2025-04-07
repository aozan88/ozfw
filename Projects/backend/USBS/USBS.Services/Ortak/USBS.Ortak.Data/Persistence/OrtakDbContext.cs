using KonFw.BaseInfrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using USBS.Common.Domain;

namespace Ortak.Data.Persistence
{
    public class OrtakDbContext : KonFwDbContext
    {
        private readonly IHttpContextAccessor _context;
        public OrtakDbContext(DbContextOptions<OrtakDbContext> options, IHttpContextAccessor context) : base(options, context) 
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            _context = context;
        }

        public DbSet<SistemTanim> SistemTanims { get; set; }
        public DbSet<SistemTanimTipi> SistemTanimTipis { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<SistemTanimTipi>().ToTable("Sistem_TanimTipi");
            modelBuilder.Entity<SistemTanim>().ToTable("Sistem_Tanim").HasOne<SistemTanimTipi>(x=>x.TanimTipi).WithMany(x=>x.SistemTanims).HasForeignKey(x => x.TanimTipiId);
        }
    }
}
