using System.Collections.ObjectModel;
using Avalonia;

namespace Nodify.Avalonia.Sample;

public sealed class EditorViewModel
{
    public EditorViewModel()
    {
        Nodes = new ObservableCollection<NodeViewModel>
        {
            new()
            {
                Title = "Input",
                Location = new Point(120, 120),
                Outputs = { "Value" }
            },
            new()
            {
                Title = "Transform",
                Location = new Point(380, 220),
                Inputs = { "A", "B" },
                Outputs = { "Result" }
            },
            new()
            {
                Title = "Output",
                Location = new Point(660, 160),
                Inputs = { "In" }
            }
        };
    }

    public ObservableCollection<NodeViewModel> Nodes { get; }
}
