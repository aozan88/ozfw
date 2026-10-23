using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OzFw.BaseInfrastructure;

namespace Ornek.Data.Persistence
{
    // Bu dosya generator tarafından bir kez oluşturuldu ve artık size ait; yeniden üretimde değiştirilmez.
    // DbSet'ler OrnekDbContext.g.cs, tablo eşlemeleri Configurations/*.g.cs dosyalarındadır.
    public partial class OrnekDbContext : OzFwDbContext
    {
        static OrnekDbContext()
        {
            // DateTime kolonları "timestamp" (time zone'suz); diğer modüllerle aynı davranış
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        public OrnekDbContext(DbContextOptions<OrnekDbContext> options, IHttpContextAccessor context) : base(options, context)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrnekDbContext).Assembly);
        }
    }
}
