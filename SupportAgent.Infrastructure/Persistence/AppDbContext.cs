using Microsoft.EntityFrameworkCore;
using SupportAgent.Domain.Entities;

namespace SupportAgent.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public DbSet<UserAccount> Users { get; set; }
        public DbSet<Chamado> Chamados { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserAccount>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Email).IsRequired();
            });

            modelBuilder.Entity<Chamado>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Category).IsRequired();
                entity.Property(e => e.Status).HasConversion<string>(); // Store enum as string for readability
            });
        }
    }
}
