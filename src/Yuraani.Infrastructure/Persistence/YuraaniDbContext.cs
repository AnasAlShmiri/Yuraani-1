using Microsoft.EntityFrameworkCore;

namespace Yuraani.Infrastructure.Persistence;

public sealed class YuraaniDbContext(DbContextOptions<YuraaniDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(YuraaniDbContext).Assembly);
    }
}
