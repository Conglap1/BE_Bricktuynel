using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BrickShowcase.Infrastructure.Data;
using BrickShowcase.Domain.Entities;
using BrickShowcase.Application.DTOs;

namespace BrickShowcase.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PartnersController : ControllerBase
{
    private readonly BrickDbContext _db;

    public PartnersController(BrickDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PartnerDto>>> GetAll([FromQuery] bool? activeOnly)
    {
        var query = _db.Partner.AsQueryable();
        if (activeOnly == true)
        {
            query = query.Where(p => p.IsActive);
        }

        var list = await query.OrderBy(p => p.DisplayOrder).ToListAsync();
        return Ok(list.Select(MapToDto));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<PartnerDto>> Create([FromBody] UpsertPartnerDto dto)
    {
        var partner = new Partner
        {
            Name = dto.Name,
            LogoPath = dto.LogoPath,
            Website = dto.Website,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive
        };

        _db.Partner.Add(partner);
        await _db.SaveChangesAsync();

        return Ok(MapToDto(partner));
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<PartnerDto>> Update(int id, [FromBody] UpsertPartnerDto dto)
    {
        var partner = await _db.Partner.FindAsync(id);
        if (partner == null) return NotFound();

        partner.Name = dto.Name;
        partner.LogoPath = dto.LogoPath;
        partner.Website = dto.Website;
        partner.DisplayOrder = dto.DisplayOrder;
        partner.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();
        return Ok(MapToDto(partner));
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)

    {
        var partner = await _db.Partner.FindAsync(id);
        if (partner == null) return NotFound();

        _db.Partner.Remove(partner);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static PartnerDto MapToDto(Partner p) =>
        new(p.Id, p.Name, p.LogoPath, p.Website, p.DisplayOrder, p.IsActive);
}
