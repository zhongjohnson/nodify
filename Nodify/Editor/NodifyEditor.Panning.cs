using Nodify.Interactivity;
using System;
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Threading;

namespace Nodify
{
    public partial class NodifyEditor
    {
        #region Dependency properties

        public static readonly StyledProperty<double> AutoPanSpeedProperty =
            AvaloniaProperty.Register<NodifyEditor, double>(nameof(AutoPanSpeed), 15d);

        public static readonly StyledProperty<double> AutoPanEdgeDistanceProperty =
            AvaloniaProperty.Register<NodifyEditor, double>(nameof(AutoPanEdgeDistance), 15d);

        public static readonly StyledProperty<bool> DisableAutoPanningProperty =
            AvaloniaProperty.Register<NodifyEditor, bool>(nameof(DisableAutoPanning), false);

        public static readonly StyledProperty<bool> DisablePanningProperty =
            AvaloniaProperty.Register<NodifyEditor, bool>(nameof(DisablePanning), false);

        private bool _isPanning;
        public static readonly DirectProperty<NodifyEditor, bool> IsPanningProperty =
            AvaloniaProperty.RegisterDirect<NodifyEditor, bool>(
                nameof(IsPanning),
                o => o._isPanning,
                (o, v) => o._isPanning = v);

        /// <summary>
        /// Gets or sets whether panning should be disabled.
        /// </summary>
        public bool DisablePanning
        {
            get => (bool)GetValue(DisablePanningProperty);
            set => SetValue(DisablePanningProperty, value);
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
        /// Gets a value that indicates whether a panning operation is in progress.
        /// </summary>
        public bool IsPanning
        {
            get => _isPanning;
            private set => SetAndRaise(IsPanningProperty, ref _isPanning, value);
        }

        #endregion

        /// <summary>
        /// Gets or sets whether panning cancellation is allowed (see <see cref="EditorGestures.NodifyEditorGestures.CancelAction"/>).
        /// </summary>
        public static bool AllowPanningCancellation { get; set; }

        /// <summary>
        /// Gets or sets how often the new <see cref="ViewportLocation"/> is calculated in milliseconds when <see cref="DisableAutoPanning"/> is false.
        /// </summary>
        public static double AutoPanningTickRate { get; set; } = 1;

        private DispatcherTimer? _autoPanningTimer;

        private Point _initialPanningLocation;

        /// <summary>
        /// Starts the panning operation from the specified location. Call <see cref="EndPanning"/> to end the panning operation.
        /// </summary>
        /// <remarks>This method has no effect if a panning operation is already in progress.</remarks>
        /// <param name="location">The initial location where panning starts, in graph space coordinates.</param>
        public void BeginPanning(Point location)
        {
            if (IsPanning)
            {
                return;
            }

            _initialPanningLocation = location;
            ViewportLocation = location;
            IsPanning = true;
        }

        /// <summary>
        /// Starts the panning operation from the current <see cref="ViewportLocation" />.
        /// </summary>
        /// <remarks>This method has no effect if a panning operation is already in progress.</remarks>
        public void BeginPanning()
            => BeginPanning(ViewportLocation);

        /// <summary>
        /// Pans the viewport by the specified amount.
        /// </summary>
        /// <param name="amount">The amount to pan the viewport.</param>
        /// <remarks>
        /// This method adjusts the current <see cref="ViewportLocation"/> incrementally based on the provided amount.
        /// </remarks>
        public void UpdatePanning(Vector amount)
        {
            ViewportLocation -= amount;
        }

        /// <summary>
        /// Ends the current panning operation, retaining the current <see cref="ViewportLocation"/>.
        /// </summary>
        /// <remarks>This method has no effect if there's no panning operation in progress.</remarks>
        public void EndPanning()
        {
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
                ViewportLocation = _initialPanningLocation;
                IsPanning = false;
            }
        }

        #region Auto panning

        // Avalonia doesn't need pre-constructed event args for auto-panning
        // We'll track mouse position directly

        private void HandleAutoPanning(object? sender, EventArgs e)
        {
            // In Avalonia, we track the last mouse position from events
            // IsMouseCaptureWithin is checked via pointer capture tracking
            if (!IsPanning && _lastMousePosition.HasValue)
            {
                Point mousePosition = _lastMousePosition.Value;
                double edgeDistance = AutoPanEdgeDistance;
                double autoPanSpeed = Math.Min(AutoPanSpeed, AutoPanSpeed * AutoPanningTickRate) / (ViewportZoom * 2);
                double x = ViewportLocation.X;
                double y = ViewportLocation.Y;

                if (mousePosition.X <= edgeDistance)
                {
                    x -= autoPanSpeed;
                }
                else if (mousePosition.X >= Bounds.Width - edgeDistance)
                {
                    x += autoPanSpeed;
                }

                if (mousePosition.Y <= edgeDistance)
                {
                    y -= autoPanSpeed;
                }
                else if (mousePosition.Y >= Bounds.Height - edgeDistance)
                {
                    y += autoPanSpeed;
                }

                ViewportLocation = new Point(x, y);
                // Update MouseLocation based on current position relative to ItemsHost
                if (ItemsHost != null && _lastMousePosition.HasValue)
                {
                    MouseLocation = _lastMousePosition.Value;
                }
            }
        }

        private Point? _lastMousePosition;

        /// <summary>
        /// Called when the <see cref="DisableAutoPanning"/> changes.
        /// </summary>
        /// <param name="shouldDisable">Whether to enable or disable auto panning.</param>
        private void OnDisableAutoPanningChanged(bool shouldDisable)
        {
            ClearTimer();
            if (!shouldDisable)
            {
                _autoPanningTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(AutoPanningTickRate), DispatcherPriority.Background, HandleAutoPanning);
                _autoPanningTimer.Start();
            }

            void ClearTimer()
            {
                if (_autoPanningTimer != null)
                {
                    _autoPanningTimer.Stop();
                    _autoPanningTimer.Tick -= HandleAutoPanning;
                    _autoPanningTimer = null;
                }
            }
        }

        #endregion
    }
}
