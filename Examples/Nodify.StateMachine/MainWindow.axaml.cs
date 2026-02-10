using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Nodify.Interactivity;
using System.Windows.Input;

namespace Nodify.StateMachine
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ConnectorState.EnableToggledConnectingMode = true;
            NodifyEditor.EnableCuttingLinePreview = true;

            EditorGestures.Mappings.Connection.Disconnect.Unbind();
            EditorGestures.Mappings.Editor.ZoomModifierKey = ModifierKeys.Control;
            EditorGestures.Mappings.Editor.PanWithMouseWheel = true;
        }

        private void ScrollViewer_PreviewKeyDown(object sender, Avalonia.Input.KeyEventArgs e)
        {
            if (!e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                return;

            var scrollViewer = (ScrollViewer)sender;

            if (e.Key == Key.PageUp)
            {
                scrollViewer.Offset = new Avalonia.Vector(scrollViewer.Offset.X - scrollViewer.Viewport.Width, scrollViewer.Offset.Y);
                e.Handled = true;
            }
            else if (e.Key == Key.PageDown)
            {
                scrollViewer.Offset = new Avalonia.Vector(scrollViewer.Offset.X + scrollViewer.Viewport.Width, scrollViewer.Offset.Y);
                e.Handled = true;
            }
        }

        private void InitializeComponent()
            => AvaloniaXamlLoader.Load(this);
    }
}
