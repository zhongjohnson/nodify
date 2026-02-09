using Avalonia;
using Avalonia.Metadata;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace Nodify
{
    /// <summary>
    /// Represents a control that acts as a <see cref="Connector"/>.
    /// </summary>
    public class StateNode : Connector
    {
        protected const string ElementContent = "PART_Content";

        #region Dependency Properties

        public static readonly StyledProperty<IBrush?> HighlightBrushProperty = ItemContainer.HighlightBrushProperty.AddOwner<StateNode>();
        // ContentPresenter properties - define our own since Avalonia's ContentPresenter might not expose these as styled properties
        public static readonly StyledProperty<object?> ContentProperty =
            AvaloniaProperty.Register<StateNode, object?>(nameof(Content));
        public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
            AvaloniaProperty.Register<StateNode, IDataTemplate?>(nameof(ContentTemplate));
        public static readonly StyledProperty<CornerRadius> CornerRadiusProperty = Border.CornerRadiusProperty.AddOwner<StateNode>();

        /// <summary>
        /// Gets or sets the brush used when the <see cref="PendingConnection.IsOverElementProperty"/> attached property is true for this <see cref="StateNode"/>.
        /// </summary>
        public IBrush? HighlightBrush
        {
            get => GetValue(HighlightBrushProperty);
            set => SetValue(HighlightBrushProperty, value);
        }

        /// <summary>
        /// Gets or sets the data for the control's content.
        /// </summary>
        public object? Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        /// <summary>
        /// Gets or sets the template used to display the content of the control's header.
        /// </summary>
        public IDataTemplate? ContentTemplate
        {
            get => GetValue(ContentTemplateProperty);
            set => SetValue(ContentTemplateProperty, value);
        }
        
        /// <summary>
        /// Gets or sets a value that represents the degree to which the corners of the <see cref="StateNode"/> are rounded.
        /// </summary>
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion
        
        /// <summary>
        /// Gets the <see cref="ContentControl"/> control of this <see cref="StateNode"/>.
        /// </summary>
        protected Control? ContentControl { get; private set; }

        static StateNode()
        {
            // In Avalonia, style keys are automatically inferred from type
            // No need to override DefaultStyleKeyProperty explicitly
            FocusableProperty.OverrideDefaultValue<StateNode>(false);
        }

        /// <inheritdoc />
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            ContentControl = e.NameScope.Find<Control>(ElementContent);
        }

        /// <inheritdoc />
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            // Do not raise PendingConnection events if clicked on content
            if (e.Source is Visual visual && (ContentControl == null || !visual.IsDescendantOf(ContentControl)))
            {
                base.OnPointerPressed(e);
            }
        }

        /// <inheritdoc />
        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            // Do not raise PendingConnection events if clicked on content
            if (e.Source is Visual visual && (ContentControl == null || !visual.IsDescendantOf(ContentControl)))
            {
                base.OnPointerReleased(e);
            }
        }
    }
}
