using System;
using System.Collections.Generic;
using System.Text;

namespace System.Windows
{
    public partial struct Point
    {
        public static implicit operator global::Avalonia.Point(Point value) => new global::Avalonia.Point(value.X, value.Y);

        public static implicit operator Point(global::Avalonia.Point value) => new Point(value.X, value.Y);

        // NOTE: the Vector -> Point conversion is intentionally NOT declared here; it already
        // exists as an explicit operator on System.Windows.Vector. Declaring it in both places
        // makes every `(Point)someVector` cast ambiguous (CS0457).

        public static implicit operator global::Avalonia.Vector(Point value) => new global::Avalonia.Vector(value.X, value.Y);

        public static implicit operator Point(global::Avalonia.Vector value) => new Point(value.X, value.Y);

        /// <summary>
        /// WPF-compatible <c>Point.Parse</c>. Delegates to Avalonia's parser, which accepts the
        /// same "x,y" form used by WPF (upstream passes command-parameter strings such as "10,20").
        /// </summary>
        public static Point Parse(string source) => global::Avalonia.Point.Parse(source);
    }
}
