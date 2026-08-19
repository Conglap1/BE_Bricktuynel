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
        }
        catch { }

        // 1. Seed ContactInfo
        var contact = await db.ContactInfo.FirstOrDefaultAsync();
        if (contact == null)
        {
            contact = new ContactInfo
            {
                CompanyName = "Công ty TNHH Gạch Thuận Lợi",
                Address = "KCN Mỹ Phước, Bến Cát, Bình Dương",
                Phone = "0908 555 888",
                Hotline = "1900 1234",
                Email = "kinhdoanh@gachthuanloi.vn",
                Facebook = "https://facebook.com",
                Zalo = "0908555888",
                Tiktok = "https://tiktok.com",
                GoogleMapEmbed = "https://maps.google.com/maps?q=KCN+My+Phuoc,+Ben+Cat,+Binh+Duong,+Vietnam&t=&z=14&ie=UTF8&iwloc=&output=embed",
                WorkingHours = "T2 – T7 · 07:30 – 17:30"
            };
            db.ContactInfo.Add(contact);
        }
        else if (string.IsNullOrWhiteSpace(contact.Address) || contact.CompanyName.Contains("Tuynel") || contact.CompanyName.Contains("Trường Sơn"))
        {
            contact.CompanyName = "Công ty TNHH Gạch Thuận Lợi";
            contact.Address = "KCN Mỹ Phước, Bến Cát, Bình Dương";
            contact.Phone = "0908 555 888";
            contact.Hotline = "1900 1234";
            contact.Email = "kinhdoanh@gachthuanloi.vn";
            contact.GoogleMapEmbed = "https://maps.google.com/maps?q=KCN+My+Phuoc,+Ben+Cat,+Binh+Duong,+Vietnam&t=&z=14&ie=UTF8&iwloc=&output=embed";
            contact.WorkingHours = "T2 – T7 · 07:30 – 17:30";
        }

        // Update legacy external image paths for Product & Project if present
        var legacyProductImages = await db.ProductImage.Where(i => i.ImagePath.Contains("unsplash.com")).ToListAsync();
        if (legacyProductImages.Any())
        {
            var p1Img = legacyProductImages.FirstOrDefault(i => i.ProductId == 1);
            if (p1Img != null) p1Img.ImagePath = "/uploads/products/gach-ong-lo-vuong.svg";

            var p2Img = legacyProductImages.FirstOrDefault(i => i.ProductId == 2);
            if (p2Img != null) p2Img.ImagePath = "/uploads/products/gach-ong-lo-tron.svg";

            var p3Img = legacyProductImages.FirstOrDefault(i => i.ProductId == 3);
            if (p3Img != null) p3Img.ImagePath = "/uploads/products/gach-ong-semi.svg";

            var p4Img = legacyProductImages.FirstOrDefault(i => i.ProductId == 4);
            if (p4Img != null) p4Img.ImagePath = "/uploads/products/gach-the-co-lo.svg";

            foreach (var img in legacyProductImages)
            {
                if (img.ImagePath.Contains("unsplash.com"))
                {
                    img.ImagePath = "/uploads/products/gach-ong-lo-vuong.svg";
                }
            }
        }

        var legacyProjectImages = await db.ProjectImage.Where(i => i.ImagePath.Contains("unsplash.com")).ToListAsync();
        if (legacyProjectImages.Any())
        {
            var prj1Img = legacyProjectImages.FirstOrDefault(i => i.ProjectId == 1);
            if (prj1Img != null) prj1Img.ImagePath = "/uploads/projects/khu-dan-cu-an-phu.svg";

            var prj2Img = legacyProjectImages.FirstOrDefault(i => i.ProjectId == 2);
            if (prj2Img != null) prj2Img.ImagePath = "/uploads/projects/nha-may-det-phong-phu.svg";

            foreach (var img in legacyProjectImages)
            {
                if (img.ImagePath.Contains("unsplash.com"))
                {
                    img.ImagePath = "/uploads/projects/khu-dan-cu-an-phu.svg";
                }
            }
        }

        // 2. Seed Products
        if (!await db.Product.AnyAsync())
        {
            var p1 = new Product
            {
                Name = "Gạch ống lỗ vuông",
                Slug = "gach-ong-lo-vuong",
                ShortDescription = "Gạch đất sét nung lỗ vuông, nung lò Tuynel 1.050°C. Cách nhiệt tốt, trọng lượng nhẹ, phù hợp tường ngăn và tường bao.",
                Description = "Sản phẩm gạch ống lỗ vuông được sản xuất từ nguyên liệu đất sét tự nhiên tinh lọc...",
                Length = 220, Width = 105, Height = 60, HoleCount = 6, CompressionStrength = 75, WaterAbsorption = 14,
                IsFeatured = true, DisplayOrder = 1, IsActive = true, CreatedAt = DateTime.UtcNow
            };
            p1.Images.Add(new ProductImage { ImagePath = "/uploads/products/gach-ong-lo-vuong.svg", IsThumbnail = true, DisplayOrder = 1 });

            var p2 = new Product
            {
                Name = "Gạch ống lỗ tròn",
                Slug = "gach-ong-lo-tron",
                ShortDescription = "Gạch đất sét nung lỗ tròn, bề mặt nhẵn mịn đều màu. Phổ biến nhất trong xây dựng nhà dân dụng.",
                Description = "Dòng gạch ống lỗ tròn bán chạy hàng đầu với độ bền nén cao...",
                Length = 220, Width = 105, Height = 60, HoleCount = 6, CompressionStrength = 75, WaterAbsorption = 14,
                IsFeatured = true, DisplayOrder = 2, IsActive = true, CreatedAt = DateTime.UtcNow
            };
            p2.Images.Add(new ProductImage { ImagePath = "/uploads/products/gach-ong-lo-tron.svg", IsThumbnail = true, DisplayOrder = 1 });

            var p3 = new Product
            {
                Name = "Gạch ống semi",
                Slug = "gach-ong-semi",
                ShortDescription = "Gạch bán phần dày 30mm, lý tưởng cho vách ngăn nội thất, ốp tường mỏng và các công trình cần tiết kiệm không gian.",
                Description = "Giải pháp tối ưu không gian nội thất với trọng lượng siêu nhẹ...",
                Length = 220, Width = 105, Height = 30, Weight = 1.2m, HoleCount = 4, CompressionStrength = 50,
                IsFeatured = true, DisplayOrder = 3, IsActive = true, CreatedAt = DateTime.UtcNow
            };
            p3.Images.Add(new ProductImage { ImagePath = "/uploads/products/gach-ong-semi.svg", IsThumbnail = true, DisplayOrder = 1 });

            var p4 = new Product
            {
                Name = "Gạch thẻ có lỗ",
                Slug = "gach-the-co-lo",
                ShortDescription = "Gạch thẻ mỏng có lỗ, bề mặt phẳng đều màu đỏ tự nhiên. Ứng dụng ốp mặt tiền, tường trang trí.",
                Description = "Tính thẩm mỹ cao cho các mảng tường trang trí và kiến trúc độc đáo...",
                Length = 220, Width = 105, Height = 40, HoleCount = 3, CompressionStrength = 60, WaterAbsorption = 12,
                IsFeatured = true, DisplayOrder = 4, IsActive = true, CreatedAt = DateTime.UtcNow
            };
            p4.Images.Add(new ProductImage { ImagePath = "/uploads/products/gach-the-co-lo.svg", IsThumbnail = true, DisplayOrder = 1 });

            db.Product.AddRange(p1, p2, p3, p4);
        }

        // 3. Seed Projects
        if (!await db.Project.AnyAsync())
        {
            var prj1 = new Project
            {
                Name = "Khu dân cư An Phú", Slug = "khu-dan-cu-an-phu", ShortDescription = "Cung cấp 1.5 triệu viên gạch cho dự án dân cư cao cấp.",
                Description = "Dự án Khu dân cư An Phú sử dụng 100% gạch nung Tuynel Thuận Lợi...", Location = "Bình Dương",
                CompletedDate = new DateOnly(2024, 1, 1), DisplayOrder = 1, IsFeatured = true, IsActive = true, CreatedAt = DateTime.UtcNow
            };
            prj1.Images.Add(new ProjectImage { ImagePath = "/uploads/projects/khu-dan-cu-an-phu.svg", IsThumbnail = true });

            var prj2 = new Project
            {
                Name = "Nhà máy dệt Phong Phú", Slug = "nha-may-det-phong-phu", ShortDescription = "Công trình công nghiệp quy mô lớn.",
                Description = "Nhà máy dệt Phong Phú tại KCN Đồng Nai...", Location = "Đồng Nai",
                CompletedDate = new DateOnly(2023, 6, 1), DisplayOrder = 2, IsFeatured = false, IsActive = true, CreatedAt = DateTime.UtcNow
            };
            prj2.Images.Add(new ProjectImage { ImagePath = "/uploads/projects/nha-may-det-phong-phu.svg", IsThumbnail = true });

            db.Project.AddRange(prj1, prj2);
        }

        // 4. Seed News
        if (!await db.News.AnyAsync())
        {
            db.News.AddRange(
                new News
                {
                    Title = "Khánh thành dây chuyền lò Tuynel số 3",
                    Slug = "khanh-thanh-day-chuyen-lo-tuynel-so-3",
                    Summary = "Nhà máy Thuận Lợi chính thức vận hành dây chuyền lò Tuynel thứ ba, nâng tổng công suất lên 5 triệu viên/tháng.",
                    Content = "<p>Nhà máy Thuận Lợi chính thức vận hành dây chuyền lò Tuynel thứ ba...</p>",
                    ThumbnailPath = "https://images.unsplash.com/photo-1706715201231-b703e7df3395?w=800&h=600&fit=crop",
                    PublishedAt = new DateTime(2026, 6, 12), IsActive = true, CreatedAt = DateTime.UtcNow
                },
                new News
                {
                    Title = "Ra mắt dòng gạch ống lỗ tròn thế hệ mới",
                    Slug = "ra-mat-dong-gach-ong-lo-tron-the-he-moi",
                    Summary = "Gạch ống lỗ tròn cải tiến với bề mặt đồng đều hơn, cắt giảm 8% trọng lượng.",
                    Content = "<p>Dòng gạch ống lỗ tròn thế hệ mới của Thuận Lợi Brick...</p>",
                    ThumbnailPath = "https://images.unsplash.com/photo-1657007508392-d68322544f70?w=800&h=600&fit=crop",
                    PublishedAt = new DateTime(2026, 5, 28), IsActive = true, CreatedAt = DateTime.UtcNow
                }
            );
        }

        // 5. Seed Partners
        var defaultPartners = new[]
        {
            new Partner { Name = "Coteccons", LogoPath = "/uploads/partners/coteccons.svg", Website = "https://coteccons.vn", DisplayOrder = 1, IsActive = true },
            new Partner { Name = "Hòa Bình Construction", LogoPath = "/uploads/partners/hoabinh.svg", Website = "https://hbcg.vn", DisplayOrder = 2, IsActive = true },
            new Partner { Name = "Vinaconex", LogoPath = "/uploads/partners/vinaconex.svg", Website = "https://vinaconex.com.vn", DisplayOrder = 3, IsActive = true },
            new Partner { Name = "Delta Group", LogoPath = "/uploads/partners/deltagroup.svg", Website = "https://deltagroup.vn", DisplayOrder = 4, IsActive = true },
            new Partner { Name = "Ricons", LogoPath = "/uploads/partners/ricons.svg", Website = "https://ricons.vn", DisplayOrder = 5, IsActive = true },
            new Partner { Name = "An Phong Construction", LogoPath = "/uploads/partners/anphong.svg", Website = "https://anphong.vn", DisplayOrder = 6, IsActive = true }
        };

        if (!await db.Partner.AnyAsync())
        {
            db.Partner.AddRange(defaultPartners);
        }
        else
        {
            var existingPartners = await db.Partner.ToListAsync();
            foreach (var p in existingPartners)
            {
                if (string.IsNullOrWhiteSpace(p.LogoPath))
                {
                    var match = defaultPartners.FirstOrDefault(x => x.Name.ToLower().Contains(p.Name.ToLower()) || p.Name.ToLower().Contains(x.Name.ToLower()));
                    if (match != null)
                    {
                        p.LogoPath = match.LogoPath;
                        if (string.IsNullOrWhiteSpace(p.Website)) p.Website = match.Website;
                    }
                }
            }
        }

        // 6. Seed AdminUser
        var adminUser = await db.AdminUser.FirstOrDefaultAsync(u => u.Username == "admin");
        if (adminUser == null)
        {
            db.AdminUser.Add(new AdminUser
            {
                Username = "admin",
                PasswordHash = "184ea4c56b9455ab63ba14510a314c1e93f7ccac6f77c41605b54f198b1ce6f5"
            });
        }
        else
        {
            adminUser.PasswordHash = "184ea4c56b9455ab63ba14510a314c1e93f7ccac6f77c41605b54f198b1ce6f5";
        }

        await db.SaveChangesAsync();
    }
}
