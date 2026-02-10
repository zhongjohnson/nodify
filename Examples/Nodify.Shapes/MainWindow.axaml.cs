using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Nodify.Shapes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            PendingConnection.HotKeysDisplayMode = HotKeysDisplayMode.All;
        }

        private void InitializeComponent()
            => AvaloniaXamlLoader.Load(this);
    }
}
