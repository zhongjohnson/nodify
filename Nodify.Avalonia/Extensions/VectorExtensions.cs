using Avalonia;

namespace Nodify.Avalonia.Extensions;

public static class VectorExtensions
{
    public static double LengthSquared(this Vector vector) => vector.SquaredLength;
}