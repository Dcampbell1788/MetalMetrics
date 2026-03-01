namespace MetalMetrics.Core.Entities;

public class JobAttachment : BaseEntity
{
    public Guid JobId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string Category { get; set; } = "Document";
    public string? Description { get; set; }
    public string UploadedByUserId { get; set; } = string.Empty;
    public string UploadedByName { get; set; } = string.Empty;

    public Job? Job { get; set; }
}
