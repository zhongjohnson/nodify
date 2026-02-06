using Nodify.Events;
using Nodify.Interactivity;
using System;
using System.Linq;
using Avalonia;
using Avalonia.Metadata;
using System.Windows.Input;
using Avalonia.Interactivity;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;

namespace Nodify
{
    /// <summary>
    /// Specifies the possible movement modes of a <see cref="GroupingNode"/>.
    /// </summary>
    public enum GroupingMovementMode
    {
        /// <summary>
        /// The <see cref="GroupingNode"/> will move its content when moved.
        /// </summary>
        Group,

        /// <summary>
        /// The <see cref="GroupingNode"/> will not move its content when moved.
        /// </summary>
        Self
    }

    /// <summary>
    /// Defines a panel with a header that groups <see cref="ItemContainer"/>s inside it and can be resized.
    /// </summary>
    public class GroupingNode : ContentControl
    {
        protected static readonly object GroupMovementBoxed = GroupingMovementMode.Group;

        protected const string ElementResizeThumb = "PART_ResizeThumb";
        protected const string ElementHeader = "PART_Header";
        protected const string ElementContent = "PART_Content";

        #region Routed Events

        public static readonly RoutedEvent<RoutedEventArgs> ResizeStartedEvent =
            RoutedEvent.Register<GroupingNode, RoutedEventArgs>(nameof(ResizeStarted), RoutingStrategies.Bubble);
        public static readonly RoutedEvent<RoutedEventArgs> ResizeCompletedEvent =
            RoutedEvent.Register<GroupingNode, RoutedEventArgs>(nameof(ResizeCompleted), RoutingStrategies.Bubble);

        /// <summary>
        /// Occurs when the node finished resizing.
        /// </summary>
        public event ResizeEventHandler ResizeCompleted
        {
            add => AddHandler(ResizeCompletedEvent, value);
            remove => RemoveHandler(ResizeCompletedEvent, value);
        }

        /// <summary>
        /// Occurs when the node started resizing.
        /// </summary>
        public event ResizeEventHandler ResizeStarted
        {
            add => AddHandler(ResizeStartedEvent, value);
            remove => RemoveHandler(ResizeStartedEvent, value);
        }

        #endregion

        #region Dependency Properties

        public static readonly StyledProperty<IBrush?> HeaderBrushProperty = Node.HeaderBrushProperty.AddOwner<GroupingNode>();
        public static readonly StyledProperty<bool> CanResizeProperty = AvaloniaProperty.Register<GroupingNode, bool>(nameof(CanResize), true);
        public static readonly StyledProperty<Size> ActualSizeProperty = AvaloniaProperty.Register<GroupingNode, Size>(nameof(ActualSize), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay, coerce: (o, v) => { OnActualSizeChanged(o, v); return v; });
        public static readonly StyledProperty<GroupingMovementMode> MovementModeProperty = AvaloniaProperty.Register<GroupingNode, GroupingMovementMode>(nameof(MovementMode), GroupingMovementMode.Group);
        public static readonly StyledProperty<ICommand?> ResizeCompletedCommandProperty = AvaloniaProperty.Register<GroupingNode, ICommand?>(nameof(ResizeCompletedCommand));
        public static readonly StyledProperty<ICommand?> ResizeStartedCommandProperty = AvaloniaProperty.Register<GroupingNode, ICommand?>(nameof(ResizeStartedCommand));

        private static void OnActualSizeChanged(GroupingNode node, Size newSize)
        {
            node.Width = newSize.Width;
            node.Height = newSize.Height;
        }

        /// <summary>
        /// Gets or sets the brush used for the background of the <see cref="HeaderedContentControl.Header"/> of this <see cref="GroupingNode"/>.
        /// </summary>
        public Brush HeaderBrush
        {
            get => (Brush)GetValue(HeaderBrushProperty);
            set => SetValue(HeaderBrushProperty, value);
        }

        /// <summary>
        /// Gets or sets a value that indicates whether this <see cref="GroupingNode"/> can be resized.
        /// </summary>
        public bool CanResize
        {
            get => (bool)GetValue(CanResizeProperty);
            set => SetValue(CanResizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the actual size of this <see cref="GroupingNode"/>.
        /// </summary>
        public Size ActualSize
        {
            get => (Size)GetValue(ActualSizeProperty);
            set => SetValue(ActualSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the default movement mode which can be temporarily changed by holding the <see cref="SwitchMovementModeModifierKey"/> while dragging by the header.
        /// </summary>
        public GroupingMovementMode MovementMode
        {
            get => (GroupingMovementMode)GetValue(MovementModeProperty);
            set => SetValue(MovementModeProperty, value);
        }

        /// <summary>
        /// Invoked when the <see cref="ResizeCompleted"/> event is not handled.
        /// Parameter is the <see cref="ItemContainer.ActualSize"/> of the container.
        /// </summary>
        public ICommand? ResizeCompletedCommand
        {
            get => (ICommand?)GetValue(ResizeCompletedCommandProperty);
            set => SetValue(ResizeCompletedCommandProperty, value);
        }

        /// <summary>
        /// Invoked when the <see cref="ResizeStarted"/> event is not handled.
        /// Parameter is the <see cref="ItemContainer.ActualSize"/> of the container.
        /// </summary>
        public ICommand? ResizeStartedCommand
        {
            get => (ICommand?)GetValue(ResizeStartedCommandProperty);
            set => SetValue(ResizeStartedCommandProperty, value);
        }

        #endregion

        #region Fields

        /// <summary>
        /// Gets the <see cref="NodifyEditor"/> that owns this <see cref="GroupingNode"/>.
        /// </summary>
        protected NodifyEditor? Editor { get; private set; }

        /// <summary>
        /// Gets the <see cref="NodifyEditor"/> that owns this <see cref="Container"/>.
        /// </summary>
        protected ItemContainer? Container { get; private set; }

        /// <summary>
        /// Gets the <see cref="Control"/> used to resize this <see cref="GroupingNode"/>.
        /// </summary>
        protected Control? ResizeThumb;

        /// <summary>
        /// Gets the <see cref="HeaderedContentControl.Header"/> control of this <see cref="GroupingNode"/>.
        /// </summary>
        protected Control? HeaderControl;

        /// <summary>
        /// Gets the <see cref="System.Windows.Controls.ContentControl"/> control of this <see cref="GroupingNode"/>.
        /// </summary>
        protected Control? ContentControl;

        private double _minHeight = 30;
        private double _minWidth = 30;

        #endregion

        static GroupingNode()
        {
            // Avalonia uses different metadata override patterns
            // DefaultStyleKeyProperty doesn't exist in Avalonia - handled by theme system
            // FocusableProperty override not needed in same way
            ZIndexProperty.Changed.AddClassHandler<GroupingNode>(OnZIndexPropertyChanged);
        }

        private static void OnZIndexPropertyChanged(GroupingNode node, AvaloniaPropertyChangedEventArgs e)
        {
            if (node.Container != null)
            {
                node.Container.ZIndex = (int)e.NewValue!;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupingNode"/> class.
        /// </summary>
        public GroupingNode()
        {
            // TODO: Avalonia Thumb control has different event handler patterns
            // Need to implement resize functionality using Avalonia Thumb events
            // AddHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler(OnResize));
            // AddHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler(OnResizeCompleted));
            // AddHandler(Thumb.DragStartedEvent, new DragStartedEventHandler(OnResizeStarted));

            Loaded += OnNodeLoaded;
            Unloaded += OnNodeUnloaded;
        }

        private void OnNodeLoaded(object sender, RoutedEventArgs e)
        {
            if (HeaderControl != null)
            {
                // TODO: Replace MouseDown with PointerPressed in Avalonia
                // HeaderControl.MouseDown += OnHeaderMouseDown;
                HeaderControl.SizeChanged += OnHeaderSizeChanged;
                CalculateDesiredHeaderSize();
            }
        }

        private void OnNodeUnloaded(object sender, RoutedEventArgs e)
        {
            if (HeaderControl != null)
            {
                // TODO: Replace MouseDown with PointerPressed
                // HeaderControl.MouseDown -= OnHeaderMouseDown;
                HeaderControl.SizeChanged -= OnHeaderSizeChanged;
            }
        }

        // TODO: Implement mouse handling using Avalonia pointer events
        // MouseButtonEventArgs and Keyboard static class don't exist in Avalonia
        /*
        private void OnHeaderMouseDown(object sender, MouseButtonEventArgs e)
        {
            EditorGestures.ItemContainerGestures gestures = EditorGestures.Mappings.ItemContainer;
            if (Container != null && Editor != null && gestures.Drag.Matches(e.Source, e))
            {
                // Switch the default movement mode if necessary
                var prevMovementMode = MovementMode;
                if (KeyModifiersMatch()) // TODO: Replace Keyboard.Modifiers check
                {
                    MovementMode = MovementMode == GroupingMovementMode.Group ? GroupingMovementMode.Self : GroupingMovementMode.Group;
                }

                var groupBounds = new Rect(Container.Location, Bounds.Size);

                // Select the content and move with it
                if (gestures.Selection.Append.Matches(e.Source, e))
                {
                    Editor.SelectArea(groupBounds, append: true, fit: true);
                }
                else if (gestures.Selection.Remove.Matches(e.Source, e))
                {
                    Editor.UnselectArea(groupBounds, fit: true);
                }
                else if (gestures.Selection.Invert.Matches(e.Source, e))
                {
                    if (Container.IsSelected)
                    {
                        Editor.UnselectArea(groupBounds, fit: true);
                        Container.IsSelected = true;
                    }
                    else
                    {
                        Editor.SelectArea(groupBounds, append: true, fit: true);
                    }
                }
                else if (gestures.Selection.Replace.Matches(e.Source, e) || EditorGestures.Mappings.ItemContainer.Drag.Matches(e.Source, e))
                {
                    Editor.SelectArea(groupBounds, append: Container.IsSelected, fit: true);
                }

                // Deselect content
                if (MovementMode == GroupingMovementMode.Self)
                {
                    Editor.UnselectArea(groupBounds, fit: true);
                    Container.IsSelected = true;
                }

                // Switch the default movement mode back
                MovementMode = prevMovementMode;
            }
        }

        /// <summary>
        /// Toggles the selection of nodes inside this group.
        /// If any contained nodes are selected, all will be unselected.
        /// If none are selected, all will be selected.
        /// </summary>
        public void ToggleContentSelection()
        {
            if (Editor != null && Container != null)
            {
                var groupBounds = new Rect(Container.Location, RenderSize);
                bool hasSelection = Editor.SelectedContainers.Any(x => x != Container && groupBounds.Contains(x.Bounds));
                if (hasSelection)
                {
                    Editor.UnselectArea(groupBounds, fit: true);
                    Container.IsSelected = true;
                }
                else
                {
                    Editor.SelectArea(groupBounds, append: true, fit: true);
                }
            }
        }

        /// <inheritdoc />
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            ResizeThumb = e.NameScope.Find<Control>(ElementResizeThumb);
            HeaderControl = e.NameScope.Find<Control>(ElementHeader);
            ContentControl = e.NameScope.Find<Control>(ElementContent);

            Container = this.GetParentOfType<ItemContainer>();
            Editor = Container?.Editor ?? this.GetParentOfType<NodifyEditor>();

            if (Container != null)
            {
                Container.ZIndex = this.ZIndex;
            }
        }

        // TODO: Implement resize using Avalonia Thumb DragDelta event pattern
        // WPF DragDeltaEventArgs, DragStartedEventArgs, DragCompletedEventArgs don't exist in Avalonia
        /*
        private void OnResize(object sender, DragDeltaEventArgs e)
        {
            if (CanResize && ReferenceEquals(e.OriginalSource, ResizeThumb))
            {
                double resultWidth = Bounds.Width + e.HorizontalChange;
                double resultHeight = Bounds.Height + e.VerticalChange;

                // Snap to grid
                if (Editor != null)
                {
                    uint cellSize = Editor.GridCellSize;
                    resultWidth = (int)resultWidth / cellSize * cellSize;
                    resultHeight = (int)resultHeight / cellSize * cellSize;
                }

                Width = Math.Max(_minWidth, resultWidth);
                Height = Math.Max(_minHeight, resultHeight);

                e.Handled = true;
            }
        }

        private void OnResizeStarted(object sender, DragStartedEventArgs e)
        {
            ActualSize = new Size(Bounds.Width, Bounds.Height);
            var args = new ResizeEventArgs(ActualSize, ActualSize)
            {
                RoutedEvent = ResizeStartedEvent,
                Source = this
            };

            RaiseEvent(args);

            // Raise ResizeStartedCommand if ResizeStartedEvent event is not handled
            if (!args.Handled && (ResizeStartedCommand?.CanExecute(ActualSize) ?? false))
            {
                ResizeStartedCommand.Execute(ActualSize);
            }
        }

        private void OnResizeCompleted(object sender, DragCompletedEventArgs e)
        {
            Size previousSize = ActualSize;
            var newSize = new Size(Bounds.Width, Bounds.Height);
            ActualSize = newSize;

            var args = new ResizeEventArgs(previousSize, newSize)
            {
                RoutedEvent = ResizeCompletedEvent,
                Source = this
            };

            RaiseEvent(args);

            // Raise ResizeCompletedCommand if ResizeCompletedEvent event is not handled
            if (!args.Handled && (ResizeCompletedCommand?.CanExecute(newSize) ?? false))
            {
                ResizeCompletedCommand.Execute(newSize);
            }
        }
        */

        private void OnHeaderSizeChanged(object sender, SizeChangedEventArgs e)
            => CalculateDesiredHeaderSize();

        private void CalculateDesiredHeaderSize()
        {
            if (HeaderControl != null && ResizeThumb != null)
            {
                _minHeight = Math.Max(HeaderControl.ActualHeight + ResizeThumb.ActualHeight, MinHeight);
                _minWidth = Math.Max(ResizeThumb.ActualWidth, MinWidth);

                // If there's content don't resize it
                if (ContentControl != null)
                {
                    _minWidth = Math.Max(_minWidth, ContentControl.DesiredSize.Width);
                    _minHeight = Math.Max(_minHeight, _minHeight + ContentControl.DesiredSize.Height);
                }
            }

            // Allow selecting only by the header
            if (Container != null)
            {
                Container.DesiredSizeForSelection = new Size(ActualWidth, Math.Max(HeaderControl?.ActualHeight ?? _minHeight, MinHeight));
            }
        }
    }
}
