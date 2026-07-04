using Wargame.Domain.ValueObjects;

namespace Wargame.Domain.Entities;

// regroupe toutes les figurines d'une même unité
public class Unit
{
    public string Id { get; private set; }
    public string Name { get; private set; }
    public string Faction { get; private set; }
    public int PointsCost { get; private set; }
    public double BaseSizeInches { get; private set; }
    public UnitProfile Profile { get; private set; }
    
    // La liste des figurines est protégée. On ne peut pas la modifier de l'extérieur.
    private readonly List<Model> _models = new();
    public IReadOnlyList<Model> Models => _models.AsReadOnly();

    public Unit(string id, string name, string faction, int pointsCost, double baseSizeInches, UnitProfile profile)
    {
        Id = id;
        Name = name;
        Faction = faction;
        PointsCost = pointsCost;
        BaseSizeInches = baseSizeInches;
        Profile = profile;
    }

    // ajoute une figurine au déploiement
    public void AddModel(Model model)
    {
        _models.Add(model);
    }

    public void CommitMovement(Dictionary<string, Position> proposedPositions)
    {
        // 1. Vérifier que toutes les figurines proposées appartiennent bien à l'unité
        if (proposedPositions.Keys.Any(id => _models.All(m => m.Id != id)))
            throw new ArgumentException("Une ou plusieurs figurines n'appartiennent pas à cette unité.");

        // 2. Vérifier la caractéristique de Mouvement (M) pour CHAQUE figurine
        foreach (var kvp in proposedPositions)
        {
            var model = _models.First(m => m.Id == kvp.Key);
            var distance = model.Position.DistanceTo(kvp.Value);
            
            if (distance > Profile.Movement) 
                throw new InvalidOperationException($"La figurine {model.Name} a bougé de {distance}\", ce qui dépasse son mouvement de {Profile.Movement}\".");
        }

        // Créer la liste des positions finales hypothétiques de TOUTE l'unité
        var finalPositions = new List<Position>();
        foreach (var model in _models)
        {
            // Si la figurine bouge, on prend sa nouvelle position, sinon on garde l'actuelle
            if (proposedPositions.TryGetValue(model.Id, out var newPos))
                finalPositions.Add(newPos);
            else
                finalPositions.Add(model.Position);
        }

        // 3. Vérifier les chevauchements
        ValidateNoOverlap(finalPositions);

        // 4. Vérifier la Cohésion d'Unité
        ValidateCohesion(finalPositions);

        // 5. tout est valide, application de la nouvelle position
        foreach (var kvp in proposedPositions)
        {
            var model = _models.First(m => m.Id == kvp.Key);
            model.MoveTo(kvp.Value);
        }
    }

    private void ValidateCohesion(List<Position> newPositions)
    {
        // S'il ne reste qu'une seule figurine (ou 0), elle est toujours en cohésion.
        if (newPositions.Count <= 1) return;

        // Règle : > 5 figs = 2 voisines min. <= 5 figs = 1 voisine min.
        int requiredConnections = newPositions.Count > 5 ? 2 : 1;
        
        // Calcul de la distance max entre les centres (2" + le diamètre du socle)
        double maxDistanceForCohesion = 2.0 + BaseSizeInches;

        // 1. Construire la matrice d'adjacence (qui est à portée de qui ?)
        var adjacencyList = new Dictionary<int, List<int>>();
        for (int i = 0; i < newPositions.Count; i++)
        {
            adjacencyList[i] = new List<int>();
        }

        for (int i = 0; i < newPositions.Count; i++)
        {
            for (int j = i + 1; j < newPositions.Count; j++)
            {
                if (newPositions[i].DistanceTo(newPositions[j]) <= maxDistanceForCohesion)
                {
                    adjacencyList[i].Add(j);
                    adjacencyList[j].Add(i);
                }
            }
        }

        // 2. Vérifier la règle des voisines minimum pour chaque figurine
        for (int i = 0; i < newPositions.Count; i++)
        {
            if (adjacencyList[i].Count < requiredConnections)
            {
                throw new InvalidOperationException(
                    $"Rupture de cohésion : une figurine n'est pas à 2\" d'au moins {requiredConnections} autres figurines.");
            }
        }

        // 3. Vérifier que tout le groupe est connecté (Parcours en largeur - BFS)
        var visited = new HashSet<int>();
        var queue = new Queue<int>();
        
        queue.Enqueue(0); // On commence le parcours par la première figurine
        visited.Add(0);

        while (queue.Count > 0)
        {
            int current = queue.Dequeue();
            foreach (var neighbor in adjacencyList[current])
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        // Si on n'a pas visité toutes les figurines, c'est que l'unité est scindée
        if (visited.Count != newPositions.Count)
        {
            throw new InvalidOperationException(
                "Rupture de cohésion : l'unité est scindée en plusieurs groupes isolés.");
        }
    }

    private void ValidateNoOverlap(List<Position> positions)
    {
        for (int i = 0; i < positions.Count; i++)
        {
            for (int j = i + 1; j < positions.Count; j++)
            {
                // Si la distance entre les centres est strictement inférieure au diamètre du socle
                if (positions[i].DistanceTo(positions[j]) < BaseSizeInches)
                {
                    throw new InvalidOperationException(
                        "Mouvement invalide : le mouvement termine sur l'emplacement d'une autre figurine.");
                }
            }
        }
    }


}