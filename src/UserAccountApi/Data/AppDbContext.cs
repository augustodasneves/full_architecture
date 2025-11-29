using Microsoft.EntityFrameworkCore;
using UserAccountApi.Domain;

namespace UserAccountApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().OwnsOne(u => u.ContactInfo);
        modelBuilder.Entity<User>().OwnsOne(u => u.Address);
    }
}
