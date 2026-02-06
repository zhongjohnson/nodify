using Nodify.Events;
using Nodify.Interactivity;
using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Metadata;
using Avalonia.Interactivity;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Controls.Shapes;
using Avalonia.Styling;

namespace Nodify
{
    /// <summary>
    /// A minimap control that can position the viewport, and zoom in and out.
    /// </summary>
    public class Minimap : ItemsControl
    {
        private const string ElementItemsHost = "PART_ItemsHost";

        public static readonly StyledProperty<Point> ViewportLocationProperty = NodifyEditor.ViewportLocationProperty.AddOwner<Minimap>(new StyledPropertyMetadata<Point>(default, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay));
        public static readonly StyledProperty<Size> ViewportSizeProperty = NodifyEditor.ViewportSizeProperty.AddOwner<Minimap>();
        public static readonly StyledProperty<Style?> ViewportStyleProperty = AvaloniaProperty.Register<Minimap, Style?>(nameof(ViewportStyle));
        public static readonly StyledProperty<Rect> ExtentProperty = NodifyCanvas.ExtentProperty.AddOwner<Minimap>();
        public static readonly StyledProperty<Rect> ItemsExtentProperty = AvaloniaProperty.Register<Minimap, Rect>(nameof(ItemsExtent));
        public static readonly StyledProperty<Size> MaxViewportOffsetProperty = AvaloniaProperty.Register<Minimap, Size>(nameof(MaxViewportOffset), new Size(2000, 2000));
        public static readonly StyledProperty<bool> ResizeToViewportProperty = AvaloniaProperty.Register<Minimap, bool>(nameof(ResizeToViewport));
        public static readonly StyledProperty<bool> IsReadOnlyProperty = TextBox.IsReadOnlyProperty.AddOwner<Minimap>();

        public static readonly RoutedEvent ZoomEvent = EventManager.RegisterRoutedEvent(nameof(Zoom), RoutingStrategy.Bubble, typeof(ZoomEventHandler), typeof(Minimap));

        /// <inheritdoc cref="NodifyEditor.ViewportLocation" />
        public Point ViewportLocation
        {
            get => (Point)GetValue(ViewportLocationProperty);
            set => SetValue(ViewportLocationProperty, value);
        }

        /// <inheritdoc cref="NodifyEditor.ViewportSize" />
        public Size ViewportSize
        {
            get => (Size)GetValue(ViewportSizeProperty);
            set => SetValue(ViewportSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the style to use for the viewport rectangle.
        /// </summary>
        public Style ViewportStyle
        {
            get => (Style)GetValue(ViewportStyleProperty);
            set => SetValue(ViewportStyleProperty, value);
        }

        /// <summary>The area covered by the items and the viewport rectangle in graph space.</summary>
        public Rect Extent
        {
            get => (Rect)GetValue(ExtentProperty);
            set => SetValue(ExtentProperty, value);
        }

        /// <summary>The area covered by the <see cref="MinimapItem"/>s in graph space.</summary>
        public Rect ItemsExtent
        {
            get => (Rect)GetValue(ItemsExtentProperty);
            set => SetValue(ItemsExtentProperty, value);
        }

        /// <summary>The max position from the <see cref="NodifyEditor.ItemsExtent"/> that the viewport can move to.</summary>
        public Size MaxViewportOffset
        {
            get => (Size)GetValue(MaxViewportOffsetProperty);
            set => SetValue(MaxViewportOffsetProperty, value);
        }

        /// <summary>Whether the minimap should resize to also display the whole viewport.</summary>
        public bool ResizeToViewport
        {
            get => (bool)GetValue(ResizeToViewportProperty);
            set => SetValue(ResizeToViewportProperty, value);
        }

        /// <summary>Whether the minimap can move and zoom the viewport.</summary>
        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        /// <summary>Triggered when zooming in or out using the mouse wheel.</summary>
        public event ZoomEventHandler Zoom
        {
            add => AddHandler(ZoomEvent, value);
            remove => RemoveHandler(ZoomEvent, value);
        }

        /// <summary>
        /// Gets the panel that holds all the <see cref="MinimapItem"/>s.
        /// </summary>
        protected Panel ItemsHost { get; private set; } = default!;

        /// <summary>
        /// Whether the user is currently panning the minimap.
        /// </summary>
        protected bool IsPanning { get; private set; }

        /// <summary>
        /// Gets the current mouse location in graph space coordinates (relative to the <see cref="ItemsHost" />).
        /// </summary>
        public Point MouseLocation { get; private set; }

        /// <summary>
        /// Gets or sets whether panning cancellation is allowed (see <see cref="EditorGestures.MinimapGestures.CancelAction"/>).
        /// </summary>
        public static bool AllowPanningCancellation { get; set; } = true;

        /// <summary>
        /// Defines the distance to pan when using directional input (such as arrow keys).
        /// </summary>
        public static double NavigationStepSize { get; set; } = 50d;

        private Point _initialViewportLocation;

        static Minimap()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Minimap), new StyledPropertyMetadata(typeof(Minimap)));
            FocusableProperty.OverrideMetadata(typeof(Minimap), new StyledPropertyMetadata(BoxValue.True));

            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(Minimap), new StyledPropertyMetadata(KeyboardNavigationMode.None));
            KeyboardNavigation.ControlTabNavigationProperty.OverrideMetadata(typeof(Minimap), new StyledPropertyMetadata(KeyboardNavigationMode.None));
            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(Minimap), new StyledPropertyMetadata(KeyboardNavigationMode.None));
        }

        public Minimap()
        {
            InputProcessor.AddSharedHandlers(this);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            ItemsHost = e.NameScope.Find<Panel>(ElementItemsHost) ?? throw new InvalidOperationException($"{ElementItemsHost} is missing or is not of type {nameof(Panel)}.");
        }

        // NOTE: Avalonia ItemsControl doesn't have IsItemItsOwnContainerOverride/GetContainerForItemOverride
        // Container creation is handled differently in Avalonia using ItemTemplate
        // TODO: Implement container generation using Avalonia patterns

        #region Gesture Handling

        protected InputProcessor InputProcessor { get; } = new InputProcessor();

        /// <inheritdoc />
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            MouseLocation = e.GetPosition(ItemsHost);
            InputProcessor.ProcessEvent(e);
        }

        /// <inheritdoc />
        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            MouseLocation = e.GetPosition(ItemsHost);
            InputProcessor.ProcessEvent(e);

            // Release the pointer capture if there's no interaction in progress
            if (!InputProcessor.RequiresInputCapture && e.Pointer.Captured == this)
            {
                e.Pointer.Capture(null);
            }
        }

        /// <inheritdoc />
        protected override void OnPointerMoved(PointerEventArgs e)
        {
            MouseLocation = e.GetPosition(ItemsHost);
            InputProcessor.ProcessEvent(e);
        }

        /// <inheritdoc />
        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            MouseLocation = e.GetPosition(ItemsHost);
            InputProcessor.ProcessEvent(e);
        }

        /// <inheritdoc />
        protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
            => InputProcessor.ProcessEvent(e);

        /// <inheritdoc />
        protected override void OnKeyUp(KeyEventArgs e)
        {
            InputProcessor.ProcessEvent(e);

            // TODO: Need to track pointer state differently - Avalonia doesn't have Mouse.LeftButton static properties
        }

        /// <inheritdoc />
        protected override void OnKeyDown(KeyEventArgs e)
            => InputProcessor.ProcessEvent(e);

        #endregion

        #region Panning

        /// <summary>
        /// Starts the panning operation from the current <see cref="MouseLocation" />.
        /// </summary>
        /// <remarks>This method has no effect if a panning operation is already in progress.</remarks>
        public void BeginPanning()
            => BeginPanning(MouseLocation);

        /// <summary>
        /// Starts the panning operation from the specified location. Call <see cref="EndPanning"/> to end the panning operation.
        /// </summary>
        /// <remarks>This method has no effect if a panning operation is already in progress.</remarks>
        /// <param name="location">The initial location where panning starts, in graph space coordinates.</param>
        public void BeginPanning(Point location)
        {
            if (IsPanning || IsReadOnly)
            {
                return;
            }

            IsPanning = true;
            _initialViewportLocation = location;
            SetViewportLocation(location);
        }

        /// <summary>
        /// Sets the viewport location to the specified location.
        /// </summary>
        /// <param name="location">The location to pan the viewport to.</param>
        public void UpdatePanning(Point location)
        {
            Debug.Assert(IsPanning);
            SetViewportLocation(location);
        }

        /// <summary>
        /// Pans the viewport by the specified amount.
        /// </summary>
        /// <param name="amount">The amount to pan the viewport.</param>
        /// <remarks>
        /// This method adjusts the current <see cref="ViewportLocation"/> incrementally based on the provided amount.
        /// </remarks>
        public void UpdatePanning(Vector amount)
        {
            if (IsReadOnly)
            {
                return;
            }

            ViewportLocation -= amount;
        }

        /// <summary>
        /// Ends the current panning operation, retaining the current <see cref="ViewportLocation"/>.
        /// </summary>
        /// <remarks>This method has no effect if there's no panning operation in progress.</remarks>
        public void EndPanning()
        {
            if (!IsPanning)
            {
                return;
            }

            _initialViewportLocation = MouseLocation;
            IsPanning = false;
        }

        /// <summary>
        /// Cancels the current panning operation and reverts the viewport to its initial location if <see cref="AllowPanningCancellation"/> is true.
        /// Otherwise, it ends the panning operation by calling <see cref="EndPanning"/>.
        /// </summary>
        /// <remarks>This method has no effect if there's no panning operation in progress.</remarks>
        public void CancelPanning()
        {
            if (!AllowPanningCancellation)
            {
                EndPanning();
                return;
            }

            if (IsPanning)
            {
                SetViewportLocation(_initialViewportLocation);
                IsPanning = false;
            }
        }

        protected void SetViewportLocation(Point location)
        {
            var position = location - new Vector(ViewportSize.Width / 2, ViewportSize.Height / 2) + (Vector)Extent.Location;

            if (MaxViewportOffset.Width != 0 || MaxViewportOffset.Height != 0)
            {
                double maxRight = ResizeToViewport ? ItemsExtent.Right : Math.Max(ItemsExtent.Right, ItemsExtent.Left + ViewportSize.Width);
                double maxBottom = ResizeToViewport ? ItemsExtent.Bottom : Math.Max(ItemsExtent.Bottom, ItemsExtent.Top + ViewportSize.Height);

                position.X = position.X.Clamp(ItemsExtent.Left - ViewportSize.Width / 2 - MaxViewportOffset.Width, maxRight - ViewportSize.Width / 2 + MaxViewportOffset.Width);
                position.Y = position.Y.Clamp(ItemsExtent.Top - ViewportSize.Height / 2 - MaxViewportOffset.Height, maxBottom - ViewportSize.Height / 2 + MaxViewportOffset.Height);
            }

            ViewportLocation = position;
        }

        #endregion

        #region Zooming

        /// <summary>
        /// Zoom at the specified location in graph space coordinates.
        /// </summary>
        /// <param name="zoom">The zoom factor.</param>
        /// <param name="location">The location to focus when zooming.</param>
        public void ZoomAtPosition(double zoom, Point location)
        {
            if (IsReadOnly)
            {
                return;
            }

            if (!ResizeToViewport)
            {
                SetViewportLocation(location);
            }

            var viewportLocation = ViewportLocation + (Vector)ViewportSize / 2;
            var args = new ZoomEventArgs(zoom, viewportLocation)
            {
                RoutedEvent = ZoomEvent,
                Source = this
            };
            RaiseEvent(args);
        }

        /// <summary>
        /// Zoom in at the viewport's center.
        /// </summary>
        public void ZoomIn() => SetZoom(Math.Pow(2.0, 120.0 / 3.0 / Mouse.MouseWheelDeltaForOneLine));

        /// <summary>
        /// Zoom out at the viewport's center.
        /// </summary>
        public void ZoomOut() => SetZoom(Math.Pow(2.0, -120.0 / 3.0 / Mouse.MouseWheelDeltaForOneLine));

        public void ResetViewport()
        {
            SetCurrentValue(ViewportLocationProperty, new Point(0, 0));
            var args = new ZoomEventArgs(1d, new Point(ViewportSize.Width / 2, ViewportSize.Height / 2))
            {
                RoutedEvent = ZoomEvent,
                Source = this
            };
            RaiseEvent(args);
        }

        private void SetZoom(double zoom)
        {
            var viewportLocation = ViewportLocation + (Vector)ViewportSize / 2;
            var args = new ZoomEventArgs(zoom, viewportLocation)
            {
                RoutedEvent = ZoomEvent,
                Source = this
            };
            RaiseEvent(args);
        }

        #endregion

        /// <summary>
        /// Translates the event location to graph space coordinates (relative to the <see cref="ItemsHost" />).
        /// </summary>
        /// <param name="args">The mouse event.</param>
        /// <returns>A location inside the minimap</returns>
        public Point GetLocationInsideMinimap(MouseEventArgs args)
            => args.GetPosition(ItemsHost);
    }
}
