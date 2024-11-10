#if Avalonia
using Avalonia;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;
#else
using System.Windows;
using System.Windows.Controls;
#endif

namespace Nodify
{
    /// <summary>
    /// Represents the default control for the <see cref="Node.OutputConnectorTemplate"/>.
    /// </summary>
    public class NodeOutput : Connector
    {
        #region Dependency Properties

#if Avalonia
        public static readonly StyledProperty<object?> HeaderProperty = HeaderedContentControl.HeaderProperty.AddOwner<NodeOutput>();
        public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty = HeaderedContentControl.HeaderTemplateProperty.AddOwner<NodeOutput>();
        public static readonly StyledProperty<ControlTemplate> ConnectorTemplateProperty = AvaloniaProperty.Register<NodeOutput, ControlTemplate>(nameof(ConnectorTemplate));
        public static readonly StyledProperty<Orientation> OrientationProperty = StackPanel.OrientationProperty.AddOwner<NodeOutput>();
#else
        public static readonly DependencyProperty HeaderProperty = HeaderedContentControl.HeaderProperty.AddOwner(typeof(NodeOutput));
        public static readonly DependencyProperty HeaderTemplateProperty = HeaderedContentControl.HeaderTemplateProperty.AddOwner(typeof(NodeOutput));
        public static readonly DependencyProperty ConnectorTemplateProperty = NodeInput.ConnectorTemplateProperty.AddOwner(typeof(NodeOutput));
        public static readonly DependencyProperty OrientationProperty = NodeInput.OrientationProperty.AddOwner(typeof(NodeOutput), new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure));
#endif

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
#if !Avalonia
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NodeOutput), new FrameworkPropertyMetadata(typeof(NodeOutput)));
#endif
        }
    }
}
