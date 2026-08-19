using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BrickShowcase.Infrastructure.Data;
using BrickShowcase.Domain.Entities;
using BrickShowcase.Application.DTOs;

namespace BrickShowcase.Api.Controllers;

[ApiController]
[Route("api/contact-info")]
public class ContactInfoController : ControllerBase
{
    private readonly BrickDbContext _db;

    public ContactInfoController(BrickDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<ContactInfoDto>> Get()
    {
        var info = await _db.ContactInfo.FirstOrDefaultAsync();
        if (info == null)
        {
            info = new ContactInfo
            {
                CompanyName = "Công ty TNHH Gạch Thuận Lợi",
                Address = "KCN Mỹ Phước, Bến Cát, Bình Dương",
                Phone = "0908 555 888",
                Hotline = "1900 1234",
                Email = "kinhdoanh@gachthuanloi.vn",
                WorkingHours = "T2 – T7 · 07:30 – 17:30"
            };
            _db.ContactInfo.Add(info);
            await _db.SaveChangesAsync();
        }
        else if (info.CompanyName.Contains("Tuynel") || info.CompanyName.Contains("Trường Sơn"))
        {
            info.CompanyName = "Công ty TNHH Gạch Thuận Lợi";
            await _db.SaveChangesAsync();
        }

        return Ok(MapToDto(info));
    }

    [HttpPut]
    [Authorize]
    public async Task<ActionResult<ContactInfoDto>> Update([FromBody] ContactInfoDto dto)

    {
        var info = await _db.ContactInfo.FirstOrDefaultAsync();
        if (info == null)
        {
            info = new ContactInfo();
            _db.ContactInfo.Add(info);
        }

        info.CompanyName = dto.CompanyName;
        info.Address = dto.Address;
        info.Phone = dto.Phone;
        info.Hotline = dto.Hotline;
        info.Email = dto.Email;
        info.Facebook = dto.Facebook;
        info.Zalo = dto.Zalo;
        info.Tiktok = dto.Tiktok;
        info.GoogleMapEmbed = dto.GoogleMapEmbed;
        info.WorkingHours = dto.WorkingHours;

        await _db.SaveChangesAsync();
        return Ok(MapToDto(info));
    }

    private static ContactInfoDto MapToDto(ContactInfo c) =>
        new(c.Id, c.CompanyName, c.Address, c.Phone, c.Hotline, c.Email, c.Facebook, c.Zalo, c.Tiktok, c.GoogleMapEmbed, c.WorkingHours);
}
