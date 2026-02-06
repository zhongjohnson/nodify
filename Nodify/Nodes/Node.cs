using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Metadata;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Styling;
using Avalonia.Media;

namespace Nodify
{
    /// <summary>
    /// Represents a control that has a list of <see cref="Input"/> <see cref="Connector"/>s and a list of <see cref="Output"/> <see cref="Connector"/>s.
    /// </summary>
    public class Node : HeaderedContentControl
    {
        protected const string ElementInputItemsControl = "PART_Input";
        protected const string ElementOutputItemsControl = "PART_Output";

        #region Dependency Properties

        public static readonly StyledProperty<IBrush?> ContentBrushProperty = AvaloniaProperty.Register<Node, IBrush?>(nameof(ContentBrush));
        public static readonly StyledProperty<IBrush?> HeaderBrushProperty = AvaloniaProperty.Register<Node, IBrush?>(nameof(HeaderBrush));
        public static readonly StyledProperty<IBrush?> FooterBrushProperty = AvaloniaProperty.Register<Node, IBrush?>(nameof(FooterBrush));
        public static readonly StyledProperty<object?> FooterProperty = AvaloniaProperty.Register<Node, object?>(nameof(Footer), coerce: (o, v) => { OnFooterChanged((Node)o, v); return v; });
        public static readonly StyledProperty<IDataTemplate?> FooterTemplateProperty = AvaloniaProperty.Register<Node, IDataTemplate?>(nameof(FooterTemplate));
        public static readonly StyledProperty<IDataTemplate?> InputConnectorTemplateProperty = AvaloniaProperty.Register<Node, IDataTemplate?>(nameof(InputConnectorTemplate));

        private bool _hasFooter;
        public static readonly DirectProperty<Node, bool> HasFooterProperty = 
            AvaloniaProperty.RegisterDirect<Node, bool>(nameof(HasFooter), o => o._hasFooter, (o, v) => o._hasFooter = v);

        public static readonly StyledProperty<IDataTemplate?> OutputConnectorTemplateProperty = AvaloniaProperty.Register<Node, IDataTemplate?>(nameof(OutputConnectorTemplate));

        private static void OnFooterChanged(Node node, object? footer)
        {
            node._hasFooter = footer != null;
        }
        public static readonly StyledProperty<IEnumerable?> InputProperty = AvaloniaProperty.Register<Node, IEnumerable?>(nameof(Input));
        public static readonly StyledProperty<IEnumerable?> OutputProperty = AvaloniaProperty.Register<Node, IEnumerable?>(nameof(Output));
        public static readonly StyledProperty<Style?> ContentContainerStyleProperty = AvaloniaProperty.Register<Node, Style?>(nameof(ContentContainerStyle));
        public static readonly StyledProperty<Style?> HeaderContainerStyleProperty = AvaloniaProperty.Register<Node, Style?>(nameof(HeaderContainerStyle));
        public static readonly StyledProperty<Style?> FooterContainerStyleProperty = AvaloniaProperty.Register<Node, Style?>(nameof(FooterContainerStyle));

        /// <summary>
        /// Gets or sets the brush used for the background of the <see cref="ContentControl.Content"/> of this <see cref="Node"/>.
        /// </summary>
        public Brush ContentBrush
        {
            get => (Brush)GetValue(ContentBrushProperty);
            set => SetValue(ContentBrushProperty, value);
        }

        /// <summary>
        /// Gets or sets the brush used for the background of the <see cref="HeaderedContentControl.Header"/> of this <see cref="Node"/>.
        /// </summary>
        public Brush HeaderBrush
        {
            get => (Brush)GetValue(HeaderBrushProperty);
            set => SetValue(HeaderBrushProperty, value);
        }

        /// <summary>
        /// Gets or sets the brush used for the background of the <see cref="Node.Footer"/> of this <see cref="Node"/>.
        /// </summary>
        public Brush FooterBrush
        {
            get => (Brush)GetValue(FooterBrushProperty);
            set => SetValue(FooterBrushProperty, value);
        }

        /// <summary>
        /// Gets or sets the data for the footer of this control.
        /// </summary>
        public object Footer
        {
            get => GetValue(FooterProperty);
            set => SetValue(FooterProperty, value);
        }

        /// <summary>
        /// Gets or sets the template used to display the content of the control's footer.
        /// </summary>
        public IDataTemplate? FooterTemplate
        {
            get => GetValue(FooterTemplateProperty);
            set => SetValue(FooterTemplateProperty, value);
        }

        /// <summary>
        /// Gets or sets the template used to display the content of the control's <see cref="Input"/> connectors.
        /// </summary>
        public IDataTemplate? InputConnectorTemplate
        {
            get => GetValue(InputConnectorTemplateProperty);
            set => SetValue(InputConnectorTemplateProperty, value);
        }

        /// <summary>
        /// Gets or sets the template used to display the content of the control's <see cref="Output"/> connectors.
        /// </summary>
        public IDataTemplate? OutputConnectorTemplate
        {
            get => GetValue(OutputConnectorTemplateProperty);
            set => SetValue(OutputConnectorTemplateProperty, value);
        }

        /// <summary>
        /// Gets or sets the data for the input <see cref="Connector"/>s of this control.
        /// </summary>
        public IEnumerable Input
        {
            get => (IEnumerable)GetValue(InputProperty);
            set => SetValue(InputProperty, value);
        }

        /// <summary>
        /// Gets or sets the data for the output <see cref="Connector"/>s of this control.
        /// </summary>
        public IEnumerable Output
        {
            get => (IEnumerable)GetValue(OutputProperty);
            set => SetValue(OutputProperty, value);
        }

        /// <summary>
        /// Gets or sets the style for the content container.
        /// </summary>
        public Style ContentContainerStyle
        {
            get => (Style)GetValue(ContentContainerStyleProperty);
            set => SetValue(ContentContainerStyleProperty, value);
        }

        /// <summary>
        /// Gets or sets the style for the header container.
        /// </summary>
        public Style HeaderContainerStyle
        {
            get => (Style)GetValue(HeaderContainerStyleProperty);
            set => SetValue(HeaderContainerStyleProperty, value);
        }

        /// <summary>
        /// Gets or sets the style for the footer container.
        /// </summary>
        public Style FooterContainerStyle
        {
            get => (Style)GetValue(FooterContainerStyleProperty);
            set => SetValue(FooterContainerStyleProperty, value);
        }

        /// <summary>
        /// Gets a value that indicates whether the <see cref="Footer"/> is <see langword="null" />.
        /// </summary>
        public bool HasFooter => _hasFooter;

        private static void OnFooterChanged(Node node, AvaloniaPropertyChangedEventArgs<object?> e)
        {
            node._hasFooter = e.NewValue.Value != null;
        }

        #endregion

        // TODO: GroupStyle is WPF-specific and doesn't exist in Avalonia
        // Need to implement custom grouping logic or remove this feature
        // public ObservableCollection<GroupStyle> InputGroupStyle { get; } = new ObservableCollection<GroupStyle>();
        // public ObservableCollection<GroupStyle> OutputGroupStyle { get; } = new ObservableCollection<GroupStyle>();

        protected ItemsControl? InputItemsControl { get; private set; }
        protected ItemsControl? OutputItemsControl { get; private set; }

        // TODO: Avalonia doesn't have DefaultStyleKeyProperty - handled by theme system differently
        // FocusableProperty override not needed in same way
        static Node()
        {
            FocusableProperty.OverrideDefaultValue<Node>(false);
        }

        public Node()
        {
            // TODO: GroupStyle is WPF-specific - commented out for Avalonia
            // InputGroupStyle.CollectionChanged += OnInputGroupStyleCollectionChanged;
            // OutputGroupStyle.CollectionChanged += OnOutputGroupStyleCollectionChanged;
            TemplateApplied += OnNodeTemplateApplied;
        }

        private void OnNodeTemplateApplied(object? sender, TemplateAppliedEventArgs e)
        {
            InputItemsControl = e.NameScope.Find<ItemsControl>(ElementInputItemsControl);
            OutputItemsControl = e.NameScope.Find<ItemsControl>(ElementOutputItemsControl);

            // TODO: GroupStyle is WPF-specific - commented out for Avalonia
            /*
            if (InputItemsControl != null)
            {
                foreach (var style in InputGroupStyle)
                {
                    InputItemsControl.GroupStyle.Add(style);
                }
            }

            if (OutputItemsControl != null)
            {
                foreach (var style in OutputGroupStyle)
                {
                    OutputItemsControl.GroupStyle.Add(style);
                }
            }
            */
        }

        // TODO: GroupStyle is WPF-specific - commented out for Avalonia
        /*
        private void OnInputGroupStyleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (InputItemsControl != null)
            {
                SynchronizeCollection(InputItemsControl.GroupStyle, e);
            }
        }

        private void OnOutputGroupStyleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (OutputItemsControl != null)
            {
                SynchronizeCollection(OutputItemsControl.GroupStyle, e);
            }
        }

        private static void SynchronizeCollection(ObservableCollection<GroupStyle> collection, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null)
                    {
                        for (int i = 0; i < e.NewItems.Count; i++)
                        {
                            var item = (GroupStyle)e.NewItems[i]!;
                            collection.Add(item);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null)
                    {
                        for (int i = 0; i < e.OldItems.Count; i++)
                        {
                            var item = (GroupStyle)e.OldItems[i]!;
                            collection.Remove(item);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    collection[e.NewStartingIndex] = (GroupStyle)e.NewItems![0]!;
                    break;
                case NotifyCollectionChangedAction.Move:
                    collection.Move(e.OldStartingIndex, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    collection.Clear();
                    break;
            }
        }
        */
    }
}
