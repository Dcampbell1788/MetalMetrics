using MetalMetrics.Core.Entities;

namespace MetalMetrics.Tests.Core;

[TestClass]
public class BaseEntityTests
{
    private class TestEntity : BaseEntity { }

    [TestMethod]
    public void NewEntity_Id_IsNonEmpty()
    {
        var entity = new TestEntity();
        Assert.AreNotEqual(Guid.Empty, entity.Id);
    }

    [TestMethod]
    public void TwoNewEntities_GetDifferentIds()
    {
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();
        Assert.AreNotEqual(entity1.Id, entity2.Id);
    }

    [TestMethod]
    public void NewEntity_TenantId_DefaultsToEmpty()
    {
        var entity = new TestEntity();
        Assert.AreEqual(Guid.Empty, entity.TenantId);
    }
}
