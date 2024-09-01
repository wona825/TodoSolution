using Domain.Entites;
using Microsoft.EntityFrameworkCore;

namespace Infrasfructure.Data
{
    internal class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<Todo> Todos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne(u => u.Token)
                .WithOne(t => t.ApplicationUser)
                .HasForeignKey<Token>(t => t.UserId);

            modelBuilder.Entity<Todo>()
                .HasOne(t => t.Owner)
                .WithMany(u => u.Todos)
                .HasForeignKey(t => t.OwnerId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
