using BrickShowcase.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrickShowcase.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(BrickDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        try
        {
            await db.Database.ExecuteSqlRawAsync(
                "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ContactInfo') AND name = 'Tiktok') " +
                "ALTER TABLE ContactInfo ADD Tiktok NVARCHAR(255) NULL;"
            );
            await db.Database.ExecuteSqlRawAsync(
                "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Product') AND name = 'FlexuralStrength') " +
                "ALTER TABLE Product ADD FlexuralStrength DECIMAL(10,2) NULL;"
            );
            await db.Database.ExecuteSqlRawAsync(
                "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Product') AND name = 'BrickGrade') " +
                "ALTER TABLE Product ADD BrickGrade NVARCHAR(50) NULL;"
            );
        }
        catch { }

        // Seed AdminUser if not exists
        var adminUser = await db.AdminUser.FirstOrDefaultAsync(u => u.Username == "admin");
        if (adminUser == null)
        {
            db.AdminUser.Add(new AdminUser
            {
                Username = "admin",
                PasswordHash = "184ea4c56b9455ab63ba14510a314c1e93f7ccac6f77c41605b54f198b1ce6f5"
            });
            await db.SaveChangesAsync();
        }

        // Seed Initial Products if empty
        if (!await db.Product.AnyAsync())
        {
            var p1 = new Product
            {
                Name = "Gạch đất sét nung 2 lỗ (40x80x180 mm)",
                Slug = "gach-dat-set-nung-2-lo-40x80x180",
                ShortDescription = "Gạch đất sét nung Tuynel 2 lỗ (gạch thẻ 2 lỗ) đạt quy chuẩn QCVN 16:2023/BXD & TCVN 6355:2009.",
                Description = "Sản phẩm gạch đất sét nung loại 2 lỗ Tuynel Thuận Lợi Mộc Hóa được sản xuất trên dây chuyền công nghệ cao, nung trong lò Tuynel liên tục ở nhiệt độ 1.050°C. Đạt chứng nhận hợp quy QCVN 16:2023/BXD, mác gạch M75, độ chịu nén vượt trội và độ hút nước thấp.",
                Length = 180,
                Width = 80,
                Height = 40,
                CompressionStrength = 7.9m,
                FlexuralStrength = 1.9m,
                BrickGrade = "Mác 75",
                IsFeatured = true,
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var p2 = new Product
            {
                Name = "Gạch đất sét nung 4 lỗ (80x80x180 mm)",
                Slug = "gach-dat-set-nung-4-lo-80x80x180",
                ShortDescription = "Gạch đất sét nung Tuynel 4 lỗ (gạch ống 4 lỗ) đạt hợp quy QCVN 16:2023/BXD, Mác 75.",
                Description = "Gạch 4 lỗ Tuynel Thuận Lợi Mộc Hóa chuẩn kích thước 80x80x180 mm, nung lò Tuynel công nghệ nén ép đùn chân không. Độ chịu nén trung bình 7.7 - 8.1 MPa, khối lượng thể tích 0.96 g/cm³, độ hút nước 11.9 - 12.8%, tối ưu cho xây dựng tường bao và công trình dân dụng.",
                Length = 180,
                Width = 80,
                Height = 80,
                CompressionStrength = 7.7m,
                FlexuralStrength = 1.8m,
                BrickGrade = "Mác 75",
                IsFeatured = true,
                DisplayOrder = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            db.Product.AddRange(p1, p2);
            await db.SaveChangesAsync();

            db.ProductImage.AddRange(
                new ProductImage { ProductId = p1.Id, ImagePath = "/uploads/products/gach-the-co-lo.svg", IsThumbnail = true, DisplayOrder = 1 },
                new ProductImage { ProductId = p2.Id, ImagePath = "/uploads/products/gach-ong-lo-vuong.svg", IsThumbnail = true, DisplayOrder = 1 }
            );
            await db.SaveChangesAsync();
        }
    }
}
