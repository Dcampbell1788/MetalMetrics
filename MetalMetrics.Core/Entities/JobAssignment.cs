namespace MetalMetrics.Core.Entities;

public class JobAssignment : BaseEntity
{
    public Guid JobId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string AssignedByUserId { get; set; } = string.Empty;

    public Job? Job { get; set; }
    public AppUser? User { get; set; }
}
