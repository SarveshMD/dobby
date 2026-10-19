using Microsoft.EntityFrameworkCore;
using Dobby.Models;

namespace Dobby.Database;

public class UrlDbContext : DbContext
{
    public UrlDbContext(DbContextOptions<UrlDbContext> options) : base(options)
    {
    }

    public DbSet<UrlItem> UrlItems => Set<UrlItem>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UrlDbContext).Assembly);
    }
}
