using MetalMetrics.Core.DTOs;
using MetalMetrics.Core.Entities;

namespace MetalMetrics.Core.Interfaces;

public interface IPdfService
{
    byte[] GenerateQuotePdf(string companyName, Job job, JobEstimate estimate);
    byte[] GenerateProfitabilityPdf(string companyName, Job job, JobProfitabilityReport report);
    byte[] GenerateReportsPdf(string companyName, DateTime from, DateTime to,
        List<JobSummaryDto> jobs, List<CustomerProfitabilityDto> customers,
        int jobCount, decimal totalRevenue, decimal avgMargin);
}
