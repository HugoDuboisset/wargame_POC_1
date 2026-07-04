using System.Text.Json;
using Wargame.Application.Interfaces;
using Wargame.Domain.Entities;
using Wargame.Domain.ValueObjects;
using Wargame.Infrastructure.DataTransferObjects;

namespace Wargame.Infrastructure.Repositories;

public class JsonUnitRepository : IUnitRepository
{
    private readonly string _filePath;

    // On passera le chemin du fichier (ex: "../data/units.json") via l'injection de dépendances dans l'API
    public JsonUnitRepository(string filePath)
    {
        _filePath = filePath;
    }

    public async Task<IEnumerable<Unit>> GetAllUnitsAsync()
    {
        if (!File.Exists(_filePath))
            return Enumerable.Empty<Unit>();

        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var jsonContent = await File.ReadAllTextAsync(_filePath);
        
        var dtos = JsonSerializer.Deserialize<List<UnitJsonDto>>(jsonContent, jsonOptions);
        if (dtos == null) return Enumerable.Empty<Unit>();

        return dtos.Select(MapToDomain);
    }

    public async Task<Unit?> GetUnitByIdAsync(string id)
    {
        var allUnits = await GetAllUnitsAsync();
        return allUnits.FirstOrDefault(u => u.Id == id);
    }

    // Le Mapper : transformer la donnée du JSON en Entité
    private Unit MapToDomain(UnitJsonDto dto)
    {
        var profile = new UnitProfile(
            dto.Profile.M, 
            dto.Profile.T, 
            dto.Profile.C, 
            dto.Profile.I, 
            dto.Profile.Mo, 
            dto.Profile.Ca);

        var unit = new Unit(
            dto.UnitId, 
            dto.Name, 
            dto.Faction, 
            dto.PointsCost, 
            dto.BaseSizeInches, 
            profile);

        foreach (var modelDto in dto.Models)
        {
            var position = new Position(modelDto.Position.X, modelDto.Position.Y);
            var model = new Model(modelDto.ModelId, modelDto.Name, modelDto.MaxHp, position);
            
            if (modelDto.CurrentHp < modelDto.MaxHp)
            {
                model.TakeDamage(modelDto.MaxHp - modelDto.CurrentHp);
            }

            unit.AddModel(model);
        }

        return unit;
    }
}