using FluentAssertions;
using Wargame.Domain.Entities;
using Wargame.Domain.ValueObjects;

namespace Wargame.Domain.Tests.Entities;

public class ModelTests
{
    [Fact]
    public void TakeDamage_ShouldReduceHpAndDestroyModel_WhenHpReachesZero()
    {
        var startingPosition = new Position(0, 0);
        var model = new Model("m-001", "Soldat", maxHp: 1, startingPosition);

        model.TakeDamage(1);

        model.CurrentHp.Should().Be(0);
        model.IsDestroyed.Should().BeTrue();
    }

    [Fact]
    public void TakeDamage_ShouldNotDropHpBelowZero()
    {
        var startingPosition = new Position(0, 0);
        var model = new Model("m-001", "Soldat", maxHp: 1, startingPosition);

        model.TakeDamage(5);

        model.CurrentHp.Should().Be(0);
        model.IsDestroyed.Should().BeTrue();
    }
}