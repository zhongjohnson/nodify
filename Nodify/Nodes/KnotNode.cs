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
            // In Avalonia, style keys are automatically inferred from type
            // No need to override DefaultStyleKeyProperty explicitly
            FocusableProperty.OverrideDefaultValue<KnotNode>(false);
        }
    }
}
