using Nodify.Events;
using Nodify.Interactivity;
using System;
using System.ComponentModel;
using System.Globalization;
using Avalonia;
using System.Windows.Input;
using Avalonia.Interactivity;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Controls.Shapes;

namespace Nodify
{
    /// <summary>
    /// Specifies the offset type that can be applied to a <see cref="BaseConnection"/> using the <see cref="BaseConnection.SourceOffset"/> and the <see cref="BaseConnection.TargetOffset"/> values.
    /// </summary>
    public enum ConnectionOffsetMode
    {
        /// <summary>
        /// No offset applied.
        /// </summary>
        None,

        /// <summary>
        /// The offset is applied in a circle around the point.
        /// </summary>
        Circle,

        /// <summary>
        /// The offset is applied in a rectangle shape around the point.
        /// </summary>
        Rectangle,

        /// <summary>
        /// The offset is applied in a rectangle shape around the point, perpendicular to the edges.
        /// </summary>
        Edge,

        /// <summary>
        /// The offset is applied as a fixed margin.
        /// </summary>
        Static
    }

    /// <summary>
    /// The direction in which a connection is oriented.
    /// </summary>
    public enum ConnectionDirection
    {
        /// <summary>
        /// From <see cref="BaseConnection.Source"/> to <see cref="BaseConnection.Target"/>.
        /// </summary>
        Forward,

        /// <summary>
        /// From <see cref="BaseConnection.Target"/> to <see cref="BaseConnection.Source"/>.
        /// </summary>
        Backward
    }

    /// <summary>
    /// The end at which the arrow head is drawn.
    /// </summary>
    public enum ArrowHeadEnds
    {
        /// <summary>
        /// Arrow head at start.
        /// </summary>
        Start,

        /// <summary>
        /// Arrow head at end.
        /// </summary>
        End,

        /// <summary>
        /// Arrow heads at both ends.
        /// </summary>
        Both,

        /// <summary>
        /// No arrow head.
        /// </summary>
        None
    }

    /// <summary>
    /// The shape of the arrowhead.
    /// </summary>
    public enum ArrowHeadShape
    {
        /// <summary>
        /// The default arrowhead.
        /// </summary>
        Arrowhead,

        /// <summary>
        /// An ellipse.
        /// </summary>
        Ellipse,

        /// <summary>
        /// A rectangle.
        /// </summary>
        Rectangle
    }

    /// <summary>
    /// Represents the base class for shapes that are drawn from a <see cref="Source"/> point to a <see cref="Target"/> point.
    /// </summary>
    public abstract class BaseConnection : Shape, IKeyboardFocusTarget<Control>
    {
        #region Avalonia Properties

        public static readonly StyledProperty<Point> SourceProperty =
            AvaloniaProperty.Register<BaseConnection, Point>(nameof(Source), defaultValue: default(Point));

        public static readonly StyledProperty<Point> TargetProperty =
            AvaloniaProperty.Register<BaseConnection, Point>(nameof(Target), defaultValue: default(Point));

        public static readonly StyledProperty<Size> SourceOffsetProperty =
            AvaloniaProperty.Register<BaseConnection, Size>(nameof(SourceOffset), defaultValue: new Size(14, 0));

        public static readonly StyledProperty<Size> TargetOffsetProperty =
            AvaloniaProperty.Register<BaseConnection, Size>(nameof(TargetOffset), defaultValue: new Size(14, 0));

        public static readonly StyledProperty<ConnectionOffsetMode> SourceOffsetModeProperty =
            AvaloniaProperty.Register<BaseConnection, ConnectionOffsetMode>(nameof(SourceOffsetMode), defaultValue: ConnectionOffsetMode.Static);

        public static readonly StyledProperty<ConnectionOffsetMode> TargetOffsetModeProperty =
            AvaloniaProperty.Register<BaseConnection, ConnectionOffsetMode>(nameof(TargetOffsetMode), defaultValue: ConnectionOffsetMode.Static);

        public static readonly StyledProperty<Orientation> SourceOrientationProperty =
            AvaloniaProperty.Register<BaseConnection, Orientation>(nameof(SourceOrientation), defaultValue: Orientation.Horizontal);

        public static readonly StyledProperty<Orientation> TargetOrientationProperty =
            AvaloniaProperty.Register<BaseConnection, Orientation>(nameof(TargetOrientation), defaultValue: Orientation.Horizontal);

        public static readonly StyledProperty<ConnectionDirection> DirectionProperty =
            AvaloniaProperty.Register<BaseConnection, ConnectionDirection>(nameof(Direction), defaultValue: default(ConnectionDirection));

        public static readonly StyledProperty<uint> DirectionalArrowsCountProperty =
            AvaloniaProperty.Register<BaseConnection, uint>(nameof(DirectionalArrowsCount), defaultValue: 0);

        public static readonly StyledProperty<double> DirectionalArrowsOffsetProperty =
            AvaloniaProperty.Register<BaseConnection, double>(nameof(DirectionalArrowsOffset), defaultValue: 0.0);

        public static readonly StyledProperty<bool> IsAnimatingDirectionalArrowsProperty =
            AvaloniaProperty.Register<BaseConnection, bool>(nameof(IsAnimatingDirectionalArrows), defaultValue: false);

        public static readonly StyledProperty<double> DirectionalArrowsAnimationDurationProperty =
            AvaloniaProperty.Register<BaseConnection, double>(nameof(DirectionalArrowsAnimationDuration), defaultValue: 2.0);

        public static readonly StyledProperty<double> SpacingProperty =
            AvaloniaProperty.Register<BaseConnection, double>(nameof(Spacing), defaultValue: 0.0);

        public static readonly StyledProperty<Size> ArrowSizeProperty =
            AvaloniaProperty.Register<BaseConnection, Size>(nameof(ArrowSize), defaultValue: new Size(8, 8));

        public static readonly StyledProperty<ArrowHeadEnds> ArrowEndsProperty =
            AvaloniaProperty.Register<BaseConnection, ArrowHeadEnds>(nameof(ArrowEnds), defaultValue: ArrowHeadEnds.End);

        public static readonly StyledProperty<ArrowHeadShape> ArrowShapeProperty =
            AvaloniaProperty.Register<BaseConnection, ArrowHeadShape>(nameof(ArrowShape), defaultValue: ArrowHeadShape.Arrowhead);

        public static readonly StyledProperty<ICommand?> SplitCommandProperty =
            AvaloniaProperty.Register<BaseConnection, ICommand?>(nameof(SplitCommand));

        public static readonly StyledProperty<ICommand?> DisconnectCommandProperty =
            AvaloniaProperty.Register<BaseConnection, ICommand?>(nameof(DisconnectCommand));

        public static readonly StyledProperty<double> OutlineThicknessProperty =
            AvaloniaProperty.Register<BaseConnection, double>(nameof(OutlineThickness), defaultValue: 5.0);

        public static readonly StyledProperty<IBrush?> OutlineBrushProperty =
            AvaloniaProperty.Register<BaseConnection, IBrush?>(nameof(OutlineBrush));

        public static readonly StyledProperty<IPen?> FocusVisualPenProperty =
            AvaloniaProperty.Register<BaseConnection, IPen?>(nameof(FocusVisualPen));

        public static readonly StyledProperty<double> FocusVisualPaddingProperty =
            AvaloniaProperty.Register<BaseConnection, double>(nameof(FocusVisualPadding), defaultValue: 1.0);

        public static readonly StyledProperty<IBrush?> ForegroundProperty =
            TextBlock.ForegroundProperty.AddOwner<BaseConnection>();

        public static readonly StyledProperty<string?> TextProperty =
            TextBlock.TextProperty.AddOwner<BaseConnection>();

        public static readonly StyledProperty<double> FontSizeProperty =
            TextBlock.FontSizeProperty.AddOwner<BaseConnection>();

        public static readonly StyledProperty<FontFamily> FontFamilyProperty =
            TextBlock.FontFamilyProperty.AddOwner<BaseConnection>();

        public static readonly StyledProperty<FontWeight> FontWeightProperty =
            TextBlock.FontWeightProperty.AddOwner<BaseConnection>();

        public static readonly StyledProperty<FontStyle> FontStyleProperty =
            TextBlock.FontStyleProperty.AddOwner<BaseConnection>();

        public static readonly StyledProperty<FontStretch> FontStretchProperty =
            TextBlock.FontStretchProperty.AddOwner<BaseConnection>();

        public static readonly AttachedProperty<bool> IsSelectableProperty =
            AvaloniaProperty.RegisterAttached<BaseConnection, Control, bool>("IsSelectable", defaultValue: false);

        public static readonly AttachedProperty<bool> IsSelectedProperty =
            AvaloniaProperty.RegisterAttached<BaseConnection, Control, bool>("IsSelected", defaultValue: false, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public static readonly StyledProperty<bool> HasCustomContextMenuProperty =
            AvaloniaProperty.Register<BaseConnection, bool>(nameof(HasCustomContextMenu), defaultValue: false);

        private static void OnIsSelectedChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            var container = d is BaseConnection conn ? conn.Container : ((Control)d).GetParentOfType<ConnectionContainer>();
            if (container != null)
            {
                container.IsSelected = (bool)e.NewValue!;
            }
        }

        public static bool GetIsSelectable(Control elem)
            => (bool)elem.GetValue(IsSelectableProperty);

        public static void SetIsSelectable(Control elem, bool value)
            => elem.SetValue(IsSelectableProperty, value);

        public static bool GetIsSelected(Control elem)
            => (bool)elem.GetValue(IsSelectedProperty);

        public static void SetIsSelected(Control? elem, bool value)
            => elem?.SetValue(IsSelectedProperty, value);

        static BaseConnection()
        {
            // Register property changed handlers
            IsSelectedProperty.Changed.AddClassHandler<BaseConnection>((x, e) => OnIsSelectedChanged(x, e));
            IsAnimatingDirectionalArrowsProperty.Changed.AddClassHandler<BaseConnection>((x, e) => OnIsAnimatingDirectionalArrowsChanged(x, e));
            DirectionalArrowsAnimationDurationProperty.Changed.AddClassHandler<BaseConnection>((x, e) => OnDirectionalArrowsAnimationDurationChanged(x, e));
            OutlineThicknessProperty.Changed.AddClassHandler<BaseConnection>((x, e) => OnOutlinePenChanged(x, e));
            OutlineBrushProperty.Changed.AddClassHandler<BaseConnection>((x, e) => OnOutlinePenChanged(x, e));

            // Register for rendering invalidation on property changes
            AffectsRender<BaseConnection>(
                SourceProperty, TargetProperty, SourceOffsetProperty, TargetOffsetProperty,
                SourceOffsetModeProperty, TargetOffsetModeProperty, SourceOrientationProperty, TargetOrientationProperty,
                DirectionProperty, DirectionalArrowsCountProperty, DirectionalArrowsOffsetProperty,
                IsAnimatingDirectionalArrowsProperty, DirectionalArrowsAnimationDurationProperty,
                SpacingProperty, ArrowSizeProperty, ArrowEndsProperty, ArrowShapeProperty,
                OutlineThicknessProperty, OutlineBrushProperty, FocusVisualPenProperty, FocusVisualPaddingProperty,
                ForegroundProperty, TextProperty, FontSizeProperty, FontFamilyProperty,
                FontWeightProperty, FontStyleProperty, FontStretchProperty
            );
        }

        private static void OnIsAnimatingDirectionalArrowsChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            var con = (BaseConnection)d;
            if (e.NewValue is true)
            {
                con.StartAnimation(con.DirectionalArrowsAnimationDuration);
            }
            else
            {
                con.StopAnimation();
            }
        }

        private static void OnDirectionalArrowsAnimationDurationChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            var con = (BaseConnection)d;
            if (con.IsAnimatingDirectionalArrows)
            {
                con.StartAnimation((double)e.NewValue!);
            }
        }

        private static void OnOutlinePenChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            ((BaseConnection)d)._outlinePen = null;
        }

        /// <summary>
        /// Gets or sets the start point of this connection.
        /// </summary>
        public Point Source
        {
            get => (Point)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        /// <summary>
        /// Gets or sets the end point of this connection.
        /// </summary>
        public Point Target
        {
            get => (Point)GetValue(TargetProperty);
            set => SetValue(TargetProperty, value);
        }

        /// <summary>
        /// Gets or sets the offset from the <see cref="Source"/> point.
        /// </summary>
        public Size SourceOffset
        {
            get => (Size)GetValue(SourceOffsetProperty);
            set => SetValue(SourceOffsetProperty, value);
        }

        /// <summary>
        /// Gets or sets the offset from the <see cref="Target"/> point.
        /// </summary>
        public Size TargetOffset
        {
            get => (Size)GetValue(TargetOffsetProperty);
            set => SetValue(TargetOffsetProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="ConnectionOffsetMode"/> to apply to the <see cref="Source"/> when drawing the connection.
        /// </summary>
        public ConnectionOffsetMode SourceOffsetMode
        {
            get => (ConnectionOffsetMode)GetValue(SourceOffsetModeProperty);
            set => SetValue(SourceOffsetModeProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="ConnectionOffsetMode"/> to apply to the <see cref="Target"/> when drawing the connection.
        /// </summary>
        public ConnectionOffsetMode TargetOffsetMode
        {
            get => (ConnectionOffsetMode)GetValue(TargetOffsetModeProperty);
            set => SetValue(TargetOffsetModeProperty, value);
        }

        /// <summary>
        /// Gets or sets the orientation in which this connection is flowing.
        /// </summary>
        public Orientation SourceOrientation
        {
            get => (Orientation)GetValue(SourceOrientationProperty);
            set => SetValue(SourceOrientationProperty, value);
        }

        /// <summary>
        /// Gets or sets the orientation in which this connection is flowing.
        /// </summary>
        public Orientation TargetOrientation
        {
            get => (Orientation)GetValue(TargetOrientationProperty);
            set => SetValue(TargetOrientationProperty, value);
        }

        /// <summary>
        /// Gets or sets the direction in which this connection is flowing.
        /// </summary>
        public ConnectionDirection Direction
        {
            get => (ConnectionDirection)GetValue(DirectionProperty);
            set => SetValue(DirectionProperty, value);
        }

        /// <summary>
        /// Gets or sets the number of arrows to be drawn on the line in the direction of the connection (see <see cref="Direction"/>).
        /// </summary>
        public uint DirectionalArrowsCount
        {
            get => (uint)GetValue(DirectionalArrowsCountProperty);
            set => SetValue(DirectionalArrowsCountProperty, value);
        }

        /// <summary>
        /// Gets or sets the offset of the arrows drawn by the <see cref="DirectionalArrowsCount"/> (value is clamped between 0 and 1).
        /// </summary>
        public double DirectionalArrowsOffset
        {
            get => (double)GetValue(DirectionalArrowsOffsetProperty);
            set => SetValue(DirectionalArrowsOffsetProperty, value);
        }

        /// <summary>
        /// Gets or sets whether the directional arrows should be flowing through the connection wire.
        /// </summary>
        public bool IsAnimatingDirectionalArrows
        {
            get => (bool)GetValue(IsAnimatingDirectionalArrowsProperty);
            set => SetValue(IsAnimatingDirectionalArrowsProperty, value);
        }

        /// <summary>
        /// Gets or sets the duration in seconds of a directional arrow flowing from <see cref="Source"/> to <see cref="Target"/>.
        /// </summary>
        public double DirectionalArrowsAnimationDuration
        {
            get => (double)GetValue(DirectionalArrowsAnimationDurationProperty);
            set => SetValue(DirectionalArrowsAnimationDurationProperty, value);
        }

        /// <summary>
        /// Gets or sets the arrowhead ends.
        /// </summary>
        public ArrowHeadEnds ArrowEnds
        {
            get => (ArrowHeadEnds)GetValue(ArrowEndsProperty);
            set => SetValue(ArrowEndsProperty, value);
        }

        /// <summary>
        /// Gets or sets the arrowhead ends.
        /// </summary>
        public ArrowHeadShape ArrowShape
        {
            get => (ArrowHeadShape)GetValue(ArrowShapeProperty);
            set => SetValue(ArrowShapeProperty, value);
        }

        /// <summary>
        /// The distance between the start point and the where the angle breaks.
        /// </summary>
        public double Spacing
        {
            get => (double)GetValue(SpacingProperty);
            set => SetValue(SpacingProperty, value);
        }

        /// <summary>
        /// Gets or sets the size of the arrow head.
        /// </summary>
        public Size ArrowSize
        {
            get => (Size)GetValue(ArrowSizeProperty);
            set => SetValue(ArrowSizeProperty, value);
        }

        /// <summary>
        /// Splits the connection. Triggered by <see cref="EditorGestures.ConnectionGestures.Split"/> gesture.
        /// Parameter is the location where the splitting ocurred.
        /// </summary>
        public ICommand? SplitCommand
        {
            get => (ICommand)GetValue(SplitCommandProperty);
            set => SetValue(SplitCommandProperty, value);
        }

        /// <summary>
        /// Removes this connection. Triggered by <see cref="EditorGestures.ConnectionGestures.Disconnect"/> gesture.
        /// Parameter is the location where the disconnect ocurred.
        /// </summary>
        public ICommand? DisconnectCommand
        {
            get => (ICommand?)GetValue(DisconnectCommandProperty);
            set => SetValue(DisconnectCommandProperty, value);
        }

        /// <summary>
        /// The thickness of the outline.
        /// </summary>
        public double OutlineThickness
        {
            get => (double)GetValue(OutlineThicknessProperty);
            set => SetValue(OutlineThicknessProperty, value);
        }

        /// <summary>
        /// The brush used to render the outline.
        /// </summary>
        public Brush? OutlineBrush
        {
            get => (Brush?)GetValue(OutlineBrushProperty);
            set => SetValue(OutlineBrushProperty, value);
        }

        /// <summary>
        /// The pen used to render the focus visual.
        /// </summary>
        public Pen? FocusVisualPen
        {
            get => (Pen?)GetValue(FocusVisualPenProperty);
            set => SetValue(FocusVisualPenProperty, value);
        }

        /// <summary>
        /// The space between the focus visual and the connection geometry.
        /// </summary>
        public double FocusVisualPadding
        {
            get => (double)GetValue(FocusVisualPaddingProperty);
            set => SetValue(FocusVisualPaddingProperty, value);
        }

        /// <summary>
        /// The brush used to render the <see cref="Text"/>.
        /// </summary>
        public Brush? Foreground
        {
            get => (Brush?)GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the text contents of the <see cref="BaseConnection"/>.
        /// </summary>
        public string? Text
        {
            get => (string?)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        /// <inheritdoc cref="TextElement.FontSize" />
        [TypeConverter(typeof(FontSizeConverter))]
        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        /// <inheritdoc cref="TextElement.FontFamily" />
        public FontFamily FontFamily
        {
            get => (FontFamily)GetValue(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }

        /// <inheritdoc cref="TextElement.FontStyle" />
        public FontStyle FontStyle
        {
            get => (FontStyle)GetValue(FontStyleProperty);
            set => SetValue(FontStyleProperty, value);
        }

        /// <inheritdoc cref="TextElement.FontWeight" />
        public FontWeight FontWeight
        {
            get => (FontWeight)GetValue(FontWeightProperty);
            set => SetValue(FontWeightProperty, value);
        }

        /// <inheritdoc cref="TextElement.FontStretch" />
        public FontStretch FontStretch
        {
            get => (FontStretch)GetValue(FontStretchProperty);
            set => SetValue(FontStretchProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the connection uses a custom context menu.
        /// </summary>
        /// <remarks>When set to true, the connection handles the right-click event for specific interactions.</remarks>
        public bool HasCustomContextMenu
        {
            get => (bool)GetValue(HasCustomContextMenuProperty);
            set => SetValue(HasCustomContextMenuProperty, value);
        }

        /// <summary>
        /// Gets a value indicating whether the connection has a context menu.
        /// </summary>
        public bool HasContextMenu => ContextMenu != null || HasCustomContextMenu;

        #endregion

        #region Routed Events

        public static readonly RoutedEvent<ConnectionEventArgs> DisconnectEvent =
            RoutedEvent.Register<BaseConnection, ConnectionEventArgs>(nameof(Disconnect), RoutingStrategy.Bubble);

        public static readonly RoutedEvent<ConnectionEventArgs> SplitEvent =
            RoutedEvent.Register<BaseConnection, ConnectionEventArgs>(nameof(Split), RoutingStrategy.Bubble);

        /// <summary>Triggered by the <see cref="EditorGestures.ConnectionGestures.Disconnect"/> gesture.</summary>
        public event EventHandler<ConnectionEventArgs> Disconnect
        {
            add => AddHandler(DisconnectEvent, value);
            remove => RemoveHandler(DisconnectEvent, value);
        }

        /// <summary>Triggered by the <see cref="EditorGestures.ConnectionGestures.Split"/> gesture.</summary>
        public event EventHandler<ConnectionEventArgs> Split
        {
            add => AddHandler(SplitEvent, value);
            remove => RemoveHandler(SplitEvent, value);
        }

        #endregion

        /// <summary>
        /// Whether to prioritize controls of type <see cref="BaseConnection"/> inside custom connections (connection wrappers) 
        /// when setting the <see cref="IsSelectableProperty"/> and <see cref="IsSelectedProperty"/> attached properties.
        /// </summary>
        /// <remarks>
        /// Will fallback to the first <see cref="Control"/> if no <see cref="BaseConnection"/> is found or the value is false.
        /// </remarks>
        public static bool PrioritizeBaseConnectionForSelection { get; set; } = true;

        /// <summary>
        /// Gets a vector that has its coordinates set to 0.
        /// </summary>
        protected static readonly Vector ZeroVector = new Vector(0d, 0d);

        // Use Source for both corners to ensure Top-Left aligns with intended keyboard focus point.
        Rect IKeyboardFocusTarget<Control>.Bounds
            => new Rect(Direction == ConnectionDirection.Forward ? Target : Source, Direction == ConnectionDirection.Forward ? Target : Source);

        Control IKeyboardFocusTarget<Control>.Element => this;

        /// <summary>
        /// The key used to retrieve the <see cref="FocusVisualPen"/> resource.
        /// </summary>
        public static ResourceKey FocusVisualPenKey { get; } = new ComponentResourceKey(typeof(BaseConnection), nameof(FocusVisualPen));

        private Pen? _outlinePen;
        private static Pen? _defaultFocusVisualPen;
        private FocusVisualAdorner? _focusVisualAdorner;
        private AdornerLayer? _adornerLayer;

        private AdornerLayer AdornerLayer => _adornerLayer ??= AdornerLayer.GetAdornerLayer(this);

        private FocusVisualAdorner FocusVisualPenAdorner => _focusVisualAdorner ??= new FocusVisualAdorner(this);

        private static Pen DefaultFocusVisualPen
        {
            get
            {
                if (_defaultFocusVisualPen is null)
                {
                    _defaultFocusVisualPen = new Pen(SystemColors.ControlTextBrush, 1)
                    {
                        DashStyle = new DashStyle { Dashes = { 0.5d, 3d } }
                    };
                    _defaultFocusVisualPen.Freeze();
                }

                return _defaultFocusVisualPen;
            }
        }

        private StreamGeometry? _geometry;

        private ConnectionContainer? _container;
        private ConnectionContainer? Container => _container ??= this.GetParentOfType<ConnectionContainer>();

        protected override Geometry? CreateDefiningGeometry()
        {
            _geometry = new StreamGeometry();

            using (var context = _geometry.Open())
            {
                (Vector sourceOffset, Vector targetOffset) = GetOffset();
                var (arrowStart, arrowEnd) = DrawLineGeometry(context, Source + sourceOffset, Target + targetOffset);

                if (ArrowSize.Width != 0d && ArrowSize.Height != 0d)
                {
                    var reverseDirection = Direction == ConnectionDirection.Forward ? ConnectionDirection.Backward : ConnectionDirection.Forward;
                    switch (ArrowEnds)
                    {
                        case ArrowHeadEnds.Start:
                            DrawArrowGeometry(context, arrowStart.ArrowStartSource, arrowStart.ArrowStartTarget, reverseDirection, ArrowShape, SourceOrientation);
                            break;
                        case ArrowHeadEnds.End:
                            DrawArrowGeometry(context, arrowEnd.ArrowEndSource, arrowEnd.ArrowEndTarget, Direction, ArrowShape, TargetOrientation);
                            break;
                        case ArrowHeadEnds.Both:
                            DrawArrowGeometry(context, arrowEnd.ArrowEndSource, arrowEnd.ArrowEndTarget, Direction, ArrowShape, TargetOrientation);
                            DrawArrowGeometry(context, arrowStart.ArrowStartSource, arrowStart.ArrowStartTarget, reverseDirection, ArrowShape, SourceOrientation);
                            break;
                        case ArrowHeadEnds.None:
                        default:
                            break;
                    }

                    if (DirectionalArrowsCount > 0)
                    {
                        DrawDirectionalArrowsGeometry(context, Source + sourceOffset, Target + targetOffset);
                    }
                }
            }

            return _geometry;
        }

        protected BaseConnection()
        {
            InputProcessor.AddSharedHandlers(this);
        }

        #region Drawing

        protected abstract ((Point ArrowStartSource, Point ArrowStartTarget), (Point ArrowEndSource, Point ArrowEndTarget)) DrawLineGeometry(StreamGeometryContext context, Point source, Point target);

        protected virtual void DrawDirectionalArrowsGeometry(StreamGeometryContext context, Point source, Point target) { }

        protected virtual void DrawDirectionalArrowheadGeometry(StreamGeometryContext context, Vector direction, Point location)
        {
            var x = direction;
            x.Normalize();
            // If Normalization failed (e.g. because the vector is too small), just don't draw anything.
#if !NETFRAMEWORK
            if (!double.IsFinite(x.X)) return;
#else
            if (double.IsNaN(x.X) || double.IsInfinity(x.X)) return;
#endif
            var y = new Vector(x.Y, -x.X);

            var headWidth = ArrowSize.Width * x;
            var headHeight = ArrowSize.Height / 2 * y;
            var headMiddle = location + headWidth;

            var from = headMiddle + headHeight;
            var to = headMiddle - headHeight;

            context.BeginFigure(location, true, true);
            context.LineTo(from, true, true);
            context.LineTo(to, true, true);
        }

        protected virtual void DrawArrowGeometry(StreamGeometryContext context, Point source, Point target, ConnectionDirection arrowDirection = ConnectionDirection.Forward, ArrowHeadShape shape = ArrowHeadShape.Arrowhead, Orientation orientation = Orientation.Horizontal)
        {
            switch (shape)
            {
                case ArrowHeadShape.Ellipse:
                    DrawEllipseArrowhead(context, source, target, arrowDirection, orientation);
                    break;
                case ArrowHeadShape.Rectangle:
                    DrawRectangleArrowhead(context, source, target, arrowDirection, orientation);
                    break;
                case ArrowHeadShape.Arrowhead:
                default:
                    DrawDefaultArrowhead(context, source, target, arrowDirection, orientation);
                    break;
            }
        }

        protected virtual void DrawDefaultArrowhead(StreamGeometryContext context, Point source, Point target, ConnectionDirection arrowDirection = ConnectionDirection.Forward, Orientation orientation = Orientation.Horizontal)
        {
            double direction = arrowDirection == ConnectionDirection.Forward ? 1d : -1d;

            if (orientation == Orientation.Horizontal)
            {
                double headWidth = ArrowSize.Width;
                double headHeight = ArrowSize.Height / 2;

                var from = new Point(target.X - headWidth * direction, target.Y + headHeight);
                var to = new Point(target.X - headWidth * direction, target.Y - headHeight);

                context.BeginFigure(target, true, true);
                context.LineTo(from, true, true);
                context.LineTo(to, true, true);
            }
            else
            {
                double headWidth = ArrowSize.Width / 2;
                double headHeight = ArrowSize.Height;

                var from = new Point(target.X - headWidth, target.Y - headHeight * direction);
                var to = new Point(target.X + headWidth, target.Y - headHeight * direction);

                context.BeginFigure(target, true, true);
                context.LineTo(from, true, true);
                context.LineTo(to, true, true);
            }
        }

        protected virtual void DrawRectangleArrowhead(StreamGeometryContext context, Point source, Point target, ConnectionDirection arrowDirection = ConnectionDirection.Forward, Orientation orientation = Orientation.Horizontal)
        {
            double direction = arrowDirection == ConnectionDirection.Forward ? 1d : -1d;

            if (orientation == Orientation.Horizontal)
            {
                double headWidth = ArrowSize.Width;
                double headHeight = ArrowSize.Height / 2;
                var bottomRight = new Point(target.X, target.Y + headHeight);
                var bottomLeft = new Point(target.X - headWidth * direction, target.Y + headHeight);
                var topLeft = new Point(target.X - headWidth * direction, target.Y - headHeight);
                var topRight = new Point(target.X, target.Y - headHeight);

                context.BeginFigure(target, true, true);
                context.LineTo(bottomRight, true, true);
                context.LineTo(bottomLeft, true, true);
                context.LineTo(topLeft, true, true);
                context.LineTo(topRight, true, true);
            }
            else
            {
                double headWidth = ArrowSize.Width / 2;
                double headHeight = ArrowSize.Height;
                var bottomLeft = new Point(target.X - headWidth, target.Y);
                var topLeft = new Point(target.X - headWidth, target.Y - headHeight * direction);
                var topRight = new Point(target.X + headWidth, target.Y - headHeight * direction);
                var bottomRight = new Point(target.X + headWidth, target.Y);

                context.BeginFigure(target, true, true);
                context.LineTo(bottomLeft, true, true);
                context.LineTo(topLeft, true, true);
                context.LineTo(topRight, true, true);
                context.LineTo(bottomRight, true, true);
            }
        }

        protected virtual void DrawEllipseArrowhead(StreamGeometryContext context, Point source, Point target, ConnectionDirection arrowDirection = ConnectionDirection.Forward, Orientation orientation = Orientation.Horizontal)
        {
            const double ControlPointRatio = 0.55228474983079356; // (Math.Sqrt(2) - 1) * 4 / 3;

            double direction = arrowDirection == ConnectionDirection.Forward ? 1d : -1d;
            var targetLocation = orientation == Orientation.Horizontal
                ? new Point(target.X - ArrowSize.Width / 2 * direction, target.Y)
                : new Point(target.X, target.Y - ArrowSize.Height / 2 * direction);

            double headWidth = ArrowSize.Width / 2;
            double headHeight = ArrowSize.Height / 2;

            double x0 = targetLocation.X - headWidth;
            double x1 = targetLocation.X - headWidth * ControlPointRatio;
            double x2 = targetLocation.X;
            double x3 = targetLocation.X + headWidth * ControlPointRatio;
            double x4 = targetLocation.X + headWidth;

            double y0 = targetLocation.Y - headHeight;
            double y1 = targetLocation.Y - headHeight * ControlPointRatio;
            double y2 = targetLocation.Y;
            double y3 = targetLocation.Y + headHeight * ControlPointRatio;
            double y4 = targetLocation.Y + headHeight;

            context.BeginFigure(new Point(x2, y0), true, true);
            context.BezierTo(new Point(x3, y0), new Point(x4, y1), new Point(x4, y2), true, true);
            context.BezierTo(new Point(x4, y3), new Point(x3, y4), new Point(x2, y4), true, true);
            context.BezierTo(new Point(x1, y4), new Point(x0, y3), new Point(x0, y2), true, true);
            context.BezierTo(new Point(x0, y1), new Point(x1, y0), new Point(x2, y0), true, true);
        }

        /// <summary>
        /// Gets the resulting offset after applying the <see cref="SourceOffsetMode"/>.
        /// </summary>
        /// <returns></returns>
        protected virtual (Vector SourceOffset, Vector TargetOffset) GetOffset()
        {
            Vector sourceDelta = Target - Source;
            Vector targetDelta = Source - Target;
            double arrowDirection = Direction == ConnectionDirection.Forward ? 1d : -1d;

            var sourceOffset = GetOffset(SourceOffsetMode, sourceDelta, SourceOffset, arrowDirection);
            var targetOffset = GetOffset(TargetOffsetMode, targetDelta, TargetOffset, -arrowDirection);

            if (SourceOrientation == Orientation.Vertical)
            {
                (sourceOffset.X, sourceOffset.Y) = (sourceOffset.Y, sourceOffset.X);
            }

            if (TargetOrientation == Orientation.Vertical)
            {
                (targetOffset.X, targetOffset.Y) = (targetOffset.Y, targetOffset.X);
            }

            return (sourceOffset, targetOffset);

            static Vector GetOffset(ConnectionOffsetMode mode, Vector delta, Size currentOffset, double arrowDirection) => mode switch
            {
                ConnectionOffsetMode.Rectangle => GetRectangleModeOffset(delta, currentOffset),
                ConnectionOffsetMode.Circle => GetCircleModeOffset(delta, currentOffset),
                ConnectionOffsetMode.Edge => GetEdgeModeOffset(delta, currentOffset),
                ConnectionOffsetMode.Static => GetStaticModeOffset(arrowDirection, currentOffset),
                ConnectionOffsetMode.None => ZeroVector,
                _ => throw new NotImplementedException()
            };

            static Vector GetStaticModeOffset(double direction, Size offset)
            {
                double xOffset = offset.Width * direction;
                double yOffset = offset.Height * direction;

                return new Vector(xOffset, yOffset);
            }

            static Vector GetEdgeModeOffset(Vector delta, Size offset)
            {
                double xOffset = Math.Min(Math.Abs(delta.X) / 2d, offset.Width) * Math.Sign(delta.X);
                double yOffset = Math.Min(Math.Abs(delta.Y) / 2d, offset.Height) * Math.Sign(delta.Y);

                return new Vector(xOffset, yOffset);
            }

            static Vector GetCircleModeOffset(Vector delta, Size offset)
            {
                if (delta.LengthSquared > 0d)
                {
                    delta.Normalize();
                }

                return new Vector(delta.X * offset.Width, delta.Y * offset.Height);
            }

            static Vector GetRectangleModeOffset(Vector delta, Size offset)
            {
                if (delta.LengthSquared > 0d)
                {
                    delta.Normalize();
                }

                double angle = Math.Atan2(delta.Y, delta.X);
                var result = new Vector();

                if (offset.Width * 2d * Math.Abs(delta.Y) < offset.Height * 2d * Math.Abs(delta.X))
                {
                    result.X = Math.Sign(delta.X) * offset.Width;
                    result.Y = Math.Tan(angle) * result.X;
                }
                else
                {
                    result.Y = Math.Sign(delta.Y) * offset.Height;
                    result.X = 1.0d / Math.Tan(angle) * result.Y;
                }

                return result;
            }
        }

        protected virtual Point GetTextPosition(FormattedText text, Point source, Point target)
        {
            double direction = Direction == ConnectionDirection.Forward ? 1d : -1d;
            var spacing = new Vector(Spacing * direction, 0d);
            var spacingVertical = new Vector(spacing.Y, spacing.X);

            var p0 = source + (SourceOrientation == Orientation.Vertical ? spacingVertical : spacing);
            var p1 = target - (TargetOrientation == Orientation.Vertical ? spacingVertical : spacing);

            return new Point((p0.X + p1.X - text.Width) / 2, (p0.Y + p1.Y - text.Height) / 2);
        }

        #endregion

        /// <summary>Starts animating the directional arrows.</summary>
        /// <param name="duration">The duration for moving an arrowhead from <see cref="Source"/> to <see cref="Target"/>.</param>
        public void StartAnimation(double duration = 1.5d)
        {
            StopAnimation();
            this.StartLoopingAnimation(DirectionalArrowsOffsetProperty, DirectionalArrowsOffset + 1d, duration);
        }

        /// <summary>Stops the animation started by <see cref="StartAnimation(double)"/></summary>
        public void StopAnimation()
        {
            this.CancelAnimation(DirectionalArrowsOffsetProperty);
        }

        #region Gesture Handling

        private InputProcessor InputProcessor { get; } = new InputProcessor();

        /// <inheritdoc />
        protected override void OnMouseDown(MouseButtonEventArgs e)
            => InputProcessor.ProcessEvent(e);

        /// <inheritdoc />
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            InputProcessor.ProcessEvent(e);

            // Release the mouse capture if all the mouse buttons are released
            if (!InputProcessor.RequiresInputCapture && IsMouseCaptured && e.RightButton == MouseButtonState.Released && e.LeftButton == MouseButtonState.Released && e.MiddleButton == MouseButtonState.Released)
            {
                ReleaseMouseCapture();
            }
        }

        /// <inheritdoc />
        protected override void OnMouseMove(MouseEventArgs e)
            => InputProcessor.ProcessEvent(e);

        /// <inheritdoc />
        protected override void OnMouseWheel(MouseWheelEventArgs e)
            => InputProcessor.ProcessEvent(e);

        /// <inheritdoc />
        protected override void OnLostMouseCapture(MouseEventArgs e)
            => InputProcessor.ProcessEvent(e);

        /// <inheritdoc />
        protected override void OnKeyUp(KeyEventArgs e)
        {
            InputProcessor.ProcessEvent(e);

            // Release the mouse capture if all the mouse buttons are released
            if (!InputProcessor.RequiresInputCapture && IsMouseCaptured && Mouse.RightButton == MouseButtonState.Released && Mouse.LeftButton == MouseButtonState.Released && Mouse.MiddleButton == MouseButtonState.Released)
            {
                ReleaseMouseCapture();
            }
        }

        /// <inheritdoc />
        protected override void OnKeyDown(KeyEventArgs e)
            => InputProcessor.ProcessEvent(e);

        #endregion

        /// <summary>
        /// Splits the connection at the specified location.
        /// </summary>
        /// <param name="splitLocation">The <see cref="Point"/> where the connection should be split.</param>
        /// <remarks>
        /// This method raises the <see cref="SplitEvent"/> to notify listeners. If the event is not handled,
        /// it checks whether the <see cref="SplitCommand"/> can execute with the provided location and executes it if possible.
        /// </remarks>
        public void SplitAtLocation(Point splitLocation)
        {
            var args = new ConnectionEventArgs(DataContext)
            {
                RoutedEvent = SplitEvent,
                SplitLocation = splitLocation,
                Source = this
            };

            RaiseEvent(args);

            // Raise SplitCommand if SplitEvent is not handled
            if (!args.Handled && (SplitCommand?.CanExecute(splitLocation) ?? false))
            {
                SplitCommand.Execute(splitLocation);
            }
        }

        /// <summary>
        /// Removes the connection.
        /// </summary>
        /// <remarks>
        /// This method raises the <see cref="DisconnectEvent"/> to notify listeners. If the event is not handled,
        /// it checks whether the <see cref="DisconnectCommand"/> can execute and executes it if possible.
        /// </remarks>
        public void Remove()
        {
            var args = new ConnectionEventArgs(DataContext)
            {
                RoutedEvent = DisconnectEvent,
                Source = this
            };

            RaiseEvent(args);

            // Raise DisconnectCommand if DisconnectEvent is not handled
            if (!args.Handled && (DisconnectCommand?.CanExecute(null) ?? false))
            {
                DisconnectCommand.Execute(null);
            }
        }

        private Pen GetOutlinePen()
        {
            return _outlinePen ??= new Pen(OutlineBrush, StrokeThickness + OutlineThickness * 2d);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (OutlineBrush != null)
            {
                drawingContext.DrawGeometry(OutlineBrush, GetOutlinePen(), DefiningGeometry);
            }

            base.OnRender(drawingContext);

            if (!string.IsNullOrEmpty(Text))
            {
                double dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
                var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
                var text = new FormattedText(Text, CultureInfo.CurrentUICulture, FlowDirection, typeface, FontSize, Foreground ?? Stroke, dpi);

                (Vector sourceOffset, Vector targetOffset) = GetOffset();
                drawingContext.DrawText(text, GetTextPosition(text, Source + sourceOffset, Target + targetOffset));
            }

            if (AdornerLayer != null && Container is { IsKeyboardFocused: true })
            {
                AdornerLayer.Update(this);
            }
        }

        internal void UpdateFocusVisual()
        {
            if (AdornerLayer != null)
            {
                if (Container is { IsKeyboardFocused: true })
                {
                    AdornerLayer.Add(FocusVisualPenAdorner);
                }
                else
                {
                    AdornerLayer.Remove(FocusVisualPenAdorner);
                }
            }
        }

        private class FocusVisualAdorner : Adorner
        {
            private readonly BaseConnection _baseConnection;
            private Pen? _cachedPenResource;
            private Pen? CachedPenResource => _cachedPenResource ??= TryFindResource(FocusVisualPenKey) as Pen;

            public FocusVisualAdorner(BaseConnection baseConnection) : base(baseConnection)
            {
                IsHitTestVisible = false;
                IsEnabled = false;
                IsClipEnabled = true;
                _baseConnection = baseConnection;
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                var drawPen = _baseConnection.FocusVisualPen == DefaultFocusVisualPen
                    ? CachedPenResource
                    : _baseConnection.FocusVisualPen;

                if (drawPen != null)
                {
                    var widenPen = new Pen(null, _baseConnection.StrokeThickness + drawPen.Thickness + _baseConnection.FocusVisualPadding * 2d);
                    var geometry = _baseConnection.DefiningGeometry;
                    var expandedGeometry = Geometry.Combine(geometry, geometry.GetWidenedPathGeometry(widenPen), GeometryCombineMode.Union, Transform.Identity);
                    drawingContext.DrawGeometry(null, drawPen, expandedGeometry.GetOutlinedPathGeometry());
                }
            }
        }
    }
}
