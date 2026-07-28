using System;
using System.Collections.Generic;
using System.Text;

namespace System.Windows
{
    public partial struct Rect
    {
        public static implicit operator global::Avalonia.Rect(Rect value) => new global::Avalonia.Rect(value.X, value.Y, value.Width, value.Height);

        public static implicit operator Rect(global::Avalonia.Rect value) => new Rect(value.X, value.Y, value.Width, value.Height);
    }
}
