using Avalonia;
using Avalonia.Controls;

namespace Nodify
{
    /// <summary>
    /// Represents a control that owns a <see cref="Connector"/>.
    /// </summary>
    public class KnotNode : ContentControl
    {
        static KnotNode()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(KnotNode), new StyledPropertyMetadata(typeof(KnotNode)));
            FocusableProperty.OverrideMetadata(typeof(KnotNode), new StyledPropertyMetadata(BoxValue.False));
        }
    }
}
