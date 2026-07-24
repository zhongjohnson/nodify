using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Nodify;

namespace Nodify.Avalonia.Sample;

public sealed partial class MainWindow : Window
{
    private readonly EditorViewModel _viewModel = new();

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
    }

    private static void OnContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        if (e.Container is not ItemContainer container || container.DataContext is not NodeViewModel node)
        {
            return;
        }

        container.Location = node.Location;
        container.Content = BuildNodeContent(node);
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
