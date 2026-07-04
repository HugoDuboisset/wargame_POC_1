using Wargame.Domain.Entities;

namespace Wargame.Application.Interfaces;

public interface IUnitRepository
{
    Task<IEnumerable<Unit>> GetAllUnitsAsync();
    Task<Unit?> GetUnitByIdAsync(string id);
}