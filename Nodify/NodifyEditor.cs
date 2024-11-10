using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

#if Avalonia
using Avalonia;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Animation;
using Nodify.Avalonia.Extensions;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Styling;
using Avalonia.Threading;
using Nodify.Avalonia.EditorStates;
using Tmds.DBus.Protocol;
using Nodify.Avalonia.Helpers;
using UIElement = Avalonia.Controls.Control;
using UIElementCollection = Avalonia.Controls.Controls;
using DependencyObject = Avalonia.AvaloniaObject;
using DependencyPropertyChangedEventArgs = Avalonia.AvaloniaPropertyChangedEventArgs<double>;
using MouseButtonEventArgs = Avalonia.Input.PointerPressedEventArgs;
using MouseEventArgs = Avalonia.Input.PointerEventArgs;
using MouseWheelEventArgs = Avalonia.Input.PointerWheelEventArgs;
using MultiSelector = Avalonia.Controls.Primitives.SelectingItemsControl;
using DragStartedEventHandler = System.EventHandler<Avalonia.Input.VectorEventArgs>;
using DragDeltaEventHandler = System.EventHandler<Avalonia.Input.VectorEventArgs>;
using DragCompletedEventHandler = System.EventHandler<Nodify.Avalonia.EditorStates.DragCompletedEventArgs>;
using DragDeltaEventArgs = Avalonia.Input.VectorEventArgs;
using DragStartedEventArgs = Avalonia.Input.VectorEventArgs;
using RoutedEventHandler = System.EventHandler<Avalonia.Interactivity.RoutedEventArgs>;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
#endif

namespace Nodify
{
    /// <summary>
    /// Groups <see cref="ItemContainer"/>s and <see cref="Connection"/>s in an area that you can drag, zoom and select.
    /// </summary>
#if Avalonia
    [TemplatePart(Name = "PART_ItemsHostPresenter", Type = typeof(ItemsPresenter))]
#else
    [TemplatePart(Name = ElementItemsHost, Type = typeof(Panel))]
    [TemplatePart(Name = ElementConnectionsHost, Type = typeof(FrameworkElement))]
    [StyleTypedProperty(Property = nameof(ItemContainerStyle), StyleTargetType = typeof(ItemContainer))]
    [StyleTypedProperty(Property = nameof(DecoratorContainerStyle), StyleTargetType = typeof(DecoratorContainer))]
    [StyleTypedProperty(Property = nameof(SelectionRectangleStyle), StyleTargetType = typeof(Rectangle))]
    [StyleTypedProperty(Property = nameof(CuttingLineStyle), StyleTargetType = typeof(CuttingLine))]
    [ContentProperty(nameof(Decorators))]
#endif
    [DefaultProperty(nameof(Decorators))]
#if Avalonia
    public class NodifyEditor : SelectingItemsControl
#else
    public class NodifyEditor : MultiSelector
#endif
    {

#if Avalonia
        public PointerEventArgs? Mouse => State.CurrentPointerArgs;
        public bool IsMouseCaptureWithin => State.CurrentPointerArgs != null && this.IsPointerCapturedWithin(State.CurrentPointerArgs);

        public double ActualWidth => Bounds.Width;
        public double ActualHeight => Bounds.Height;

        public void UnselectAll() => Selection.Clear();
#else
        protected const string ElementItemsHost = "PART_ItemsHost";
        protected const string ElementConnectionsHost = "PART_ConnectionsHost";
#endif

        #region Viewport

#if Avalonia
        public static readonly StyledProperty<double> ViewportZoomProperty = AvaloniaProperty.Register<NodifyEditor, double>(nameof(ViewportZoom), 1d, defaultBindingMode: BindingMode.TwoWay, coerce: ConstrainViewportZoomToRange);
        public static readonly StyledProperty<double> MinViewportZoomProperty = AvaloniaProperty.Register<NodifyEditor, double>(nameof(MinViewportZoom), 0.1d, coerce: CoerceMinViewportZoom);
        public static readonly StyledProperty<double> MaxViewportZoomProperty = AvaloniaProperty.Register<NodifyEditor, double>(nameof(MaxViewportZoom), 2d, coerce: CoerceMaxViewportZoom);
        public static readonly StyledProperty<Point> ViewportLocationProperty = AvaloniaProperty.Register<NodifyEditor, Point>(nameof(ViewportLocation), defaultBindingMode: BindingMode.TwoWay);
        public static readonly StyledProperty<Size> ViewportSizeProperty = AvaloniaProperty.Register<NodifyEditor, Size>(nameof(ViewportSize));
        public static readonly StyledProperty<Rect> ItemsExtentProperty = AvaloniaProperty.Register<NodifyEditor, Rect>(nameof(ItemsExtent));
        public static readonly StyledProperty<Rect> DecoratorsExtentProperty = AvaloniaProperty.Register<NodifyEditor, Rect>(nameof(DecoratorsExtent));

        public static readonly DirectProperty<NodifyEditor, TransformGroup> ViewportTransformProperty = AvaloniaProperty.RegisterDirect<NodifyEditor, TransformGroup>(nameof(ViewportTransform), o => o.ViewportTransform);
#else
        public static readonly DependencyProperty ViewportZoomProperty = DependencyProperty.Register(nameof(ViewportZoom), typeof(double), typeof(NodifyEditor), new FrameworkPropertyMetadata(BoxValue.Double1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnViewportZoomChanged, ConstrainViewportZoomToRange));
        public static readonly DependencyProperty MinViewportZoomProperty = DependencyProperty.Register(nameof(MinViewportZoom), typeof(double), typeof(NodifyEditor), new FrameworkPropertyMetadata(0.1d, OnMinViewportZoomChanged, CoerceMinViewportZoom));
        public static readonly DependencyProperty MaxViewportZoomProperty = DependencyProperty.Register(nameof(MaxViewportZoom), typeof(double), typeof(NodifyEditor), new FrameworkPropertyMetadata(BoxValue.Double2, OnMaxViewportZoomChanged, CoerceMaxViewportZoom));
        public static readonly DependencyProperty ViewportLocationProperty = DependencyProperty.Register(nameof(ViewportLocation), typeof(Point), typeof(NodifyEditor), new FrameworkPropertyMetadata(BoxValue.Point, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnViewportLocationChanged));
        public static readonly DependencyProperty ViewportSizeProperty = DependencyProperty.Register(nameof(ViewportSize), typeof(Size), typeof(NodifyEditor), new FrameworkPropertyMetadata(BoxValue.Size));
        public static readonly DependencyProperty ItemsExtentProperty = DependencyProperty.Register(nameof(ItemsExtent), typeof(Rect), typeof(NodifyEditor), new FrameworkPropertyMetadata(BoxValue.Rect));
        public static readonly DependencyProperty DecoratorsExtentProperty = DependencyProperty.Register(nameof(DecoratorsExtent), typeof(Rect), typeof(NodifyEditor), new FrameworkPropertyMetadata(BoxValue.Rect));

        protected internal static readonly DependencyPropertyKey ViewportTransformPropertyKey = DependencyProperty.RegisterReadOnly(nameof(ViewportTransform), typeof(Transform), typeof(NodifyEditor), new FrameworkPropertyMetadata(new TransformGroup()));
        public static readonly DependencyProperty ViewportTransformProperty = ViewportTransformPropertyKey.DependencyProperty;
#endif

        #region Callbacks

#if Avalonia
        private static void OnViewportLocationChanged(NodifyEditor d, AvaloniaPropertyChangedEventArgs<Point> e)
#else
        private static void OnViewportLocationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
#endif
        {
            var editor = (NodifyEditor)d;
#if Avalonia
            var translate = (Point)e.NewValue.Value;
#else
            var translate = (Point)e.NewValue;
#endif

            editor.TranslateTransform.X = -translate.X * editor.ViewportZoom;
            editor.TranslateTransform.Y = -translate.Y * editor.ViewportZoom;

            editor.OnViewportUpdated();
        }

        private static void OnViewportZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (NodifyEditor)d;
#if Avalonia
            double zoom = (double)e.NewValue.Value;
#else
            double zoom = (double)e.NewValue;
#endif

            editor.ScaleTransform.ScaleX = zoom;
            editor.ScaleTransform.ScaleY = zoom;

#if Avalonia
            editor.ViewportSize = new Size(editor.Bounds.Width / zoom, editor.Bounds.Height / zoom);
#else
            editor.ViewportSize = new Size(editor.ActualWidth / zoom, editor.ActualHeight / zoom);
#endif

            editor.ApplyRenderingOptimizations();
            editor.OnViewportUpdated();
        }

        private static void OnMinViewportZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var zoom = (NodifyEditor)d;
            zoom.CoerceValue(MaxViewportZoomProperty);
            zoom.CoerceValue(ViewportZoomProperty);
        }

#if Avalonia
        private static double CoerceMinViewportZoom(AvaloniaObject d, double value)
#else
        private static object CoerceMinViewportZoom(DependencyObject d, object value)
#endif
            => (double)value > 0.1d ? value : 0.1d;

        private static void OnMaxViewportZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var zoom = (NodifyEditor)d;
            zoom.CoerceValue(ViewportZoomProperty);
        }

#if Avalonia
        private static double CoerceMaxViewportZoom(AvaloniaObject d, double value)
#else
        private static object CoerceMaxViewportZoom(DependencyObject d, object value)
#endif
        {
            var editor = (NodifyEditor)d;
            double min = editor.MinViewportZoom;

            return (double)value < min ? min : value;
        }

#if Avalonia
        private static double ConstrainViewportZoomToRange(AvaloniaObject d, double value)
#else
        private static object ConstrainViewportZoomToRange(DependencyObject d, object value)
#endif
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

#if Avalonia
        public static readonly RoutedEvent ViewportUpdatedEvent = RoutedEvent.Register<NodifyEditor, RoutedEventArgs>(nameof(ViewportUpdated), RoutingStrategies.Bubble);
#else
        public static readonly RoutedEvent ViewportUpdatedEvent = EventManager.RegisterRoutedEvent(nameof(ViewportUpdated), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NodifyEditor));
#endif

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
        /// Called when the <see cref="UIElement.RenderSize"/> or <see cref="ViewportZoom"/> is changed.
        /// </summary>
        protected void OnViewportUpdated() => RaiseEvent(new RoutedEventArgs(ViewportUpdatedEvent, this));

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

#if Avalonia
        private TransformGroup _viewportTransform = new TransformGroup();

        public TransformGroup ViewportTransform => _viewportTransform;
#else
        /// <summary>
        /// Gets the transform that is applied to all child controls.
        /// </summary>
        public Transform ViewportTransform => (Transform)GetValue(ViewportTransformProperty);
#endif

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
#if Avalonia
            // TODO: cache
#else
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
#endif
        }

#endregion

        #region Cosmetic Dependency Properties

#if Avalonia
        public static readonly StyledProperty<double> BringIntoViewSpeedProperty = AvaloniaProperty.Register<NodifyEditor, double>(nameof(BringIntoViewSpeed), 1000d);
        public static readonly StyledProperty<double> BringIntoViewMaxDurationProperty = AvaloniaProperty.Register<NodifyEditor, double>(nameof(BringIntoViewMaxDuration), 1d);
        public static readonly StyledProperty<bool> DisplayConnectionsOnTopProperty = AvaloniaProperty.Register<NodifyEditor, bool>(nameof(DisplayConnectionsOnTop), false);
        public static readonly StyledProperty<bool> DisableAutoPanningProperty = AvaloniaProperty.Register<NodifyEditor, bool>(nameof(DisableAutoPanning), false);
        public static readonly StyledProperty<double> AutoPanSpeedProperty = AvaloniaProperty.Register<NodifyEditor, double>(nameof(AutoPanSpeed), 15d);
        public static readonly StyledProperty<double> AutoPanEdgeDistanceProperty = AvaloniaProperty.Register<NodifyEditor, double>(nameof(AutoPanEdgeDistance), 15d);
        public static readonly StyledProperty<DataTemplate> ConnectionTemplateProperty = AvaloniaProperty.Register<NodifyEditor, DataTemplate>(nameof(ConnectionTemplate));
        public static readonly StyledProperty<DataTemplate> DecoratorTemplateProperty = AvaloniaProperty.Register<NodifyEditor, DataTemplate>(nameof(DecoratorTemplate));
        public static readonly StyledProperty<DataTemplate> PendingConnectionTemplateProperty = AvaloniaProperty.Register<NodifyEditor, DataTemplate>(nameof(PendingConnectionTemplate));
        public static readonly StyledProperty<Style> SelectionRectangleStyleProperty = AvaloniaProperty.Register<NodifyEditor, Style>(nameof(SelectionRectangleStyle));
        public static readonly StyledProperty<Style> CuttingLineStyleProperty = AvaloniaProperty.Register<NodifyEditor, Style>(nameof(CuttingLineStyle));
        public static readonly StyledProperty<Style> DecoratorContainerStyleProperty = AvaloniaProperty.Register<NodifyEditor, Style>(nameof(DecoratorContainerStyle));
#else
        public static readonly DependencyProperty BringIntoViewSpeedProperty = DependencyProperty.Register(nameof(BringIntoViewSpeed), typeof(double), typeof(NodifyEditor), new FrameworkPropertyMetadata(BoxValue.Double1000));
        public static readonly DependencyProperty BringIntoViewMaxDurationProperty = DependencyProperty.Register(nameof(BringIntoViewMaxDuration), typeof(double), typeof(NodifyEditor), new FrameworkPropertyMetadata(BoxValue.Double1));
        public static readonly DependencyProperty DisplayConnectionsOnTopProperty = DependencyProperty.Register(nameof(DisplayConnectionsOnTop), typeof(bool), typeof(NodifyEditor), new FrameworkPropertyMetadata(BoxValue.False));
        public static readonly DependencyProperty DisableAutoPanningProperty = DependencyProperty.Register(nameof(DisableAutoPanning), typeof(bool), typeof(NodifyEditor), new FrameworkPropertyMetadata(BoxValue.False, OnDisableAutoPanningChanged));
        public static readonly DependencyProperty AutoPanSpeedProperty = DependencyProperty.Register(nameof(AutoPanSpeed), typeof(double), typeof(NodifyEditor), new FrameworkPropertyMetadata(15d));
        public static readonly DependencyProperty AutoPanEdgeDistanceProperty = DependencyProperty.Register(nameof(AutoPanEdgeDistance), typeof(double), typeof(NodifyEditor), new FrameworkPropertyMetadata(15d));
        public static readonly DependencyProperty ConnectionTemplateProperty = DependencyProperty.Register(nameof(ConnectionTemplate), typeof(DataTemplate), typeof(NodifyEditor));
        public static readonly DependencyProperty DecoratorTemplateProperty = DependencyProperty.Register(nameof(DecoratorTemplate), typeof(DataTemplate), typeof(NodifyEditor));
        public static readonly DependencyProperty PendingConnectionTemplateProperty = DependencyProperty.Register(nameof(PendingConnectionTemplate), typeof(DataTemplate), typeof(NodifyEditor));
        public static readonly DependencyProperty SelectionRectangleStyleProperty = DependencyProperty.Register(nameof(SelectionRectangleStyle), typeof(Style), typeof(NodifyEditor));
        public static readonly DependencyProperty CuttingLineStyleProperty = DependencyProperty.Register(nameof(CuttingLineStyle), typeof(Style), typeof(NodifyEditor));
        public static readonly DependencyProperty DecoratorContainerStyleProperty = DependencyProperty.Register(nameof(DecoratorContainerStyle), typeof(Style), typeof(NodifyEditor));
#endif

#if Avalonia
        private static void OnDisableAutoPanningChanged(NodifyEditor editor, AvaloniaPropertyChangedEventArgs<bool> args)
            => editor.OnDisableAutoPanningChanged(args.NewValue.Value);
#else
        private static void OnDisableAutoPanningChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((NodifyEditor)d).OnDisableAutoPanningChanged((bool)e.NewValue);
#endif

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
        /// Gets or sets whether to disable the auto panning when selecting or dragging near the edge of the editor configured by <see cref="AutoPanEdgeDistance"/>.
        /// </summary>
        public bool DisableAutoPanning
        {
            get => (bool)GetValue(DisableAutoPanningProperty);
            set => SetValue(DisableAutoPanningProperty, value);
        }

        /// <summary>
        /// Gets or sets the speed used when auto-panning scaled by <see cref="AutoPanningTickRate"/>
        /// </summary>
        public double AutoPanSpeed
        {
            get => (double)GetValue(AutoPanSpeedProperty);
            set => SetValue(AutoPanSpeedProperty, value);
        }

        /// <summary>
        /// Gets or sets the maximum distance in pixels from the edge of the editor that will trigger auto-panning.
        /// </summary>
        public double AutoPanEdgeDistance
        {
            get => (double)GetValue(AutoPanEdgeDistanceProperty);
            set => SetValue(AutoPanEdgeDistanceProperty, value);
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
        /// Gets or sets the <see cref="DataTemplate"/> to use when generating a new <see cref="DecoratorContainer"/>.
        /// </summary>
        public DataTemplate DecoratorTemplate
        {
            get => (DataTemplate)GetValue(DecoratorTemplateProperty);
            set => SetValue(DecoratorTemplateProperty, value);
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
        /// Gets or sets the style to use for the selection rectangle.
        /// </summary>
        public Style SelectionRectangleStyle
        {
            get => (Style)GetValue(SelectionRectangleStyleProperty);
            set => SetValue(SelectionRectangleStyleProperty, value);
        }

        /// <summary>
        /// Gets or sets the style to use for the cutting line.
        /// </summary>
        public Style CuttingLineStyle
        {
            get => (Style)GetValue(CuttingLineStyleProperty);
            set => SetValue(CuttingLineStyleProperty, value);
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

#if Avalonia
        public static readonly DirectProperty<NodifyEditor, Rect> SelectedAreaProperty = AvaloniaProperty.RegisterDirect<NodifyEditor, Rect>(nameof(SelectedArea), o => o.SelectedArea, null, default(Rect));
        public static readonly DirectProperty<NodifyEditor, bool> IsSelectingProperty = AvaloniaProperty.RegisterDirect<NodifyEditor, bool>(nameof(IsSelecting), o => o.IsSelecting, null, false);
        public static readonly DirectProperty<NodifyEditor, Point> CuttingLineStartProperty = AvaloniaProperty.RegisterDirect<NodifyEditor, Point>(nameof(CuttingLineStart), o => o.CuttingLineStart, null, default(Point));
        public static readonly DirectProperty<NodifyEditor, Point> CuttingLineEndProperty = AvaloniaProperty.RegisterDirect<NodifyEditor, Point>(nameof(CuttingLineEnd), o => o.CuttingLineEnd, null, default(Point));
        public static readonly DirectProperty<NodifyEditor, bool> IsCuttingProperty = AvaloniaProperty.RegisterDirect<NodifyEditor, bool>(nameof(IsCutting), o => o.IsCutting, null, false);
        public static readonly DirectProperty<NodifyEditor, bool> IsPanningProperty = AvaloniaProperty.RegisterDirect<NodifyEditor, bool>(nameof(IsPanning), o => o.IsPanning, null, false);
        public static readonly DirectProperty<NodifyEditor, Point> MouseLocationProperty = AvaloniaProperty.RegisterDirect<NodifyEditor, Point>(nameof(MouseLocation), o => o.MouseLocation, null, default(Point));
#else
        protected static readonly DependencyPropertyKey SelectedAreaPropertyKey = DependencyProperty.RegisterReadOnly(nameof(SelectedArea), typeof(Rect), typeof(NodifyEditor), new FrameworkPropertyMetadata(BoxValue.Rect));
        public static readonly DependencyProperty SelectedAreaProperty = SelectedAreaPropertyKey.DependencyProperty;

        protected static readonly DependencyPropertyKey IsSelectingPropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsSelecting), typeof(bool), typeof(NodifyEditor), new FrameworkPropertyMetadata(BoxValue.False, OnIsSelectingChanged));
        public static readonly DependencyProperty IsSelectingProperty = IsSelectingPropertyKey.DependencyProperty;

        protected static readonly DependencyPropertyKey CuttingLineStartPropertyKey = DependencyProperty.RegisterReadOnly(nameof(CuttingLineStart), typeof(Point), typeof(NodifyEditor), new FrameworkPropertyMetadata(BoxValue.Point));
        public static readonly DependencyProperty CuttingLineStartProperty = CuttingLineStartPropertyKey.DependencyProperty;

        protected static readonly DependencyPropertyKey CuttingLineEndPropertyKey = DependencyProperty.RegisterReadOnly(nameof(CuttingLineEnd), typeof(Point), typeof(NodifyEditor), new FrameworkPropertyMetadata(BoxValue.Point));
        public static readonly DependencyProperty CuttingLineEndProperty = CuttingLineEndPropertyKey.DependencyProperty;

        protected static readonly DependencyPropertyKey IsCuttingPropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsCutting), typeof(bool), typeof(NodifyEditor), new FrameworkPropertyMetadata(BoxValue.False, OnIsCuttingChanged));
        public static readonly DependencyProperty IsCuttingProperty = IsCuttingPropertyKey.DependencyProperty;

        public static readonly DependencyPropertyKey IsPanningPropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsPanning), typeof(bool), typeof(NodifyEditor), new FrameworkPropertyMetadata(BoxValue.False));
        public static readonly DependencyProperty IsPanningProperty = IsPanningPropertyKey.DependencyProperty;

        protected static readonly DependencyPropertyKey MouseLocationPropertyKey = DependencyProperty.RegisterReadOnly(nameof(MouseLocation), typeof(Point), typeof(NodifyEditor), new FrameworkPropertyMetadata(BoxValue.Point));
        public static readonly DependencyProperty MouseLocationProperty = MouseLocationPropertyKey.DependencyProperty;
#endif

#if Avalonia
        private void OnIsSelectingChanged(bool value)
        {
            if (value)
                OnItemsSelectStarted();
            else
                OnItemsSelectCompleted();
        }
#else
        private static void OnIsSelectingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (NodifyEditor)d;
            if ((bool)e.NewValue == true)
                editor.OnItemsSelectStarted();
            else
                editor.OnItemsSelectCompleted();
        }
#endif

        private void OnItemsSelectCompleted()
        {
            if (ItemsSelectCompletedCommand?.CanExecute(DataContext) ?? false)
                ItemsSelectCompletedCommand.Execute(DataContext);
        }

        private void OnItemsSelectStarted()
        {
            if (ItemsSelectStartedCommand?.CanExecute(DataContext) ?? false)
                ItemsSelectStartedCommand.Execute(DataContext);
        }

#if Avalonia
        private void OnIsCuttingChanged(bool value)
        {
            if (value)
                OnCuttingStarted();
            else
                OnCuttingCompleted();
        }
#else
        private static void OnIsCuttingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (NodifyEditor)d;
            if ((bool)e.NewValue == true)
                editor.OnCuttingStarted();
            else
                editor.OnCuttingCompleted();
        }
#endif

        private void OnCuttingCompleted()
        {
            if (CuttingCompletedCommand?.CanExecute(DataContext) ?? false)
                CuttingCompletedCommand.Execute(DataContext);
        }

        private void OnCuttingStarted()
        {
            if (CuttingStartedCommand?.CanExecute(DataContext) ?? false)
                CuttingStartedCommand.Execute(DataContext);
        }

#if Avalonia
        private Rect _selectedArea;
        private bool _isSelecting;
        private Point _cuttingLineStart;
        private Point _cuttingLineEnd;
        private bool _isCutting;
        private bool _isPanning;
        private Point _mouseLocation;

        /// <summary>
        /// Gets the currently selected area while <see cref="IsSelecting"/> is true.
        /// </summary>
        public Rect SelectedArea
        {
            get => _selectedArea;
            internal set => SetAndRaise(SelectedAreaProperty, ref _selectedArea, value);
        }

        /// <summary>
        /// Gets a value that indicates whether a selection operation is in progress.
        /// </summary>
        public bool IsSelecting
        {
            get => _isSelecting;
            internal set
            {
                if (SetAndRaise(IsSelectingProperty, ref _isSelecting, value))
                {
                    OnIsSelectingChanged(value);
                }
            }
        }

        public Point CuttingLineStart
        {
            get => _cuttingLineStart;
            protected set => SetAndRaise(CuttingLineStartProperty, ref _cuttingLineStart, value);
        }

        public Point CuttingLineEnd
        {
            get => _cuttingLineEnd;
            protected internal set => SetAndRaise(CuttingLineEndProperty, ref _cuttingLineEnd, value);
        }

        public bool IsCutting
        {
            get => _isCutting;
            protected internal set => SetAndRaise(IsCuttingProperty, ref _isCutting, value);
        }

        /// <summary>
        /// Gets a value that indicates whether a panning operation is in progress.
        /// </summary>
        public bool IsPanning
        {
            get => _isPanning;
            protected internal set => SetAndRaise(IsPanningProperty, ref _isPanning, value);
        }

        /// <summary>
        /// Gets the current mouse location in graph space coordinates (relative to the <see cref="ItemsHost" />).
        /// </summary>
        public Point MouseLocation
        {
            get => _mouseLocation;
            protected set => SetAndRaise(MouseLocationProperty, ref _mouseLocation, value);
        }
#else
        /// <summary>
        /// Gets the currently selected area while <see cref="IsSelecting"/> is true.
        /// </summary>
        public Rect SelectedArea
        {
            get => (Rect)GetValue(SelectedAreaProperty);
            internal set => SetValue(SelectedAreaPropertyKey, value);
        }

        /// <summary>
        /// Gets a value that indicates whether a selection operation is in progress.
        /// </summary>
        public bool IsSelecting
        {
            get => (bool)GetValue(IsSelectingProperty);
            internal set => SetValue(IsSelectingPropertyKey, value);
        }

        /// <summary>
        /// Gets the start point of the <see cref="CuttingLine"/> while <see cref="IsCutting"/> is true.
        /// </summary>
        public Point CuttingLineStart
        {
            get => (Point)GetValue(CuttingLineStartProperty);
            private set => SetValue(CuttingLineStartPropertyKey, value);
        }

        /// <summary>
        /// Gets the end point of the <see cref="CuttingLine"/> while <see cref="IsCutting"/> is true.
        /// </summary>
        public Point CuttingLineEnd
        {
            get => (Point)GetValue(CuttingLineEndProperty);
            protected internal set => SetValue(CuttingLineEndPropertyKey, value);
        }

        /// <summary>
        /// Gets a value that indicates whether a cutting operation is in progress.
        /// </summary>
        public bool IsCutting
        {
            get => (bool)GetValue(IsCuttingProperty);
            private set => SetValue(IsCuttingPropertyKey, value);
        }

        /// <summary>
        /// Gets a value that indicates whether a panning operation is in progress.
        /// </summary>
        public bool IsPanning
        {
            get => (bool)GetValue(IsPanningProperty);
            protected internal set => SetValue(IsPanningPropertyKey, value);
        }

        /// <summary>
        /// Gets the current mouse location in graph space coordinates (relative to the <see cref="ItemsHost" />).
        /// </summary>
        public Point MouseLocation
        {
            get => (Point)GetValue(MouseLocationProperty);
            protected set => SetValue(MouseLocationPropertyKey, value);
        }
#endif

        #endregion

        #region Dependency Properties

#if Avalonia
        public static readonly StyledProperty<IEnumerable> ConnectionsProperty = AvaloniaProperty.Register<NodifyEditor, IEnumerable>(nameof(Connections));
        public new static readonly DirectProperty<SelectingItemsControl, IList?> SelectedItemsProperty = SelectingItemsControl.SelectedItemsProperty;
        public static readonly StyledProperty<IList> SelectedConnectionsProperty = AvaloniaProperty.Register<NodifyEditor, IList>(nameof(SelectedConnections));
        public static readonly StyledProperty<object> SelectedConnectionProperty = AvaloniaProperty.Register<NodifyEditor, object>(nameof(SelectedConnection));
        public static readonly StyledProperty<object> PendingConnectionProperty = AvaloniaProperty.Register<NodifyEditor, object>(nameof(PendingConnection));
        public static readonly StyledProperty<uint> GridCellSizeProperty = AvaloniaProperty.Register<NodifyEditor, uint>(nameof(GridCellSize), 1u, coerce: OnCoerceGridCellSize);
        public static readonly StyledProperty<bool> DisableZoomingProperty = AvaloniaProperty.Register<NodifyEditor, bool>(nameof(DisableZooming), false);
        public static readonly StyledProperty<bool> DisablePanningProperty = AvaloniaProperty.Register<NodifyEditor, bool>(nameof(DisablePanning), false);
        public static readonly StyledProperty<bool> EnableRealtimeSelectionProperty = AvaloniaProperty.Register<NodifyEditor, bool>(nameof(EnableRealtimeSelection), false);
        public static readonly StyledProperty<IEnumerable> DecoratorsProperty = AvaloniaProperty.Register<NodifyEditor, IEnumerable>(nameof(Decorators));
        public static readonly StyledProperty<bool> CanSelectMultipleConnectionsProperty = AvaloniaProperty.Register<NodifyEditor, bool>(nameof(CanSelectMultipleConnections), BoxValue.True);
        public static readonly StyledProperty<bool> CanSelectMultipleItemsProperty = AvaloniaProperty.Register<NodifyEditor, bool>(nameof(CanSelectMultipleItems), BoxValue.True);
#else
        public static readonly DependencyProperty ConnectionsProperty = DependencyProperty.Register(nameof(Connections), typeof(IEnumerable), typeof(NodifyEditor));
        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register(nameof(SelectedItems), typeof(IList), typeof(NodifyEditor), new FrameworkPropertyMetadata(default(IList), OnSelectedItemsSourceChanged));
        public static readonly DependencyProperty SelectedConnectionsProperty = DependencyProperty.Register(nameof(SelectedConnections), typeof(IList), typeof(NodifyEditor), new FrameworkPropertyMetadata(default(IList)));
        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(nameof(SelectedConnection), typeof(object), typeof(NodifyEditor), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty PendingConnectionProperty = DependencyProperty.Register(nameof(PendingConnection), typeof(object), typeof(NodifyEditor));
        public static readonly DependencyProperty GridCellSizeProperty = DependencyProperty.Register(nameof(GridCellSize), typeof(uint), typeof(NodifyEditor), new FrameworkPropertyMetadata(BoxValue.UInt1, OnGridCellSizeChanged, OnCoerceGridCellSize));
        public static readonly DependencyProperty DisableZoomingProperty = DependencyProperty.Register(nameof(DisableZooming), typeof(bool), typeof(NodifyEditor), new FrameworkPropertyMetadata(BoxValue.False));
        public static readonly DependencyProperty DisablePanningProperty = DependencyProperty.Register(nameof(DisablePanning), typeof(bool), typeof(NodifyEditor), new FrameworkPropertyMetadata(BoxValue.False, OnDisablePanningChanged));
        public static readonly DependencyProperty EnableRealtimeSelectionProperty = DependencyProperty.Register(nameof(EnableRealtimeSelection), typeof(bool), typeof(NodifyEditor), new FrameworkPropertyMetadata(BoxValue.False));
        public static readonly DependencyProperty DecoratorsProperty = DependencyProperty.Register(nameof(Decorators), typeof(IEnumerable), typeof(NodifyEditor));
        public static readonly DependencyProperty CanSelectMultipleConnectionsProperty = DependencyProperty.Register(nameof(CanSelectMultipleConnections), typeof(bool), typeof(NodifyEditor), new FrameworkPropertyMetadata(BoxValue.True));
        public static readonly DependencyProperty CanSelectMultipleItemsProperty = DependencyProperty.Register(nameof(CanSelectMultipleItems), typeof(bool), typeof(NodifyEditor), new FrameworkPropertyMetadata(BoxValue.True, OnCanSelectMultipleItemsChanged, CoerceCanSelectMultipleItems));
#endif

#if !Avalonia
        private static void OnCanSelectMultipleItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((NodifyEditor)d).CanSelectMultipleItemsBase = (bool)e.NewValue;

        private static object CoerceCanSelectMultipleItems(DependencyObject d, object baseValue)
            => ((NodifyEditor)d).CanSelectMultipleItemsBase = (bool)baseValue;
#endif

#if Avalonia
        private static void OnSelectedItemsSourceChanged(NodifyEditor d, AvaloniaPropertyChangedEventArgs<IList?> e)
            => d.OnSelectedItemsSourceChanged(e.OldValue.Value, e.NewValue.Value);
#else
        private static void OnSelectedItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((NodifyEditor)d).OnSelectedItemsSourceChanged((IList)e.OldValue, (IList)e.NewValue);
#endif

#if Avalonia
        private static uint OnCoerceGridCellSize(AvaloniaObject avaloniaObject, uint value)
#else
        private static object OnCoerceGridCellSize(DependencyObject d, object value)
#endif
            => (uint)value > 0u ? value : BoxValue.UInt1;

#if Avalonia
        private static void OnGridCellSizeChanged(DependencyObject d, AvaloniaPropertyChangedEventArgs<uint> e) { }
#else
        private static void OnGridCellSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) { }
#endif

#if Avalonia
        private static void OnDisablePanningChanged(DependencyObject d, AvaloniaPropertyChangedEventArgs<bool> e)
#else
        private static void OnDisablePanningChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
#endif
        {
            var editor = (NodifyEditor)d;
            editor.OnDisableAutoPanningChanged(editor.DisableAutoPanning || editor.DisablePanning);
        }

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
        /// Gets of sets the <see cref="FrameworkElement.DataContext"/> of the <see cref="Nodify.PendingConnection"/>.
        /// </summary>
        public object PendingConnection
        {
            get => GetValue(PendingConnectionProperty);
            set => SetValue(PendingConnectionProperty, value);
        }

        /// <summary>
        /// Gets or sets the selected connection.
        /// </summary>
        public object? SelectedConnection
        {
            get => GetValue(SelectedConnectionProperty);
            set => SetValue(SelectedConnectionProperty, value);
        }

        /// <summary>
        /// Gets or sets the selected connections in the <see cref="NodifyEditor"/>.
        /// </summary>
        public IList? SelectedConnections
        {
            get => (IList?)GetValue(SelectedConnectionsProperty);
            set => SetValue(SelectedConnectionsProperty, value);
        }

        /// <summary>
        /// Gets or sets the selected items in the <see cref="NodifyEditor"/>.
        /// </summary>
        public new IList? SelectedItems
        {
#if Avalonia
            get => base.SelectedItems;
            set => base.SelectedItems = value;
#else
            get => (IList?)GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
#endif
        }

#if Avalonia
        public bool HasItems => this.Items.Count != 0;

        public void SelectAll()
        {
            IsSelecting = true;
            Selection.BeginBatchUpdate();
            Selection.SelectAll();
            Selection.EndBatchUpdate();
            IsSelecting = false;
        }
#endif

        /// <summary>
        /// Gets or sets whether zooming should be disabled.
        /// </summary>
        public bool DisableZooming
        {
            get => (bool)GetValue(DisableZoomingProperty);
            set => SetValue(DisableZoomingProperty, value);
        }

        /// <summary>
        /// Gets or sets whether panning should be disabled.
        /// </summary>
        public bool DisablePanning
        {
            get => (bool)GetValue(DisablePanningProperty);
            set => SetValue(DisablePanningProperty, value);
        }

        /// <summary>
        /// Enables selecting and deselecting items while the <see cref="SelectedArea"/> changes.
        /// Disable for maximum performance when hundreds of items are generated.
        /// </summary>
        public bool EnableRealtimeSelection
        {
            get => (bool)GetValue(EnableRealtimeSelectionProperty);
            set => SetValue(EnableRealtimeSelectionProperty, value);
        }

        /// <summary>
        /// Gets or sets whether multiple connections can be selected.
        /// </summary>
        public bool CanSelectMultipleConnections
        {
            get => (bool)GetValue(CanSelectMultipleConnectionsProperty);
            set => SetValue(CanSelectMultipleConnectionsProperty, value);
        }

        /// <summary>
        /// Gets or sets whether multiple <see cref="ItemContainer" />s can be selected.
        /// </summary>
        public new bool CanSelectMultipleItems
        {
            get => (bool)GetValue(CanSelectMultipleItemsProperty);
            set => SetValue(CanSelectMultipleItemsProperty, value);
        }

#if !Avalonia
        private bool CanSelectMultipleItemsBase
        {
            get => base.CanSelectMultipleItems;
            set => base.CanSelectMultipleItems = value;
        }
#endif

        #endregion

        #region Command Dependency Properties

#if Avalonia
        private ICommand _connectionCompletedCommand;
        private ICommand _connectionStartedCommand;
        private ICommand _itemsSelectCompletedCommand;
        private ICommand _removeConnectionCommand;
        private ICommand _disconnectConnectorCommand;
        private ICommand _itemsDragStartedCommand;
        private ICommand _itemsDragCompletedCommand;
        private ICommand _itemsSelectStartedCommand;
        private ICommand _cuttingStartedCommand;
        private ICommand _cuttingCompletedCommand;

        public static readonly DirectProperty<NodifyEditor, ICommand?> ConnectionCompletedCommandProperty = AvaloniaProperty.RegisterDirect<NodifyEditor, ICommand?>(nameof(ConnectionCompletedCommand), o => o.ConnectionCompletedCommand, (o, v) => o.ConnectionCompletedCommand = v);
        public static readonly DirectProperty<NodifyEditor, ICommand?> ConnectionStartedCommandProperty = AvaloniaProperty.RegisterDirect<NodifyEditor, ICommand?>(nameof(ConnectionStartedCommand), o => o.ConnectionStartedCommand, (o, v) => o.ConnectionStartedCommand = v);
        public static readonly DirectProperty<NodifyEditor, ICommand?> DisconnectConnectorCommandProperty = AvaloniaProperty.RegisterDirect<NodifyEditor, ICommand?>(nameof(DisconnectConnectorCommand), o => o.DisconnectConnectorCommand, (o, v) => o.DisconnectConnectorCommand = v);
        public static readonly DirectProperty<NodifyEditor, ICommand?> RemoveConnectionCommandProperty = AvaloniaProperty.RegisterDirect<NodifyEditor, ICommand?>(nameof(RemoveConnectionCommand), o => o.RemoveConnectionCommand, (o, v) => o.RemoveConnectionCommand = v);
        public static readonly DirectProperty<NodifyEditor, ICommand?> ItemsDragStartedCommandProperty = AvaloniaProperty.RegisterDirect<NodifyEditor, ICommand?>(nameof(ItemsDragStartedCommand), o => o.ItemsDragStartedCommand, (o, v) => o.ItemsDragStartedCommand = v);
        public static readonly DirectProperty<NodifyEditor, ICommand?> ItemsDragCompletedCommandProperty = AvaloniaProperty.RegisterDirect<NodifyEditor, ICommand?>(nameof(ItemsDragCompletedCommand), o => o.ItemsDragCompletedCommand, (o, v) => o.ItemsDragCompletedCommand = v);
        public static readonly DirectProperty<NodifyEditor, ICommand?> ItemsSelectStartedCommandProperty = AvaloniaProperty.RegisterDirect<NodifyEditor, ICommand?>(nameof(ItemsSelectStartedCommand), o => o.ItemsSelectStartedCommand, (o, v) => o.ItemsSelectStartedCommand = v);
        public static readonly DirectProperty<NodifyEditor, ICommand?> ItemsSelectCompletedCommandProperty = AvaloniaProperty.RegisterDirect<NodifyEditor, ICommand?>(nameof(ItemsSelectCompletedCommand), o => o.ItemsSelectCompletedCommand, (o, v) => o.ItemsSelectCompletedCommand = v);
        public static readonly DirectProperty<NodifyEditor, ICommand?> CuttingStartedCommandProperty = AvaloniaProperty.RegisterDirect<NodifyEditor, ICommand?>(nameof(CuttingStartedCommand), o => o.CuttingStartedCommand, (o, v) => o.CuttingStartedCommand = v);
        public static readonly DirectProperty<NodifyEditor, ICommand?> CuttingCompletedCommandProperty = AvaloniaProperty.RegisterDirect<NodifyEditor, ICommand?>(nameof(CuttingCompletedCommand), o => o.CuttingCompletedCommand, (o, v) => o.CuttingCompletedCommand = v);

        /// <summary>
        /// Invoked when the <see cref="Connections.PendingConnection"/> is completed. <br />
        /// Use <see cref="PendingConnection.StartedCommand"/> if you want to control the visibility of the connection from the viewmodel. <br />
        /// Parameter is <see cref="PendingConnection.Source"/>.
        /// </summary>
        public ICommand? ConnectionStartedCommand
        {
            get => _connectionStartedCommand;
            set => SetAndRaise(ConnectionStartedCommandProperty, ref _connectionStartedCommand, value);
        }

        /// <summary>
        /// Invoked when the <see cref="Connections.PendingConnection"/> is completed. <br />
        /// Use <see cref="PendingConnection.CompletedCommand"/> if you want to control the visibility of the connection from the viewmodel. <br />
        /// Parameter is <see cref="Tuple{T, U}"/> where <see cref="Tuple{T, U}.Item1"/> is the <see cref="PendingConnection.Source"/> and <see cref="Tuple{T, U}.Item2"/> is <see cref="PendingConnection.Target"/>.
        /// </summary>
        public ICommand? ConnectionCompletedCommand
        {
            get => _connectionCompletedCommand;
            set => SetAndRaise(ConnectionCompletedCommandProperty, ref _connectionCompletedCommand, value);
        }

        /// <summary>
        /// Invoked when the <see cref="Connector.Disconnect"/> event is raised. <br />
        /// Can also be handled at the <see cref="Connector"/> level using the <see cref="Connector.DisconnectCommand"/> command. <br />
        /// Parameter is the <see cref="Connector"/>'s <see cref="FrameworkElement.DataContext"/>.
        /// </summary>
        public ICommand? DisconnectConnectorCommand
        {
            get => _disconnectConnectorCommand;
            set => SetAndRaise(DisconnectConnectorCommandProperty, ref _disconnectConnectorCommand, value);
        }

        /// <summary>
        /// Invoked when the <see cref="BaseConnection.Disconnect"/> event is raised. <br />
        /// Can also be handled at the <see cref="BaseConnection"/> level using the <see cref="BaseConnection.DisconnectCommand"/> command. <br />
        /// Parameter is the <see cref="BaseConnection"/>'s <see cref="FrameworkElement.DataContext"/>.
        /// </summary>
        public ICommand? RemoveConnectionCommand
        {
            get => _removeConnectionCommand;
            set => SetAndRaise(RemoveConnectionCommandProperty, ref _removeConnectionCommand, value);
        }

        /// <summary>
        /// Invoked when a drag operation starts for the <see cref="SelectedItems"/>.
        /// </summary>
        public ICommand? ItemsDragStartedCommand
        {
            get => _itemsDragStartedCommand;
            set => SetAndRaise(ItemsDragStartedCommandProperty, ref _itemsDragStartedCommand, value);
        }

        /// <summary>
        /// Invoked when a drag operation is completed for the <see cref="SelectedItems"/>.
        /// </summary>
        public ICommand? ItemsDragCompletedCommand
        {
            get => _itemsDragCompletedCommand;
            set => SetAndRaise(ItemsDragCompletedCommandProperty, ref _itemsDragCompletedCommand, value);
        }

        /// <summary>Invoked when a selection operation is started.</summary>
        public ICommand? ItemsSelectStartedCommand
        {
            get => _itemsSelectStartedCommand;
            set => SetAndRaise(ItemsSelectStartedCommandProperty, ref _itemsSelectStartedCommand, value);
        }

        /// <summary>Invoked when a selection operation is completed.</summary>
        public ICommand? ItemsSelectCompletedCommand
        {
            get => _itemsSelectCompletedCommand;
            set => SetAndRaise(ItemsSelectCompletedCommandProperty, ref _itemsSelectCompletedCommand, value);
        }

        /// <summary>Invoked when a cutting operation is started.</summary>
        public ICommand? CuttingStartedCommand
        {
            get => _cuttingStartedCommand;
            set => SetAndRaise(ItemsSelectCompletedCommandProperty, ref _cuttingStartedCommand, value);
        }

        /// <summary>Invoked when a cutting operation is completed.</summary>
        public ICommand? CuttingCompletedCommand
        {
            get => _cuttingCompletedCommand;
            set => SetAndRaise(ItemsSelectCompletedCommandProperty, ref _cuttingCompletedCommand, value);
        }
#else
        public static readonly DependencyProperty ConnectionCompletedCommandProperty = DependencyProperty.Register(nameof(ConnectionCompletedCommand), typeof(ICommand), typeof(NodifyEditor));
        public static readonly DependencyProperty ConnectionStartedCommandProperty = DependencyProperty.Register(nameof(ConnectionStartedCommand), typeof(ICommand), typeof(NodifyEditor));
        public static readonly DependencyProperty DisconnectConnectorCommandProperty = DependencyProperty.Register(nameof(DisconnectConnectorCommand), typeof(ICommand), typeof(NodifyEditor));
        public static readonly DependencyProperty RemoveConnectionCommandProperty = DependencyProperty.Register(nameof(RemoveConnectionCommand), typeof(ICommand), typeof(NodifyEditor));
        public static readonly DependencyProperty ItemsDragStartedCommandProperty = DependencyProperty.Register(nameof(ItemsDragStartedCommand), typeof(ICommand), typeof(NodifyEditor));
        public static readonly DependencyProperty ItemsDragCompletedCommandProperty = DependencyProperty.Register(nameof(ItemsDragCompletedCommand), typeof(ICommand), typeof(NodifyEditor));
        public static readonly DependencyProperty ItemsSelectStartedCommandProperty = DependencyProperty.Register(nameof(ItemsSelectStartedCommand), typeof(ICommand), typeof(NodifyEditor));
        public static readonly DependencyProperty ItemsSelectCompletedCommandProperty = DependencyProperty.Register(nameof(ItemsSelectCompletedCommand), typeof(ICommand), typeof(NodifyEditor));
        public static readonly DependencyProperty CuttingStartedCommandProperty = DependencyProperty.Register(nameof(CuttingStartedCommand), typeof(ICommand), typeof(NodifyEditor));
        public static readonly DependencyProperty CuttingCompletedCommandProperty = DependencyProperty.Register(nameof(CuttingCompletedCommand), typeof(ICommand), typeof(NodifyEditor));

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
        /// Parameter is the <see cref="Connector"/>'s <see cref="FrameworkElement.DataContext"/>.
        /// </summary>
        public ICommand? DisconnectConnectorCommand
        {
            get => (ICommand?)GetValue(DisconnectConnectorCommandProperty);
            set => SetValue(DisconnectConnectorCommandProperty, value);
        }

        /// <summary>
        /// Invoked when the <see cref="BaseConnection.Disconnect"/> event is raised. <br />
        /// Can also be handled at the <see cref="BaseConnection"/> level using the <see cref="BaseConnection.DisconnectCommand"/> command. <br />
        /// Parameter is the <see cref="BaseConnection"/>'s <see cref="FrameworkElement.DataContext"/>.
        /// </summary>
        public ICommand? RemoveConnectionCommand
        {
            get => (ICommand?)GetValue(RemoveConnectionCommandProperty);
            set => SetValue(RemoveConnectionCommandProperty, value);
        }

        /// <summary>
        /// Invoked when a drag operation starts for the <see cref="SelectedItems"/>.
        /// </summary>
        public ICommand? ItemsDragStartedCommand
        {
            get => (ICommand?)GetValue(ItemsDragStartedCommandProperty);
            set => SetValue(ItemsDragStartedCommandProperty, value);
        }

        /// <summary>
        /// Invoked when a drag operation is completed for the <see cref="SelectedItems"/>.
        /// </summary>
        public ICommand? ItemsDragCompletedCommand
        {
            get => (ICommand?)GetValue(ItemsDragCompletedCommandProperty);
            set => SetValue(ItemsDragCompletedCommandProperty, value);
        }

        /// <summary>Invoked when a selection operation is started.</summary>
        public ICommand? ItemsSelectStartedCommand
        {
            get => (ICommand?)GetValue(ItemsSelectStartedCommandProperty);
            set => SetValue(ItemsSelectStartedCommandProperty, value);
        }

        /// <summary>Invoked when a selection operation is completed.</summary>
        public ICommand? ItemsSelectCompletedCommand
        {
            get => (ICommand?)GetValue(ItemsSelectCompletedCommandProperty);
            set => SetValue(ItemsSelectCompletedCommandProperty, value);
        }

        /// <summary>Invoked when a cutting operation is started.</summary>
        public ICommand? CuttingStartedCommand
        {
            get => (ICommand?)GetValue(CuttingStartedCommandProperty);
            set => SetValue(CuttingStartedCommandProperty, value);
        }

        /// <summary>Invoked when a cutting operation is completed.</summary>
        public ICommand? CuttingCompletedCommand
        {
            get => (ICommand?)GetValue(CuttingCompletedCommandProperty);
            set => SetValue(CuttingCompletedCommandProperty, value);
        }
#endif

        #endregion

        #region Fields

        /// <summary>
        /// Gets or sets the maximum number of pixels allowed to move the mouse before cancelling the mouse event.
        /// Useful for <see cref="ContextMenu"/>s to appear if mouse only moved a bit or not at all.
        /// </summary>
        public static double HandleRightClickAfterPanningThreshold { get; set; } = 12d;

        /// <summary>
        /// Correct <see cref="ItemContainer"/>'s position after moving if starting position is not snapped to grid.
        /// </summary>
        public static bool EnableSnappingCorrection { get; set; } = true;

        /// <summary>
        /// Gets or sets how often the new <see cref="ViewportLocation"/> is calculated in milliseconds when <see cref="DisableAutoPanning"/> is false.
        /// </summary>
        public static double AutoPanningTickRate { get; set; } = 1;

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
        /// Gets or sets if the current position of containers that are being dragged should not be committed until the end of the dragging operation.
        /// </summary>
        public static bool EnableDraggingContainersOptimizations { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the cutting line should apply the preview style to the interesected elements.
        /// </summary>
        /// <remarks>
        /// This may hurt performance because intersection must be calculated on mouse move.
        /// </remarks>
        public static bool EnableCuttingLinePreview { get; set; } = false;

        /// <summary>
        /// The list of supported connection types for cutting. Type must be derived from <see cref="FrameworkElement">.
        /// </summary>
        public static readonly HashSet<Type> CuttingConnectionTypes = new HashSet<Type>();

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
        protected internal UIElement ConnectionsHost { get; private set; } = default!;

        private IDraggingStrategy? _draggingStrategy;
        private DispatcherTimer? _autoPanningTimer;

        /// <summary>
        /// Gets a list of <see cref="ItemContainer"/>s that are selected.
        /// </summary>
        /// <remarks>Cache the result before using it to avoid extra allocations.</remarks>
        protected internal IReadOnlyList<ItemContainer> SelectedContainers
        {
            get
            {
                IList selectedItems = base.SelectedItems;
                var selectedContainers = new List<ItemContainer>(selectedItems.Count);

                for (var i = 0; i < selectedItems.Count; i++)
                {
#if Avalonia
                    var container = (ItemContainer)ContainerFromItem(selectedItems[i]);
#else
                    var container = (ItemContainer)ItemContainerGenerator.ContainerFromItem(selectedItems[i]);
#endif
                    selectedContainers.Add(container);
                }

                return selectedContainers;
            }
        }

#endregion

        #region Construction

        static NodifyEditor()
        {
#if Avalonia
            FocusableProperty.OverrideMetadata(typeof(NodifyEditor), new StyledPropertyMetadata<bool>(true));
            ViewportZoomProperty.Changed.AddClassHandler<NodifyEditor, double>(OnViewportZoomChanged);
            DisableAutoPanningProperty.Changed.AddClassHandler<NodifyEditor, bool>(OnDisableAutoPanningChanged);
            MinViewportZoomProperty.Changed.AddClassHandler<NodifyEditor, double>(OnMinViewportZoomChanged);
            MaxViewportZoomProperty.Changed.AddClassHandler<NodifyEditor, double>(OnMaxViewportZoomChanged);
            ViewportLocationProperty.Changed.AddClassHandler<NodifyEditor, Point>(OnViewportLocationChanged);
            SelectedItemsProperty.Changed.AddClassHandler<NodifyEditor, IList?>(OnSelectedItemsSourceChanged);
            GridCellSizeProperty.Changed.AddClassHandler<NodifyEditor, uint>(OnGridCellSizeChanged);
            DisablePanningProperty.Changed.AddClassHandler<NodifyEditor, bool>(OnDisablePanningChanged);
#else
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NodifyEditor), new FrameworkPropertyMetadata(typeof(NodifyEditor)));
            FocusableProperty.OverrideMetadata(typeof(NodifyEditor), new FrameworkPropertyMetadata(BoxValue.True));

            EditorCommands.Register(typeof(NodifyEditor));
#endif
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

            AddHandler(ItemContainer.DragStartedEvent, new DragStartedEventHandler(OnItemsDragStarted));
            AddHandler(ItemContainer.DragCompletedEvent, new DragCompletedEventHandler(OnItemsDragCompleted));
            AddHandler(ItemContainer.DragDeltaEvent, new DragDeltaEventHandler(OnItemsDragDelta));

            var transform = new TransformGroup();
            transform.Children.Add(ScaleTransform);
            transform.Children.Add(TranslateTransform);

#if Avalonia
            SetAndRaise(ViewportTransformProperty, ref _viewportTransform, transform);
#else
            SetValue(ViewportTransformPropertyKey, transform);
#endif

            _states.Push(GetInitialState());
        }

        /// <inheritdoc />
#if Avalonia
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            // TODO:
            //ItemsHost = GetTemplateChild(ElementItemsHost) as Panel ?? throw new InvalidOperationException($"{ElementItemsHost} is missing or is not of type Panel.");
            //ConnectionsHost = GetTemplateChild(ElementConnectionsHost) as UIElement ?? throw new InvalidOperationException($"{ElementConnectionsHost} is missing or is not of type UIElement.");

#else
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            
            ItemsHost = GetTemplateChild(ElementItemsHost) as Panel ?? throw new InvalidOperationException($"{ElementItemsHost} is missing or is not of type Panel.");
            ConnectionsHost = GetTemplateChild(ElementConnectionsHost) as UIElement ?? throw new InvalidOperationException($"{ElementConnectionsHost} is missing or is not of type UIElement.");
#endif

            OnDisableAutoPanningChanged(DisableAutoPanning);

            State.Enter(null);
        }

#if Avalonia
        protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        {
            return new ItemContainer(this)
            {
                RenderTransform = new TranslateTransform()
            };
        }
#else
        /// <inheritdoc />
        protected override DependencyObject GetContainerForItemOverride()
            => new ItemContainer(this)
            {
                RenderTransform = new TranslateTransform()
            };

        /// <inheritdoc />
        protected override bool IsItemItsOwnContainerOverride(object item)
            => item is ItemContainer;
#endif

        #endregion

        #region Methods

#if Avalonia
        /// <summary>
        /// Zoom in at the viewports center
        /// </summary>
        public void ZoomIn() => ZoomAtPosition(Math.Pow(2.0, 120.0 / 3.0 / PointerHelper.PointerWheelDeltaForOneLine), (Point)((Vector)ViewportLocation + ViewportSize.ToVector() / 2));

        /// <summary>
        /// Zoom out at the viewports center
        /// </summary>
        public void ZoomOut() => ZoomAtPosition(Math.Pow(2.0, -120.0 / 3.0 / PointerHelper.PointerWheelDeltaForOneLine), (Point)((Vector)ViewportLocation + ViewportSize.ToVector() / 2));
#else
        /// <summary>
        /// Zoom in at the viewports center
        /// </summary>
        public void ZoomIn() => ZoomAtPosition(Math.Pow(2.0, 120.0 / 3.0 / Mouse.MouseWheelDeltaForOneLine), ViewportLocation + (Vector)ViewportSize / 2);

        /// <summary>
        /// Zoom out at the viewports center
        /// </summary>
        public void ZoomOut() => ZoomAtPosition(Math.Pow(2.0, -120.0 / 3.0 / Mouse.MouseWheelDeltaForOneLine), ViewportLocation + (Vector)ViewportSize / 2);
#endif

        /// <summary>
        /// Zoom at the specified location in graph space coordinates.
        /// </summary>
        /// <param name="zoom">The zoom factor.</param>
        /// <param name="location">The location to focus when zooming.</param>
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
                    Vector position = (Vector)location;

                    var dist = position - (Vector)ViewportLocation;
                    var zoomedDist = dist * zoom;
                    var diff = zoomedDist - dist;
                    ViewportLocation += diff / zoom;
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
#if Avalonia
            Point newLocation = (Point)((Vector)point - ViewportSize.ToVector() / 2);
#else
            Point newLocation = (Point)((Vector)point - (Vector)ViewportSize / 2);
#endif

            if (animated && newLocation != ViewportLocation)
            {
                IsPanning = true;
                DisablePanning = true;
                DisableZooming = true;

#if Avalonia
                double distance = newLocation.VectorSubtract(ViewportLocation).Length;
#else
                double distance = (newLocation - ViewportLocation).Length;
#endif
                double duration = distance / (BringIntoViewSpeed + (distance / 10)) * ViewportZoom;
                duration = Math.Max(0.1, Math.Min(duration, BringIntoViewMaxDuration));

                this.StartAnimation(ViewportLocationProperty, newLocation, duration, (s, e) =>
                {
                    IsPanning = false;
                    DisablePanning = false;
                    DisableZooming = false;

                    onFinish?.Invoke();
                });
            }
            else
            {
                ViewportLocation = newLocation;
                onFinish?.Invoke();
            }
        }

#if Avalonia
        public async Task StartAnimation(AvaloniaProperty dependencyProperty, Point toValue, double animationDurationSeconds, EventHandler? completedEvent = null)
        {
            var animation = new Animation()
            {
                Duration = TimeSpan.FromSeconds(animationDurationSeconds),
                Children =
                {
                    new KeyFrame()
                    {
                        Setters =
                        {
                            new Setter(ViewportLocationProperty,ViewportLocation)
                        },
                        Cue = new Cue(0)
                    },
                    new KeyFrame()
                    {
                        Setters =
                        {
                            new Setter(ViewportLocationProperty, toValue),
                        },
                        Cue = new Cue(1)
                    }
                }
            };
            await animation.RunAsync(this, default);
            ViewportLocation = toValue;

            completedEvent?.Invoke(this, EventArgs.Empty);
        }
#endif

        /// <summary>
        /// Moves the viewport center at the center of the specified area.
        /// </summary>
        /// <param name="area">The location in graph space coordinates.</param>
        public new void BringIntoView(Rect area)
            => BringIntoView(new Point(area.X + area.Width / 2, area.Y + area.Height / 2));

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

#endregion

        #region Auto panning

        private void HandleAutoPanning(object? sender, EventArgs e)
        {
            if (!IsPanning && IsMouseCaptureWithin)
            {
                Point mousePosition = Mouse.GetPosition(this);
                double edgeDistance = AutoPanEdgeDistance;
                double autoPanSpeed = Math.Min(AutoPanSpeed, AutoPanSpeed * AutoPanningTickRate) / (ViewportZoom * 2);
                double x = ViewportLocation.X;
                double y = ViewportLocation.Y;

                if (mousePosition.X <= edgeDistance)
                {
                    x -= autoPanSpeed;
                }
                else if (mousePosition.X >= ActualWidth - edgeDistance)
                {
                    x += autoPanSpeed;
                }

                if (mousePosition.Y <= edgeDistance)
                {
                    y -= autoPanSpeed;
                }
                else if (mousePosition.Y >= ActualHeight - edgeDistance)
                {
                    y += autoPanSpeed;
                }

                ViewportLocation = new Point(x, y);
                MouseLocation = Mouse.GetPosition(ItemsHost);

#if Avalonia
                State.HandleAutoPanning(null);
#else
                State.HandleAutoPanning(new MouseEventArgs(Mouse.PrimaryDevice, 0));
#endif
            }
        }

        /// <summary>
        /// Called when the <see cref="DisableAutoPanning"/> changes.
        /// </summary>
        /// <param name="shouldDisable">Whether to enable or disable auto panning.</param>
        protected virtual void OnDisableAutoPanningChanged(bool shouldDisable)
        {
            if (shouldDisable)
            {
                _autoPanningTimer?.Stop();
            }
            else if (_autoPanningTimer == null)
            {
#if Avalonia
                _autoPanningTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(AutoPanningTickRate),
                    DispatcherPriority.Background, HandleAutoPanning);
#else
                _autoPanningTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(AutoPanningTickRate),
                    DispatcherPriority.Background, HandleAutoPanning, Dispatcher);
#endif
            }
            else
            {
                _autoPanningTimer.Interval = TimeSpan.FromMilliseconds(AutoPanningTickRate);
                _autoPanningTimer.Start();
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

        #region State Handling

        private readonly Stack<EditorState> _states = new Stack<EditorState>();

        /// <summary>The current state of the editor.</summary>
        public EditorState State => _states.Peek();

        /// <summary>Creates the initial state of the editor.</summary>
        /// <returns>The initial state.</returns>
        protected virtual EditorState GetInitialState()
            => new EditorDefaultState(this);

        /// <summary>Pushes the given state to the stack.</summary>
        /// <param name="state">The new state of the editor.</param>
        /// <remarks>Calls <see cref="EditorState.Enter"/> on the new state.</remarks>
        public void PushState(EditorState state)
        {
            var prev = State;
            _states.Push(state);
            state.Enter(prev);
        }

        /// <summary>Pops the current <see cref="State"/> from the stack.</summary>
        /// <remarks>It doesn't pop the initial state. (see <see cref="GetInitialState"/>)
        /// <br />Calls <see cref="EditorState.Exit"/> on the current state.
        /// <br />Calls <see cref="EditorState.ReEnter"/> on the previous state.
        /// </remarks>
        public void PopState()
        {
            // Never remove the default state
            if (_states.Count > 1)
            {
                EditorState prev = _states.Pop();
                prev.Exit();
                State.ReEnter(prev);
            }
        }

        /// <summary>Pops all states from the editor.</summary>
        /// <remarks>It doesn't pop the initial state. (see <see cref="GetInitialState"/>)</remarks>
        public void PopAllStates()
        {
            while (_states.Count > 1)
            {
                PopState();
            }
        }

        /// <inheritdoc />
#if Avalonia
        protected override void OnPointerPressed(PointerPressedEventArgs e)
#else
        protected override void OnMouseDown(MouseButtonEventArgs e)
#endif
        {
            // Needed to not steal mouse capture from children
#if Avalonia
            if (e.Pointer.Captured == null || this.IsMouseCaptured(e))
#else
            if (Mouse.Captured == null || IsMouseCaptured)
#endif
            {
                Focus();
#if Avalonia
                e.Pointer.Capture(this);
#else
                CaptureMouse();
#endif

                MouseLocation = e.GetPosition(ItemsHost);
                State.HandleMouseDown(e);
            }
        }

        /// <inheritdoc />
#if Avalonia
        protected override void OnPointerReleased(PointerReleasedEventArgs e)
#else
        protected override void OnMouseUp(MouseButtonEventArgs e)
#endif
        {
            MouseLocation = e.GetPosition(ItemsHost);
            State.HandleMouseUp(e);

            // Release the mouse capture if all the mouse buttons are released
#if Avalonia
            var pointerProps = e.GetPointerPointProperties();
            if (this.IsMouseCaptured(e) && pointerProps is { IsLeftButtonPressed: false, IsMiddleButtonPressed: false, IsRightButtonPressed: false })
            {
                this.ReleaseMouseCapture(e);
            }
#else
            if (IsMouseCaptured && e.RightButton == MouseButtonState.Released && e.LeftButton == MouseButtonState.Released && e.MiddleButton == MouseButtonState.Released)
            {
                ReleaseMouseCapture();
            }
#endif

            // Disable context menu if selecting
            if (IsSelecting)
            {
                e.Handled = true;
            }
        }

        /// <inheritdoc />
#if Avalonia
        protected override void OnPointerMoved(PointerEventArgs e)
#else
        protected override void OnMouseMove(MouseEventArgs e)
#endif
        {
            MouseLocation = e.GetPosition(ItemsHost);
            State.HandleMouseMove(e);
        }

        /// <inheritdoc />
#if Avalonia
        protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
#else
        protected override void OnLostMouseCapture(MouseEventArgs e)
#endif
            => PopAllStates();

        /// <inheritdoc />
#if Avalonia
        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
#else
        protected override void OnMouseWheel(MouseWheelEventArgs e)
#endif
        {
            State.HandleMouseWheel(e);

#if Avalonia
            if (!e.Handled && EditorGestures.Mappings.Editor.ZoomModifierKey == e.KeyModifiers)
            {
                var delta = e.Delta.Length * Math.Sign(e.Delta.X + e.Delta.Y);
                double zoom = Math.Pow(2.0, delta / 3.0 );
#else
            if (!e.Handled && EditorGestures.Mappings.Editor.ZoomModifierKey == Keyboard.Modifiers)
            {
                double zoom = Math.Pow(2.0, e.Delta / 3.0 / Mouse.MouseWheelDeltaForOneLine);
#endif
                ZoomAtPosition(zoom, e.GetPosition(ItemsHost));

                // Handle it for nested editors
                if (e.Source is NodifyEditor)
                {
                    e.Handled = true;
                }
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
            => State.HandleKeyUp(e);

        protected override void OnKeyDown(KeyEventArgs e)
            => State.HandleKeyDown(e);

#endregion

        #region Selection Handlers

        private void OnSelectedItemsSourceChanged(IList oldValue, IList newValue)
        {
            if (oldValue is INotifyCollectionChanged oc)
            {
                oc.CollectionChanged -= OnSelectedItemsChanged;
            }

            if (newValue is INotifyCollectionChanged nc)
            {
                nc.CollectionChanged += OnSelectedItemsChanged;
            }

            IList selectedItems = base.SelectedItems;

            BeginUpdateSelectedItems();
            selectedItems.Clear();
            if (newValue != null)
            {
                for (var i = 0; i < newValue.Count; i++)
                {
                    selectedItems.Add(newValue[i]);
                }
            }
            EndUpdateSelectedItems();
        }

        private void OnSelectedItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (!CanSelectMultipleItems)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    base.SelectedItems.Clear();
                    break;

                case NotifyCollectionChangedAction.Add:
                    IList? newItems = e.NewItems;
                    if (newItems != null)
                    {
                        IList selectedItems = base.SelectedItems;
                        for (var i = 0; i < newItems.Count; i++)
                        {
                            selectedItems.Add(newItems[i]);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    IList? oldItems = e.OldItems;
                    if (oldItems != null)
                    {
                        IList selectedItems = base.SelectedItems;
                        for (var i = 0; i < oldItems.Count; i++)
                        {
                            selectedItems.Remove(oldItems[i]);
                        }
                    }
                    break;
            }
        }

#if !Avalonia
        /// <inheritdoc />
        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            IList? selected = SelectedItems;
            if (selected != null)
            {
                IList added = e.AddedItems;
                for (var i = 0; i < added.Count; i++)
                {
                    // Ensure no duplicates are added
                    if (!selected.Contains(added[i]))
                    {
                        selected.Add(added[i]);
                    }
                }

                IList removed = e.RemovedItems;
                for (var i = 0; i < removed.Count; i++)
                {
                    selected.Remove(removed[i]);
                }
            }
        }
#endif

        #endregion

        #region Selection

#if Avalonia
        public void BeginUpdateSelectedItems()
        {
            Selection.BeginBatchUpdate();
        }

        public void EndUpdateSelectedItems()
        {
            Selection.EndBatchUpdate();
        }
#endif

        internal void ApplyPreviewingSelection()
        {
            ItemCollection items = Items;
            IList selected = base.SelectedItems;

            IsSelecting = true;
            BeginUpdateSelectedItems();
            for (var i = 0; i < items.Count; i++)
            {
                var container = (ItemContainer)ItemContainerGenerator.ContainerFromIndex(i);
                if (container.IsPreviewingSelection == true && container.IsSelectable)
                {
                    selected.Add(items[i]);
                }
                else if (container.IsPreviewingSelection == false)
                {
                    selected.Remove(items[i]);
                }
                container.IsPreviewingSelection = null;
            }
            EndUpdateSelectedItems();
            IsSelecting = false;
        }

        internal void ClearPreviewingSelection()
        {
            ItemCollection items = Items;
            for (var i = 0; i < items.Count; i++)
            {
                var container = (ItemContainer)ItemContainerGenerator.ContainerFromIndex(i);
                container.IsPreviewingSelection = null;
            }
        }

        /// <summary>
        /// Inverts the <see cref="ItemContainer"/>s selection in the specified <paramref name="area"/>.
        /// </summary>
        /// <param name="area">The area to look for <see cref="ItemContainer"/>s.</param>
        /// <param name="fit">True to check if the <paramref name="area"/> contains the <see cref="ItemContainer"/>. <br />False to check if <paramref name="area"/> intersects the <see cref="ItemContainer"/>.</param>
        public void InvertSelection(Rect area, bool fit = false)
        {
            ItemCollection items = Items;
            IList selected = base.SelectedItems;

            IsSelecting = true;
            BeginUpdateSelectedItems();
            for (var i = 0; i < items.Count; i++)
            {
                var container = (ItemContainer)ItemContainerGenerator.ContainerFromIndex(i);

                if (container.IsSelectableInArea(area, fit))
                {
                    object? item = items[i];
                    if (container.IsSelected)
                    {
                        selected.Remove(item);
                    }
                    else
                    {
                        selected.Add(item);
                    }
                }
            }
            EndUpdateSelectedItems();
            IsSelecting = false;
        }

        /// <summary>
        /// Selects the <see cref="ItemContainer"/>s in the specified <paramref name="area"/>.
        /// </summary>
        /// <param name="area">The area to look for <see cref="ItemContainer"/>s.</param>
        /// <param name="append">If true, it will add to the existing selection.</param>
        /// <param name="fit">True to check if the <paramref name="area"/> contains the <see cref="ItemContainer"/>. <br />False to check if <paramref name="area"/> intersects the <see cref="ItemContainer"/>.</param>
        public void SelectArea(Rect area, bool append = false, bool fit = false)
        {
            if (!append)
            {
                UnselectAll();
            }

            ItemCollection items = Items;
            IList selected = base.SelectedItems;

            IsSelecting = true;
            BeginUpdateSelectedItems();
            for (var i = 0; i < items.Count; i++)
            {
                var container = (ItemContainer)ItemContainerGenerator.ContainerFromIndex(i);
                if (container.IsSelectableInArea(area, fit))
                {
                    selected.Add(items[i]);
                }
            }
            EndUpdateSelectedItems();
            IsSelecting = false;
        }

        /// <summary>
        /// Unselect the <see cref="ItemContainer"/>s in the specified <paramref name="area"/>.
        /// </summary>
        /// <param name="area">The area to look for <see cref="ItemContainer"/>s.</param>
        /// <param name="fit">True to check if the <paramref name="area"/> contains the <see cref="ItemContainer"/>. <br />False to check if <paramref name="area"/> intersects the <see cref="ItemContainer"/>.</param>
        public void UnselectArea(Rect area, bool fit = false)
        {
            IList items = base.SelectedItems;

            IsSelecting = true;
            BeginUpdateSelectedItems();
            for (var i = 0; i < items.Count; i++)
            {
#if Avalonia
                var container = (ItemContainer)ContainerFromItem(items[i]);
#else
                var container = (ItemContainer)ItemContainerGenerator.ContainerFromItem(items[i]);
#endif
                if (container.IsSelectableInArea(area, fit))
                {
                    items.Remove(items[i]);
                }
            }
            EndUpdateSelectedItems();
            IsSelecting = false;
        }

        /// <summary>
        /// Unselect all <see cref="Connections"/>.
        /// </summary>
        public void UnselectAllConnection()
        {
            if (ConnectionsHost is MultiSelector selector)
            {
                selector.UnselectAll();
            }
        }

        /// <summary>
        /// Select all <see cref="Connections"/>.
        /// </summary>
        public void SelectAllConnections()
        {
            if (ConnectionsHost is MultiSelector selector)
            {
                selector.SelectAll();
            }
        }

#endregion

        #region Dragging

        private void OnItemsDragDelta(object sender, DragDeltaEventArgs e)
        {
#if Avalonia
            _draggingStrategy?.Update(e.Vector);
#else
            _draggingStrategy?.Update(new Vector(e.HorizontalChange, e.VerticalChange));
#endif
        }

        private void OnItemsDragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (e.Canceled && ItemContainer.AllowDraggingCancellation)
            {
#if Avalonia
                _draggingStrategy?.Abort(e.Vector);
#else
                _draggingStrategy?.Abort(new Vector(e.HorizontalChange, e.VerticalChange));
#endif
            }
            else
            {
                IsBulkUpdatingItems = true;

#if Avalonia
                _draggingStrategy?.End(e.Vector);
#else
                _draggingStrategy?.End(new Vector(e.HorizontalChange, e.VerticalChange));
#endif

                IsBulkUpdatingItems = false;

                // Draw the containers at the new position.
                ItemsHost.InvalidateArrange();
            }

            if (ItemsDragCompletedCommand?.CanExecute(DataContext) ?? false)
            {
                ItemsDragCompletedCommand.Execute(DataContext);
            }
        }

        private void OnItemsDragStarted(object sender, DragStartedEventArgs e)
        {
            IList selectedItems = base.SelectedItems;

            if (EnableDraggingContainersOptimizations)
            {
                _draggingStrategy = new DraggingOptimized(this);
            }
            else
            {
                _draggingStrategy = new DraggingSimple(this);
            }

#if Avalonia
            _draggingStrategy.Start(e.Vector);
#else
            _draggingStrategy.Start(new Vector(e.HorizontalOffset, e.VerticalOffset));
#endif

            if (selectedItems.Count > 0)
            {
                if (ItemsDragStartedCommand?.CanExecute(DataContext) ?? false)
                {
                    ItemsDragStartedCommand.Execute(DataContext);
                }

                e.Handled = true;
            }
        }

#endregion

        #region Cutting

        /// <summary>
        /// Starts the cutting operation at the specified location. Call <see cref="EndCutting"/> to finish cutting.
        /// </summary>
        protected internal void StartCutting(Point location)
        {
            CuttingLineStart = location;
            CuttingLineEnd = location;
            IsCutting = true;
        }

        /// <summary>
        /// Cancels the cutting operation.
        /// </summary>
        protected internal void CancelCutting()
        {
            if (IsCutting)
            {
                IsCutting = false;
            }
        }

        /// <summary>
        /// Ends the cutting operation at the specified location.
        /// </summary>
        protected internal void EndCutting(Point location)
        {
            CuttingLineEnd = location;

            var lineGeometry = new LineGeometry(CuttingLineStart, CuttingLineEnd);
            var connections = ConnectionsHost.GetIntersectingElements(lineGeometry, CuttingConnectionTypes);

            if (RemoveConnectionCommand != null)
            {
                foreach (var connection in connections)
                {
                    OnRemoveConnection(connection.DataContext);
                }
            }
            else
            {
                foreach (var connection in connections)
                {
                    if (connection is BaseConnection bc)
                    {
                        bc.OnDisconnect();
                    }
                }
            }

            IsCutting = false;
        }

        #endregion

        /// <inheritdoc />
#if Avalonia
        protected void OnRenderSizeChanged(object? sender, SizeChangedEventArgs sizeInfo)
        {
#else
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
#endif

            double zoom = ViewportZoom;
#if Avalonia
            var editor = (NodifyEditor)sender;
            ViewportSize = new Size( editor.Bounds.Width / zoom, editor.Bounds.Height / zoom);
#else
            ViewportSize = new Size(ActualWidth / zoom, ActualHeight / zoom);
#endif

            OnViewportUpdated();
        }

        #region Utilities

        /// <summary>
        /// Translates the specified location to graph space coordinates (relative to the <see cref="ItemsHost" />).
        /// </summary>
        /// <param name="location">The location coordinates relative to <paramref name="relativeTo"/></param>
        /// <param name="relativeTo">The element where the <paramref name="location"/> was calculated from.</param>
        /// <returns>A location inside the graph.</returns>
        public Point GetLocationInsideEditor(Point location, UIElement relativeTo)
            => (Point)relativeTo.TranslatePoint(location, ItemsHost);

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

        #endregion
    }
}
