using System;
using System.Collections.Generic;
using System.Text;

namespace Nodify.Avalonia.Compatibility
{
    public static class MatrixExtensions
    {
        extension(Matrix value)
        {
            public bool TryInvert(out Matrix inverted)
            {
                var result = ((global::Avalonia.Matrix)value).TryInvert(out global::Avalonia.Matrix invertedMatrix);
                inverted = (Matrix)invertedMatrix;
                return result;
            }
        }
    }
}