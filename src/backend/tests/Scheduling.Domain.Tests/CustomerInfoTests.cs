using FluentAssertions;
using Scheduling.Domain.Common;
using Scheduling.Domain.ValueObjects;

namespace Scheduling.Domain.Tests;

public class CustomerInfoTests
{
    [Fact]
    public void Create_Normalizes_And_Trims()
    {
        var c = CustomerInfo.Create("  Gustavo  ", "  GUS@EXAMPLE.COM  ", "  123  ");
        c.Name.Should().Be("Gustavo");
        c.Email.Should().Be("gus@example.com");
        c.Phone.Should().Be("123");
    }

    [Fact]
    public void Create_Throws_When_Email_Missing()
    {
        var act = () => CustomerInfo.Create("A", " ", "1");
        act.Should().Throw<DomainException>();
    }
}
