using Nodify.Events;
using Nodify.Interactivity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia;
using System.Windows.Input;
using Avalonia.Interactivity;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Metadata;
using Avalonia.Media;

namespace Nodify
{
    /// <summary>
    /// Specifies the possible alignment values used by the <see cref="NodifyEditor.AlignSelection(Alignment)"/> method.
    /// </summary>
    public enum Alignment
    {
        Top,
        Left,
        Bottom,
        Right,
        Middle,
        Center
    }

    /// <summary>
    /// Groups <see cref="ItemContainer"/>s and <see cref="Connection"/>s in an area that you can drag, zoom and select.
    /// </summary>
    [ContentProperty(nameof(Decorators))]
    [DefaultProperty(nameof(Decorators))]
    public partial class NodifyEditor
    {
        protected const string ElementItemsHost = "PART_ItemsHost";
        protected const string ElementConnectionsHost = "PART_ConnectionsHost";

        #region Viewport

        public static readonly StyledProperty ViewportZoomProperty = StyledProperty.Register(nameof(ViewportZoom), typeof(double), typeof(NodifyEditor), new StyledPropertyMetadata(BoxValue.Double1, StyledPropertyMetadataOptions.BindsTwoWayByDefault, OnViewportZoomChanged, ConstrainViewportZoomToRange));
        public static readonly StyledProperty MinViewportZoomProperty = StyledProperty.Register(nameof(MinViewportZoom), typeof(double), typeof(NodifyEditor), new StyledPropertyMetadata(0.1d, OnMinViewportZoomChanged, CoerceMinViewportZoom));
        public static readonly StyledProperty MaxViewportZoomProperty = StyledProperty.Register(nameof(MaxViewportZoom), typeof(double), typeof(NodifyEditor), new StyledPropertyMetadata(BoxValue.Double2, OnMaxViewportZoomChanged, CoerceMaxViewportZoom));
        public static readonly StyledProperty ViewportLocationProperty = StyledProperty.Register(nameof(ViewportLocation), typeof(Point), typeof(NodifyEditor), new StyledPropertyMetadata(BoxValue.Point, StyledPropertyMetadataOptions.BindsTwoWayByDefault, OnViewportLocationChanged));
        public static readonly StyledProperty ViewportSizeProperty = StyledProperty.Register(nameof(ViewportSize), typeof(Size), typeof(NodifyEditor), new StyledPropertyMetadata(BoxValue.Size));
        public static readonly StyledProperty ItemsExtentProperty = StyledProperty.Register(nameof(ItemsExtent), typeof(Rect), typeof(NodifyEditor), new StyledPropertyMetadata(BoxValue.Rect, OnItemsExtentChanged));
        public static readonly StyledProperty DecoratorsExtentProperty = StyledProperty.Register(nameof(DecoratorsExtent), typeof(Rect), typeof(NodifyEditor), new StyledPropertyMetadata(BoxValue.Rect));

        protected static readonly StyledPropertyKey ViewportTransformPropertyKey = StyledProperty.RegisterReadOnly(nameof(ViewportTransform), typeof(Transform), typeof(NodifyEditor), new StyledPropertyMetadata(new TransformGroup()));
        public static readonly StyledProperty ViewportTransformProperty = ViewportTransformPropertyKey.StyledProperty;

        #region Callbacks

        private static void OnItemsExtentChanged(AvaloniaObject d, StyledPropertyChangedEventArgs e)
        {
            var editor = (NodifyEditor)d;
            editor.UpdateScrollbars();
        }

        private static void OnViewportLocationChanged(AvaloniaObject d, StyledPropertyChangedEventArgs e)
        {
            var editor = (NodifyEditor)d;
            var translate = (Point)e.NewValue;

            editor.TranslateTransform.X = -translate.X * editor.ViewportZoom;
            editor.TranslateTransform.Y = -translate.Y * editor.ViewportZoom;

            editor.OnViewportUpdated();
        }

        private static void OnViewportZoomChanged(AvaloniaObject d, StyledPropertyChangedEventArgs e)
        {
            var editor = (NodifyEditor)d;
            double zoom = (double)e.NewValue;

            editor.ScaleTransform.ScaleX = zoom;
            editor.ScaleTransform.ScaleY = zoom;

            editor.ViewportSize = new Size(editor.ActualWidth / zoom, editor.ActualHeight / zoom);

            editor.ApplyRenderingOptimizations();
            editor.OnViewportUpdated();
        }

        private static void OnMinViewportZoomChanged(AvaloniaObject d, StyledPropertyChangedEventArgs e)
        {
            var zoom = (NodifyEditor)d;
            zoom.CoerceValue(MaxViewportZoomProperty);
            zoom.CoerceValue(ViewportZoomProperty);
        }

        private static object CoerceMinViewportZoom(AvaloniaObject d, object value)
            => (double)value > 0.1d ? value : 0.1d;

        private static void OnMaxViewportZoomChanged(AvaloniaObject d, StyledPropertyChangedEventArgs e)
        {
            var zoom = (NodifyEditor)d;
            zoom.CoerceValue(ViewportZoomProperty);
        }

        private static object CoerceMaxViewportZoom(AvaloniaObject d, object value)
        {
            var editor = (NodifyEditor)d;
            double min = editor.MinViewportZoom;

            return (double)value < min ? min : value;
        }

        private static object ConstrainViewportZoomToRange(AvaloniaObject d, object value)
        {
            var editor = (NodifyEditor)d;

            var num = (double)value;
            double minimum = editor.MinViewportZoom;
            if (num < minimum)
            {
                return minimum;
            }

            double maximum = editor.MaxViewportZoom;
            return num > maximum ? maximum : value;
        }
        #endregion

        #region Routed Events

        public static readonly RoutedEvent ViewportUpdatedEvent = EventManager.RegisterRoutedEvent(nameof(ViewportUpdated), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NodifyEditor));

        /// <summary>
        /// Occurs whenever the viewport updates.
        /// </summary>
        public event RoutedEventHandler ViewportUpdated
        {
            add => AddHandler(ViewportUpdatedEvent, value);
            remove => RemoveHandler(ViewportUpdatedEvent, value);
        }

        /// <summary>
        /// Updates the <see cref="ViewportSize"/> and raises the <see cref="ViewportUpdatedEvent"/>.
        /// Called when the <see cref="Control.RenderSize"/> or <see cref="ViewportZoom"/> is changed.
        /// </summary>
        protected void OnViewportUpdated()
        {
            UpdateScrollbars();
            UpdatePushedArea();
            RaiseEvent(new RoutedEventArgs(ViewportUpdatedEvent, this));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the transform used to offset the viewport.
        /// </summary>
        protected readonly TranslateTransform TranslateTransform = new TranslateTransform();

        /// <summary>
        /// Gets the transform used to zoom on the viewport.
        /// </summary>
        protected readonly ScaleTransform ScaleTransform = new ScaleTransform();

        /// <summary>
        /// Gets the transform that is applied to all child controls.
        /// </summary>
        public Transform ViewportTransform => (Transform)GetValue(ViewportTransformProperty);

        /// <summary>
        /// Gets the size of the viewport in graph space (scaled by the <see cref="ViewportZoom"/>).
        /// </summary>
        public Size ViewportSize
        {
            get => (Size)GetValue(ViewportSizeProperty);
            set => SetValue(ViewportSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the viewport's top-left coordinates in graph space coordinates.
        /// </summary>
        public Point ViewportLocation
        {
            get => (Point)GetValue(ViewportLocationProperty);
            set => SetValue(ViewportLocationProperty, value);
        }

        /// <summary>
        /// Gets or sets the zoom factor of the viewport.
        /// </summary>
        public double ViewportZoom
        {
            get => (double)GetValue(ViewportZoomProperty);
            set => SetValue(ViewportZoomProperty, value);
        }

        /// <summary>
        /// Gets or sets the minimum zoom factor of the viewport
        /// </summary>
        public double MinViewportZoom
        {
            get => (double)GetValue(MinViewportZoomProperty);
            set => SetValue(MinViewportZoomProperty, value);
        }

        /// <summary>
        /// Gets or sets the maximum zoom factor of the viewport
        /// </summary>
        public double MaxViewportZoom
        {
            get => (double)GetValue(MaxViewportZoomProperty);
            set => SetValue(MaxViewportZoomProperty, value);
        }

        /// <summary>
        /// The area covered by the <see cref="ItemContainer"/>s.
        /// </summary>
        public Rect ItemsExtent
        {
            get => (Rect)GetValue(ItemsExtentProperty);
            set => SetValue(ItemsExtentProperty, value);
        }

        /// <summary>
        /// The area covered by the <see cref="DecoratorContainer"/>s.
        /// </summary>
        public Rect DecoratorsExtent
        {
            get => (Rect)GetValue(DecoratorsExtentProperty);
            set => SetValue(DecoratorsExtentProperty, value);
        }

        #endregion

        private void ApplyRenderingOptimizations()
        {
            if (ItemsHost != null)
            {
                if (EnableRenderingContainersOptimizations && Items.Count >= OptimizeRenderingMinimumContainers)
                {
                    double zoom = ViewportZoom;
                    double availableZoomIn = 1.0 - MinViewportZoom;
                    bool shouldCache = zoom / availableZoomIn <= OptimizeRenderingZoomOutPercent;
                    ItemsHost.CacheMode = shouldCache ? new BitmapCache(1.0 / zoom) : null;
                }
                else
                {
                    ItemsHost.CacheMode = null;
                }
            }
        }

        #endregion

        #region Cosmetic Dependency Properties

        public static readonly StyledProperty BringIntoViewSpeedProperty = StyledProperty.Register(nameof(BringIntoViewSpeed), typeof(double), typeof(NodifyEditor), new StyledPropertyMetadata(BoxValue.Double1000));
        public static readonly StyledProperty BringIntoViewMaxDurationProperty = StyledProperty.Register(nameof(BringIntoViewMaxDuration), typeof(double), typeof(NodifyEditor), new StyledPropertyMetadata(BoxValue.Double1));
        public static readonly StyledProperty DisplayConnectionsOnTopProperty = StyledProperty.Register(nameof(DisplayConnectionsOnTop), typeof(bool), typeof(NodifyEditor), new StyledPropertyMetadata(BoxValue.False));
        public static readonly StyledProperty ConnectionTemplateProperty = StyledProperty.Register(nameof(ConnectionTemplate), typeof(DataTemplate), typeof(NodifyEditor));
        public static readonly StyledProperty ConnectionTemplateSelectorProperty = StyledProperty.Register(nameof(ConnectionTemplateSelector), typeof(DataTemplateSelector), typeof(NodifyEditor));
        public static readonly StyledProperty DecoratorTemplateProperty = StyledProperty.Register(nameof(DecoratorTemplate), typeof(DataTemplate), typeof(NodifyEditor));
        public static readonly StyledProperty DecoratorTemplateSelectorProperty = StyledProperty.Register(nameof(DecoratorTemplateSelector), typeof(DataTemplateSelector), typeof(NodifyEditor));
        public static readonly StyledProperty PendingConnectionTemplateProperty = StyledProperty.Register(nameof(PendingConnectionTemplate), typeof(DataTemplate), typeof(NodifyEditor));
        public static readonly StyledProperty PendingConnectionTemplateSelectorProperty = StyledProperty.Register(nameof(PendingConnectionTemplateSelector), typeof(DataTemplateSelector), typeof(NodifyEditor));
        public static readonly StyledProperty DecoratorContainerStyleProperty = StyledProperty.Register(nameof(DecoratorContainerStyle), typeof(Style), typeof(NodifyEditor));

        /// <summary>
        /// Gets or sets the maximum animation duration in seconds for bringing a location into view.
        /// </summary>
        public double BringIntoViewMaxDuration
        {
            get => (double)GetValue(BringIntoViewMaxDurationProperty);
            set => SetValue(BringIntoViewMaxDurationProperty, value);
        }

        /// <summary>
        /// Gets or sets the animation speed in pixels per second for bringing a location into view.
        /// </summary>
        /// <remarks>Total animation duration is calculated based on distance and clamped between 0.1 and <see cref="BringIntoViewMaxDuration"/>.</remarks>
        public double BringIntoViewSpeed
        {
            get => (double)GetValue(BringIntoViewSpeedProperty);
            set => SetValue(BringIntoViewSpeedProperty, value);
        }

        /// <summary>
        /// Gets or sets whether to display connections on top of <see cref="ItemContainer"/>s or not.
        /// </summary>
        public bool DisplayConnectionsOnTop
        {
            get => (bool)GetValue(DisplayConnectionsOnTopProperty);
            set => SetValue(DisplayConnectionsOnTopProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> to use when generating a new <see cref="BaseConnection"/>.
        /// </summary>
        public DataTemplate ConnectionTemplate
        {
            get => (DataTemplate)GetValue(ConnectionTemplateProperty);
            set => SetValue(ConnectionTemplateProperty, value);
        }

        /// <summary>
        /// Gets or sets the custom logic for choosing a template for <see cref="BaseConnection"/>.
        /// </summary>
        public DataTemplateSelector ConnectionTemplateSelector
        {
            get => (DataTemplateSelector)GetValue(ConnectionTemplateSelectorProperty);
            set => SetValue(ConnectionTemplateSelectorProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> to use when generating a new <see cref="DecoratorContainer"/>.
        /// </summary>
        public DataTemplate DecoratorTemplate
        {
            get => (DataTemplate)GetValue(DecoratorTemplateProperty);
            set => SetValue(DecoratorTemplateProperty, value);
        }

        /// <summary>
        /// Gets or sets the custom logic for choosing a template for <see cref="DecoratorContainer"/>.
        /// </summary>
        public DataTemplateSelector DecoratorTemplateSelector
        {
            get => (DataTemplateSelector)GetValue(DecoratorTemplateSelectorProperty);
            set => SetValue(DecoratorTemplateSelectorProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> to use for the <see cref="PendingConnection"/>.
        /// </summary>
        public DataTemplate PendingConnectionTemplate
        {
            get => (DataTemplate)GetValue(PendingConnectionTemplateProperty);
            set => SetValue(PendingConnectionTemplateProperty, value);
        }

        /// <summary>
        /// Gets or sets the custom logic for choosing a template for <see cref="PendingConnection"/>.
        /// </summary>
        public DataTemplateSelector PendingConnectionTemplateSelector
        {
            get => (DataTemplateSelector)GetValue(PendingConnectionTemplateSelectorProperty);
            set => SetValue(PendingConnectionTemplateSelectorProperty, value);
        }

        /// <summary>
        /// Gets or sets the style to use for the <see cref="DecoratorContainer"/>.
        /// </summary>
        public Style DecoratorContainerStyle
        {
            get => (Style)GetValue(DecoratorContainerStyleProperty);
            set => SetValue(DecoratorContainerStyleProperty, value);
        }

        #endregion

        #region Readonly Dependency Properties

        private static readonly StyledPropertyKey MouseLocationPropertyKey = StyledProperty.RegisterReadOnly(nameof(MouseLocation), typeof(Point), typeof(NodifyEditor), new StyledPropertyMetadata(BoxValue.Point));
        public static readonly StyledProperty MouseLocationProperty = MouseLocationPropertyKey.StyledProperty;

        /// <summary>
        /// Gets the current mouse location in graph space coordinates (relative to the <see cref="ItemsHost" />).
        /// </summary>
        public Point MouseLocation
        {
            get => (Point)GetValue(MouseLocationProperty);
            protected set => SetValue(MouseLocationPropertyKey, value);
        }

        #endregion

        #region Dependency Properties

        public static readonly StyledProperty ConnectionsProperty = StyledProperty.Register(nameof(Connections), typeof(IEnumerable), typeof(NodifyEditor));
        public static readonly StyledProperty PendingConnectionProperty = StyledProperty.Register(nameof(PendingConnection), typeof(object), typeof(NodifyEditor));
        public static readonly StyledProperty GridCellSizeProperty = StyledProperty.Register(nameof(GridCellSize), typeof(uint), typeof(NodifyEditor), new StyledPropertyMetadata(BoxValue.UInt1, OnGridCellSizeChanged, OnCoerceGridCellSize));
        public static readonly StyledProperty DisableZoomingProperty = StyledProperty.Register(nameof(DisableZooming), typeof(bool), typeof(NodifyEditor), new StyledPropertyMetadata(BoxValue.False));
        public static readonly StyledProperty HasCustomContextMenuProperty = StyledProperty.Register(nameof(HasCustomContextMenu), typeof(bool), typeof(NodifyEditor), new StyledPropertyMetadata(BoxValue.False));
        public static readonly StyledProperty DecoratorsProperty = StyledProperty.Register(nameof(Decorators), typeof(IEnumerable), typeof(NodifyEditor));

        private static object OnCoerceGridCellSize(AvaloniaObject d, object value)
            => (uint)value > 0u ? value : BoxValue.UInt1;

        private static void OnGridCellSizeChanged(AvaloniaObject d, StyledPropertyChangedEventArgs e) { }

        /// <summary>
        /// Gets or sets the items that will be rendered in the decorators layer via <see cref="DecoratorContainer"/>s.
        /// </summary>
        public IEnumerable Decorators
        {
            get => (IEnumerable)GetValue(DecoratorsProperty);
            set => SetValue(DecoratorsProperty, value);
        }

        /// <summary>
        /// Gets or sets the value of an invisible grid used to adjust locations (snapping) of <see cref="ItemContainer"/>s.
        /// </summary>
        public uint GridCellSize
        {
            get => (uint)GetValue(GridCellSizeProperty);
            set => SetValue(GridCellSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the data source that <see cref="BaseConnection"/>s will be generated for.
        /// </summary>
        public IEnumerable Connections
        {
            get => (IEnumerable)GetValue(ConnectionsProperty);
            set => SetValue(ConnectionsProperty, value);
        }

        /// <summary>
        /// Gets of sets the <see cref="Control.DataContext"/> of the <see cref="Nodify.PendingConnection"/>.
        /// </summary>
        public object PendingConnection
        {
            get => GetValue(PendingConnectionProperty);
            set => SetValue(PendingConnectionProperty, value);
        }

        /// <summary>
        /// Gets or sets whether zooming should be disabled.
        /// </summary>
        public bool DisableZooming
        {
            get => (bool)GetValue(DisableZoomingProperty);
            set => SetValue(DisableZoomingProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the editor uses a custom context menu.
        /// </summary>
        /// <remarks>When set to true, the editor handles the right-click event for specific interactions.</remarks>
        public bool HasCustomContextMenu
        {
            get => (bool)GetValue(HasCustomContextMenuProperty);
            set => SetValue(HasCustomContextMenuProperty, value);
        }

        /// <summary>
        /// Gets a value indicating whether the editor has a context menu.
        /// </summary>
        public bool HasContextMenu => ContextMenu != null || HasCustomContextMenu;

        #endregion

        #region Command Dependency Properties

        public static readonly StyledProperty ConnectionCompletedCommandProperty = StyledProperty.Register(nameof(ConnectionCompletedCommand), typeof(ICommand), typeof(NodifyEditor));
        public static readonly StyledProperty ConnectionStartedCommandProperty = StyledProperty.Register(nameof(ConnectionStartedCommand), typeof(ICommand), typeof(NodifyEditor));
        public static readonly StyledProperty DisconnectConnectorCommandProperty = StyledProperty.Register(nameof(DisconnectConnectorCommand), typeof(ICommand), typeof(NodifyEditor));
        public static readonly StyledProperty RemoveConnectionCommandProperty = StyledProperty.Register(nameof(RemoveConnectionCommand), typeof(ICommand), typeof(NodifyEditor));

        /// <summary>
        /// Invoked when the <see cref="Nodify.PendingConnection"/> is completed. <br />
        /// Use <see cref="PendingConnection.StartedCommand"/> if you want to control the visibility of the connection from the viewmodel. <br />
        /// Parameter is <see cref="PendingConnection.Source"/>.
        /// </summary>
        public ICommand? ConnectionStartedCommand
        {
            get => (ICommand?)GetValue(ConnectionStartedCommandProperty);
            set => SetValue(ConnectionStartedCommandProperty, value);
        }

        /// <summary>
        /// Invoked when the <see cref="Nodify.PendingConnection"/> is completed. <br />
        /// Use <see cref="PendingConnection.CompletedCommand"/> if you want to control the visibility of the connection from the viewmodel. <br />
        /// Parameter is <see cref="Tuple{T, U}"/> where <see cref="Tuple{T, U}.Item1"/> is the <see cref="PendingConnection.Source"/> and <see cref="Tuple{T, U}.Item2"/> is <see cref="PendingConnection.Target"/>.
        /// </summary>
        public ICommand? ConnectionCompletedCommand
        {
            get => (ICommand?)GetValue(ConnectionCompletedCommandProperty);
            set => SetValue(ConnectionCompletedCommandProperty, value);
        }

        /// <summary>
        /// Invoked when the <see cref="Connector.Disconnect"/> event is raised. <br />
        /// Can also be handled at the <see cref="Connector"/> level using the <see cref="Connector.DisconnectCommand"/> command. <br />
        /// Parameter is the <see cref="Connector"/>'s <see cref="Control.DataContext"/>.
        /// </summary>
        public ICommand? DisconnectConnectorCommand
        {
            get => (ICommand?)GetValue(DisconnectConnectorCommandProperty);
            set => SetValue(DisconnectConnectorCommandProperty, value);
        }

        /// <summary>
        /// Invoked when the <see cref="BaseConnection.Disconnect"/> event is raised. <br />
        /// Can also be handled at the <see cref="BaseConnection"/> level using the <see cref="BaseConnection.DisconnectCommand"/> command. <br />
        /// Parameter is the <see cref="BaseConnection"/>'s <see cref="Control.DataContext"/>.
        /// </summary>
        public ICommand? RemoveConnectionCommand
        {
            get => (ICommand?)GetValue(RemoveConnectionCommandProperty);
            set => SetValue(RemoveConnectionCommandProperty, value);
        }

        #endregion

        #region Fields

        /// <summary>
        /// Correct <see cref="ItemContainer"/>'s position after moving if starting position is not snapped to grid.
        /// </summary>
        public static bool EnableSnappingCorrection { get; set; } = true;

        /// <summary>
        /// Gets or sets if <see cref="NodifyEditor"/>s should enable optimizations based on <see cref="OptimizeRenderingMinimumContainers"/> and <see cref="OptimizeRenderingZoomOutPercent"/>.
        /// </summary>
        public static bool EnableRenderingContainersOptimizations { get; set; } = true;

        /// <summary>
        /// Gets or sets the minimum number of <see cref="ItemContainer"/>s needed to trigger optimizations when reaching the <see cref="OptimizeRenderingZoomOutPercent"/>.
        /// </summary>
        public static uint OptimizeRenderingMinimumContainers { get; set; } = 700;

        /// <summary>
        /// Gets or sets the minimum zoom out percent needed to start optimizing the rendering for <see cref="ItemContainer"/>s.
        /// Value is between 0 and 1.
        /// </summary>
        public static double OptimizeRenderingZoomOutPercent { get; set; } = 0.3;

        /// <summary>
        /// Gets or sets the margin to add in all directions to the <see cref="ItemsExtent"/> or area parameter when using <see cref="FitToScreen(Rect?)"/>.
        /// </summary>
        public static double FitToScreenExtentMargin { get; set; } = 30;

        /// <summary>
        /// Gets or sets the maximum distance, in pixels, that the mouse can move before suppressing certain mouse actions. 
        /// This is useful for suppressing actions like showing a <see cref="ContextMenu"/> if the mouse has moved significantly.
        /// </summary>
        public static double MouseActionSuppressionThreshold { get; set; } = 12d;

        /// <summary>
        /// Tells if the <see cref="NodifyEditor"/> is doing operations on multiple items at once.
        /// </summary>
        public bool IsBulkUpdatingItems { get; protected set; }

        /// <summary>
        /// Gets the panel that holds all the <see cref="ItemContainer"/>s.
        /// </summary>
        protected internal Panel ItemsHost { get; private set; } = default!;

        /// <summary>
        /// Gets the element that holds all the <see cref="BaseConnection"/>s and custom connections.
        /// </summary>
        protected internal Control ConnectionsHost { get; private set; } = default!;

        /// <summary>
        /// Gets a list of all <see cref="ItemContainer"/>s.
        /// </summary>
        /// <remarks>Cache the result before using it to avoid extra allocations.</remarks>
        protected internal IReadOnlyCollection<ItemContainer> ItemContainers
        {
            get
            {
                ItemCollection items = Items;
                var containers = new List<ItemContainer>(items.Count);

                for (var i = 0; i < items.Count; i++)
                {
                    containers.Add((ItemContainer)ItemContainerGenerator.ContainerFromIndex(i));
                }

                return containers;
            }
        }

        #endregion

        #region Construction

        static NodifyEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NodifyEditor), new StyledPropertyMetadata(typeof(NodifyEditor)));
            FocusableProperty.OverrideMetadata(typeof(NodifyEditor), new StyledPropertyMetadata(BoxValue.True));

            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(NodifyEditor), new StyledPropertyMetadata(KeyboardNavigationMode.None));
            KeyboardNavigation.ControlTabNavigationProperty.OverrideMetadata(typeof(NodifyEditor), new StyledPropertyMetadata(KeyboardNavigationMode.None));
            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(NodifyEditor), new StyledPropertyMetadata(KeyboardNavigationMode.None));

            EditorCommands.RegisterCommandBindings<NodifyEditor>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodifyEditor"/> class.
        /// </summary>
        public NodifyEditor()
        {
            AddHandler(Connector.DisconnectEvent, new ConnectorEventHandler(OnConnectorDisconnected));
            AddHandler(Connector.PendingConnectionStartedEvent, new PendingConnectionEventHandler(OnConnectionStarted));
            AddHandler(Connector.PendingConnectionCompletedEvent, new PendingConnectionEventHandler(OnConnectionCompleted));

            AddHandler(BaseConnection.DisconnectEvent, new ConnectionEventHandler(OnRemoveConnection));

            var transform = new TransformGroup();
            transform.Children.Add(ScaleTransform);
            transform.Children.Add(TranslateTransform);

            SetValue(ViewportTransformPropertyKey, transform);

            InputProcessor.AddSharedHandlers(this);

            Loaded += OnEditorLoaded;
            Unloaded += OnEditorUnloaded;

            _focusNavigator = new StatefulFocusNavigator<ItemContainer>(OnElementFocused);
        }

        /// <inheritdoc />
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ItemsHost = GetTemplateChild(ElementItemsHost) as Panel ?? throw new InvalidOperationException($"{ElementItemsHost} is missing or is not of type Panel.");
            ConnectionsHost = GetTemplateChild(ElementConnectionsHost) as Control ?? throw new InvalidOperationException($"{ElementConnectionsHost} is missing or is not of type Control.");

            OnDisableAutoPanningChanged(DisableAutoPanning);
        }

        private void OnEditorLoaded(object sender, RoutedEventArgs e)
        {
            // It's safe to call RegisterNavigationLayer multiple times. It only registers once for the same id.
            RegisterNavigationLayer(this);
            ActivateNavigationLayer(KeyboardNavigationLayer.Id);
        }

        private void OnEditorUnloaded(object sender, RoutedEventArgs e)
        {
            OnDisableAutoPanningChanged(true);
        }

        /// <inheritdoc />
        protected override AvaloniaObject GetContainerForItemOverride()
            => new ItemContainer(this)
            {
                RenderTransform = new TranslateTransform()
            };

        /// <inheritdoc />
        protected override bool IsItemItsOwnContainerOverride(object item)
            => item is ItemContainer;

        #endregion

        #region Methods

        /// <summary>
        /// Zoom in at the viewport's center.
        /// </summary>
        public void ZoomIn() => ZoomAtPosition(Math.Pow(2.0, 120.0 / 3.0 / Mouse.MouseWheelDeltaForOneLine), ViewportLocation + (Vector)ViewportSize / 2);

        /// <summary>
        /// Zoom out at the viewport's center.
        /// </summary>
        public void ZoomOut() => ZoomAtPosition(Math.Pow(2.0, -120.0 / 3.0 / Mouse.MouseWheelDeltaForOneLine), ViewportLocation + (Vector)ViewportSize / 2);

        /// <summary>
        /// Zoom at the specified location in graph space coordinates.
        /// </summary>
        /// <param name="zoom">The zoom factor to apply. A value greater than 1 zooms in, while a value between 0 and 1 zooms out.</param>
        /// <param name="location">The point in graph space coordinates where the zoom should be centered. </param>
        public void ZoomAtPosition(double zoom, Point location)
        {
            if (!DisableZooming)
            {
                double prevZoom = ViewportZoom;
                ViewportZoom *= zoom;

                if (Math.Abs(prevZoom - ViewportZoom) > 0.001)
                {
                    // get the actual zoom value because Zoom might have been coerced
                    zoom = ViewportZoom / prevZoom;

                    var offsetToLocation = (Vector)location - (Vector)ViewportLocation;
                    var scaledOffset = offsetToLocation * zoom;
                    var viewportAdjustment = scaledOffset - offsetToLocation;

                    // needs to be divided by zoom to negate the scaling of the translate transform on OnViewportLocationChanged
                    ViewportLocation += viewportAdjustment / zoom;
                }
            }
        }

        /// <summary>
        /// Moves the viewport center at the specified location.
        /// </summary>
        /// <param name="point">The location in graph space coordinates.</param>
        /// <param name="animated">True to animate the movement.</param>
        /// <param name="onFinish">The callback invoked when movement is finished.</param>
        /// <remarks>Temporarily disables editor controls when animated.</remarks>
        public void BringIntoView(Point point, bool animated = true, Action? onFinish = null)
        {
            Point newLocation = (Point)((Vector)point - (Vector)ViewportSize / 2);

            if (animated && newLocation != ViewportLocation)
            {
                BeginPanning();
                SetCurrentValue(DisablePanningProperty, true);
                SetCurrentValue(DisableZoomingProperty, true);

                double distance = (newLocation - ViewportLocation).Length;
                double duration = distance / (BringIntoViewSpeed + (distance / 10)) * ViewportZoom;
                duration = Math.Max(0.1, Math.Min(duration, BringIntoViewMaxDuration));

                this.StartAnimation(ViewportLocationProperty, newLocation, duration, (s, e) =>
                {
                    EndPanning();
                    SetCurrentValue(DisablePanningProperty, false);
                    SetCurrentValue(DisableZoomingProperty, false);

                    onFinish?.Invoke();
                });
            }
            else
            {
                SetCurrentValue(ViewportLocationProperty, newLocation);
                onFinish?.Invoke();
            }
        }

        /// <summary>
        /// Moves the viewport center at the center of the specified area.
        /// </summary>
        /// <param name="area">The location in graph space coordinates.</param>
        public new void BringIntoView(Rect area)
            => BringIntoView(new Point(area.X + area.Width / 2, area.Y + area.Height / 2));

        /// <summary>
        /// Ensures the specified item container is fully visible within the viewport, optionally with padding around the edges.
        /// </summary>
        /// <param name="container">The item container to bring into view.</param>
        /// <param name="offsetFromEdge">The padding to apply around the container</param>
        public void BringIntoView(Rect area, double offsetFromEdge = 32d)
        {
            var viewport = new Rect(ViewportLocation, ViewportSize);

            area.Inflate(offsetFromEdge, offsetFromEdge);

            if (!viewport.Contains(area))
            {
                if (viewport.IntersectsWith(area))
                {
                    double newX = viewport.X;
                    double newY = viewport.Y;

                    if (area.Left < viewport.Left)
                    {
                        newX = area.Left;
                    }
                    else if (area.Right > viewport.Right)
                    {
                        newX = area.Right - viewport.Width;
                    }

                    if (area.Top < viewport.Top)
                    {
                        newY = area.Top;
                    }
                    else if (area.Bottom > viewport.Bottom)
                    {
                        newY = area.Bottom - viewport.Height;
                    }

                    BringIntoView(new Point(newX, newY) + new Vector(viewport.Width / 2, viewport.Height / 2));
                }
                else
                {
                    BringIntoView(area);
                }
            }
        }

        /// <summary>
        /// Reset the viewport location to (0, 0) and the viewport zoom to 1.
        /// </summary>
        /// <param name="animated">Whether the viewport transition is animated.</param>
        /// <param name="onFinish">The callback invoked when the viewport transition is finished.</param>
        public void ResetViewport(bool animated = true, Action? onFinish = null)
        {
            BringIntoView(new Point(ViewportSize.Width / 2, ViewportSize.Height / 2), animated, () =>
            {
                if (animated)
                {
                    this.StartAnimation(ViewportZoomProperty, 1d, BringIntoViewMaxDuration, (s, e) =>
                    {
                        onFinish?.Invoke();
                    });
                }
                else
                {
                    SetCurrentValue(ViewportZoomProperty, BoxValue.Double1);
                    onFinish?.Invoke();
                }
            });
        }

        /// <summary>
        /// Scales the viewport to fit the specified <paramref name="area"/> or all the <see cref="ItemContainer"/>s if that's possible.
        /// </summary>
        /// <remarks>Does nothing if <paramref name="area"/> is null and there's no items.</remarks>
        public void FitToScreen(Rect? area = null)
        {
            Rect extent = area ?? ItemsExtent;
            extent.Inflate(FitToScreenExtentMargin, FitToScreenExtentMargin);

            if (extent.Width > 0 && extent.Height > 0)
            {
                double widthRatio = ViewportSize.Width / extent.Width;
                double heightRatio = ViewportSize.Height / extent.Height;

                double zoom = Math.Min(widthRatio, heightRatio);
                var center = new Point(extent.X + extent.Width / 2, extent.Y + extent.Height / 2);

                ZoomAtPosition(zoom, center);
                BringIntoView(center, animated: false);
            }
        }

        /// <summary>
        /// Aligns the selected containers based on the specified alignment.
        /// </summary>
        /// <param name="alignment">The alignment type to apply to the selected containers.</param>
        /// <param name="relativeTo">An optional container to use as a reference for alignment. If null, the alignment is based on the containers themselves.</param>
        /// <remarks>This method has no effect if a dragging operation is in progress.</remarks>
        public void AlignSelection(Alignment alignment, ItemContainer? relativeTo = default)
            => AlignContainers(SelectedContainers, alignment, relativeTo);

        /// <summary>
        /// Aligns a collection of containers based on the specified alignment.
        /// </summary>
        /// <param name="containers">The collection of item containers to align.</param>
        /// <param name="alignment">The alignment type to apply to the containers.</param>
        /// <param name="relativeTo">An optional container to use as a reference for alignment. If null, the alignment is based on the containers themselves.</param>
        /// <remarks>This method has no effect if a dragging operation is in progress.</remarks>
        public void AlignContainers(IEnumerable<ItemContainer> containers, Alignment alignment, ItemContainer? relativeTo = default)
        {
            if (IsDragging)
            {
                return;
            }

            IsDragging = true;
            IsBulkUpdatingItems = true;

            containers.Align(alignment, relativeTo);

            IsBulkUpdatingItems = false;
            // Draw the containers at the new position.
            ItemsHost.InvalidateArrange();

            IsDragging = false;
        }

        /// <summary>
        /// Locks the position of the <see cref="SelectedContainers"/>.
        /// </summary>
        public void LockSelection()
        {
            foreach (var container in SelectedContainers)
            {
                container.IsDraggable = false;
            }
        }

        /// <summary>
        /// Unlocks the position of the <see cref="SelectedContainers"/>.
        /// </summary>
        public void UnlockSelection()
        {
            foreach (var container in SelectedContainers)
            {
                container.IsDraggable = true;
            }
        }

        #endregion

        #region Connector handling

        private void OnConnectorDisconnected(object sender, ConnectorEventArgs e)
        {
            if (!e.Handled && (DisconnectConnectorCommand?.CanExecute(e.Connector) ?? false))
            {
                DisconnectConnectorCommand.Execute(e.Connector);
                e.Handled = true;
            }
        }

        private void OnConnectionStarted(object sender, PendingConnectionEventArgs e)
        {
            if (!e.Canceled && ConnectionStartedCommand != null)
            {
                e.Canceled = !ConnectionStartedCommand.CanExecute(e.SourceConnector);
                if (!e.Canceled)
                {
                    ConnectionStartedCommand.Execute(e.SourceConnector);
                }
            }
        }

        private void OnConnectionCompleted(object sender, PendingConnectionEventArgs e)
        {
            if (!e.Canceled)
            {
                (object SourceConnector, object? TargetConnector) result = (e.SourceConnector, e.TargetConnector);
                if (ConnectionCompletedCommand?.CanExecute(result) ?? false)
                {
                    ConnectionCompletedCommand.Execute(result);
                }
            }
        }

        private void OnRemoveConnection(object sender, ConnectionEventArgs e)
        {
            OnRemoveConnection(e.Connection);
        }

        protected void OnRemoveConnection(object? dataContext)
        {
            if (RemoveConnectionCommand?.CanExecute(dataContext) ?? false)
            {
                RemoveConnectionCommand.Execute(dataContext);
            }
        }

        #endregion

        #region Gesture Handling

        protected InputProcessor InputProcessor { get; } = new InputProcessor();

        /// <inheritdoc />
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            MouseLocation = e.GetPosition(ItemsHost);
            InputProcessor.ProcessEvent(e);
        }

        /// <inheritdoc />
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            MouseLocation = e.GetPosition(ItemsHost);
            InputProcessor.ProcessEvent(e);

            // Release the mouse capture if all the mouse buttons are released and there's no interaction in progress
            if (!InputProcessor.RequiresInputCapture && IsMouseCaptured && e.RightButton == MouseButtonState.Released && e.LeftButton == MouseButtonState.Released && e.MiddleButton == MouseButtonState.Released)
            {
                ReleaseMouseCapture();
            }
        }

        /// <inheritdoc />
        protected override void OnMouseMove(MouseEventArgs e)
        {
            MouseLocation = e.GetPosition(ItemsHost);
            InputProcessor.ProcessEvent(e);
        }

        /// <inheritdoc />
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            MouseLocation = e.GetPosition(ItemsHost);
            InputProcessor.ProcessEvent(e);
        }

        /// <inheritdoc />
        protected override void OnLostMouseCapture(MouseEventArgs e)
            => InputProcessor.ProcessEvent(e);

        /// <inheritdoc />
        protected override void OnKeyUp(KeyEventArgs e)
        {
            InputProcessor.ProcessEvent(e);

            // Release the mouse capture if all the mouse buttons are released and there's no interaction in progress
            if (!InputProcessor.RequiresInputCapture && IsMouseCaptured && Mouse.RightButton == MouseButtonState.Released && Mouse.LeftButton == MouseButtonState.Released && Mouse.MiddleButton == MouseButtonState.Released)
            {
                ReleaseMouseCapture();
            }
        }

        /// <inheritdoc />
        protected override void OnKeyDown(KeyEventArgs e)
            => InputProcessor.ProcessEvent(e);

        #endregion

        /// <inheritdoc />
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            double zoom = ViewportZoom;
            ViewportSize = new Size(ActualWidth / zoom, ActualHeight / zoom);

            OnViewportUpdated();
        }

        #region Utilities

        /// <summary>
        /// Translates the specified location to graph space coordinates (relative to the <see cref="ItemsHost" />).
        /// </summary>
        /// <param name="location">The location coordinates relative to <paramref name="relativeTo"/></param>
        /// <param name="relativeTo">The element where the <paramref name="location"/> was calculated from.</param>
        /// <returns>A location inside the graph.</returns>
        public Point GetLocationInsideEditor(Point location, Control relativeTo)
            => relativeTo.TranslatePoint(location, ItemsHost);

        /// <summary>
        /// Translates the event location to graph space coordinates (relative to the <see cref="ItemsHost" />).
        /// </summary>
        /// <param name="args">The drag event.</param>
        /// <returns>A location inside the graph</returns>
        public Point GetLocationInsideEditor(DragEventArgs args)
            => args.GetPosition(ItemsHost);

        /// <summary>
        /// Translates the event location to graph space coordinates (relative to the <see cref="ItemsHost" />).
        /// </summary>
        /// <param name="args">The mouse event.</param>
        /// <returns>A location inside the graph</returns>
        public Point GetLocationInsideEditor(MouseEventArgs args)
            => args.GetPosition(ItemsHost);

        /// <summary>
        /// Snaps the given value down to the nearest multiple of the grid cell size.
        /// </summary>
        /// <param name="value">The value to be snapped to the grid.</param>
        /// <returns>The largest multiple of the grid cell size less than or equal to the value.</returns>
        public double SnapToGrid(double value)
        {
            return (int)value / GridCellSize * GridCellSize;
        }

        /// <summary>
        /// Returns all visual elements of type <typeparamref name="T"/> that intersect with the current viewport.
        /// The bounds of each element are determined by the provided <paramref name="getBounds"/> function.
        /// </summary>
        /// <typeparam name="T">The type of visual elements to search for.</typeparam>
        /// <param name="getBounds">
        /// A function that takes an element of type <typeparamref name="T"/> and returns its bounding rectangle (in the same coordinate space as the viewport).
        /// </param>
        internal IEnumerable<Connector> GetConnectorsInViewport()
        {
            var viewport = new Rect(ViewportLocation, ViewportSize);

            var stack = new Stack<AvaloniaObject>();
            stack.Push(this);

            while (stack.Count > 0)
            {
                AvaloniaObject current = stack.Pop();
                int childrenCount = VisualTreeHelper.GetChildrenCount(current);

                for (int i = 0; i < childrenCount; i++)
                {
                    AvaloniaObject child = VisualTreeHelper.GetChild(current, i);

                    if (child is Connector connector && connector.Container != null && connector.Container.IsSelectableInArea(viewport, isContained: false))
                    {
                        connector.UpdateAnchor();
                        if (viewport.Contains(connector.Anchor))
                        {
                            yield return connector;
                            continue;
                        }
                    }

                    stack.Push(child);
                }
            }
        }

        #endregion
    }
}
