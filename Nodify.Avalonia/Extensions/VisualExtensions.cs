using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;

namespace Nodify.Avalonia.Extensions
{
    public static class VisualExtensions
    {
        public static Size RenderSize(this Visual e)
        {
            return e.Bounds.Size;
        }
    }
}
