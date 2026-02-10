using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Windows.Input;

namespace Nodify
{
    public class ResizablePanel : ContentControl
    {
        internal static readonly object BoxedResizeDirection = ResizeDirections.All;

        public static readonly StyledProperty<ResizeDirections> DirectionsProperty
            = AvaloniaProperty.Register<ResizablePanel, ResizeDirections>(nameof(Directions), (ResizeDirections)BoxedResizeDirection);

        public static readonly StyledProperty<ICommand?> ResizeStartedCommandProperty = AvaloniaProperty.Register<ResizablePanel, ICommand?>(nameof(ResizeStartedCommand));

        public static readonly StyledProperty<ICommand?> ResizeCompletedCommandProperty = AvaloniaProperty.Register<ResizablePanel, ICommand?>(nameof(ResizeCompletedCommand));

        public ResizeDirections Directions
        {
            get => GetValue(DirectionsProperty);
            set => SetValue(DirectionsProperty, value);
        }

        public ICommand? ResizeStartedCommand
        {
            get => GetValue(ResizeStartedCommandProperty);
            set => SetValue(ResizeStartedCommandProperty, value);
        }

        public ICommand? ResizeCompletedCommand
        {
            get => GetValue(ResizeCompletedCommandProperty);
            set => SetValue(ResizeCompletedCommandProperty, value);
        }

        private Resizer? _activeResizer;
        private Point _lastPointerPosition;
        private bool _isDragging;

        public ResizablePanel()
        {
            AddHandler(PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
            AddHandler(PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel);
            AddHandler(PointerMovedEvent, OnPointerMoved, RoutingStrategies.Tunnel);
        }

        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.Source is Resizer resizer && e.GetCurrentPoint(resizer).Properties.IsLeftButtonPressed)
            {
                _activeResizer = resizer;
                _lastPointerPosition = e.GetPosition(this);
                _isDragging = true;
                e.Pointer.Capture(resizer);

                if (ResizeStartedCommand?.CanExecute(null) ?? false)
                {
                    ResizeStartedCommand.Execute(null);
                }

                e.Handled = true;
            }
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (_isDragging && _activeResizer != null)
            {
                var current = e.GetPosition(this);
                var deltaX = current.X - _lastPointerPosition.X;
                var deltaY = current.Y - _lastPointerPosition.Y;
                _lastPointerPosition = current;

                OnResize(_activeResizer, deltaX, deltaY);
                e.Handled = true;
            }
        }

        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (_isDragging && _activeResizer != null)
            {
                e.Pointer.Capture(null);
                _activeResizer = null;
                _isDragging = false;

                if (ResizeCompletedCommand?.CanExecute(null) ?? false)
                {
                    ResizeCompletedCommand.Execute(null);
                }

                e.Handled = true;
            }
        }

        private void OnResize(Resizer resizer, double horizontalChange, double verticalChange)
        {
            double resizeX = 0;
            double resizeY = 0;

            double moveX = 0;
            double moveY = 0;

            if (resizer.Direction.HasFlag(ResizeDirections.Top))
            {
                moveY = resizeY = ResizeTop(verticalChange);
            }

            if (resizer.Direction.HasFlag(ResizeDirections.Bottom))
            {
                resizeY = ResizeBottom(verticalChange);
            }

            if (resizer.Direction.HasFlag(ResizeDirections.Left))
            {
                moveX = resizeX = ResizeLeft(horizontalChange);
            }

            if (resizer.Direction.HasFlag(ResizeDirections.Right))
            {
                resizeX = ResizeRight(horizontalChange);
            }

            if (resizer.Direction.HasFlag(ResizeDirections.TopLeft))
            {
                moveY = resizeY = ResizeTop(verticalChange);
                moveX = resizeX = ResizeLeft(horizontalChange);
            }

            if (resizer.Direction.HasFlag(ResizeDirections.TopRight))
            {
                moveY = resizeY = ResizeTop(verticalChange);
                resizeX = ResizeRight(horizontalChange);
            }

            if (resizer.Direction.HasFlag(ResizeDirections.BottomLeft))
            {
                resizeY = ResizeBottom(verticalChange);
                moveX = resizeX = ResizeLeft(horizontalChange);
            }

            if (resizer.Direction.HasFlag(ResizeDirections.BottomRight))
            {
                resizeY = ResizeBottom(verticalChange);
                resizeX = ResizeRight(horizontalChange);
            }

            OnProcessDelta(ref resizeX, ref resizeY);
            OnProcessDelta(ref moveX, ref moveY);

            OnMove(moveX, moveY);

            Width -= resizeX;
            Height -= resizeY;
        }

        private double ResizeBottom(double verticalChange)
        {
            return Math.Min(-verticalChange, Bounds.Height - MinHeight);
        }

        private double ResizeTop(double verticalChange)
        {
            return Math.Min(verticalChange, Bounds.Height - MinHeight);
        }

        private double ResizeRight(double horizontalChange)
        {
            return Math.Min(-horizontalChange, Bounds.Width - MinWidth);
        }

        private double ResizeLeft(double horizontalChange)
        {
            return Math.Min(horizontalChange, Bounds.Width - MinWidth);
        }

        protected virtual void OnMove(double x, double y)
        {
            Canvas.SetTop(this, Canvas.GetTop(this) + y);
            Canvas.SetLeft(this, Canvas.GetLeft(this) + x);
        }

        protected virtual void OnProcessDelta(ref double dx, ref double dy)
        {
        }
    }

    public class Resizer : Thumb
    {
        public static readonly StyledProperty<ResizeDirections> DirectionProperty
            = AvaloniaProperty.Register<Resizer, ResizeDirections>(nameof(Direction), (ResizeDirections)ResizablePanel.BoxedResizeDirection);

        public ResizeDirections Direction
        {
            get => GetValue(DirectionProperty);
            set => SetValue(DirectionProperty, value);
        }

    }

    [Flags]
    public enum ResizeDirections
    {
        Top = 1,
        Left = 2,
        Bottom = 4,
        Right = 8,
        TopLeft = 16,
        TopRight = 32,
        BottomLeft = 64,
        BottomRight = 128,

        Edges = Top | Left | Bottom | Right,
        Corners = TopLeft | TopRight | BottomLeft | BottomRight,
        All = Edges | Corners
    }
}
