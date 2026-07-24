using Avalonia;

namespace Nodify.Avalonia.Sample;

public sealed class NodeViewModel : ObservableObject
{
    private string _title = "Node";
    private Point _location;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public Point Location
    {
        get => _location;
        set => SetProperty(ref _location, value);
    }

    public List<string> Inputs { get; } = new();

    public List<string> Outputs { get; } = new();
}
