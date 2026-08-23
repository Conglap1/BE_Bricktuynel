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
        var query = _db.News
            .Include(n => n.Sections.OrderBy(s => s.DisplayOrder))
                .ThenInclude(s => s.Images.OrderBy(img => img.DisplayOrder))
            .Include(n => n.Images.Where(img => img.NewsSectionId == null).OrderBy(img => img.DisplayOrder))
            .AsQueryable();

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
        var query = _db.News
            .Include(n => n.Sections.OrderBy(s => s.DisplayOrder))
                .ThenInclude(s => s.Images.OrderBy(img => img.DisplayOrder))
            .Include(n => n.Images.Where(img => img.NewsSectionId == null).OrderBy(img => img.DisplayOrder));

        if (int.TryParse(idOrSlug, out int id))
        {
            news = await query.FirstOrDefaultAsync(n => n.Id == id);
        }
        else
        {
            news = await query.FirstOrDefaultAsync(n => n.Slug == idOrSlug);
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

        if (dto.Sections != null && dto.Sections.Any())
        {
            foreach (var secDto in dto.Sections)
            {
                var section = new NewsSection
                {
                    Question = secDto.Question,
                    Answer = secDto.Answer,
                    DisplayOrder = secDto.DisplayOrder
                };

                if (secDto.Images != null && secDto.Images.Any())
                {
                    foreach (var imgDto in secDto.Images)
                    {
                        section.Images.Add(new NewsImage
                        {
                            ImagePath = imgDto.ImagePath,
                            Caption = imgDto.Caption,
                            DisplayOrder = imgDto.DisplayOrder
                        });
                    }
                }
                news.Sections.Add(section);
            }
        }

        if (dto.Images != null && dto.Images.Any())
        {
            foreach (var imgDto in dto.Images)
            {
                news.Images.Add(new NewsImage
                {
                    ImagePath = imgDto.ImagePath,
                    Caption = imgDto.Caption,
                    DisplayOrder = imgDto.DisplayOrder
                });
            }
        }

        _db.News.Add(news);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByIdOrSlug), new { idOrSlug = news.Id }, MapToDto(news));
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<NewsDto>> Update(int id, [FromBody] UpsertNewsDto dto)
    {
        var news = await _db.News
            .Include(n => n.Sections)
                .ThenInclude(s => s.Images)
            .Include(n => n.Images)
            .FirstOrDefaultAsync(n => n.Id == id);

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

        // Clear existing sections & images
        _db.NewsImage.RemoveRange(news.Images);
        _db.NewsSection.RemoveRange(news.Sections);

        news.Sections.Clear();
        news.Images.Clear();

        if (dto.Sections != null && dto.Sections.Any())
        {
            foreach (var secDto in dto.Sections)
            {
                var section = new NewsSection
                {
                    Question = secDto.Question,
                    Answer = secDto.Answer,
                    DisplayOrder = secDto.DisplayOrder
                };

                if (secDto.Images != null && secDto.Images.Any())
                {
                    foreach (var imgDto in secDto.Images)
                    {
                        section.Images.Add(new NewsImage
                        {
                            ImagePath = imgDto.ImagePath,
                            Caption = imgDto.Caption,
                            DisplayOrder = imgDto.DisplayOrder
                        });
                    }
                }
                news.Sections.Add(section);
            }
        }

        if (dto.Images != null && dto.Images.Any())
        {
            foreach (var imgDto in dto.Images)
            {
                news.Images.Add(new NewsImage
                {
                    ImagePath = imgDto.ImagePath,
                    Caption = imgDto.Caption,
                    DisplayOrder = imgDto.DisplayOrder
                });
            }
        }

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
        var sectionDtos = n.Sections?
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new NewsSectionDto(
                s.Id,
                s.Question,
                s.Answer,
                s.DisplayOrder,
                s.Images?.OrderBy(img => img.DisplayOrder).Select(img => new NewsImageDto(
                    img.Id,
                    img.ImagePath,
                    img.Caption,
                    img.DisplayOrder,
                    img.NewsSectionId
                )).ToList()
            )).ToList();

        var imageDtos = n.Images?
            .Where(img => img.NewsSectionId == null)
            .OrderBy(img => img.DisplayOrder)
            .Select(img => new NewsImageDto(
                img.Id,
                img.ImagePath,
                img.Caption,
                img.DisplayOrder,
                img.NewsSectionId
            )).ToList();

        return new NewsDto(
            n.Id,
            n.Title,
            n.Slug,
            n.ThumbnailPath,
            n.Summary,
            n.Content,
            n.PublishedAt?.ToString("yyyy-MM-dd"),
            n.IsActive,
            sectionDtos,
            imageDtos
        );
    }

    private static string GenerateSlug(string text)
    {
        return text.ToLowerInvariant().Replace(" ", "-").Replace("đ", "d");
    }
}
