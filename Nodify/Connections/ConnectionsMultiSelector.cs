﻿using System;
using System.Collections;
using System.Collections.Specialized;

#if Avalonia
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Controls;
using Nodify.Avalonia.Extensions;
using MultiSelector = Avalonia.Controls.Primitives.SelectingItemsControl;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
#endif

namespace Nodify
{
    internal class ConnectionsMultiSelector : MultiSelector
    {
#if Avalonia
        public static readonly DirectProperty<ConnectionsMultiSelector, IList?> SelectedItemsProperty = NodifyEditor.SelectedItemsProperty.AddOwner<ConnectionsMultiSelector>(o => o.SelectedItems);
        public static readonly StyledProperty<bool> CanSelectMultipleItemsProperty = NodifyEditor.CanSelectMultipleItemsProperty.AddOwner<ConnectionsMultiSelector>();
#else
        public static readonly DependencyProperty SelectedItemsProperty = NodifyEditor.SelectedItemsProperty.AddOwner(typeof(ConnectionsMultiSelector), new FrameworkPropertyMetadata(default(IList), OnSelectedItemsSourceChanged));
        public static readonly DependencyProperty CanSelectMultipleItemsProperty = NodifyEditor.CanSelectMultipleItemsProperty.AddOwner(typeof(ConnectionsMultiSelector), new FrameworkPropertyMetadata(BoxValue.True, OnCanSelectMultipleItemsChanged, CoerceCanSelectMultipleItems));

        private static void OnCanSelectMultipleItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((ConnectionsMultiSelector)d).CanSelectMultipleItemsBase = (bool)e.NewValue;

        private static object CoerceCanSelectMultipleItems(DependencyObject d, object baseValue)
            => ((ConnectionsMultiSelector)d).CanSelectMultipleItemsBase = (bool)baseValue;

        private static void OnSelectedItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((ConnectionsMultiSelector)d).OnSelectedItemsSourceChanged((IList)e.OldValue, (IList)e.NewValue);
#endif

        /// <summary>
        /// Gets or sets the selected connections in the <see cref="NodifyEditor"/>.
        /// </summary>
        public new IList? SelectedItems
        {
            get => (IList?)GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }

        /// <summary>
        /// Gets or sets whether multiple connections can be selected.
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

        /// <summary>
        /// The <see cref="NodifyEditor"/> that owns this <see cref="ConnectionsMultiSelector"/>.
        /// </summary>
        public NodifyEditor Editor { get; private set; } = default!;

#if Avalonia
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
#else
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
#endif

            Editor = this.GetParentOfType<NodifyEditor>() ?? throw new NotSupportedException($"{nameof(ConnectionsMultiSelector)} cannot be used outside the {nameof(NodifyEditor)}");
        }

#if Avalonia
        protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        {
            return new ConnectionContainer(this);
        }
#else
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ConnectionContainer(this);
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
            => item is ConnectionContainer;
#endif

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
#if !Avalonia
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
#endif
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
    }
}
