using Nodify.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Controls.Shapes;

namespace Nodify
{
    /// <summary>
    /// Specifies how hotkeys are displayed for a pending connection.
    /// </summary>
    public enum HotKeysDisplayMode
    {
        /// <summary>
        /// No hotkeys will be displayed for the pending connection.
        /// </summary>
        None,

        /// <summary>
        /// Display hotkeys for keyboard only.
        /// </summary>
        Keyboard,

        /// <summary>
        /// Display hotkeys for both mouse and keyboard.
        /// </summary>
        All
    }

    /// <summary>
    /// Represents a pending connection usually started by a <see cref="Connector"/> which invokes the <see cref="CompletedCommand"/> when completed.
    /// </summary>
    public class PendingConnection : ContentControl
    {
        #region Avalonia Properties

        public static readonly StyledProperty<Point> SourceAnchorProperty =
            AvaloniaProperty.Register<PendingConnection, Point>(nameof(SourceAnchor), defaultValue: default(Point));

        public static readonly StyledProperty<Point> TargetAnchorProperty =
            AvaloniaProperty.Register<PendingConnection, Point>(nameof(TargetAnchor), defaultValue: default(Point));

        public static readonly StyledProperty<object?> SourceProperty =
            AvaloniaProperty.Register<PendingConnection, object?>(nameof(Source));

        public static readonly StyledProperty<object?> TargetProperty =
            AvaloniaProperty.Register<PendingConnection, object?>(nameof(Target));

        public static readonly StyledProperty<object?> PreviewTargetProperty =
            AvaloniaProperty.Register<PendingConnection, object?>(nameof(PreviewTarget));

        public static readonly StyledProperty<bool> EnablePreviewProperty =
            AvaloniaProperty.Register<PendingConnection, bool>(nameof(EnablePreview), defaultValue: false);

        public static readonly StyledProperty<double> StrokeThicknessProperty =
            Shape.StrokeThicknessProperty.AddOwner<PendingConnection>();

        public static readonly StyledProperty<AvaloniaList<double>?> StrokeDashArrayProperty =
            Shape.StrokeDashArrayProperty.AddOwner<PendingConnection>();

        public static readonly StyledProperty<IBrush?> StrokeProperty =
            Shape.StrokeProperty.AddOwner<PendingConnection>();

        public static readonly StyledProperty<bool> AllowOnlyConnectorsProperty =
            AvaloniaProperty.Register<PendingConnection, bool>(nameof(AllowOnlyConnectors), defaultValue: true);

        public static readonly StyledProperty<bool> EnableSnappingProperty =
            AvaloniaProperty.Register<PendingConnection, bool>(nameof(EnableSnapping), defaultValue: false);

        public static readonly StyledProperty<ConnectionDirection> DirectionProperty =
            BaseConnection.DirectionProperty.AddOwner<PendingConnection>();

        public new static readonly StyledProperty<bool> IsVisibleProperty =
            AvaloniaProperty.Register<PendingConnection, bool>(nameof(IsVisible), defaultValue: false, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        private static void OnVisibilityChanged(PendingConnection connection, AvaloniaPropertyChangedEventArgs e)
        {
            connection.IsVisible = (bool)e.NewValue!;
        }

        private static void OnAllowOnlyConnectorsChanged(PendingConnection connection, AvaloniaPropertyChangedEventArgs e)
        {
            connection.InvalidateHitTest();
            OnAllowOnlyConnectorsChangedInternal(connection, e);
        }

        static PendingConnection()
        {
            IsVisibleProperty.Changed.AddClassHandler<PendingConnection>((x, e) => OnVisibilityChanged(x, e));
            AllowOnlyConnectorsProperty.Changed.AddClassHandler<PendingConnection>((x, e) => OnAllowOnlyConnectorsChanged(x, e));
            AffectsRender<PendingConnection>(SourceAnchorProperty, TargetAnchorProperty);
        }

        /// <summary>
        /// Gets or sets the starting point for the connection.
        /// </summary>
        public Point SourceAnchor
        {
            get => GetValue(SourceAnchorProperty);
            set => SetValue(SourceAnchorProperty, value);
        }

        /// <summary>
        /// Gets or sets the end point for the connection.
        /// </summary>
        public Point TargetAnchor
        {
            get => GetValue(TargetAnchorProperty);
            set => SetValue(TargetAnchorProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Connector"/>'s <see cref="Control.DataContext"/> that started this pending connection.
        /// </summary>
        public object? Source
        {
            get => GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Connector"/>'s <see cref="Control.DataContext"/> (or potentially an <see cref="ItemContainer"/>'s <see cref="Control.DataContext"/> if <see cref="AllowOnlyConnectors"/> is false) that the <see cref="Source"/> can connect to.
        /// Only set when the connection is completed (see <see cref="CompletedCommand"/>).
        /// </summary>
        public object? Target
        {
            get => GetValue(TargetProperty);
            set => SetValue(TargetProperty, value);
        }

        /// <summary>
        /// <see cref="PreviewTarget"/> will be updated with a potential <see cref="Connector"/>'s <see cref="Control.DataContext"/> if this is true.
        /// </summary>
        /// <remarks>Requires <see cref="EnableHitTesting"/> to be true.</remarks>
        public bool EnablePreview
        {
            get => GetValue(EnablePreviewProperty);
            set => SetValue(EnablePreviewProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Connector"/> or the <see cref="ItemContainer"/> (if <see cref="AllowOnlyConnectors"/> is false) that we're previewing. See <see cref="EnablePreview"/>.
        /// </summary>
        public object? PreviewTarget
        {
            get => GetValue(PreviewTargetProperty);
            set => SetValue(PreviewTargetProperty, value);
        }

        /// <summary>
        /// Enables snapping the <see cref="TargetAnchor"/> to a possible <see cref="Target"/> connector.
        /// </summary>
        /// <remarks>Requires <see cref="EnableHitTesting"/> to be true.</remarks>
        public bool EnableSnapping
        {
            get => GetValue(EnableSnappingProperty);
            set => SetValue(EnableSnappingProperty, value);
        }

        /// <summary>
        /// If true will preview and connect only to <see cref="Connector"/>s, otherwise will also enable <see cref="ItemContainer"/>s.
        /// </summary>
        public bool AllowOnlyConnectors
        {
            get => GetValue(AllowOnlyConnectorsProperty);
            set => SetValue(AllowOnlyConnectorsProperty, value);
        }

        /// <summary>
        /// Gets or set the connection thickness.
        /// </summary>
        public double StrokeThickness
        {
            get => GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        /// <summary>
        /// Gets or sets the pattern of dashes and gaps that is used to outline the connection.
        /// </summary>
        public AvaloniaList<double>? StrokeDashArray
        {
            get => GetValue(StrokeDashArrayProperty);
            set => SetValue(StrokeDashArrayProperty, value);
        }

        /// <summary>
        /// Gets or sets the stroke color of the connection.
        /// </summary>
        public IBrush? Stroke
        {
            get => GetValue(StrokeProperty);
            set => SetValue(StrokeProperty, value);
        }

        /// <summary>
        /// Gets or sets the visibility of the connection.
        /// </summary>
        public new bool IsVisible
        {
            get => base.IsVisible;
            set => SetValue(IsVisibleProperty, value);
        }

        /// <summary>
        /// Gets or sets the direction of this connection.
        /// </summary>
        public ConnectionDirection Direction
        {
            get => (ConnectionDirection)GetValue(DirectionProperty);
            set => SetValue(DirectionProperty, value);
        }

        #endregion

        #region Attached Properties

        private static readonly AttachedProperty<bool> AllowOnlyConnectorsAttachedProperty =
            AvaloniaProperty.RegisterAttached<PendingConnection, Control, bool>("AllowOnlyConnectorsAttached", defaultValue: true);

        /// <summary>
        /// Will be set for <see cref="Connector"/>s and <see cref="ItemContainer"/>s when the pending connection is over the element if <see cref="EnablePreview"/> or <see cref="EnableSnapping"/> is true.
        /// </summary>
        public static readonly AttachedProperty<bool> IsOverElementProperty =
            AvaloniaProperty.RegisterAttached<PendingConnection, Control, bool>("IsOverElement", defaultValue: false);

        internal static bool GetAllowOnlyConnectorsAttached(Control elem)
            => elem.GetValue(AllowOnlyConnectorsAttachedProperty);

        internal static void SetAllowOnlyConnectorsAttached(Control elem, bool value)
            => elem.SetValue(AllowOnlyConnectorsAttachedProperty, value);

        public static bool GetIsOverElement(Control elem)
            => elem.GetValue(IsOverElementProperty);

        public static void SetIsOverElement(Control elem, bool value)
            => elem.SetValue(IsOverElementProperty, value);

        private static void OnAllowOnlyConnectorsChangedInternal(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
        {
            NodifyEditor? editor = ((PendingConnection)d).Editor;

            if (editor != null)
            {
                SetAllowOnlyConnectorsAttached(editor, (bool)e.NewValue!);
            }
        }

        #endregion

        #region Commands

        public static readonly StyledProperty<ICommand?> StartedCommandProperty =
            AvaloniaProperty.Register<PendingConnection, ICommand?>(nameof(StartedCommand));

        public static readonly StyledProperty<ICommand?> CompletedCommandProperty =
            AvaloniaProperty.Register<PendingConnection, ICommand?>(nameof(CompletedCommand));

        /// <summary>
        /// Gets or sets the command to invoke when the pending connection is started.
        /// Will not be invoked if <see cref="NodifyEditor.ConnectionStartedCommand"/> is used.
        /// <see cref="Source"/> will be set to the <see cref="Connector"/>'s <see cref="Control.DataContext"/> that started this connection and will also be the command's parameter.
        /// </summary>
        public ICommand? StartedCommand
        {
            get => GetValue(StartedCommandProperty);
            set => SetValue(StartedCommandProperty, value);
        }

        /// <summary>
        /// Gets or sets the command to invoke when the pending connection is completed.
        /// Will not be invoked if <see cref="NodifyEditor.ConnectionCompletedCommand"/> is used.
        /// <see cref="Target"/> will be set to the desired <see cref="Connector"/>'s <see cref="Control.DataContext"/> and will also be the command's parameter.
        /// </summary>
        public ICommand? CompletedCommand
        {
            get => GetValue(CompletedCommandProperty);
            set => SetValue(CompletedCommandProperty, value);
        }

        #endregion

        #region Fields

        /// <summary>
        /// Gets or sets whether hit testing is enabled for pending connections.
        /// </summary>
        /// <remarks>
        /// - When enabled, the <see cref="IsOverElementProperty"/> is updated on connectors during the drag operation. <br />
        /// - When disabled, the <see cref="EnablePreview"/> and <see cref="EnableSnapping"/> properties will have no effect. <br />
        /// - Disable hit testing to improve performance.
        /// </remarks>
        public static bool EnableHitTesting { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum number of hotkeys that can be displayed for a pending connection.
        /// </summary>
        /// <remarks>The maximum value can be 9.</remarks>
        public static uint MaxHotKeys { get; set; } = 9;

        /// <summary>
        /// Gets or sets whether hotkeys are enabled for pending connections.
        /// </summary>
        public static HotKeysDisplayMode HotKeysDisplayMode { get; set; } = HotKeysDisplayMode.Keyboard;

        /// <summary>
        /// Gets the <see cref="NodifyEditor"/> that owns this <see cref="PendingConnection"/>.
        /// </summary>
        protected NodifyEditor? Editor { get; private set; }

        private Control? _connectionTarget;
        private Connector? _hotKeysSource;
        private readonly List<HotKeyAdorner> _hotKeysAdorners = new List<HotKeyAdorner>();
        private AdornerLayer? _adornerLayer;

        private AdornerLayer? AdornerLayer => _adornerLayer ??= AdornerLayer.GetAdornerLayer(this);

        #endregion

        /// <inheritdoc />
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            if (Editor != null)
            {
                Editor.RemoveHandler(Connector.PendingConnectionStartedEvent, new PendingConnectionEventHandler(OnPendingConnectionStarted));
                Editor.RemoveHandler(Connector.PendingConnectionDragEvent, new PendingConnectionEventHandler(OnPendingConnectionDrag));
                Editor.RemoveHandler(Connector.PendingConnectionCompletedEvent, new PendingConnectionEventHandler(OnPendingConnectionCompleted));
                Editor.RemoveHandler(PreviewKeyUpEvent, new KeyEventHandler(OnKeyUp));
            }

            Editor = this.GetParentOfType<NodifyEditor>();

            if (Editor != null)
            {
                Editor.AddHandler(Connector.PendingConnectionStartedEvent, new PendingConnectionEventHandler(OnPendingConnectionStarted));
                Editor.AddHandler(Connector.PendingConnectionDragEvent, new PendingConnectionEventHandler(OnPendingConnectionDrag));
                Editor.AddHandler(Connector.PendingConnectionCompletedEvent, new PendingConnectionEventHandler(OnPendingConnectionCompleted));
                Editor.AddHandler(PreviewKeyUpEvent, new KeyEventHandler(OnKeyUp), true);

                SetAllowOnlyConnectorsAttached(Editor, AllowOnlyConnectors);
            }
        }

        #region Event Handlers

        protected virtual void OnPendingConnectionStarted(object sender, PendingConnectionEventArgs e)
        {
            if (!e.Handled && !e.Canceled)
            {
                e.Handled = true;
                e.Canceled = !StartedCommand?.CanExecute(e.SourceConnector) ?? false;

                Target = null;
                IsVisible = !e.Canceled;
                SourceAnchor = e.Anchor;
                TargetAnchor = new Point(e.Anchor.X + e.OffsetX, e.Anchor.Y + e.OffsetY);
                Source = e.SourceConnector;

                if (!e.Canceled)
                {
                    StartedCommand?.Execute(Source);

                    if (e.OriginalSource is Connector connector)
                    {
                        ShowHotKeys(connector);
                    }
                }

                if (EnablePreview)
                {
                    PreviewTarget = e.SourceConnector;
                }
            }
        }

        protected virtual void OnPendingConnectionDrag(object sender, PendingConnectionEventArgs e)
        {
            if (!e.Handled && IsVisible)
            {
                e.Handled = true;
                TargetAnchor = new Point(e.Anchor.X + e.OffsetX, e.Anchor.Y + e.OffsetY);

                if (!EnableHitTesting)
                {
                    return;
                }

                // Look for a potential connector
                Control? target = FindConnectionTarget(TargetAnchor);

                // Update the connector's anchor and snap to it, if snapping is enabled
                if (EnableSnapping && target is Connector connector)
                {
                    connector.UpdateAnchor();
                    TargetAnchor = connector.Anchor;
                }

                SetConnectionTarget(target);

                // Update the preview target if enabled
                if (EnablePreview)
                {
                    PreviewTarget = target?.DataContext;
                }
            }
        }

        protected virtual void OnPendingConnectionCompleted(object sender, PendingConnectionEventArgs e)
        {
            if (!e.Handled && IsVisible)
            {
                e.Handled = true;
                IsVisible = false;

                SetConnectionTarget(null);
                HideHotKeys();

                if (!e.Canceled)
                {
                    Target = e.TargetConnector;

                    // Invoke the CompletedCommand if event is not handled
                    if (CompletedCommand?.CanExecute(Target) ?? false)
                    {
                        CompletedCommand?.Execute(Target);
                    }
                }

                if (EnablePreview)
                {
                    PreviewTarget = null;
                }
            }
        }

        /// <summary>
        /// Sets the connection target and updates the visual state of the target element.
        /// </summary>
        private void SetConnectionTarget(Control? target)
        {
            if (target == _connectionTarget)
            {
                return;
            }

            if (_connectionTarget != null)
            {
                SetIsOverElement(_connectionTarget, false);
            }

            if (target != null)
            {
                SetIsOverElement(target, true);
            }

            _connectionTarget = target;
        }

        /// <summary>
        /// Searches for a potential <see cref="Connector"/> or <see cref="ItemContainer"/> at the specified position within the editor.
        /// </summary>
        public Control? FindConnectionTarget(Point position)
        {
            if (Editor != null)
            {
                return GetPotentialConnector(Editor, position, AllowOnlyConnectors);
            }

            return null;
        }

        #endregion

        #region Hot Keys

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (_hotKeysSource is { IsPendingConnection: true })
            {
                int hotKey = GetHotKey(e.Key);

                if (hotKey <= _hotKeysAdorners.Count)
                {
                    var adorner = _hotKeysAdorners.Find(x => x.Number == hotKey);
                    if (adorner != null)
                    {
                        var tempConnector = _hotKeysSource;
                        _hotKeysSource.EndConnecting(adorner.Connector);
                        tempConnector.ReleaseMouseCapture();
                        e.Handled = true;
                    }
                }
            }
        }

        private static int GetHotKey(Key key)
        {
            if (key >= Key.D1 && key <= Key.D9)
            {
                return key - Key.D0;
            }

            if (key >= Key.NumPad1 && key <= Key.NumPad9)
            {
                return key - Key.NumPad0;
            }

            return -1;
        }

        private void ShowHotKeys(Connector sourceConnector)
        {
            if (Editor == null
                || AdornerLayer == null
                || HotKeysDisplayMode == HotKeysDisplayMode.None
                || HotKeysDisplayMode == HotKeysDisplayMode.Keyboard && !(InputManager.Current.MostRecentInputDevice is KeyboardDevice))
            {
                return;
            }

            _hotKeysSource = sourceConnector;

            var connectCommand = Editor.ConnectionCompletedCommand ?? CompletedCommand;
            if (connectCommand != null)
            {
                bool isEditorConnect = connectCommand == Editor.ConnectionCompletedCommand;
                var connectorsInViewport = Editor.GetConnectorsInViewport();

                var possibleTargets = connectorsInViewport
                    .Where(x => isEditorConnect ? connectCommand.CanExecute((sourceConnector.DataContext, x.DataContext)) : connectCommand.CanExecute(x.DataContext))
                    .OrderBy(x => (sourceConnector.Anchor - x.Anchor).LengthSquared)
                    .Take((int)Math.Min(MaxHotKeys, 9));

                var adorners = possibleTargets.Select((x, i) => new HotKeyAdorner(x, i + 1));

                foreach (var adorner in adorners)
                {
                    _hotKeysAdorners.Add(adorner);
                    AdornerLayer.Add(adorner);
                }
            }
        }

        private void HideHotKeys()
        {
            _hotKeysSource = null;

            if (AdornerLayer != null)
            {
                foreach (var hotKeyAdorner in _hotKeysAdorners)
                {
                    AdornerLayer.Remove(hotKeyAdorner);
                }

                _hotKeysAdorners.Clear();
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Searches for a potential <see cref="Connector"/> or <see cref="ItemContainer"/> at the specified position within the editor.
        /// </summary>
        /// <param name="editor">The <see cref="NodifyEditor"/> to scan for connectors or item containers.</param>
        /// <param name="position">The position in the editor to check for intersections.</param>
        /// <param name="allowOnlyConnectors">
        /// If true, only <see cref="Connector"/>s are considered; otherwise, the method will also check for <see cref="ItemContainer"/>s.
        /// </param>
        /// <returns>
        /// Returns one of the following, depending on what is found at the specified position:
        /// <br /> - A <see cref="Connector"/> if one is present.
        /// <br /> - An <see cref="ItemContainer"/> if <paramref name="allowOnlyConnectors"/> is false and a <see cref="Connector"/> is not found.
        /// <br /> - The provided <see cref="NodifyEditor"/> itself if neither a <see cref="Connector"/> nor an <see cref="ItemContainer" /> is found, and <paramref name="allowOnlyConnectors"/> is true.
        /// <br /> - Null if no valid element is identified at the specified position.
        /// </returns>
        internal static Control? GetPotentialConnector(NodifyEditor editor, Point position, bool allowOnlyConnectors)
        {
            Connector? connector = editor.ItemsHost.GetElementAtPosition<Connector>(position);
            if (connector != null && connector.Editor == editor)
                return connector;

            if (allowOnlyConnectors)
                return null;

            var itemContainer = editor.ItemsHost.GetElementAtPosition<ItemContainer>(position);
            if (itemContainer != null && itemContainer.Editor == editor)
                return itemContainer;

            return editor;
        }

        /// <summary>
        /// Searches for a potential <see cref="Connector"/> or <see cref="ItemContainer"/> at the specified position,
        /// automatically determining whether to prioritize connectors based on editor settings.
        /// </summary>
        /// <param name="editor">The <see cref="NodifyEditor"/> to scan.</param>
        /// <param name="position">The position in the editor to check for intersections.</param>
        /// <returns>
        /// Returns a <see cref="Connector"/>, an <see cref="ItemContainer"/>, the <see cref="NodifyEditor"/>, or null.
        /// </returns>
        internal static Control? GetPotentialConnector(NodifyEditor editor, Point position)
            => GetPotentialConnector(editor, position, GetAllowOnlyConnectorsAttached(editor));

        #endregion

        private class HotKeyAdorner : Adorner
        {
            private readonly HotKeyControl _hotKeyControl;
            public Connector Connector { get; }
            public int Number { get; }
            private Point _offset;

            public HotKeyAdorner(Connector connector, int number) : base(connector)
            {
                IsHitTestVisible = false;
                Connector = connector;
                Number = number;

                _hotKeyControl = new HotKeyControl
                {
                    Number = number,
                    DataContext = connector.DataContext
                };

                AddVisualChild(_hotKeyControl);
                AddLogicalChild(_hotKeyControl);

                _offset = connector.Thumb.TranslatePoint(new Point(0, 0), connector);
            }

            protected override int VisualChildrenCount => 1;

            protected override Visual GetVisualChild(int index) => _hotKeyControl;

            protected override Size MeasureOverride(Size constraint)
            {
                _hotKeyControl.Measure(constraint);
                return _hotKeyControl.DesiredSize;
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                _hotKeyControl.Arrange(new Rect(_offset, finalSize));
                return finalSize;
            }
        }
    }
}
