using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BrickShowcase.Infrastructure.Data;
using BrickShowcase.Domain.Entities;
using BrickShowcase.Application.DTOs;

namespace BrickShowcase.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly BrickDbContext _db;

    public ProjectsController(BrickDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectDto>>> GetAll([FromQuery] bool? activeOnly)
    {
        var query = _db.Project.Include(p => p.Images).AsQueryable();
        if (activeOnly == true)
        {
            query = query.Where(p => p.IsActive);
        }

        var projects = await query.OrderBy(p => p.DisplayOrder).ToListAsync();
        return Ok(projects.Select(MapToDto));
    }

    [HttpGet("{idOrSlug}")]
    public async Task<ActionResult<ProjectDto>> GetByIdOrSlug(string idOrSlug)
    {
        Project? project = null;
        if (int.TryParse(idOrSlug, out int id))
        {
            project = await _db.Project.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
        }
        else
        {
            project = await _db.Project.Include(p => p.Images).FirstOrDefaultAsync(p => p.Slug == idOrSlug);
        }

        if (project == null) return NotFound();
        return Ok(MapToDto(project));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ProjectDto>> Create([FromBody] UpsertProjectDto dto)
    {
        DateOnly? compDate = null;
        if (DateOnly.TryParse(dto.CompletedDate, out var d)) compDate = d;

        var project = new Project
        {
            Name = dto.Name,
            Slug = string.IsNullOrWhiteSpace(dto.Slug) ? GenerateSlug(dto.Name) : dto.Slug,
            ShortDescription = dto.ShortDescription,
            Description = dto.Description,
            Location = dto.Location,
            CompletedDate = compDate,
            DisplayOrder = dto.DisplayOrder,
            IsFeatured = dto.IsFeatured,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.Project.Add(project);
        await _db.SaveChangesAsync();

        var imagesToSave = dto.Images?.Where(img => !string.IsNullOrWhiteSpace(img)).Distinct().ToList() ?? new List<string>();
        if (imagesToSave.Count == 0 && !string.IsNullOrWhiteSpace(dto.Image))
        {
            imagesToSave.Add(dto.Image);
        }

        for (int i = 0; i < imagesToSave.Count; i++)
        {
            _db.ProjectImage.Add(new ProjectImage
            {
                ProjectId = project.Id,
                ImagePath = imagesToSave[i],
                IsThumbnail = (i == 0),
                DisplayOrder = i + 1
            });
        }
        if (imagesToSave.Count > 0)
        {
            await _db.SaveChangesAsync();
        }

        return CreatedAtAction(nameof(GetByIdOrSlug), new { idOrSlug = project.Id }, MapToDto(project));
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<ProjectDto>> Update(int id, [FromBody] UpsertProjectDto dto)
    {
        var project = await _db.Project.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
        if (project == null) return NotFound();

        DateOnly? compDate = null;
        if (DateOnly.TryParse(dto.CompletedDate, out var d)) compDate = d;

        project.Name = dto.Name;
        project.Slug = string.IsNullOrWhiteSpace(dto.Slug) ? GenerateSlug(dto.Name) : dto.Slug;
        project.ShortDescription = dto.ShortDescription;
        project.Description = dto.Description;
        project.Location = dto.Location;
        project.CompletedDate = compDate;
        project.DisplayOrder = dto.DisplayOrder;
        project.IsFeatured = dto.IsFeatured;
        project.IsActive = dto.IsActive;
        project.UpdatedAt = DateTime.UtcNow;

        var imagesToSave = dto.Images?.Where(img => !string.IsNullOrWhiteSpace(img)).Distinct().ToList() ?? new List<string>();
        if (imagesToSave.Count == 0 && !string.IsNullOrWhiteSpace(dto.Image))
        {
            imagesToSave.Add(dto.Image);
        }

        if (imagesToSave.Count > 0)
        {
            _db.ProjectImage.RemoveRange(project.Images);
            for (int i = 0; i < imagesToSave.Count; i++)
            {
                _db.ProjectImage.Add(new ProjectImage
                {
                    ProjectId = project.Id,
                    ImagePath = imagesToSave[i],
                    IsThumbnail = (i == 0),
                    DisplayOrder = i + 1
                });
            }
        }

        await _db.SaveChangesAsync();
        return Ok(MapToDto(project));
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)

    {
        var project = await _db.Project.FindAsync(id);
        if (project == null) return NotFound();

        _db.Project.Remove(project);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static ProjectDto MapToDto(Project p)
    {
        var imagePaths = p.Images?.OrderBy(i => i.DisplayOrder).Select(i => i.ImagePath).ToList() ?? new List<string>();
        var thumb = p.Images?.FirstOrDefault(i => i.IsThumbnail)?.ImagePath ?? imagePaths.FirstOrDefault();
        return new ProjectDto(
            p.Id,
            p.Name,
            p.Slug,
            p.ShortDescription,
            p.Description,
            p.Location,
            p.CompletedDate?.ToString("yyyy-MM-dd"),
            p.DisplayOrder,
            p.IsFeatured,
            p.IsActive,
            thumb,
            imagePaths
        );
    }

    private static string GenerateSlug(string text)
    {
        return text.ToLowerInvariant().Replace(" ", "-").Replace("đ", "d");
    }
}
