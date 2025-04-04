using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace KonFw.HttpContextAccessor
{
    public class OrtakContext : DbContext
    {
        private readonly IHttpContextAccessor _context;

        public OrtakContext(DbContextOptions<OrtakContext> options, IHttpContextAccessor context) : base(options)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            _context = context;
        }
          //public DbSet<Order> Orders { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);
        //   // modelBuilder.Entity<Order>().ToTable("orders");

        //}

        //public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        //{
        //    foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        //    {
        //        switch (entry.State)
        //        {
        //            case EntityState.Added:
        //                entry.Entity.OlusturulmaTarihi = DateTime.Now;
        //                entry.Entity.Olusturan = _context.HttpContext.User?.Identity.Name;
        //                break;
        //            case EntityState.Modified:
        //                entry.Entity.DegistirilmeTarihi = DateTime.Now;
        //                entry.Entity.Guncelleyen = _context.HttpContext.User?.Identity.Name;
        //                break;
        //            case EntityState.Deleted:
        //                entry.Entity.SilinmeTarihi = DateTime.Now;
        //                entry.Entity.Silen = _context.HttpContext.User?.Identity.Name;
        //                break;
        //        }
        //    }
        //    return base.SaveChangesAsync(cancellationToken);
        //}
    }
}
