using FluentAssertions;
using Wargame.Domain.Entities;
using Wargame.Domain.ValueObjects;

namespace Wargame.Domain.Tests.Entities;

public class UnitTests
{
    // Méthode utilitaire pour générer une unité de test rapidement
    private Unit CreateTestUnit(int modelCount)
    {
        var profile = new UnitProfile(Movement: 6, Shooting: 6, Combat: 6, Initiative: 4, Morale: 3, ArmorClass: 2);
        var unit = new Unit("unit-1", "Escouade Test", "Test", 100, 1.0, profile);
        
        for (int i = 0; i < modelCount; i++)
        {
            // On fait spawner tout le monde au point (0,0)
            unit.AddModel(new Model($"m-{i}", $"Soldat {i}", 1, new Position(0, 0)));
        }
        
        return unit;
    }

    [Fact]
    public void CommitMovement_ShouldSucceed_WhenMovementAndCohesionAreValid()
    {
        // Arrange
        var unit = CreateTestUnit(3); // 3 figurines
        var proposedPositions = new Dictionary<string, Position>
        {
            { "m-0", new Position(0, 2) }, // Déplacement de 2" (Valide)
            { "m-1", new Position(0, 4) }, // À 2" du premier
            { "m-2", new Position(0, 6) }  // À 2" du deuxième
        };

        // Act
        unit.CommitMovement(proposedPositions);

        // Assert
        unit.Models.First(m => m.Id == "m-0").Position.Y.Should().Be(2);
        unit.Models.First(m => m.Id == "m-2").Position.Y.Should().Be(6);
    }

    [Fact]
    public void CommitMovement_ShouldThrow_WhenOneModelExceedsMovementLimit()
    {
        // Arrange
        var unit = CreateTestUnit(2);
        var proposedPositions = new Dictionary<string, Position>
        {
            { "m-0", new Position(0, 2) },
            { "m-1", new Position(0, 10) } // Mouvement de 10" ! Le max est 6"
        };

        // Act
        Action act = () => unit.CommitMovement(proposedPositions);

        // Assert
        // CORRECTION 1 : On met le bon texte pour correspondre à l'exception
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*dépasse son mouvement*"); 
    }

    [Fact]
    public void CommitMovement_ShouldThrow_WhenUnitIsSplitInTwoGroups()
    {
        var unit = CreateTestUnit(4); 
        

        var proposedPositions = new Dictionary<string, Position>
        {
            // Groupe A part vers le haut (Mouvement de 4")
            { "m-0", new Position(0, 4) },
            { "m-1", new Position(1, 4) },
            
            // Groupe B part vers le bas (Mouvement de 4")
            { "m-2", new Position(0, -4) },
            { "m-3", new Position(1, -4) }
        };
        // La distance entre le Groupe A et B sera de 8 pouces. 
        // La cohésion max est de 3 pouces (2" + 1" de socle). 

        // Act
        Action act = () => unit.CommitMovement(proposedPositions);

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*scindée en plusieurs groupes*");
    }
}