using System.Collections;

#if Avalonia
using Avalonia;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Controls.Presenters;
using Avalonia.Interactivity;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Styling;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
#endif

namespace Nodify
{
    /// <summary>
    /// Represents a control that has a list of <see cref="Input"/> <see cref="Connector"/>s and a list of <see cref="Output"/> <see cref="Connector"/>s.
    /// </summary>
    public class Node : HeaderedContentControl
    {
        #region Dependency Properties
#if Avalonia
        public static readonly StyledProperty<IBrush> ContentBrushProperty = AvaloniaProperty.Register<Node, IBrush>(nameof(ContentBrush));
        public static readonly StyledProperty<IBrush> HeaderBrushProperty = AvaloniaProperty.Register<Node, IBrush>(nameof(HeaderBrush));
        public static readonly StyledProperty<IBrush> FooterBrushProperty = AvaloniaProperty.Register<Node, IBrush>(nameof(FooterBrush));
        public static readonly StyledProperty<object?> FooterProperty = AvaloniaProperty.Register<Node, object?>(nameof(Footer));
        public static readonly StyledProperty<DataTemplate> FooterTemplateProperty = AvaloniaProperty.Register<Node, DataTemplate>(nameof(FooterTemplate));
        public static readonly StyledProperty<DataTemplate> InputConnectorTemplateProperty = AvaloniaProperty.Register<Node, DataTemplate>(nameof(InputConnectorTemplate));
        public static readonly DirectProperty<Node, bool> HasFooterProperty =
            AvaloniaProperty.RegisterDirect<Node, bool>(nameof(HasFooter), o => o.HasFooter);
        public static readonly DirectProperty<Node, bool> HasHeaderProperty =
            AvaloniaProperty.RegisterDirect<Node, bool>(nameof(HasHeader), o => o.HasHeader);
        public static readonly StyledProperty<DataTemplate> OutputConnectorTemplateProperty = AvaloniaProperty.Register<Node, DataTemplate>(nameof(OutputConnectorTemplate));
        public static readonly StyledProperty<IEnumerable> InputProperty = AvaloniaProperty.Register<Node, IEnumerable>(nameof(Input));
        public static readonly StyledProperty<IEnumerable> OutputProperty = AvaloniaProperty.Register<Node, IEnumerable>(nameof(Output));
        public static readonly StyledProperty<Style> ContentContainerStyleProperty = AvaloniaProperty.Register<Node, Style>(nameof(ContentContainerStyle));
        public static readonly StyledProperty<Style> HeaderContainerStyleProperty = AvaloniaProperty.Register<Node, Style>(nameof(HeaderContainerStyle));
        public static readonly StyledProperty<Style> FooterContainerStyleProperty = AvaloniaProperty.Register<Node, Style>(nameof(FooterContainerStyle));

        private bool _hasHeader;
        private bool _hasFooter;
        
        /// <summary>
        /// Gets a value that indicates whether the <see cref="Footer"/> is <see langword="null" />.
        /// </summary>
        public bool HasFooter
        {
            get => _hasFooter;
            private set => this.SetAndRaise(HasFooterProperty, ref _hasFooter, value);
        }

        public bool HasHeader
        {
            get => _hasHeader;
            private set => this.SetAndRaise(HasHeaderProperty, ref _hasHeader, value);
        }
#else
        public static readonly DependencyProperty ContentBrushProperty = DependencyProperty.Register(nameof(ContentBrush), typeof(Brush), typeof(Node));
        public static readonly DependencyProperty HeaderBrushProperty = DependencyProperty.Register(nameof(HeaderBrush), typeof(Brush), typeof(Node));
        public static readonly DependencyProperty FooterBrushProperty = DependencyProperty.Register(nameof(FooterBrush), typeof(Brush), typeof(Node));
        public static readonly DependencyProperty FooterProperty = DependencyProperty.Register(nameof(Footer), typeof(object), typeof(Node), new FrameworkPropertyMetadata(OnFooterChanged));
        public static readonly DependencyProperty FooterTemplateProperty = DependencyProperty.Register(nameof(FooterTemplate), typeof(DataTemplate), typeof(Node));
        public static readonly DependencyProperty InputConnectorTemplateProperty = DependencyProperty.Register(nameof(InputConnectorTemplate), typeof(DataTemplate), typeof(Node));
        protected internal static readonly DependencyPropertyKey HasFooterPropertyKey = DependencyProperty.RegisterReadOnly(nameof(HasFooter), typeof(bool), typeof(Node), new FrameworkPropertyMetadata(BoxValue.False));
        public static readonly DependencyProperty HasFooterProperty = HasFooterPropertyKey.DependencyProperty;
        public static readonly DependencyProperty OutputConnectorTemplateProperty = DependencyProperty.Register(nameof(OutputConnectorTemplate), typeof(DataTemplate), typeof(Node));
        public static readonly DependencyProperty InputProperty = DependencyProperty.Register(nameof(Input), typeof(IEnumerable), typeof(Node));
        public static readonly DependencyProperty OutputProperty = DependencyProperty.Register(nameof(Output), typeof(IEnumerable), typeof(Node));
        public static readonly DependencyProperty ContentContainerStyleProperty = DependencyProperty.Register(nameof(ContentContainerStyle), typeof(Style), typeof(Node));
        public static readonly DependencyProperty HeaderContainerStyleProperty = DependencyProperty.Register(nameof(HeaderContainerStyle), typeof(Style), typeof(Node));
        public static readonly DependencyProperty FooterContainerStyleProperty = DependencyProperty.Register(nameof(FooterContainerStyle), typeof(Style), typeof(Node));
#endif
        /// <summary>
        /// Gets or sets the brush used for the background of the <see cref="ContentControl.Content"/> of this <see cref="Node"/>.
        /// </summary>
        public Brush ContentBrush
        {
            get => (Brush)GetValue(ContentBrushProperty);
            set => SetValue(ContentBrushProperty, value);
        }

        /// <summary>
        /// Gets or sets the brush used for the background of the <see cref="HeaderedContentControl.Header"/> of this <see cref="Node"/>.
        /// </summary>
        public Brush HeaderBrush
        {
            get => (Brush)GetValue(HeaderBrushProperty);
            set => SetValue(HeaderBrushProperty, value);
        }

        /// <summary>
        /// Gets or sets the brush used for the background of the <see cref="Node.Footer"/> of this <see cref="Node"/>.
        /// </summary>
        public Brush FooterBrush
        {
            get => (Brush)GetValue(FooterBrushProperty);
            set => SetValue(FooterBrushProperty, value);
        }
        
        /// <summary>
        /// Gets or sets the data for the footer of this control.
        /// </summary>
        public object Footer
        {
            get => GetValue(FooterProperty);
            set => SetValue(FooterProperty, value);
        }

        /// <summary>
        /// Gets or sets the template used to display the content of the control's footer.
        /// </summary>
        public DataTemplate FooterTemplate
        {
            get => (DataTemplate)GetValue(FooterTemplateProperty);
            set => SetValue(FooterTemplateProperty, value);
        }
        
        /// <summary>
        /// Gets or sets the template used to display the content of the control's <see cref="Input"/> connectors.
        /// </summary>
        public DataTemplate InputConnectorTemplate
        {
            get => (DataTemplate)GetValue(InputConnectorTemplateProperty);
            set => SetValue(InputConnectorTemplateProperty, value);
        }
        
        /// <summary>
        /// Gets or sets the template used to display the content of the control's <see cref="Output"/> connectors.
        /// </summary>
        public DataTemplate OutputConnectorTemplate
        {
            get => (DataTemplate)GetValue(OutputConnectorTemplateProperty);
            set => SetValue(OutputConnectorTemplateProperty, value);
        }
        
        /// <summary>
        /// Gets or sets the data for the input <see cref="Connector"/>s of this control.
        /// </summary>
        public IEnumerable Input
        {
            get => (IEnumerable)GetValue(InputProperty);
            set => SetValue(InputProperty, value);
        }
        
        /// <summary>
        /// Gets or sets the data for the output <see cref="Connector"/>s of this control.
        /// </summary>
        public IEnumerable Output
        {
            get => (IEnumerable)GetValue(OutputProperty);
            set => SetValue(OutputProperty, value);
        }

        /// <summary>
        /// Gets or sets the style for the content container.
        /// </summary>
        public Style ContentContainerStyle
        {
            get => (Style)GetValue(ContentContainerStyleProperty);
            set => SetValue(ContentContainerStyleProperty, value);
        }

        /// <summary>
        /// Gets or sets the style for the header container.
        /// </summary>
        public Style HeaderContainerStyle
        {
            get => (Style)GetValue(HeaderContainerStyleProperty);
            set => SetValue(HeaderContainerStyleProperty, value);
        }

        /// <summary>
        /// Gets or sets the style for the footer container.
        /// </summary>
        public Style FooterContainerStyle
        {
            get => (Style)GetValue(FooterContainerStyleProperty);
            set => SetValue(FooterContainerStyleProperty, value);
        }

#if !Avalonia
        /// <summary>
        /// Gets a value that indicates whether the <see cref="Footer"/> is <see langword="null" />.
        /// </summary>
        public bool HasFooter => (bool)GetValue(HasFooterProperty);

        private static void OnFooterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Node node = (Node)d;
            node.SetValue(HasFooterPropertyKey, e.NewValue != null ? BoxValue.True : BoxValue.False);
        }
#endif

#endregion

        static Node()
        {
#if Avalonia
            FooterProperty.Changed.AddClassHandler<Node, object?>((o, e) => o.HasFooter = e.NewValue.Value != null);
            HeaderProperty.Changed.AddClassHandler<Node, object?>((o, e) => o.HasHeader = e.NewValue.Value != null);
#else
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Node), new FrameworkPropertyMetadata(typeof(Node)));
#endif
        }
    }
}
