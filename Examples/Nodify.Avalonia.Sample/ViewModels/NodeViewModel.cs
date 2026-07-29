// NOTE: Location uses System.Windows.Point -- the type ItemContainer.Location exposes. Using
// Avalonia.Point here makes comparisons against the container ambiguous, because the
// compatibility layer defines implicit conversions in both directions.
using Point = System.Windows.Point;

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
