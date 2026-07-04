namespace Wargame.Domain.ValueObjects;

public record Position(double X, double Y)
{
    // gère le positionnement des unités, pour que ce ne soit pas le front qui s'en occupe. Ce sont des pixels et plus des pouces
    public double DistanceTo(Position target)
    {
        var dx = X - target.X;
        var dy = Y - target.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}