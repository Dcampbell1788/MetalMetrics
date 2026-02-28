namespace MetalMetrics.Core.Entities;

public class JobTimeEntry : BaseEntity
{
    public Guid JobId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public decimal HoursWorked { get; set; }
    public DateTime WorkDate { get; set; }
    public string? Notes { get; set; }

    public Job? Job { get; set; }
    public AppUser? User { get; set; }
}
