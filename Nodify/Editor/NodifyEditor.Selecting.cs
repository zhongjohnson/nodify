using System.Diagnostics;
using Avalonia.Controls;
using Avalonia;
using System.Windows.Input;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using Avalonia.Input;
using Avalonia.Controls.Shapes;
using Nodify.Interactivity;

namespace Nodify
{
    /// <summary>Available selection logic.</summary>
    public enum SelectionType
    {
        /// <summary>Replaces the old selection.</summary>
        Replace,
        /// <summary>Removes items from existing selection.</summary>
        Remove,
        /// <summary>Adds items to the current selection.</summary>
        Append,
        /// <summary>Inverts the selection.</summary>
        Invert
    }

    public partial class NodifyEditor : SelectingItemsControl
    {
        #region Dependency properties

        public static readonly StyledProperty<ICommand?> ItemsSelectStartedCommandProperty =
            AvaloniaProperty.Register<NodifyEditor, ICommand?>(nameof(ItemsSelectStartedCommand));

        public static readonly StyledProperty<ICommand?> ItemsSelectCompletedCommandProperty =
            AvaloniaProperty.Register<NodifyEditor, ICommand?>(nameof(ItemsSelectCompletedCommand));

        public static readonly StyledProperty<Style?> SelectionRectangleStyleProperty =
            AvaloniaProperty.Register<NodifyEditor, Style?>(nameof(SelectionRectangleStyle));

        private Rect _selectedArea;
        public static readonly DirectProperty<NodifyEditor, Rect> SelectedAreaProperty =
            AvaloniaProperty.RegisterDirect<NodifyEditor, Rect>(
                nameof(SelectedArea),
                o => o._selectedArea,
                (o, v) => o._selectedArea = v);

        private bool _isSelecting;
        public static readonly DirectProperty<NodifyEditor, bool> IsSelectingProperty =
            AvaloniaProperty.RegisterDirect<NodifyEditor, bool>(
                nameof(IsSelecting),
                o => o._isSelecting,
                (o, v) => o._isSelecting = v);

        public static readonly StyledProperty<bool> EnableRealtimeSelectionProperty =
            AvaloniaProperty.Register<NodifyEditor, bool>(nameof(EnableRealtimeSelection), false);

        public static readonly StyledProperty<bool> CanSelectMultipleConnectionsProperty =
            AvaloniaProperty.Register<NodifyEditor, bool>(nameof(CanSelectMultipleConnections), true);

        public static readonly StyledProperty<bool> CanSelectMultipleItemsProperty =
            AvaloniaProperty.Register<NodifyEditor, bool>(nameof(CanSelectMultipleItems), true);

        public static readonly StyledProperty<IList?> SelectedItemsProperty =
            AvaloniaProperty.Register<NodifyEditor, IList?>(nameof(SelectedItems));

        public static readonly StyledProperty<IList?> SelectedConnectionsProperty =
            AvaloniaProperty.Register<NodifyEditor, IList?>(nameof(SelectedConnections));

        public static readonly StyledProperty<object?> SelectedConnectionProperty =
            AvaloniaProperty.Register<NodifyEditor, object?>(nameof(SelectedConnection), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

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

        /// <summary>Invoked when a selection operation is started (see <see cref="BeginSelecting(SelectionType)"/>).</summary>
        public ICommand? ItemsSelectStartedCommand
        {
            get => (ICommand?)GetValue(ItemsSelectStartedCommandProperty);
            set => SetValue(ItemsSelectStartedCommandProperty, value);
        }

        /// <summary>Invoked when a selection operation is completed (see <see cref="EndSelecting"/>).</summary>
        public ICommand? ItemsSelectCompletedCommand
        {
            get => (ICommand?)GetValue(ItemsSelectCompletedCommandProperty);
            set => SetValue(ItemsSelectCompletedCommandProperty, value);
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

        private bool CanSelectMultipleItemsBase
        {
            get => base.CanSelectMultipleItems;
            set => base.CanSelectMultipleItems = value;
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
            get => (IList?)GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }

        /// <summary>
        /// Gets the currently selected area while <see cref="IsSelecting"/> is true.
        /// </summary>
        public Rect SelectedArea
        {
            get => (Rect)GetValue(SelectedAreaProperty);
            private set => SetValue(SelectedAreaPropertyKey, value);
        }

        /// <summary>
        /// Gets a value that indicates whether a selection operation is in progress.
        /// </summary>
        public bool IsSelecting
        {
            get => (bool)GetValue(IsSelectingProperty);
            private set => SetValue(IsSelectingPropertyKey, value);
        }

        /// <summary>
        /// Gets or sets the style to use for the selection rectangle.
        /// </summary>
        public Style SelectionRectangleStyle
        {
            get => (Style)GetValue(SelectionRectangleStyleProperty);
            set => SetValue(SelectionRectangleStyleProperty, value);
        }

        #endregion

        /// <summary>
        /// Gets a list of <see cref="ItemContainer"/>s that are selected (see <see cref="SelectedContainersCount"/>).
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
                    var container = (ItemContainer)ItemContainerGenerator.ContainerFromItem(selectedItems[i]);
                    selectedContainers.Add(container);
                }

                return selectedContainers;
            }
        }

        /// <summary>
        /// Gets the number of selected containers, without allocating (see <see cref="SelectedContainers"/>).
        /// </summary>
        public int SelectedContainersCount => base.SelectedItems.Count;

        /// <summary>
        /// Gets or sets whether cancelling a selection operation is allowed (see <see cref="EditorGestures.SelectionGestures.Cancel"/>).
        /// </summary>
        public static bool AllowSelectionCancellation { get; set; } = true;

        /// <summary>The selection helper.</summary>
        private readonly SelectionHelper _selection = new SelectionHelper();

        #region Selection

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
            IsSelecting = true;
            BeginUpdateSelectedItems();

            IList selected = base.SelectedItems;
            if (!append)
            {
                selected.Clear();
            }

            ItemCollection items = Items;
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
        /// Clears the current selection and selects the specified <see cref="ItemContainer"/> within the same selection transaction.
        /// </summary>
        /// <param name="container"></param>
        public void Select(ItemContainer container)
        {
            BeginUpdateSelectedItems();
            var selected = base.SelectedItems;
            selected.Clear();
            selected.Add(container.DataContext);
            EndUpdateSelectedItems();

            UnselectAllConnections();
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
                var container = (ItemContainer)ItemContainerGenerator.ContainerFromItem(items[i]);
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
        public void UnselectAllConnections()
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

        /// <summary>
        /// Initiates a selection operation from the current <see cref="MouseLocation"/>.
        /// </summary>
        /// <remarks>This method has no effect if a selection operation is already in progress.</remarks>
        /// <param name="type">The type of selection to perform. Defaults to <see cref="SelectionType.Replace"/>.</param>
        public void BeginSelecting(SelectionType type = SelectionType.Replace)
            => BeginSelecting(MouseLocation, type);

        /// <summary>
        /// Initiates a selection operation from the specified location.
        /// </summary>
        /// <remarks>This method has no effect if a selection operation is already in progress.</remarks>
        /// <param name="location">The starting point for the selection, in graph space coordinates.</param>
        /// <param name="type">The type of selection to perform. Defaults to <see cref="SelectionType.Replace"/>.</param>
        public void BeginSelecting(Point location, SelectionType type = SelectionType.Replace)
        {
            if (IsSelecting)
            {
                return;
            }

            SelectedArea = _selection.Start(ItemContainers, location, type, EnableRealtimeSelection);
            IsSelecting = true;
        }

        /// <summary>
        /// Expands or modifies the selection area by the specified amount.
        /// </summary>
        /// <param name="amount">Rrepresents the change to apply to the selection area.</param>
        public void UpdateSelection(Vector amount)
        {
            Debug.Assert(IsSelecting);
            SelectedArea = _selection.Update(amount);
        }

        /// <summary>
        /// Expands or modifies the selection area to the specified location.
        /// </summary>
        /// <param name="location">The point, in graph space coordinates, to extend or adjust the selection area to.</param>
        public void UpdateSelection(Point location)
        {
            Debug.Assert(IsSelecting);
            SelectedArea = _selection.Update(location);
        }

        /// <summary>
        /// Completes the selection operation and applies any pending changes.
        /// </summary>
        /// <remarks>This method has no effect if there's no selection operation in progress.</remarks>
        public void EndSelecting()
        {
            if (!IsSelecting)
            {
                return;
            }

            if (_selection.Type == SelectionType.Replace)
            {
                UnselectAllConnections();
            }

            SelectedArea = _selection.End();
            IsSelecting = false;
            ApplyPreviewingSelection();
        }

        /// <summary>
        /// Cancels the current selection operation and reverts any changes made during the selection process if <see cref="AllowSelectionCancellation"/> is true.
        /// Otherwise, it ends the selection operation by calling <see cref="EndSelecting"/>.
        /// </summary>
        /// <remarks>This method has no effect if there's no selection operation in progress.</remarks>
        public void CancelSelecting()
        {
            if (!AllowSelectionCancellation)
            {
                EndSelecting();
                return;
            }

            if (IsSelecting)
            {
                _selection.Cancel();
                IsSelecting = false;
            }
        }

        private void ApplyPreviewingSelection()
        {
            ItemCollection items = Items;
            IList selected = base.SelectedItems;

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
        }

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

        #endregion
    }
}
