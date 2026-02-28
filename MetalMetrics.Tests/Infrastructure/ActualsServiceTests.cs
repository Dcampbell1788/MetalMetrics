using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Interfaces;
using MetalMetrics.Infrastructure.Services;

namespace MetalMetrics.Tests.Infrastructure;

[TestClass]
public class ActualsServiceTests
{
    [TestMethod]
    public void CalculateTotals_ComputesCorrectActualCost()
    {
        var tenant = new FakeTenantProvider();
        var service = new ActualsService(null!, tenant);

        var actuals = new JobActuals
        {
            ActualLaborHours = 12,
            LaborRate = 75,
            ActualMaterialCost = 600,
            ActualMachineHours = 6,
            MachineRate = 150,
            OverheadPercent = 15
        };

        service.CalculateTotals(actuals);

        // Labor: 12 * 75 = 900
        // Material: 600
        // Machine: 6 * 150 = 900
        // Subtotal: 2400
        // Overhead: 2400 * 0.15 = 360
        // Total: 2760
        Assert.AreEqual(2760m, actuals.TotalActualCost);
    }

    [TestMethod]
    public void CalculateTotals_ZeroValues_ReturnsZero()
    {
        var tenant = new FakeTenantProvider();
        var service = new ActualsService(null!, tenant);

        var actuals = new JobActuals();

        service.CalculateTotals(actuals);

        Assert.AreEqual(0m, actuals.TotalActualCost);
    }

    private class FakeTenantProvider : ITenantProvider
    {
        public Guid TenantId { get; set; } = Guid.NewGuid();
    }
}
