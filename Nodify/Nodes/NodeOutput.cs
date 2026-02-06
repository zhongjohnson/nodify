using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Nodify
{
    /// <summary>
    /// Represents the default control for the <see cref="Node.OutputConnectorTemplate"/>.
    /// </summary>
    public class NodeOutput : Connector
    {
        #region Dependency Properties
        
        public static readonly StyledProperty HeaderProperty = HeaderedContentControl.HeaderProperty.AddOwner(typeof(NodeOutput));
        public static readonly StyledProperty HeaderTemplateProperty = HeaderedContentControl.HeaderTemplateProperty.AddOwner(typeof(NodeOutput));
        public static readonly StyledProperty ConnectorTemplateProperty = NodeInput.ConnectorTemplateProperty.AddOwner(typeof(NodeOutput));
        public static readonly StyledProperty OrientationProperty = NodeInput.OrientationProperty.AddOwner(typeof(NodeOutput), new StyledPropertyMetadata(Orientation.Horizontal, StyledPropertyMetadataOptions.AffectsMeasure));

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

        static NodeOutput()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NodeOutput), new StyledPropertyMetadata(typeof(NodeOutput)));
        }
    }
}
