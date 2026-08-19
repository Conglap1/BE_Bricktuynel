using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BrickShowcase.Infrastructure.Data;
using BrickShowcase.Domain.Entities;
using BrickShowcase.Application.DTOs;

namespace BrickShowcase.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsController : ControllerBase
{
    private readonly BrickDbContext _db;

    public NewsController(BrickDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NewsDto>>> GetAll([FromQuery] bool? activeOnly)
    {
        var query = _db.News.AsQueryable();
        if (activeOnly == true)
        {
            query = query.Where(n => n.IsActive);
        }

        var list = await query.OrderByDescending(n => n.PublishedAt).ToListAsync();
        return Ok(list.Select(MapToDto));
    }

    [HttpGet("{idOrSlug}")]
    public async Task<ActionResult<NewsDto>> GetByIdOrSlug(string idOrSlug)
    {
        News? news = null;
        if (int.TryParse(idOrSlug, out int id))
        {
            news = await _db.News.FindAsync(id);
        }
        else
        {
            news = await _db.News.FirstOrDefaultAsync(n => n.Slug == idOrSlug);
        }

        if (news == null) return NotFound();
        return Ok(MapToDto(news));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<NewsDto>> Create([FromBody] UpsertNewsDto dto)
    {
        DateTime? pubAt = null;
        if (DateTime.TryParse(dto.PublishedAt, out var p)) pubAt = p;

        var news = new News
        {
            Title = dto.Title,
            Slug = string.IsNullOrWhiteSpace(dto.Slug) ? GenerateSlug(dto.Title) : dto.Slug,
            ThumbnailPath = dto.ThumbnailPath,
            Summary = dto.Summary,
            Content = dto.Content,
            PublishedAt = pubAt ?? DateTime.UtcNow,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.News.Add(news);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByIdOrSlug), new { idOrSlug = news.Id }, MapToDto(news));
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<NewsDto>> Update(int id, [FromBody] UpsertNewsDto dto)
    {
        var news = await _db.News.FindAsync(id);
        if (news == null) return NotFound();

        DateTime? pubAt = null;
        if (DateTime.TryParse(dto.PublishedAt, out var p)) pubAt = p;

        news.Title = dto.Title;
        news.Slug = string.IsNullOrWhiteSpace(dto.Slug) ? GenerateSlug(dto.Title) : dto.Slug;
        news.ThumbnailPath = dto.ThumbnailPath;
        news.Summary = dto.Summary;
        news.Content = dto.Content;
        news.PublishedAt = pubAt ?? news.PublishedAt;
        news.IsActive = dto.IsActive;
        news.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(MapToDto(news));
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)

    {
        var news = await _db.News.FindAsync(id);
        if (news == null) return NotFound();

        _db.News.Remove(news);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static NewsDto MapToDto(News n)
    {
        return new NewsDto(
            n.Id,
            n.Title,
            n.Slug,
            n.ThumbnailPath,
            n.Summary,
            n.Content,
            n.PublishedAt?.ToString("yyyy-MM-dd"),
            n.IsActive
        );
    }

    private static string GenerateSlug(string text)
    {
        return text.ToLowerInvariant().Replace(" ", "-").Replace("đ", "d");
    }
}
