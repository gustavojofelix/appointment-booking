using FluentAssertions;
using Scheduling.Domain.Common;
using Scheduling.Domain.Entities;

namespace Scheduling.Domain.Tests;

public class SlotTests
{
    [Fact]
    public void Constructor_Throws_When_End_Before_Start()
    {
        var providerId = Guid.NewGuid();
        var start = new DateTime(2025, 12, 17, 10, 0, 0, DateTimeKind.Utc);
        var end = start.AddMinutes(-30);

        var act = () => new Slot(providerId, start, end);

        act.Should().Throw<DomainException>().WithMessage("*end must be after start*");
    }
}
