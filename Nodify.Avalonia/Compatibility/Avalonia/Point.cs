using System;
using System.Collections.Generic;
using System.Text;

namespace System.Windows
{
    public partial struct Point
    {
        public static implicit operator global::Avalonia.Point(Point value) => new global::Avalonia.Point(value.X, value.Y);

        public static implicit operator Point(global::Avalonia.Point value) => new Point(value.X, value.Y);

        public static implicit operator Point(Vector value) => new Point(value.X, value.Y);

        public static implicit operator global::Avalonia.Vector(Point value) => new global::Avalonia.Vector(value.X, value.Y);

        public static implicit operator Point(global::Avalonia.Vector value) => new Point(value.X, value.Y);
    }
}
