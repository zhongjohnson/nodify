using System;
using System.Collections.Generic;
using System.Text;

namespace System.Windows
{
    public partial struct Vector
    {
        public static implicit operator global::Avalonia.Vector(Vector value) => new global::Avalonia.Vector(value.X, value.Y);

        public static implicit operator Vector(global::Avalonia.Vector value) => new Vector(value.X, value.Y);

        public static implicit operator Vector(Point value) => new Vector(value.X, value.Y);
    }
}
