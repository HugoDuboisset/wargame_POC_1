using Wargame.Domain.ValueObjects;

namespace Wargame.Domain.Entities;

// correspond à un personnage au sein d'une unité. 
public class Model
{
    public string Id { get; private set; }
    public string Name { get; private set; }
    public int MaxHp { get; private set; }
    public int CurrentHp { get; private set; }
    public Position Position { get; private set; }

    // La figurine est détruite si ses PV sont à 0 ou moins
    public bool IsDestroyed => CurrentHp <= 0;

    public Model(string id, string name, int maxHp, Position startingPosition)
    {
        Id = id;
        Name = name;
        MaxHp = maxHp;
        CurrentHp = maxHp;
        Position = startingPosition;
    }

    public void MoveTo(Position newPosition)
    {
        Position = newPosition;
    }

    public void TakeDamage(int damageAmount)
    {
        if (damageAmount < 0) return; // gestion de dégats négatifs (bugs)
        
        CurrentHp -= damageAmount;
        
        if (CurrentHp < 0)
        {
            CurrentHp = 0; // On ne descend pas en dessous de 0
        }
    }
}