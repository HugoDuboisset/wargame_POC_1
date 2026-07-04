using System.Text.Json.Serialization;

namespace Wargame.Infrastructure.DataTransferObjects;

//reprend la structure du unit.json dans data
internal class UnitJsonDto
{
    public string UnitId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Faction { get; set; } = string.Empty;
    public int PointsCost { get; set; }
    public double BaseSizeInches { get; set; }
    public ProfileDto Profile { get; set; } = new();
    public List<ModelDto> Models { get; set; } = new();
}

internal class ProfileDto
{
    public int M { get; set; }
    public int T { get; set; }
    public int C { get; set; }
    public int I { get; set; }
    public int Mo { get; set; }
    public int Ca { get; set; }
}

internal class ModelDto
{
    public string ModelId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int MaxHp { get; set; }
    public int CurrentHp { get; set; }
    public PositionDto Position { get; set; } = new();
}

internal class PositionDto
{
    public double X { get; set; }
    public double Y { get; set; }
}