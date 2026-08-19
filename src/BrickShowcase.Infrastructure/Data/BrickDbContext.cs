using Microsoft.EntityFrameworkCore;
using BrickShowcase.Domain.Entities;

namespace BrickShowcase.Infrastructure.Data;

public class BrickDbContext : DbContext
{
    public BrickDbContext(DbContextOptions<BrickDbContext> options) : base(options)
    {
    }

    public DbSet<AdminUser> AdminUser => Set<AdminUser>();
    public DbSet<Product> Product => Set<Product>();
    public DbSet<ProductImage> ProductImage => Set<ProductImage>();
    public DbSet<Project> Project => Set<Project>();
    public DbSet<ProjectImage> ProjectImage => Set<ProjectImage>();
    public DbSet<News> News => Set<News>();
    public DbSet<Partner> Partner => Set<Partner>();
    public DbSet<ContactInfo> ContactInfo => Set<ContactInfo>();
    public DbSet<ContactRequest> ContactRequest => Set<ContactRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BrickDbContext).Assembly);
    }
}
