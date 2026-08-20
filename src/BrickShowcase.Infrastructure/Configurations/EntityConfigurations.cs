using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BrickShowcase.Domain.Entities;

namespace BrickShowcase.Infrastructure.Configurations;

public class AdminUserConfiguration : IEntityTypeConfiguration<AdminUser>
{
    public void Configure(EntityTypeBuilder<AdminUser> builder)
    {
        builder.ToTable("AdminUser");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Username).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.Username).IsUnique();
        builder.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
    }
}

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Product");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(150).IsRequired();
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.Property(x => x.ShortDescription).HasMaxLength(500);
        builder.Property(x => x.Length).HasColumnType("decimal(10,2)");
        builder.Property(x => x.Width).HasColumnType("decimal(10,2)");
        builder.Property(x => x.Height).HasColumnType("decimal(10,2)");
        builder.Property(x => x.Weight).HasColumnType("decimal(10,2)");
        builder.Property(x => x.CompressionStrength).HasColumnType("decimal(10,2)");
        builder.Property(x => x.FlexuralStrength).HasColumnType("decimal(10,2)");
        builder.Property(x => x.BulkDensity).HasColumnType("decimal(10,2)");
        builder.Property(x => x.WaterAbsorption).HasColumnType("decimal(5,2)");
        builder.Property(x => x.BrickGrade).HasMaxLength(50);
        builder.Property(x => x.StandardCode).HasMaxLength(100);
        builder.Property(x => x.IsFeatured).HasDefaultValue(false);
        builder.Property(x => x.DisplayOrder).HasDefaultValue(0);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

        builder.HasMany(x => x.Images)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("ProductImage");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ImagePath).HasMaxLength(500).IsRequired();
        builder.Property(x => x.AltText).HasMaxLength(255);
        builder.Property(x => x.IsThumbnail).HasDefaultValue(false);
        builder.Property(x => x.DisplayOrder).HasDefaultValue(0);
    }
}

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Project");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(255).IsRequired();
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.Property(x => x.ShortDescription).HasMaxLength(500);
        builder.Property(x => x.Location).HasMaxLength(255);
        builder.Property(x => x.DisplayOrder).HasDefaultValue(0);
        builder.Property(x => x.IsFeatured).HasDefaultValue(false);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

        builder.HasMany(x => x.Images)
            .WithOne(x => x.Project)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProjectImageConfiguration : IEntityTypeConfiguration<ProjectImage>
{
    public void Configure(EntityTypeBuilder<ProjectImage> builder)
    {
        builder.ToTable("ProjectImage");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ImagePath).HasMaxLength(500).IsRequired();
        builder.Property(x => x.IsThumbnail).HasDefaultValue(false);
        builder.Property(x => x.DisplayOrder).HasDefaultValue(0);
    }
}

public class NewsConfiguration : IEntityTypeConfiguration<News>
{
    public void Configure(EntityTypeBuilder<News> builder)
    {
        builder.ToTable("News");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(250).IsRequired();
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.Property(x => x.ThumbnailPath).HasMaxLength(500);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");
    }
}

public class PartnerConfiguration : IEntityTypeConfiguration<Partner>
{
    public void Configure(EntityTypeBuilder<Partner> builder)
    {
        builder.ToTable("Partner");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.LogoPath).HasMaxLength(500);
        builder.Property(x => x.Website).HasMaxLength(255);
        builder.Property(x => x.DisplayOrder).HasDefaultValue(0);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
    }
}

public class ContactInfoConfiguration : IEntityTypeConfiguration<ContactInfo>
{
    public void Configure(EntityTypeBuilder<ContactInfo> builder)
    {
        builder.ToTable("ContactInfo");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CompanyName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.Property(x => x.Phone).HasMaxLength(20);
        builder.Property(x => x.Hotline).HasMaxLength(20);
        builder.Property(x => x.Email).HasMaxLength(100);
        builder.Property(x => x.Facebook).HasMaxLength(255);
        builder.Property(x => x.Zalo).HasMaxLength(255);
        builder.Property(x => x.Tiktok).HasMaxLength(255);
        builder.Property(x => x.WorkingHours).HasMaxLength(255);
    }
}

public class ContactRequestConfiguration : IEntityTypeConfiguration<ContactRequest>
{
    public void Configure(EntityTypeBuilder<ContactRequest> builder)
    {
        builder.ToTable("ContactRequest");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FullName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Phone).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(100);
        builder.Property(x => x.Content).IsRequired();
        builder.Property(x => x.IsRead).HasDefaultValue(false);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");
    }
}
