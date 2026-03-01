using MetalMetrics.Core.Enums;

namespace MetalMetrics.Core.Entities;

public class Job : BaseEntity
{
    public string JobNumber { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Quoted;
    public DateTime? CompletedAt { get; set; }

    public Tenant? Tenant { get; set; }
    public JobEstimate? Estimate { get; set; }
    public JobActuals? Actuals { get; set; }
    public ICollection<JobAssignment> Assignments { get; set; } = new List<JobAssignment>();
    public ICollection<JobTimeEntry> TimeEntries { get; set; } = new List<JobTimeEntry>();
    public ICollection<JobNote> Notes { get; set; } = new List<JobNote>();
    public ICollection<JobAttachment> Attachments { get; set; } = new List<JobAttachment>();
}
