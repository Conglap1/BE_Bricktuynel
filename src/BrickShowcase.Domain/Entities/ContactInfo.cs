namespace BrickShowcase.Domain.Entities;

public class ContactInfo
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Hotline { get; set; }
    public string? Email { get; set; }
    public string? Facebook { get; set; }
    public string? Zalo { get; set; }
    public string? Tiktok { get; set; }
    public string? GoogleMapEmbed { get; set; }
    public string? WorkingHours { get; set; }
}
