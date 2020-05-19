using ExpensesAPI.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExpensesAPI.Domain.Persistence
{
    public class MainDbContext : DbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Scope> Scopes { get; set; }

        public DbSet<User> Users { get; set; }

        public MainDbContext() 
        {
        }

        public MainDbContext(DbContextOptions<MainDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
            // ...
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            //this.Database.EnsureCreated();
            builder.Entity<Scope>()
                .HasOne(s => s.Owner)
                .WithMany(u => u.OwnedScopes)
                .HasForeignKey(s => s.OwnerId);

            builder.Entity<ScopeUser>()
                .HasKey(su => new { su.ScopeId, su.UserId });
            builder.Entity<ScopeUser>()
                .HasOne(su => su.Scope)
                .WithMany(s => s.ScopeUsers)
                .HasForeignKey(su => su.ScopeId);
            builder.Entity<ScopeUser>()
                .HasOne(su => su.User)
                .WithMany(u => u.ScopeUsers)
                .HasForeignKey(su => su.UserId);

            //builder.Entity<User>(entity => entity.Property(m => m.Id).HasMaxLength(127));
            //builder.Entity<IdentityRole>(entity => entity.Property(m => m.Id).HasMaxLength(127));
            //builder.Entity<IdentityRole>(entity => entity.Property(m => m.Name).HasMaxLength(127));
            //builder.Entity<IdentityRole>(entity => entity.Property(m => m.NormalizedName).HasMaxLength(127));
            //builder.Entity<IdentityUserLogin<string>>(entity => entity.Property(m => m.LoginProvider).HasMaxLength(127));
            //builder.Entity<IdentityUserLogin<string>>(entity => entity.Property(m => m.ProviderKey).HasMaxLength(127));
            //builder.Entity<IdentityUserRole<string>>(entity => entity.Property(m => m.UserId).HasMaxLength(127));
            //builder.Entity<IdentityUserRole<string>>(entity => entity.Property(m => m.RoleId).HasMaxLength(127));
            //builder.Entity<IdentityUserToken<string>>(entity => entity.Property(m => m.UserId).HasMaxLength(127));
            //builder.Entity<IdentityUserToken<string>>(entity => entity.Property(m => m.LoginProvider).HasMaxLength(127));
            //builder.Entity<IdentityUserToken<string>>(entity => entity.Property(m => m.Name).HasMaxLength(127));

            base.OnModelCreating(builder);
        }
    }
}