using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.VisualTree;

namespace Nodify.Avalonia.Extensions
{
    public static class VisualExtensions
    {
        public static Size RenderSize(this Visual e)
        {
            return e.Bounds.Size;
        }

        public static T? GetParentOfType<T>(this Visual? visual, bool includeSelf = false) where T : class
        {
            return visual.FindAncestorOfType<T>(includeSelf);
        }

        public static T? GetChildOfType<T>(this Visual? visual) where T : class
        {
            return visual.FindDescendantOfType<T>();
        }

        public static double ActualWidth(this Visual? visual)  => visual.Bounds.Width;
        public static double ActualHeight(this Visual? visual) => visual.Bounds.Height;
    }
}
