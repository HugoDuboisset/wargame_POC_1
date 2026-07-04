using Microsoft.AspNetCore.Mvc;
using Wargame.Application.Interfaces;
using Wargame.Domain.ValueObjects;

namespace Wargame.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly IUnitRepository _unitRepository;

    // L'interface IUnitRepository est injectée automatiquement par .NET
    public GameController(IUnitRepository unitRepository)
    {
        _unitRepository = unitRepository;
    }

    // Route : GET /api/game/units
    [HttpGet("units")]
    public async Task<IActionResult> GetUnits()
    {
        var units = await _unitRepository.GetAllUnitsAsync();
        return Ok(units);
    }

    // Route : POST /api/game/units/{id}/move
    [HttpPost("units/{id}/move")]
    public async Task<IActionResult> MoveUnit(string id, [FromBody] Dictionary<string, Position> proposedPositions)
    {
        var unit = await _unitRepository.GetUnitByIdAsync(id);
        
        if (unit == null) 
            return NotFound($"Unité {id} introuvable sur la table.");

        try
        {
            // On tente de valider et d'appliquer le mouvement via notre Aggregate Root
            unit.CommitMovement(proposedPositions);

            // Note POC : Comme nous n'avons pas de vraie base de données, l'état n'est 
            // modifié qu'en mémoire. Dans un vrai jeu, on ferait ici :
            // await _unitRepository.SaveAsync(unit);

            return Ok(unit);
        }
        catch (InvalidOperationException ex)
        {
            // Si la cohésion est rompue ou le mouvement dépassé, on renvoie une erreur 400
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Une erreur serveur est survenue." });
        }
    }
}