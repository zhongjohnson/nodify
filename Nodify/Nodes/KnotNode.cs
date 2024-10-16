#if Avalonia
using Avalonia;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Controls;
#else
using System.Windows;
using System.Windows.Controls;
#endif

namespace Nodify
{
    /// <summary>
    /// Represents a control that owns a <see cref="Connector"/>.
    /// </summary>
    public class KnotNode : ContentControl
    {
        static KnotNode()
        {
#if !Avalonia
            DefaultStyleKeyProperty.OverrideMetadata(typeof(KnotNode), new FrameworkPropertyMetadata(typeof(KnotNode)));
#endif
        }
    }
}
