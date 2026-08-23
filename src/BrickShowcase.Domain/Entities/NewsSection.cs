namespace BrickShowcase.Domain.Entities;

public class NewsSection
{
    public int Id { get; set; }
    public int NewsId { get; set; }
    public string? Question { get; set; }
    public string? Answer { get; set; }
    public int DisplayOrder { get; set; } = 0;

    public News? News { get; set; }
    public ICollection<NewsImage> Images { get; set; } = new List<NewsImage>();
}
