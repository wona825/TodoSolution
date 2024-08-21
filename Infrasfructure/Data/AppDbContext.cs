using Domain.Entites;
using Microsoft.EntityFrameworkCore;

namespace Infrasfructure.Data
{
    internal class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Token> Tokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Token)
                .WithOne(t => t.ApplicationUser)
                .HasForeignKey<Token>(t => t.UserId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
