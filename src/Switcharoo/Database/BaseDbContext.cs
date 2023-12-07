using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Switcharoo.Entities;
using Environment = Switcharoo.Entities.Environment;

namespace Switcharoo.Database;

public abstract class BaseDbContext(DbContextOptions options) : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Environment> Environments { get; set; }
    public DbSet<Feature> Features { get; set; }
    public DbSet<FeatureEnvironment> FeatureEnvironments { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Environment>().HasKey(x => x.Id);
        builder.Entity<Environment>().Property(x => x.Name).IsRequired();
        builder.Entity<Environment>().HasMany(x => x.Features).WithOne(x => x.Environment);
        
        builder.Entity<Feature>().HasKey(x => x.Id);
        builder.Entity<Feature>().Property(x => x.Name).IsRequired();
        builder.Entity<Feature>().Property(x => x.Key).IsRequired();
        builder.Entity<Feature>().HasMany(x => x.Environments).WithOne(x => x.Feature);
        
        builder.Entity<FeatureEnvironment>().HasKey(x => x.Id);
        builder.Entity<FeatureEnvironment>().Property(x => x.IsEnabled).IsRequired();
        builder.Entity<FeatureEnvironment>().HasOne(x => x.Environment).WithMany(x => x.Features);
        builder.Entity<FeatureEnvironment>().HasOne(x => x.Feature).WithMany(x => x.Environments);
        
        base.OnModelCreating(builder);
    }
}
