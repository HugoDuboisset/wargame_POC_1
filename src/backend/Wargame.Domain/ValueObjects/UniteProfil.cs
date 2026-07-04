namespace Wargame.Domain.ValueObjects;

// regroupe toutes les caractéristiques du profil des unités

public record UnitProfile(
    int Movement,      // M
    int Shooting,      // T
    int Combat,        // C
    int Initiative,    // I
    int Morale,        // Mo
    int ArmorClass     // CA
);