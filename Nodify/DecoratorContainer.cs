#if Avalonia
using Avalonia;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Nodify.Avalonia.Extensions;
using DependencyObject = Avalonia.AvaloniaObject;
using DependencyPropertyChangedEventArgs = Avalonia.AvaloniaPropertyChangedEventArgs<Avalonia.Point>;
using RoutedEventHandler = System.EventHandler<Avalonia.Interactivity.RoutedEventArgs>;
#else
using System.Windows;
using System.Windows.Controls;
#endif

namespace Nodify
{
    /// <summary>
    /// The container for all the items generated from the <see cref="NodifyEditor.Decorators"/> collection.
    /// </summary>
    public class DecoratorContainer : ContentControl, INodifyCanvasItem
    {
        #region Dependency Properties

#if Avalonia
        public static readonly StyledProperty<Point> LocationProperty = ItemContainer.LocationProperty.AddOwner<DecoratorContainer>();
        public static readonly StyledProperty<Size> ActualSizeProperty = ItemContainer.ActualSizeProperty.AddOwner<DecoratorContainer>();
#else
        public static readonly DependencyProperty LocationProperty = ItemContainer.LocationProperty.AddOwner(typeof(DecoratorContainer), new FrameworkPropertyMetadata(BoxValue.Point, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsParentArrange, OnLocationChanged));
        public static readonly DependencyProperty ActualSizeProperty = ItemContainer.ActualSizeProperty.AddOwner(typeof(DecoratorContainer));
#endif

        /// <summary>
        /// Gets or sets the location of this <see cref="DecoratorContainer"/> inside the <see cref="NodifyEditor.DecoratorsHost"/>.
        /// </summary>
        public Point Location
        {
            get => (Point)GetValue(LocationProperty);
            set => SetValue(LocationProperty, value);
        }

        /// <summary>
        /// Gets the actual size of this <see cref="DecoratorContainer"/>.
        /// </summary>
        public Size ActualSize
        {
            get => (Size)GetValue(ActualSizeProperty);
            set => SetValue(ActualSizeProperty, value);
        }

        private static void OnLocationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var item = (DecoratorContainer)d;
            item.OnLocationChanged();
        }

        #endregion

        #region Routed Events

#if Avalonia
        public static readonly RoutedEvent LocationChangedEvent = RoutedEvent.Register<DecoratorContainer, RoutedEventArgs>(nameof(LocationChanged), RoutingStrategies.Bubble);
#else
        public static readonly RoutedEvent LocationChangedEvent = EventManager.RegisterRoutedEvent(nameof(LocationChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DecoratorContainer));
#endif

        /// <summary>
        /// Occurs when the <see cref="Location"/> of this <see cref="DecoratorContainer"/> is changed.
        /// </summary>
        public event RoutedEventHandler LocationChanged
        {
            add => AddHandler(LocationChangedEvent, value);
            remove => RemoveHandler(LocationChangedEvent, value);
        }

        /// <summary>
        /// Raises the <see cref="LocationChangedEvent"/>.
        /// </summary>
        protected void OnLocationChanged()
        {
            RaiseEvent(new RoutedEventArgs(LocationChangedEvent, this));
        }

#endregion

        static DecoratorContainer()
        {
#if Avalonia
            LocationProperty.Changed.AddClassHandler<DecoratorContainer, Point>(OnLocationChanged);
#else
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DecoratorContainer), new FrameworkPropertyMetadata(typeof(DecoratorContainer)));
#endif
        }

#if Avalonia
        public DecoratorContainer()
        {
            SizeChanged += OnRenderSizeChanged;
        }

        /// <inheritdoc />
        protected void OnRenderSizeChanged(object? sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            ActualSize = ((DecoratorContainer)sender).RenderSize();
        }

#else
        /// <inheritdoc />
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            ActualSize = sizeInfo.NewSize;
        }
#endif
    }
}
