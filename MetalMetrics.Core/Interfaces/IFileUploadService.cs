using MetalMetrics.Core.Entities;
using Microsoft.AspNetCore.Http;

namespace MetalMetrics.Core.Interfaces;

public interface IFileUploadService
{
    Task<JobAttachment> UploadAsync(IFormFile file, Guid jobId, string category,
        string? description, string userId, string userName);
    Task<List<JobAttachment>> GetByJobIdAsync(Guid jobId);
    Task DeleteAsync(Guid attachmentId);
    (bool IsValid, string? Error) ValidateFile(IFormFile file);
}
