using EduHub.Domain.Entities;

namespace EduHub.Tests.Domain;

// Test mẫu cho lớp Domain (chạy được ngay, không cần database)
public class EventTests
{
    [Fact]
    public void IsFull_WhenRegistrationsReachCapacity_ReturnsTrue()
    {
        var ev = Event.Create("Workshop", Guid.NewGuid(), DateTime.UtcNow.AddDays(1), 2);
        Assert.False(ev.IsFull(1));
        Assert.True(ev.IsFull(2));
    }

    [Fact]
    public void Create_WithNonPositiveCapacity_Throws()
    {
        Assert.Throws<ArgumentException>(() => Event.Create("Workshop", Guid.NewGuid(), DateTime.UtcNow.AddDays(1), 0));
    }
}
