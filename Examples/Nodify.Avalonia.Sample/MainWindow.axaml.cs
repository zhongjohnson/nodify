using System.Collections.Generic;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Nodify;

namespace Nodify.Avalonia.Sample;

public sealed partial class MainWindow : Window
{
    private readonly EditorViewModel _viewModel = new();
    private readonly Dictionary<NodeViewModel, ItemContainer> _containersByNode = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;

        // The ported controls expose their properties through the WPF-compatibility
        // shim, which the Avalonia XAML compiler cannot bind to. Instead we drive the
        // editor from data at runtime: feed it the view models and configure each
        // generated ItemContainer from code using its public CLR properties.
        Editor.ItemsSource = _viewModel.Nodes;
        Editor.ContainerPrepared += OnContainerPrepared;
        Editor.ContainerClearing += OnContainerClearing;
        Closed += OnClosed;
    }

    private void OnContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        if (e.Container is not ItemContainer container || container.DataContext is not NodeViewModel node)
        {
            return;
        }

        _containersByNode[node] = container;

        container.Location = node.Location;
        container.LocationChanged -= OnContainerLocationChanged;
        container.LocationChanged += OnContainerLocationChanged;
        container.Content = BuildNodeContent(node);

        node.PropertyChanged -= OnNodePropertyChanged;
        node.PropertyChanged += OnNodePropertyChanged;
    }

    private void OnContainerClearing(object? sender, ContainerClearingEventArgs e)
    {
        if (e.Container is not ItemContainer container || container.DataContext is not NodeViewModel node)
        {
            return;
        }

        container.LocationChanged -= OnContainerLocationChanged;
        node.PropertyChanged -= OnNodePropertyChanged;
        _containersByNode.Remove(node);
    }

    private void OnNodePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not NodeViewModel node || e.PropertyName != nameof(NodeViewModel.Location))
        {
            return;
        }

        if (_containersByNode.TryGetValue(node, out var container) && container.Location != node.Location)
        {
            container.Location = node.Location;
        }
    }

    private static void OnContainerLocationChanged(object? sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is not ItemContainer container || container.DataContext is not NodeViewModel node)
        {
            return;
        }

        if (node.Location != container.Location)
        {
            node.Location = container.Location;
        }
    }

    private void OnClosed(object? sender, System.EventArgs e)
    {
        Editor.ContainerPrepared -= OnContainerPrepared;
        Editor.ContainerClearing -= OnContainerClearing;

        foreach (var (node, container) in _containersByNode)
        {
            container.LocationChanged -= OnContainerLocationChanged;
            node.PropertyChanged -= OnNodePropertyChanged;
        }

        _containersByNode.Clear();
        Closed -= OnClosed;
    }

    private static Control BuildNodeContent(NodeViewModel node)
    {
        var layout = new StackPanel { Spacing = 4 };

        layout.Children.Add(new TextBlock
        {
            Text = node.Title,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White
        });

        foreach (var input in node.Inputs)
        {
            layout.Children.Add(new TextBlock
            {
                Text = "< " + input,
                HorizontalAlignment = HorizontalAlignment.Left,
                Foreground = Brushes.LightGray
            });
        }

        foreach (var output in node.Outputs)
        {
            layout.Children.Add(new TextBlock
            {
                Text = output + " >",
                HorizontalAlignment = HorizontalAlignment.Right,
                Foreground = Brushes.LightGray
            });
        }

        return new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(0x2D, 0x2D, 0x30)),
            BorderBrush = Brushes.DimGray,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(10, 6),
            MinWidth = 120,
            Child = layout
        };
    }
}
