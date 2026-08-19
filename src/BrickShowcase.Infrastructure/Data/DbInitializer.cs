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
    }
}
