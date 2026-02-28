using MetalMetrics.Core.Entities;
using MetalMetrics.Infrastructure.Services;

namespace MetalMetrics.Tests.Infrastructure;

[TestClass]
public class ProfitabilityServiceTests
{
    private static (JobEstimate estimate, JobActuals actuals) CreateTestData(
        decimal estLaborHrs = 10, decimal estLaborRate = 75,
        decimal estMaterial = 500, decimal estMachineHrs = 5, decimal estMachineRate = 150,
        decimal estOverhead = 15, decimal quotePrice = 3000,
        decimal actLaborHrs = 10, decimal actLaborRate = 75,
        decimal actMaterial = 500, decimal actMachineHrs = 5, decimal actMachineRate = 150,
        decimal actOverhead = 15, decimal actRevenue = 3000)
    {
        var estimate = new JobEstimate
        {
            EstimatedLaborHours = estLaborHrs, LaborRate = estLaborRate,
            EstimatedMaterialCost = estMaterial, EstimatedMachineHours = estMachineHrs,
            MachineRate = estMachineRate, OverheadPercent = estOverhead, QuotePrice = quotePrice
        };
        var actuals = new JobActuals
        {
            ActualLaborHours = actLaborHrs, LaborRate = actLaborRate,
            ActualMaterialCost = actMaterial, ActualMachineHours = actMachineHrs,
            MachineRate = actMachineRate, OverheadPercent = actOverhead, ActualRevenue = actRevenue
        };
        return (estimate, actuals);
    }

    [TestMethod]
    public void Calculate_ProfitScenario_ReturnsProfit()
    {
        // Actuals match estimates, revenue = quote price = $3000, cost = $2300
        var (est, act) = CreateTestData();
        var report = ProfitabilityService.Calculate(est, act, 20m);

        Assert.AreEqual("Profit", report.OverallVerdict);
        Assert.AreEqual(2300m, report.TotalEstimatedCost);
        Assert.AreEqual(2300m, report.TotalActualCost);
        Assert.AreEqual(700m, report.ActualMarginDollars);
        Assert.IsTrue(report.ActualMarginPercent > 23m);
    }

    [TestMethod]
    public void Calculate_LossScenario_ReturnsLoss()
    {
        // Actual costs way over budget, revenue same
        var (est, act) = CreateTestData(
            actLaborHrs: 20, actMaterial: 1000, actMachineHrs: 10, actRevenue: 3000);

        var report = ProfitabilityService.Calculate(est, act, 20m);

        Assert.AreEqual("Loss", report.OverallVerdict);
        Assert.IsTrue(report.ActualMarginDollars < 0);
        Assert.IsTrue(report.Warnings.Count > 0);
    }

    [TestMethod]
    public void Calculate_BreakEven_ReturnsBreakEven()
    {
        // Revenue exactly equals cost: labor=10*75=750, material=500, machine=5*150=750 => subtotal=2000, overhead=300 => total=2300
        var (est, act) = CreateTestData(actRevenue: 2300m);

        var report = ProfitabilityService.Calculate(est, act, 20m);

        Assert.AreEqual("Break Even", report.OverallVerdict);
        Assert.AreEqual(0m, report.ActualMarginDollars);
    }

    [TestMethod]
    public void Calculate_CategoryOverBudget_GeneratesWarning()
    {
        // Material 25% over budget (500 -> 625+)
        var (est, act) = CreateTestData(actMaterial: 650);

        var report = ProfitabilityService.Calculate(est, act, 20m);

        Assert.IsTrue(report.Warnings.Any(w => w.Contains("Material")));
        Assert.IsTrue(report.MaterialVariance.VariancePercent > 20m);
    }

    [TestMethod]
    public void Calculate_BelowTargetMargin_GeneratesWarning()
    {
        // Revenue just above cost -> low margin
        var (est, act) = CreateTestData(actRevenue: 2400m);

        var report = ProfitabilityService.Calculate(est, act, 20m);

        Assert.IsTrue(report.Warnings.Any(w => w.Contains("below target")));
    }
}
