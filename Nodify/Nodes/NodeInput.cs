using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Nodify
{
    /// <summary>
    /// Represents the default control for the <see cref="Node.InputConnectorTemplate"/>.
    /// </summary>
    public class NodeInput : Connector
    {
        #region Dependency Properties

        public static readonly StyledProperty HeaderProperty = HeaderedContentControl.HeaderProperty.AddOwner(typeof(NodeInput));
        public static readonly StyledProperty HeaderTemplateProperty = HeaderedContentControl.HeaderTemplateProperty.AddOwner(typeof(NodeInput));
        public static readonly StyledProperty ConnectorTemplateProperty = StyledProperty.Register(nameof(ConnectorTemplate), typeof(ControlTemplate), typeof(NodeInput));
        public static readonly StyledProperty OrientationProperty = StackPanel.OrientationProperty.AddOwner(typeof(NodeInput), new StyledPropertyMetadata(Orientation.Horizontal, StyledPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Gets of sets the data used for the control's header.
        /// </summary>
        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        /// <summary>
        /// Gets or sets the template used to display the content of the control's header.
        /// </summary>
        public DataTemplate HeaderTemplate
        {
            get => (DataTemplate)GetValue(HeaderTemplateProperty);
            set => SetValue(HeaderTemplateProperty, value);
        }

        /// <summary>
        /// Gets or sets the template used to display the connecting point of this <see cref="Connector"/>.
        /// </summary>
        public ControlTemplate ConnectorTemplate
        {
            get => (ControlTemplate)GetValue(ConnectorTemplateProperty);
            set => SetValue(ConnectorTemplateProperty, value);
        }

        /// <inheritdoc cref="StackPanel.Orientation" />
        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        #endregion

        static NodeInput()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NodeInput), new StyledPropertyMetadata(typeof(NodeInput)));
        }
    }
}
