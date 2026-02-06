using Nodify.Interactivity;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Controls.Shapes;
using Avalonia.Styling;

namespace Nodify
{
    public partial class NodifyEditor
    {
        #region Dependency properties

        public static readonly StyledProperty<Style?> PushedAreaStyleProperty =
            AvaloniaProperty.Register<NodifyEditor, Style?>(nameof(PushedAreaStyle));

        private Rect _pushedArea;
        public static readonly DirectProperty<NodifyEditor, Rect> PushedAreaProperty =
            AvaloniaProperty.RegisterDirect<NodifyEditor, Rect>(
                nameof(PushedArea),
                o => o._pushedArea,
                (o, v) => o._pushedArea = v);

        private bool _isPushingItems;
        public static readonly DirectProperty<NodifyEditor, bool> IsPushingItemsProperty =
            AvaloniaProperty.RegisterDirect<NodifyEditor, bool>(
                nameof(IsPushingItems),
                o => o._isPushingItems,
                (o, v) => o._isPushingItems = v);

        private Orientation _pushedAreaOrientation = Orientation.Horizontal;
        public static readonly DirectProperty<NodifyEditor, Orientation> PushedAreaOrientationProperty =
            AvaloniaProperty.RegisterDirect<NodifyEditor, Orientation>(
                nameof(PushedAreaOrientation),
                o => o._pushedAreaOrientation,
                (o, v) => o._pushedAreaOrientation = v);

        /// <summary>
        /// Gets the currently pushed area while <see cref="IsPushingItems"/> is true.
        /// </summary>
        public Rect PushedArea
        {
            get => _pushedArea;
            private set => SetAndRaise(PushedAreaProperty, ref _pushedArea, value);
        }

        /// <summary>
        /// Gets a value that indicates whether a pushing operation is in progress.
        /// </summary>
        public bool IsPushingItems
        {
            get => _isPushingItems;
            private set => SetAndRaise(IsPushingItemsProperty, ref _isPushingItems, value);
        }

        /// <summary>
        /// Gets the orientation of the <see cref="PushedArea"/>.
        /// </summary>
        public Orientation PushedAreaOrientation
        {
            get => _pushedAreaOrientation;
            private set => SetAndRaise(PushedAreaOrientationProperty, ref _pushedAreaOrientation, value);
        }

        /// <summary>
        /// Gets or sets the style to use for the pushed area.
        /// </summary>
        public Style? PushedAreaStyle
        {
            get => GetValue(PushedAreaStyleProperty);
            set => SetValue(PushedAreaStyleProperty, value);
        }

        #endregion

        /// <summary>
        /// Gets or sets whether push items cancellation is allowed (see <see cref="EditorGestures.NodifyEditorGestures.CancelAction"/>).
        /// </summary>
        /// <remarks>Has no effect if <see cref="AllowDraggingCancellation"/> is false.</remarks>
        public static bool AllowPushItemsCancellation { get; set; } = true;

        private IPushStrategy? _pushStrategy;

        /// <summary>
        /// Starts the pushing items operation at the specified location with the specified orientation.
        /// </summary>
        /// <remarks>This method has no effect if a pushing operation is already in progress.</remarks>
        /// <param name="location">The starting location for pushing items, in graph space coordinates.</param>
        /// <param name="orientation">The orientation of the <see cref="PushedArea"/>.</param>
        public void BeginPushingItems(Point location, Orientation orientation)
        {
            if (IsPushingItems)
            {
                return;
            }

            IsPushingItems = true;
            PushedAreaOrientation = orientation;

            _pushStrategy = CreatePushStrategy(orientation);

            PushedArea = _pushStrategy.Start(location);
        }

        /// <summary>
        /// Updates the pushed area based on the specified amount taking the <see cref="PushedAreaOrientation"/> into account.
        /// </summary>
        /// <param name="amount">The amount to adjust the pushed area by.</param>
        /// <remarks>
        /// This method adjusts the pushed area incrementally. It should only be called while a pushing operation is in progress (see <see cref="BeginPushingItems(Point, Orientation)"/>).
        /// </remarks>
        public void UpdatePushedArea(Vector amount)
        {
            Debug.Assert(IsPushingItems);
            PushedArea = _pushStrategy!.Push(amount);
        }

        /// <summary>
        /// Ends the current pushing operation and finalizes the pushed area state.
        /// </summary>
        /// <remarks>This method has no effect if there's no pushing operation in progress.</remarks>
        public void EndPushingItems()
        {
            if (!IsPushingItems)
            {
                return;
            }

            PushedArea = _pushStrategy!.End();
            _pushStrategy = null;
            IsPushingItems = false;
        }

        /// <summary>
        /// Cancels the current pushing operation and reverts the <see cref="PushedArea"/> to its initial state if <see cref="AllowPushItemsCancellation"/> is true.
        /// Otherwise, it ends the pushing operation by calling <see cref="EndPushingItems"/>.
        /// </summary>
        /// <remarks>This method has no effect if there's no pushing operation in progress.</remarks>
        public void CancelPushingItems()
        {
            if (!AllowPushItemsCancellation)
            {
                EndPushingItems();
                return;
            }

            if (IsPushingItems)
            {
                PushedArea = _pushStrategy!.Cancel();
                IsPushingItems = false;
            }
        }

        private void UpdatePushedArea()
        {
            if (IsPushingItems)
            {
                PushedArea = _pushStrategy!.GetPushedArea();
            }
        }

        private IPushStrategy CreatePushStrategy(Orientation orientation)
        {
            if (orientation == Orientation.Horizontal)
            {
                return new HorizontalPushStrategy(this);
            }

            return new VerticalPushStrategy(this);
        }
    }
}
