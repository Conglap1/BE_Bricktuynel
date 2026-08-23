namespace BrickShowcase.Domain.Entities;

public class NewsImage
{
    public int Id { get; set; }
    public int NewsId { get; set; }
    public int? NewsSectionId { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public int DisplayOrder { get; set; } = 0;

    public News? News { get; set; }
    public NewsSection? NewsSection { get; set; }
}
