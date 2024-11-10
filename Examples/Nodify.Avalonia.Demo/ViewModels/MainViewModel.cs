using Avalonia;
using ReactiveUI;

namespace Nodify.Avalonia.Demo.ViewModels;

public class MainViewModel : ViewModelBase
{
    public string Greeting => "Welcome to Avalonia!";

    public MainViewModel()
    {
        for (int i = 0; i < 10; i++)
        {
            this.Nodes.Add(new FlowNodeViewModel() { Title = $"test_{i}", Location = new Point(10 * i, 10 * i) });
        }
    }

    private NodifyObservableCollection<NodeViewModel> _nodes = new NodifyObservableCollection<NodeViewModel>();
    public NodifyObservableCollection<NodeViewModel> Nodes
    {
        get => _nodes;
        set => this.RaiseAndSetIfChanged(ref _nodes, value);
    }
}
