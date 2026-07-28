using System;
using System.Collections.Generic;
using System.Text;

namespace System.Windows
{
    public partial struct Size
    {
        public static implicit operator global::Avalonia.Size(Size value) => new global::Avalonia.Size(value.Width, value.Height);

        public static implicit operator Size(global::Avalonia.Size value) => new Size(value.Width, value.Height);
    }
}
