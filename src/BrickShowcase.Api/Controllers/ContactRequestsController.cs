using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BrickShowcase.Infrastructure.Data;
using BrickShowcase.Domain.Entities;
using BrickShowcase.Application.DTOs;

namespace BrickShowcase.Api.Controllers;

[ApiController]
[Route("api/contact-requests")]
public class ContactRequestsController : ControllerBase
{
    private readonly BrickDbContext _db;

    public ContactRequestsController(BrickDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ContactRequestDto>>> GetAll()
    {
        var list = await _db.ContactRequest.OrderByDescending(r => r.CreatedAt).ToListAsync();
        return Ok(list.Select(MapToDto));
    }

    [HttpPost]
    public async Task<ActionResult<ContactRequestDto>> Create([FromBody] CreateContactRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName) || string.IsNullOrWhiteSpace(dto.Phone))
        {
            return BadRequest("Họ tên và Số điện thoại là bắt buộc.");
        }

        var req = new ContactRequest
        {
            FullName = dto.FullName,
            Phone = dto.Phone,
            Email = dto.Email,
            Content = dto.Content ?? "",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.ContactRequest.Add(req);
        await _db.SaveChangesAsync();

        return Ok(MapToDto(req));
    }

    [HttpPut("{id}/mark-read")]
    public async Task<ActionResult<ContactRequestDto>> ToggleRead(int id)
    {
        var req = await _db.ContactRequest.FindAsync(id);
        if (req == null) return NotFound();

        req.IsRead = !req.IsRead;
        await _db.SaveChangesAsync();

        return Ok(MapToDto(req));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var req = await _db.ContactRequest.FindAsync(id);
        if (req == null) return NotFound();

        _db.ContactRequest.Remove(req);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static ContactRequestDto MapToDto(ContactRequest r) =>
        new(r.Id, r.FullName, r.Phone, r.Email, r.Content, r.IsRead, r.CreatedAt.ToString("yyyy-MM-dd HH:mm"));
}
