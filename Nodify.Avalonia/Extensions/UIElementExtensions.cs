using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;
using UIElement = Avalonia.Controls.Control;
using UIElementCollection = Avalonia.Controls.Controls;
using FrameworkElement = Avalonia.Controls.Control;

namespace Nodify.Avalonia.Extensions
{
    public static class UIElementExtensions
    {
        public static List<FrameworkElement> GetIntersectingElements(this UIElement container, Geometry geometry, IReadOnlyCollection<Type> supportedTypes)
        {
            // TODO:
            return null;
        }
    }
}
