using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BrickShowcase.Infrastructure.Data;

namespace BrickShowcase.Api.Controllers;

[ApiController]
public class SitemapController : ControllerBase
{
    private readonly BrickDbContext _db;
    private readonly IConfiguration _config;

    public SitemapController(BrickDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpGet("/sitemap.xml")]
    [HttpGet("api/sitemap.xml")]
    [HttpGet("api/sitemap")]
    [Produces("application/xml")]
    public async Task<IActionResult> GetSitemap()
    {
        var rawBaseUrl = _config["SiteUrl"] ?? "https://www.gachtuynelthuanloi.com";
        var baseUrl = rawBaseUrl.TrimEnd('/');
        var now = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

        // 1. Static Core Pages
        var staticPages = new (string Path, string Priority, string Freq)[]
        {
            ("", "1.0", "weekly"),
            ("/gioi-thieu", "0.9", "monthly"),
            ("/san-pham", "0.9", "weekly"),
            ("/quy-trinh", "0.8", "monthly"),
            ("/nang-luc", "0.8", "monthly"),
            ("/du-an", "0.8", "weekly"),
            ("/tin-tuc", "0.8", "weekly"),
            ("/lien-he", "0.8", "monthly")
        };

        foreach (var (path, priority, freq) in staticPages)
        {
            sb.AppendLine("  <url>");
            sb.AppendLine($"    <loc>{baseUrl}{path}</loc>");
            sb.AppendLine($"    <lastmod>{now}</lastmod>");
            sb.AppendLine($"    <changefreq>{freq}</changefreq>");
            sb.AppendLine($"    <priority>{priority}</priority>");
            sb.AppendLine("  </url>");
        }

        // 2. Dynamic Active Products
        var products = await _db.Product
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .Select(p => new
            {
                p.Slug,
                LastMod = (p.UpdatedAt ?? p.CreatedAt).ToString("yyyy-MM-dd")
            })
            .ToListAsync();

        foreach (var prod in products)
        {
            if (string.IsNullOrWhiteSpace(prod.Slug)) continue;
            sb.AppendLine("  <url>");
            sb.AppendLine($"    <loc>{baseUrl}/san-pham/{prod.Slug}</loc>");
            sb.AppendLine($"    <lastmod>{prod.LastMod}</lastmod>");
            sb.AppendLine("    <changefreq>weekly</changefreq>");
            sb.AppendLine("    <priority>0.8</priority>");
            sb.AppendLine("  </url>");
        }

        // 3. Dynamic Active News Articles
        var newsList = await _db.News
            .AsNoTracking()
            .Where(n => n.IsActive)
            .OrderByDescending(n => n.PublishedAt ?? n.CreatedAt)
            .Select(n => new
            {
                n.Slug,
                LastMod = (n.UpdatedAt ?? n.PublishedAt ?? n.CreatedAt).ToString("yyyy-MM-dd")
            })
            .ToListAsync();

        foreach (var item in newsList)
        {
            if (string.IsNullOrWhiteSpace(item.Slug)) continue;
            sb.AppendLine("  <url>");
            sb.AppendLine($"    <loc>{baseUrl}/tin-tuc/{item.Slug}</loc>");
            sb.AppendLine($"    <lastmod>{item.LastMod}</lastmod>");
            sb.AppendLine("    <changefreq>monthly</changefreq>");
            sb.AppendLine("    <priority>0.7</priority>");
            sb.AppendLine("  </url>");
        }

        // 4. Dynamic Active Projects
        var projects = await _db.Project
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .Select(p => new
            {
                p.Slug,
                LastMod = (p.UpdatedAt ?? p.CreatedAt).ToString("yyyy-MM-dd")
            })
            .ToListAsync();

        foreach (var prj in projects)
        {
            if (string.IsNullOrWhiteSpace(prj.Slug)) continue;
            sb.AppendLine("  <url>");
            sb.AppendLine($"    <loc>{baseUrl}/du-an/{prj.Slug}</loc>");
            sb.AppendLine($"    <lastmod>{prj.LastMod}</lastmod>");
            sb.AppendLine("    <changefreq>monthly</changefreq>");
            sb.AppendLine("    <priority>0.7</priority>");
            sb.AppendLine("  </url>");
        }

        sb.AppendLine("</urlset>");

        return Content(sb.ToString(), "application/xml", Encoding.UTF8);
    }
}
