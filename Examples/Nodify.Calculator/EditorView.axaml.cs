using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System.Windows.Input;
using Nodify.Interactivity;

namespace Nodify.Calculator
{
    public class OperationsMenuHandler : InputElementState<NodifyEditor>
    {
        private static InputGesture OpenGesture { get; } = new Interactivity.MouseGesture(MouseAction.RightClick);
        private static InputGesture CloseGesture { get; } = new Interactivity.MouseGesture(MouseAction.LeftClick);

        private OperationsMenuViewModel ViewModel => ((CalculatorViewModel)Element.DataContext).OperationsMenu;

        public OperationsMenuHandler(NodifyEditor element) : base(element)
        {
            ProcessHandledEvents = true;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (!e.Handled && OpenGesture.Matches(e.Source, e))
            {
                ViewModel.OpenAt(Element.MouseLocation);
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (CloseGesture.Matches(e.Source, e))
            {
                ViewModel.Close();
            }
        }
    }

    public partial class EditorView : UserControl
    {
        public EditorView()
        {
            InitializeComponent();
        }

        static EditorView()
        {
            InputProcessor.Shared<NodifyEditor>.RegisterHandlerFactory(editor => new OperationsMenuHandler(editor));
        }

        private void OnDropNode(object sender, DragEventArgs e)
        {
            if (e.Source is NodifyEditor editor && editor.DataContext is CalculatorViewModel calculator
                && e.Data.Contains(OperationInfoFormat)
                && e.Data.Get(OperationInfoFormat) is OperationInfoViewModel operation)
            {
                OperationViewModel op = OperationFactory.GetOperation(operation);
                op.Location = editor.GetLocationInsideEditor(e);
                calculator.Operations.Add(op);

                e.Handled = true;
            }
        }

        private async void OnNodeDrag(object sender, PointerEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && sender is Control control && control.DataContext is OperationInfoViewModel operation)
            {
                var data = new DataObject();
                data.Set(OperationInfoFormat, operation);
                await DragDrop.DoDragDrop(e, data, DragDropEffects.Copy);
            }
        }

        private void InitializeComponent()
            => AvaloniaXamlLoader.Load(this);

        private const string OperationInfoFormat = "OperationInfoViewModel";
    }
}
