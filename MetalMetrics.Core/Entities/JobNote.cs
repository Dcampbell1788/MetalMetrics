namespace MetalMetrics.Core.Entities;

public class JobNote : BaseEntity
{
    public Guid JobId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ImageFileName { get; set; }

    public Job? Job { get; set; }
}
