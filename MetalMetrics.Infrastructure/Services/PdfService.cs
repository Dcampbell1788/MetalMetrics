using MetalMetrics.Core.DTOs;
using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MetalMetrics.Infrastructure.Services;

public class PdfService : IPdfService
{
    private const string ProfitColor = "#28a745";
    private const string LossColor = "#dc3545";
    private const string HeaderBg = "#1a1a2e";
    private const string AccentColor = "#f39c12";

    public byte[] GenerateQuotePdf(string companyName, Job job, JobEstimate estimate)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(40);

                page.Header().Element(c => ComposeHeader(c, companyName, $"Quote — {job.JobNumber}"));

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().Text(t =>
                    {
                        t.Span("Customer: ").SemiBold();
                        t.Span(job.CustomerName);
                    });
                    if (!string.IsNullOrEmpty(job.Description))
                    {
                        col.Item().Text(t =>
                        {
                            t.Span("Description: ").SemiBold();
                            t.Span(job.Description);
                        });
                    }

                    col.Item().PaddingTop(15).Text("Cost Breakdown").FontSize(14).SemiBold();

                    col.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(3);
                            c.RelativeColumn(3);
                            c.RelativeColumn(2);
                        });

                        table.Header(h =>
                        {
                            h.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Category").SemiBold();
                            h.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Details").SemiBold();
                            h.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Amount").SemiBold();
                        });

                        var laborCost = estimate.EstimatedLaborHours * estimate.LaborRate;
                        var machineCost = estimate.EstimatedMachineHours * estimate.MachineRate;
                        var subtotal = laborCost + estimate.EstimatedMaterialCost + machineCost;
                        var overhead = subtotal * (estimate.OverheadPercent / 100m);

                        AddRow(table, "Labor", $"{estimate.EstimatedLaborHours:F1} hrs x {estimate.LaborRate:C}/hr", laborCost);
                        AddRow(table, "Material", "", estimate.EstimatedMaterialCost);
                        AddRow(table, "Machine", $"{estimate.EstimatedMachineHours:F1} hrs x {estimate.MachineRate:C}/hr", machineCost);
                        AddRow(table, "Overhead", $"{estimate.OverheadPercent:F1}%", overhead);

                        table.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Total Estimated Cost").Bold();
                        table.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("");
                        table.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text(estimate.TotalEstimatedCost.ToString("C")).Bold();
                    });

                    col.Item().PaddingTop(15).Text("Pricing").FontSize(14).SemiBold();

                    var profit = estimate.QuotePrice - estimate.TotalEstimatedCost;
                    col.Item().PaddingTop(5).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(t => { t.Span("Quote Price: ").SemiBold(); t.Span(estimate.QuotePrice.ToString("C")); });
                            c.Item().Text(t => { t.Span("Estimated Margin: ").SemiBold(); t.Span($"{estimate.EstimatedMarginPercent:F1}%").FontColor(profit >= 0 ? ProfitColor : LossColor); });
                            c.Item().Text(t => { t.Span("Profit/Loss: ").SemiBold(); t.Span(profit.ToString("C")).FontColor(profit >= 0 ? ProfitColor : LossColor); });
                        });
                    });

                    if (estimate.AIGenerated)
                        col.Item().PaddingTop(10).Text("This estimate was generated with AI assistance.").FontSize(9).Italic().FontColor(Colors.Grey.Medium);

                    col.Item().PaddingTop(5).Text($"Created by {estimate.CreatedBy ?? "Unknown"} on {estimate.CreatedAt:MMM dd, yyyy h:mm tt}").FontSize(9).FontColor(Colors.Grey.Medium);
                });

                page.Footer().Element(ComposeFooter);
            });
        }).GeneratePdf();
    }

    public byte[] GenerateProfitabilityPdf(string companyName, Job job, JobProfitabilityReport report)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(40);

                page.Header().Element(c => ComposeHeader(c, companyName, $"Profitability — {job.JobNumber}"));

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().Text(t =>
                    {
                        t.Span("Customer: ").SemiBold();
                        t.Span(job.CustomerName);
                    });

                    // Verdict banner
                    var verdictColor = report.OverallVerdict == "Profit" ? ProfitColor : (report.OverallVerdict == "Loss" ? LossColor : Colors.Grey.Medium);
                    col.Item().PaddingTop(10).Background(verdictColor).Padding(10).AlignCenter().Text(t =>
                    {
                        t.Span($"{report.OverallVerdict.ToUpper()} ").FontSize(16).Bold().FontColor(Colors.White);
                        t.Span($"{(report.ActualMarginDollars >= 0 ? "+" : "")}{report.ActualMarginDollars:C}").FontSize(16).FontColor(Colors.White);
                        t.Line($"Actual Margin: {report.ActualMarginPercent:F1}%").FontSize(11).FontColor(Colors.White);
                    });

                    // Warnings
                    if (report.Warnings.Count > 0)
                    {
                        col.Item().PaddingTop(10).Column(wc =>
                        {
                            foreach (var w in report.Warnings)
                                wc.Item().PaddingBottom(2).Text($"  {w}").FontSize(9).FontColor(Colors.Orange.Darken2);
                        });
                    }

                    // Variance table
                    col.Item().PaddingTop(15).Text("Detailed Breakdown").FontSize(14).SemiBold();

                    col.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                            c.RelativeColumn(1.5f);
                        });

                        table.Header(h =>
                        {
                            h.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Category").SemiBold().FontSize(10);
                            h.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Estimated").SemiBold().FontSize(10);
                            h.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Actual").SemiBold().FontSize(10);
                            h.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Var ($)").SemiBold().FontSize(10);
                            h.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Var (%)").SemiBold().FontSize(10);
                        });

                        AddVarianceRow(table, "Labor", report.LaborVariance);
                        AddVarianceRow(table, "Material", report.MaterialVariance);
                        AddVarianceRow(table, "Machine", report.MachineVariance);
                        AddVarianceRow(table, "Overhead", report.OverheadVariance);

                        // Total row
                        var totalVar = report.TotalActualCost - report.TotalEstimatedCost;
                        var totalVarPct = report.TotalEstimatedCost != 0 ? totalVar / report.TotalEstimatedCost * 100 : 0;
                        table.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text("Total").Bold().FontSize(10);
                        table.Cell().Background(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(report.TotalEstimatedCost.ToString("C")).Bold().FontSize(10);
                        table.Cell().Background(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(report.TotalActualCost.ToString("C")).Bold().FontSize(10);
                        table.Cell().Background(Colors.Grey.Lighten2).Padding(4).AlignRight().Text($"{(totalVar >= 0 ? "+" : "")}{totalVar:C}").Bold().FontSize(10).FontColor(totalVar > 0 ? LossColor : ProfitColor);
                        table.Cell().Background(Colors.Grey.Lighten2).Padding(4).AlignRight().Text($"{(totalVarPct >= 0 ? "+" : "")}{totalVarPct:F1}%").Bold().FontSize(10).FontColor(totalVar > 0 ? LossColor : ProfitColor);
                    });

                    // Margin analysis
                    col.Item().PaddingTop(15).Text("Margin Analysis").FontSize(14).SemiBold();
                    col.Item().PaddingTop(5).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(t => { t.Span("Quote Price: ").SemiBold(); t.Span(report.QuotedPrice.ToString("C")); });
                            c.Item().Text(t => { t.Span("Actual Revenue: ").SemiBold(); t.Span(report.ActualRevenue.ToString("C")); });
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(t => { t.Span("Est. Margin: ").SemiBold(); t.Span($"{report.EstimatedMarginPercent:F1}%"); });
                            c.Item().Text(t => { t.Span("Margin Drift: ").SemiBold(); t.Span($"{(report.MarginDriftPercent >= 0 ? "+" : "")}{report.MarginDriftPercent:F1} pts").FontColor(report.MarginDriftPercent < 0 ? LossColor : ProfitColor); });
                        });
                    });
                });

                page.Footer().Element(ComposeFooter);
            });
        }).GeneratePdf();
    }

    public byte[] GenerateReportsPdf(string companyName, DateTime from, DateTime to,
        List<JobSummaryDto> jobs, List<CustomerProfitabilityDto> customers,
        int jobCount, decimal totalRevenue, decimal avgMargin)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(40);

                page.Header().Element(c => ComposeHeader(c, companyName, $"Report: {from:MMM dd, yyyy} — {to:MMM dd, yyyy}"));

                page.Content().PaddingVertical(10).Column(col =>
                {
                    // KPIs
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(t => { t.Span("Jobs in Period: ").SemiBold(); t.Span(jobCount.ToString()); });
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(t => { t.Span("Total Revenue: ").SemiBold(); t.Span(totalRevenue.ToString("C0")); });
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(t => { t.Span("Avg Margin: ").SemiBold(); t.Span($"{avgMargin:F1}%").FontColor(avgMargin >= 0 ? ProfitColor : LossColor); });
                        });
                    });

                    // Job History
                    col.Item().PaddingTop(15).Text("Job History").FontSize(14).SemiBold();

                    col.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(1.5f);
                            c.RelativeColumn(2);
                            c.RelativeColumn(1.5f);
                            c.RelativeColumn(1.5f);
                            c.RelativeColumn(1.2f);
                            c.RelativeColumn(1);
                            c.RelativeColumn(2);
                        });

                        table.Header(h =>
                        {
                            h.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Job #").SemiBold().FontSize(9);
                            h.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Customer").SemiBold().FontSize(9);
                            h.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text("Quote").SemiBold().FontSize(9);
                            h.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text("Actual").SemiBold().FontSize(9);
                            h.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text("Margin").SemiBold().FontSize(9);
                            h.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Status").SemiBold().FontSize(9);
                            h.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Completed").SemiBold().FontSize(9);
                        });

                        foreach (var j in jobs)
                        {
                            table.Cell().Padding(3).Text(j.JobNumber).FontSize(9);
                            table.Cell().Padding(3).Text(j.CustomerName).FontSize(9);
                            table.Cell().Padding(3).AlignRight().Text(j.QuotePrice.ToString("C0")).FontSize(9);
                            table.Cell().Padding(3).AlignRight().Text(j.TotalActualCost.ToString("C0")).FontSize(9);
                            table.Cell().Padding(3).AlignRight().Text($"{j.ActualMarginPercent:F1}%").FontSize(9).FontColor(j.ActualMarginPercent >= 0 ? ProfitColor : LossColor);
                            table.Cell().Padding(3).Text(j.Status).FontSize(9);
                            table.Cell().Padding(3).Text(j.CompletedAt.ToString("MMM dd, yyyy")).FontSize(9);
                        }
                    });

                    // Customer Profitability
                    if (customers.Count > 0)
                    {
                        col.Item().PaddingTop(15).Text("Customer Profitability").FontSize(14).SemiBold();

                        col.Item().PaddingTop(5).Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(3);
                                c.RelativeColumn(1);
                                c.RelativeColumn(2);
                                c.RelativeColumn(2);
                                c.RelativeColumn(1.5f);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Customer").SemiBold().FontSize(9);
                                h.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignCenter().Text("Jobs").SemiBold().FontSize(9);
                                h.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text("Revenue").SemiBold().FontSize(9);
                                h.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text("P/L").SemiBold().FontSize(9);
                                h.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text("Margin").SemiBold().FontSize(9);
                            });

                            foreach (var c in customers)
                            {
                                table.Cell().Padding(3).Text(c.CustomerName).FontSize(9);
                                table.Cell().Padding(3).AlignCenter().Text(c.JobCount.ToString()).FontSize(9);
                                table.Cell().Padding(3).AlignRight().Text(c.TotalRevenue.ToString("C0")).FontSize(9);
                                table.Cell().Padding(3).AlignRight().Text($"{(c.ProfitLoss >= 0 ? "+" : "")}{c.ProfitLoss:C0}").FontSize(9).FontColor(c.ProfitLoss >= 0 ? ProfitColor : LossColor);
                                table.Cell().Padding(3).AlignRight().Text($"{c.MarginPercent:F1}%").FontSize(9).FontColor(c.MarginPercent >= 0 ? ProfitColor : LossColor);
                            }
                        });
                    }
                });

                page.Footer().Element(ComposeFooter);
            });
        }).GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, string companyName, string title)
    {
        container.Background(HeaderBg).Padding(15).Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text(companyName).FontSize(16).Bold().FontColor(Colors.White);
                col.Item().Text(title).FontSize(12).FontColor(AccentColor);
            });
            row.ConstantItem(150).AlignRight().AlignMiddle()
                .Text($"Generated {DateTime.UtcNow:MMM dd, yyyy}").FontSize(9).FontColor(Colors.Grey.Lighten2);
        });
    }

    private static void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(t =>
        {
            t.Span("Generated by MetalMetrics | Page ").FontSize(8).FontColor(Colors.Grey.Medium);
            t.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
        });
    }

    private static void AddRow(TableDescriptor table, string category, string details, decimal amount)
    {
        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(category).FontSize(10);
        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(details).FontSize(10);
        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignRight().Text(amount.ToString("C")).FontSize(10);
    }

    private static void AddVarianceRow(TableDescriptor table, string category, VarianceDetail v)
    {
        var varColor = v.VarianceDollars > 0 ? LossColor : (v.VarianceDollars < 0 ? ProfitColor : Colors.Grey.Medium);

        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(category).FontSize(10);
        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(v.EstimatedAmount.ToString("C")).FontSize(10);
        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(v.ActualAmount.ToString("C")).FontSize(10);
        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignRight().Text($"{(v.VarianceDollars >= 0 ? "+" : "")}{v.VarianceDollars:C}").FontSize(10).FontColor(varColor);
        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignRight().Text($"{(v.VariancePercent >= 0 ? "+" : "")}{v.VariancePercent:F1}%").FontSize(10).FontColor(varColor);
    }
}
