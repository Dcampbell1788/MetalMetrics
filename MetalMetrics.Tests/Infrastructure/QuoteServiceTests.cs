using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Interfaces;
using MetalMetrics.Infrastructure.Services;

namespace MetalMetrics.Tests.Infrastructure;

[TestClass]
public class QuoteServiceTests
{
    [TestMethod]
    public void CalculateTotals_ComputesCorrectCostAndMargin()
    {
        var tenant = new FakeTenantProvider();
        var service = new QuoteService(null!, tenant);

        var estimate = new JobEstimate
        {
            EstimatedLaborHours = 10,
            LaborRate = 75,
            EstimatedMaterialCost = 500,
            EstimatedMachineHours = 5,
            MachineRate = 150,
            OverheadPercent = 15,
            QuotePrice = 3000
        };

        service.CalculateTotals(estimate);

        // Labor: 10 * 75 = 750
        // Material: 500
        // Machine: 5 * 150 = 750
        // Subtotal: 2000
        // Overhead: 2000 * 0.15 = 300
        // Total: 2300
        Assert.AreEqual(2300m, estimate.TotalEstimatedCost);

        // Margin: (3000 - 2300) / 3000 * 100 = 23.33...
        var expectedMargin = (3000m - 2300m) / 3000m * 100m;
        Assert.AreEqual(Math.Round(expectedMargin, 2), Math.Round(estimate.EstimatedMarginPercent, 2));
    }

    [TestMethod]
    public void CalculateTotals_ZeroQuotePrice_MarginIsZero()
    {
        var tenant = new FakeTenantProvider();
        var service = new QuoteService(null!, tenant);

        var estimate = new JobEstimate
        {
            EstimatedLaborHours = 10,
            LaborRate = 75,
            EstimatedMaterialCost = 500,
            EstimatedMachineHours = 5,
            MachineRate = 150,
            OverheadPercent = 15,
            QuotePrice = 0
        };

        service.CalculateTotals(estimate);

        Assert.AreEqual(0m, estimate.EstimatedMarginPercent);
    }

    private class FakeTenantProvider : ITenantProvider
    {
        public Guid TenantId { get; set; } = Guid.NewGuid();
    }
}
