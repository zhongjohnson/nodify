using System;
using Nodify.Interactivity;
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace Nodify
{
    /// <summary>
    /// The container for all the items generated from the <see cref="NodifyEditor.Decorators"/> collection.
    /// </summary>
    public class DecoratorContainer : ContentControl, INodifyCanvasItem, IKeyboardFocusTarget<DecoratorContainer>
    {
        #region Avalonia Properties

        public static readonly StyledProperty<Point> LocationProperty =
            AvaloniaProperty.Register<DecoratorContainer, Point>(
                nameof(Location),
                defaultValue: default(Point),
                defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public static readonly StyledProperty<Size> ActualSizeProperty =
            AvaloniaProperty.Register<DecoratorContainer, Size>(nameof(ActualSize), defaultValue: default(Size));

        /// <summary>
        /// Gets or sets the location of this <see cref="DecoratorContainer"/> inside the <see cref="NodifyEditor.DecoratorsHost"/>.
        /// </summary>
        public Point Location
        {
            get => GetValue(LocationProperty);
            set => SetValue(LocationProperty, value);
        }

        /// <summary>
        /// Gets the actual size of this <see cref="DecoratorContainer"/>.
        /// </summary>
        public Size ActualSize
        {
            get => GetValue(ActualSizeProperty);
            set => SetValue(ActualSizeProperty, value);
        }

        private static void OnLocationChanged(DecoratorContainer item, AvaloniaPropertyChangedEventArgs e)
        {
            item.OnLocationChanged();
        }

        #endregion

        #region Routed Events

        public static readonly RoutedEvent<RoutedEventArgs> LocationChangedEvent =
            RoutedEvent.Register<DecoratorContainer, RoutedEventArgs>(nameof(LocationChanged), RoutingStrategy.Bubble);

        /// <summary>
        /// Occurs when the <see cref="Location"/> of this <see cref="DecoratorContainer"/> is changed.
        /// </summary>
        public event EventHandler<RoutedEventArgs> LocationChanged
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

        public Rect Bounds => new Rect(Location, ActualSize);
        DecoratorContainer IKeyboardFocusTarget<DecoratorContainer>.Element => this;

        private DecoratorsControl? _owner;
        public DecoratorsControl? Owner => _owner ??= this.GetParentOfType<DecoratorsControl>();

        static DecoratorContainer()
        {
            LocationProperty.Changed.AddClassHandler<DecoratorContainer>((x, e) => OnLocationChanged(x, e));
            FocusableProperty.OverrideDefaultValue<DecoratorContainer>(true);
        }

        public DecoratorContainer(DecoratorsControl parent)
        {
            _owner = parent;
        }

        public DecoratorContainer()
        {
        }

        /// <inheritdoc />
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == BoundsProperty)
            {
                var newBounds = (Rect)change.NewValue!;
                SetCurrentValue(ActualSizeProperty, newBounds.Size);
            }
        }

        protected override void OnVisualParentChanged(AvaloniaObject oldParent)
        {
            if (VisualTreeHelper.GetParent(this) == null && IsKeyboardFocusWithin)
            {
                base.OnVisualParentChanged(oldParent);

                Owner?.Editor?.Focus();
            }
            else
            {
                base.OnVisualParentChanged(oldParent);
            }
        }
    }
}
