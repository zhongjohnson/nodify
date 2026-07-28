using System;
using System.Collections.Generic;
using System.Text;

namespace System.Windows.Media
{
    public partial struct Matrix
    {
        public static implicit operator global::Avalonia.Matrix(Matrix value) => new global::Avalonia.Matrix(value.M11, value.M12, value.M21, value.M22, value.OffsetX, value.OffsetY);

        public static implicit operator Matrix(global::Avalonia.Matrix value) => new Matrix(value.M11, value.M12, value.M21, value.M22, value.M31, value.M32);
    }
}
