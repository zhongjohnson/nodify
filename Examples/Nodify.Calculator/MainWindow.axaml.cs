using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Nodify.Interactivity;

namespace Nodify.Calculator
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            EditorGestures.Mappings.Editor.Cutting.Unbind();
        }

        private void InitializeComponent()
            => AvaloniaXamlLoader.Load(this);
    }
}
