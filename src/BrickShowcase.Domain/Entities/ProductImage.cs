namespace BrickShowcase.Domain.Entities;

public class ProductImage
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public bool IsThumbnail { get; set; } = false;
    public string? AltText { get; set; }
    public int DisplayOrder { get; set; } = 0;

    public Product? Product { get; set; }
}
