using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Interfaces;
using MetalMetrics.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace MetalMetrics.Infrastructure.Services;

public class FileUploadService : IFileUploadService
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    private static readonly Dictionary<string, (long MaxSize, string[] Extensions)> CategoryRules = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Photo"] = (5 * 1024 * 1024, new[] { ".jpg", ".jpeg", ".png", ".gif" }),
        ["Document"] = (10 * 1024 * 1024, new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx" }),
        ["Drawing"] = (25 * 1024 * 1024, new[] { ".pdf", ".dxf", ".dwg", ".step", ".stp" }),
        ["PO"] = (10 * 1024 * 1024, new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx" }),
        ["Other"] = (10 * 1024 * 1024, new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png", ".gif" })
    };

    public FileUploadService(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public (bool IsValid, string? Error) ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return (false, "No file selected.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allAllowedExtensions = CategoryRules.Values.SelectMany(r => r.Extensions).Distinct().ToHashSet();

        if (!allAllowedExtensions.Contains(ext))
            return (false, $"File type '{ext}' is not allowed. Allowed: {string.Join(", ", allAllowedExtensions.OrderBy(e => e))}");

        var maxSize = CategoryRules.Values.Max(r => r.MaxSize);
        if (file.Length > maxSize)
            return (false, $"File is too large. Maximum size is {maxSize / (1024 * 1024)}MB.");

        return (true, null);
    }

    public async Task<JobAttachment> UploadAsync(IFormFile file, Guid jobId, string category,
        string? description, string userId, string userName)
    {
        // Validate category-specific rules
        if (CategoryRules.TryGetValue(category, out var rules))
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!rules.Extensions.Contains(ext))
                throw new InvalidOperationException($"File type '{ext}' is not allowed for category '{category}'.");
            if (file.Length > rules.MaxSize)
                throw new InvalidOperationException($"File is too large for category '{category}'. Max: {rules.MaxSize / (1024 * 1024)}MB.");
        }

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "attachments");
        Directory.CreateDirectory(uploadsDir);

        var storedFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var filePath = Path.Combine(uploadsDir, storedFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var attachment = new JobAttachment
        {
            JobId = jobId,
            FileName = file.FileName,
            StoredFileName = storedFileName,
            ContentType = file.ContentType,
            FileSizeBytes = file.Length,
            Category = category,
            Description = description,
            UploadedByUserId = userId,
            UploadedByName = userName
        };

        _db.JobAttachments.Add(attachment);
        await _db.SaveChangesAsync();

        return attachment;
    }

    public async Task<List<JobAttachment>> GetByJobIdAsync(Guid jobId)
    {
        return await _db.JobAttachments
            .Where(a => a.JobId == jobId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task DeleteAsync(Guid attachmentId)
    {
        var attachment = await _db.JobAttachments.FindAsync(attachmentId);
        if (attachment == null) return;

        // Delete file from disk
        var filePath = Path.Combine(_env.WebRootPath, "uploads", "attachments", attachment.StoredFileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        _db.JobAttachments.Remove(attachment);
        await _db.SaveChangesAsync();
    }
}
