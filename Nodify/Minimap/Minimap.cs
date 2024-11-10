﻿using System;

#if Avalonia
using Avalonia;
using Avalonia.Input;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Nodify.Avalonia;
using Nodify.Avalonia.Helpers.Gestures;
using Nodify.Avalonia.Helpers;
using Nodify.Avalonia.Extensions;
using MouseButtonEventArgs = Avalonia.Input.PointerEventArgs;
using MouseEventArgs = Avalonia.Input.PointerEventArgs;
using MouseWheelEventArgs = Avalonia.Input.PointerWheelEventArgs;
using CommandBinding = Nodify.Avalonia.RoutedCommandBinding;
using MultiSelector = Avalonia.Controls.Primitives.SelectingItemsControl;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Shapes;
#endif

namespace Nodify
{
    /// <summary>
    /// A minimap control that can position the viewport, and zoom in and out.
    /// </summary>
#if Avalonia
#else
    [StyleTypedProperty(Property = nameof(ViewportStyle), StyleTargetType = typeof(Rectangle))]
    [StyleTypedProperty(Property = nameof(ItemContainerStyle), StyleTargetType = typeof(MinimapItem))]
    [TemplatePart(Name = ElementItemsHost, Type = typeof(Panel))]
#endif
    public class Minimap : ItemsControl
    {
        protected const string ElementItemsHost = "PART_ItemsHost";

#if Avalonia
        public static readonly StyledProperty<Point> ViewportLocationProperty = NodifyEditor.ViewportLocationProperty.AddOwner<Minimap>();
        public static readonly StyledProperty<Size> ViewportSizeProperty = NodifyEditor.ViewportSizeProperty.AddOwner<Minimap>();
        public static readonly StyledProperty<Style> ViewportStyleProperty = AvaloniaProperty.Register<Minimap, Style>(nameof(ViewportStyle)); 
        public static readonly StyledProperty<Rect> ExtentProperty = NodifyCanvas.ExtentProperty.AddOwner<Minimap>();
        public static readonly StyledProperty<Rect> ItemsExtentProperty = AvaloniaProperty.Register<Minimap, Rect>(nameof(ItemsExtent));
        public static readonly StyledProperty<Size> MaxViewportOffsetProperty = AvaloniaProperty.Register<Minimap, Size>(nameof(ViewportSize));
        public static readonly StyledProperty<bool> ResizeToViewportProperty = AvaloniaProperty.Register<Minimap, bool>(nameof(ViewportSize));
        public static readonly StyledProperty<bool> IsReadOnlyProperty = TextBox.IsReadOnlyProperty.AddOwner<Minimap>();

        public static readonly RoutedEvent ZoomEvent = RoutedEvent.Register<Minimap, ZoomEventArgs>(nameof(Zoom), RoutingStrategies.Bubble);
#else
        public static readonly DependencyProperty ViewportLocationProperty = NodifyEditor.ViewportLocationProperty.AddOwner(typeof(Minimap), new FrameworkPropertyMetadata(BoxValue.Point, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty ViewportSizeProperty = NodifyEditor.ViewportSizeProperty.AddOwner(typeof(Minimap));
        public static readonly DependencyProperty ViewportStyleProperty = DependencyProperty.Register(nameof(ViewportStyle), typeof(Style), typeof(Minimap));
        public static readonly DependencyProperty ExtentProperty = NodifyCanvas.ExtentProperty.AddOwner(typeof(Minimap));
        public static readonly DependencyProperty ItemsExtentProperty = DependencyProperty.Register(nameof(ItemsExtent), typeof(Rect), typeof(Minimap));
        public static readonly DependencyProperty MaxViewportOffsetProperty = DependencyProperty.Register(nameof(MaxViewportOffset), typeof(Size), typeof(Minimap), new FrameworkPropertyMetadata(new Size(2000, 2000)));
        public static readonly DependencyProperty ResizeToViewportProperty = DependencyProperty.Register(nameof(ResizeToViewport), typeof(bool), typeof(Minimap));
        public static readonly DependencyProperty IsReadOnlyProperty = TextBoxBase.IsReadOnlyProperty.AddOwner(typeof(Minimap));

        public static readonly RoutedEvent ZoomEvent = EventManager.RegisterRoutedEvent(nameof(Zoom), RoutingStrategy.Bubble, typeof(ZoomEventHandler), typeof(Minimap));
#endif

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
        protected internal Panel ItemsHost { get; private set; } = default!;

        static Minimap()
        {
#if !Avalonia
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Minimap), new FrameworkPropertyMetadata(typeof(Minimap)));
            ClipToBoundsProperty.OverrideMetadata(typeof(Minimap), new FrameworkPropertyMetadata(BoxValue.True));
#endif
        }

#if Avalonia
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            // TODO:
            //ItemsHost = GetTemplateChild(ElementItemsHost) as Panel ?? throw new InvalidOperationException($"{ElementItemsHost} is missing or is not of type {nameof(Panel)}.");
#else
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ItemsHost = GetTemplateChild(ElementItemsHost) as Panel ?? throw new InvalidOperationException($"{ElementItemsHost} is missing or is not of type {nameof(Panel)}.");
#endif
        }

        protected bool IsDragging { get; private set; }

#if Avalonia
        protected override void OnPointerPressed(PointerPressedEventArgs e)
#else
        protected override void OnMouseDown(MouseButtonEventArgs e)
#endif
        {
            var gestures = EditorGestures.Mappings.Minimap;
            if (!IsReadOnly && gestures.DragViewport.Matches(this, e))
            {
#if Avalonia
                this.CaptureMouseSafe(e);
#else
                this.CaptureMouseSafe();
#endif
                IsDragging = true;

                SetViewportLocation(e.GetPosition(ItemsHost));

                e.Handled = true;
            }
        }

#if Avalonia
        protected override void OnPointerMoved(PointerEventArgs e)
#else
        protected override void OnMouseMove(MouseEventArgs e)
#endif
        {
            if (IsDragging)
            {
                SetViewportLocation(e.GetPosition(ItemsHost));
            }
        }

        private void SetViewportLocation(Point location)
        {
#if Avalonia
            var position = location - new Vector(ViewportSize.Width / 2, ViewportSize.Height / 2) + (Vector)Extent.Position;
#else
            var position = location - new Vector(ViewportSize.Width / 2, ViewportSize.Height / 2) + (Vector)Extent.Location;
#endif

            if (MaxViewportOffset.Width != 0 || MaxViewportOffset.Height != 0)
            {
                double maxRight = ResizeToViewport ? ItemsExtent.Right : Math.Max(ItemsExtent.Right, ItemsExtent.Left + ViewportSize.Width);
                double maxBottom = ResizeToViewport ? ItemsExtent.Bottom : Math.Max(ItemsExtent.Bottom, ItemsExtent.Top + ViewportSize.Height);

#if Avalonia
                var positionX = position.X.Clamp(ItemsExtent.Left - ViewportSize.Width / 2 - MaxViewportOffset.Width, maxRight - ViewportSize.Width / 2 + MaxViewportOffset.Width);
                var positionY = position.Y.Clamp(ItemsExtent.Top - ViewportSize.Height / 2 - MaxViewportOffset.Height, maxBottom - ViewportSize.Height / 2 + MaxViewportOffset.Height);
                position = new Point(positionX, positionY);
#else
                position.X = position.X.Clamp(ItemsExtent.Left - ViewportSize.Width / 2 - MaxViewportOffset.Width, maxRight - ViewportSize.Width / 2 + MaxViewportOffset.Width);
                position.Y = position.Y.Clamp(ItemsExtent.Top - ViewportSize.Height / 2 - MaxViewportOffset.Height, maxBottom - ViewportSize.Height / 2 + MaxViewportOffset.Height);
#endif
            }

            ViewportLocation = position;
        }

#if Avalonia
        protected override void OnPointerReleased(PointerReleasedEventArgs e)
#else
        protected override void OnMouseUp(MouseButtonEventArgs e)
#endif
        {
            var gestures = EditorGestures.Mappings.Minimap;
            if (IsDragging && gestures.DragViewport.Matches(this, e))
            {
                IsDragging = false;
            }
            
#if Avalonia
            var props = e.GetPointerPointProperties();
            var IsMouseCaptured = this.IsMouseCaptured(e);
            if (IsMouseCaptured && !props.IsRightButtonPressed && !props.IsLeftButtonPressed && !props.IsMiddleButtonPressed)
            {
                this.ReleaseMouseCapture(e);
            }
#else
            if (IsMouseCaptured && e.RightButton == MouseButtonState.Released && e.LeftButton == MouseButtonState.Released && e.MiddleButton == MouseButtonState.Released)
            {
                ReleaseMouseCapture();
            }
#endif
        }

#if Avalonia
        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            if (!IsReadOnly && !e.Handled && EditorGestures.Mappings.Minimap.ZoomModifierKey == e.KeyModifiers)
            {
                var delta = e.Delta.Length * Math.Sign(e.Delta.X + e.Delta.Y);
                double zoom = Math.Pow(2.0, delta / 3.0);
                var location = ViewportLocation + (Vector)ViewportSize.ToVector() / 2;
#else
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (!IsReadOnly && !e.Handled && EditorGestures.Mappings.Minimap.ZoomModifierKey == Keyboard.Modifiers)
            {
                double zoom = Math.Pow(2.0, e.Delta / 3.0 / Mouse.MouseWheelDeltaForOneLine);
                var location = ViewportLocation + (Vector)ViewportSize / 2;
#endif

                var args = new ZoomEventArgs(zoom, location)
                {
                    RoutedEvent = ZoomEvent,
                    Source = this
                };
                RaiseEvent(args);

                e.Handled = true;
            }
        }

#if Avalonia
        protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        {
            return new MinimapItem();
        }
#else
        protected override DependencyObject GetContainerForItemOverride()
            => new MinimapItem();

        protected override bool IsItemItsOwnContainerOverride(object item)
            => item is MinimapItem;
#endif
    }
}
