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
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Metadata;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Controls.Templates;

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
    [DefaultProperty(nameof(Decorators))]
    public partial class NodifyEditor
    {
        protected const string ElementItemsHost = "PART_ItemsHost";
        protected const string ElementConnectionsHost = "PART_ConnectionsHost";

        #region Viewport

        public static readonly StyledProperty<double> ViewportZoomProperty =
            AvaloniaProperty.Register<NodifyEditor, double>(nameof(ViewportZoom), 1.0, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay,
                coerce: ConstrainViewportZoomToRange);

        public static readonly StyledProperty<double> MinViewportZoomProperty =
            AvaloniaProperty.Register<NodifyEditor, double>(nameof(MinViewportZoom), 0.1,
                coerce: CoerceMinViewportZoom);

        public static readonly StyledProperty<double> MaxViewportZoomProperty =
            AvaloniaProperty.Register<NodifyEditor, double>(nameof(MaxViewportZoom), 2.0,
                coerce: CoerceMaxViewportZoom);

        public static readonly StyledProperty<Point> ViewportLocationProperty =
            AvaloniaProperty.Register<NodifyEditor, Point>(nameof(ViewportLocation), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public static readonly StyledProperty<Size> ViewportSizeProperty =
            AvaloniaProperty.Register<NodifyEditor, Size>(nameof(ViewportSize));

        public static readonly StyledProperty<Rect> ItemsExtentProperty =
            AvaloniaProperty.Register<NodifyEditor, Rect>(nameof(ItemsExtent));

        public static readonly StyledProperty<Rect> DecoratorsExtentProperty =
            AvaloniaProperty.Register<NodifyEditor, Rect>(nameof(DecoratorsExtent));

        private readonly TransformGroup _viewportTransform = new TransformGroup();
        public static readonly DirectProperty<NodifyEditor, Transform> ViewportTransformProperty =
            AvaloniaProperty.RegisterDirect<NodifyEditor, Transform>(
                nameof(ViewportTransform),
                o => o._viewportTransform);

        #region Callbacks

        private static void OnItemsExtentChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            var editor = (NodifyEditor)d;
            editor.UpdateScrollbars();
        }

        private static void OnViewportLocationChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            var editor = (NodifyEditor)d;
            var translate = (Point)(e.NewValue ?? default(Point));

            editor.TranslateTransform.X = -translate.X * editor.ViewportZoom;
            editor.TranslateTransform.Y = -translate.Y * editor.ViewportZoom;

            editor.OnViewportUpdated();
        }

        private static void OnViewportZoomChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            var editor = (NodifyEditor)d;
            double zoom = (double)(e.NewValue ?? 1.0);

            editor.ScaleTransform.ScaleX = zoom;
            editor.ScaleTransform.ScaleY = zoom;

            editor.ViewportSize = new Size(editor.Bounds.Width / zoom, editor.Bounds.Height / zoom);

            editor.ApplyRenderingOptimizations();
            editor.OnViewportUpdated();
        }

        private static void OnMinViewportZoomChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            var zoom = (NodifyEditor)d;
            zoom.CoerceValue(MaxViewportZoomProperty);
            zoom.CoerceValue(ViewportZoomProperty);
        }

        private static double CoerceMinViewportZoom(AvaloniaObject d, double value)
            => value > 0.1 ? value : 0.1;

        private static void OnMaxViewportZoomChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            var zoom = (NodifyEditor)d;
            zoom.CoerceValue(ViewportZoomProperty);
        }

        private static double CoerceMaxViewportZoom(AvaloniaObject d, double value)
        {
            var editor = (NodifyEditor)d;
            double min = editor.MinViewportZoom;

            return value < min ? min : value;
        }

        private static double ConstrainViewportZoomToRange(AvaloniaObject d, double value)
        {
            var editor = (NodifyEditor)d;

            double minimum = editor.MinViewportZoom;
            if (value < minimum)
            {
                return minimum;
            }

            double maximum = editor.MaxViewportZoom;
            return num > maximum ? maximum : value;
        }
        #endregion

        #region Routed Events

        public static readonly RoutedEvent<RoutedEventArgs> ViewportUpdatedEvent =
            RoutedEvent.Register<NodifyEditor, RoutedEventArgs>(nameof(ViewportUpdated), Avalonia.Interactivity.RoutingStrategies.Bubble);

        /// <summary>
        /// Occurs whenever the viewport updates.
        /// </summary>
        public event EventHandler<RoutedEventArgs> ViewportUpdated
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
        public Transform ViewportTransform => _viewportTransform;

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

        public static readonly StyledProperty<double> BringIntoViewSpeedProperty =
            AvaloniaProperty.Register<NodifyEditor, double>(nameof(BringIntoViewSpeed), 1000.0);

        public static readonly StyledProperty<double> BringIntoViewMaxDurationProperty =
            AvaloniaProperty.Register<NodifyEditor, double>(nameof(BringIntoViewMaxDuration), 1.0);

        public static readonly StyledProperty<bool> DisplayConnectionsOnTopProperty =
            AvaloniaProperty.Register<NodifyEditor, bool>(nameof(DisplayConnectionsOnTop), false);

        public static readonly StyledProperty<IDataTemplate?> ConnectionTemplateProperty =
            AvaloniaProperty.Register<NodifyEditor, IDataTemplate?>(nameof(ConnectionTemplate));

        // TODO: DataTemplateSelector doesn't exist in Avalonia - use custom template selection logic
        // In Avalonia, template selection is typically done via FuncDataTemplate or custom logic
        public static readonly StyledProperty<object?> ConnectionTemplateSelectorProperty =
            AvaloniaProperty.Register<NodifyEditor, object?>(nameof(ConnectionTemplateSelector));

        public static readonly StyledProperty<IDataTemplate?> DecoratorTemplateProperty =
            AvaloniaProperty.Register<NodifyEditor, IDataTemplate?>(nameof(DecoratorTemplate));

        // TODO: DataTemplateSelector doesn't exist in Avalonia
        public static readonly StyledProperty<object?> DecoratorTemplateSelectorProperty =
            AvaloniaProperty.Register<NodifyEditor, object?>(nameof(DecoratorTemplateSelector));

        public static readonly StyledProperty<IDataTemplate?> PendingConnectionTemplateProperty =
            AvaloniaProperty.Register<NodifyEditor, IDataTemplate?>(nameof(PendingConnectionTemplate));

        // TODO: DataTemplateSelector doesn't exist in Avalonia
        public static readonly StyledProperty<object?> PendingConnectionTemplateSelectorProperty =
            AvaloniaProperty.Register<NodifyEditor, object?>(nameof(PendingConnectionTemplateSelector));

        public static readonly StyledProperty<Style?> DecoratorContainerStyleProperty =
            AvaloniaProperty.Register<NodifyEditor, Style?>(nameof(DecoratorContainerStyle));

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
        /// Gets or sets the <see cref="IDataTemplate"/> to use when generating a new <see cref="BaseConnection"/>.
        /// </summary>
        public IDataTemplate? ConnectionTemplate
        {
            get => GetValue(ConnectionTemplateProperty);
            set => SetValue(ConnectionTemplateProperty, value);
        }

        /// <summary>
        /// Gets or sets the custom logic for choosing a template for <see cref="BaseConnection"/>.
        /// </summary>
        /// <remarks>In Avalonia, use custom template selection logic instead of DataTemplateSelector.</remarks>
        public object? ConnectionTemplateSelector
        {
            get => GetValue(ConnectionTemplateSelectorProperty);
            set => SetValue(ConnectionTemplateSelectorProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="IDataTemplate"/> to use when generating a new <see cref="DecoratorContainer"/>.
        /// </summary>
        public IDataTemplate? DecoratorTemplate
        {
            get => GetValue(DecoratorTemplateProperty);
            set => SetValue(DecoratorTemplateProperty, value);
        }

        /// <summary>
        /// Gets or sets the custom logic for choosing a template for <see cref="DecoratorContainer"/>.
        /// </summary>
        /// <remarks>In Avalonia, use custom template selection logic instead of DataTemplateSelector.</remarks>
        public object? DecoratorTemplateSelector
        {
            get => GetValue(DecoratorTemplateSelectorProperty);
            set => SetValue(DecoratorTemplateSelectorProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="IDataTemplate"/> to use for the <see cref="PendingConnection"/>.
        /// </summary>
        public IDataTemplate? PendingConnectionTemplate
        {
            get => GetValue(PendingConnectionTemplateProperty);
            set => SetValue(PendingConnectionTemplateProperty, value);
        }

        /// <summary>
        /// Gets or sets the custom logic for choosing a template for <see cref="PendingConnection"/>.
        /// </summary>
        /// <remarks>In Avalonia, use custom template selection logic instead of DataTemplateSelector.</remarks>
        public object? PendingConnectionTemplateSelector
        {
            get => GetValue(PendingConnectionTemplateSelectorProperty);
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

        private Point _mouseLocation;
        public static readonly DirectProperty<NodifyEditor, Point> MouseLocationProperty =
            AvaloniaProperty.RegisterDirect<NodifyEditor, Point>(
                nameof(MouseLocation),
                o => o._mouseLocation,
                (o, v) => o._mouseLocation = v);

        /// <summary>
        /// Gets the current mouse location in graph space coordinates (relative to the <see cref="ItemsHost" />).
        /// </summary>
        public Point MouseLocation
        {
            get => _mouseLocation;
            protected set => SetAndRaise(MouseLocationProperty, ref _mouseLocation, value);
        }

        #endregion

        #region Dependency Properties

        public static readonly StyledProperty<IEnumerable> ConnectionsProperty = AvaloniaProperty.Register<NodifyEditor, IEnumerable>(nameof(Connections));
        public static readonly StyledProperty<object> PendingConnectionProperty = AvaloniaProperty.Register<NodifyEditor, object>(nameof(PendingConnection));
        public static readonly StyledProperty<uint> GridCellSizeProperty = AvaloniaProperty.Register<NodifyEditor, uint>(nameof(GridCellSize), defaultValue: 1u, coerce: OnCoerceGridCellSize);
        public static readonly StyledProperty<bool> DisableZoomingProperty = AvaloniaProperty.Register<NodifyEditor, bool>(nameof(DisableZooming), defaultValue: false);
        public static readonly StyledProperty<bool> HasCustomContextMenuProperty = AvaloniaProperty.Register<NodifyEditor, bool>(nameof(HasCustomContextMenu), defaultValue: false);
        public static readonly StyledProperty<IEnumerable> DecoratorsProperty = AvaloniaProperty.Register<NodifyEditor, IEnumerable>(nameof(Decorators));

        private static uint OnCoerceGridCellSize(AvaloniaObject d, uint value)
            => value > 0u ? value : 1u;

        private static void OnGridCellSizeChanged(NodifyEditor editor, AvaloniaPropertyChangedEventArgs<uint> e) { }

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

        public static readonly StyledProperty<ICommand?> ConnectionCompletedCommandProperty = AvaloniaProperty.Register<NodifyEditor, ICommand?>(nameof(ConnectionCompletedCommand));
        public static readonly StyledProperty<ICommand?> ConnectionStartedCommandProperty = AvaloniaProperty.Register<NodifyEditor, ICommand?>(nameof(ConnectionStartedCommand));
        public static readonly StyledProperty<ICommand?> DisconnectConnectorCommandProperty = AvaloniaProperty.Register<NodifyEditor, ICommand?>(nameof(DisconnectConnectorCommand));
        public static readonly StyledProperty<ICommand?> RemoveConnectionCommandProperty = AvaloniaProperty.Register<NodifyEditor, ICommand?>(nameof(RemoveConnectionCommand));

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

            // Property change handlers from partial files
            IsCuttingProperty.Changed.AddClassHandler<NodifyEditor>((editor, e) =>
            {
                if (e.NewValue.GetValueOrDefault())
                    editor.OnCuttingStarted();
                else
                    editor.OnCuttingCompleted();
            });

            IsDraggingProperty.Changed.AddClassHandler<NodifyEditor>((editor, e) =>
            {
                if (e.NewValue.GetValueOrDefault())
                    editor.OnItemsDragStarted();
                else
                    editor.OnItemsDragCompleted();
            });

            DisableAutoPanningProperty.Changed.AddClassHandler<NodifyEditor>((editor, e) =>
                editor.OnDisableAutoPanningChanged(e.NewValue.GetValueOrDefault()));

            DisablePanningProperty.Changed.AddClassHandler<NodifyEditor>((editor, e) =>
                editor.OnDisableAutoPanningChanged(editor.DisableAutoPanning || editor.DisablePanning));

            IsSelectingProperty.Changed.AddClassHandler<NodifyEditor>((editor, e) =>
            {
                if (e.NewValue.GetValueOrDefault())
                    editor.OnItemsSelectStarted();
                else
                    editor.OnItemsSelectCompleted();
            });

            CanSelectMultipleItemsProperty.Changed.AddClassHandler<NodifyEditor>((editor, e) =>
                editor.CanSelectMultipleItemsBase = e.NewValue.GetValueOrDefault());

            SelectedItemsProperty.Changed.AddClassHandler<NodifyEditor>((editor, e) =>
                editor.OnSelectedItemsSourceChanged(e.OldValue.Value, e.NewValue.Value));

            // Property change handlers from main file
            ItemsExtentProperty.Changed.AddClassHandler<NodifyEditor>(OnItemsExtentChanged);
            ViewportLocationProperty.Changed.AddClassHandler<NodifyEditor>(OnViewportLocationChanged);
            ViewportZoomProperty.Changed.AddClassHandler<NodifyEditor>(OnViewportZoomChanged);
            MinViewportZoomProperty.Changed.AddClassHandler<NodifyEditor>(OnMinViewportZoomChanged);
            MaxViewportZoomProperty.Changed.AddClassHandler<NodifyEditor>(OnMaxViewportZoomChanged);
            GridCellSizeProperty.Changed.AddClassHandler<NodifyEditor>((x, e) => x.OnGridCellSizeChanged(x, e));
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

            _viewportTransform.Children.Add(ScaleTransform);
            _viewportTransform.Children.Add(TranslateTransform);

            InputProcessor.AddSharedHandlers(this);

            Loaded += OnEditorLoaded;
            Unloaded += OnEditorUnloaded;

            _focusNavigator = new StatefulFocusNavigator<ItemContainer>(OnElementFocused);

            // Subscribe to SelectionChanged event instead of overriding OnSelectionChanged
            SelectionChanged += OnSelectionChangedHandler;
        }

        /// <inheritdoc />
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            ItemsHost = e.NameScope.Find<Panel>(ElementItemsHost) ?? throw new InvalidOperationException($"{ElementItemsHost} is missing or is not of type Panel.");
            ConnectionsHost = e.NameScope.Find<Control>(ElementConnectionsHost) ?? throw new InvalidOperationException($"{ElementConnectionsHost} is missing or is not of type Control.");

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
        protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
            => new ItemContainer(this)
            {
                RenderTransform = new TranslateTransform()
            };

        /// <inheritdoc />
        protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
        {
            recycleKey = null;
            return item is not ItemContainer;
        }

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

            // Release the pointer capture if all buttons are released and there's no interaction in progress
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
        protected override void OnKeyUp(Avalonia.Input.KeyEventArgs e)
        {
            InputProcessor.ProcessEvent(e);

            // TODO: Need to check pointer state - Avalonia doesn't have Mouse.LeftButton static properties
            // Will need to track pointer state differently or remove this check
        }

        /// <inheritdoc />
        protected override void OnKeyDown(Avalonia.Input.KeyEventArgs e)
            => InputProcessor.ProcessEvent(e);

        #endregion

        /// <inheritdoc />
        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);

            double zoom = ViewportZoom;
            ViewportSize = new Size(e.NewSize.Width / zoom, e.NewSize.Height / zoom);

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
