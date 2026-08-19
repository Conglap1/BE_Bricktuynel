namespace BrickShowcase.Domain.Entities;

public class ProjectImage
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public bool IsThumbnail { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;

    public Project? Project { get; set; }
}
