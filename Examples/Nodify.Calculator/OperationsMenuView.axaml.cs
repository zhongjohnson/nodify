using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace Nodify.Calculator
{
    public partial class OperationsMenuView : UserControl
    {
        private readonly WeakReference<IInputElement?> _focusToRestore = new WeakReference<IInputElement?>(null!);

        public OperationsMenuView()
        {
            InitializeComponent();

            PropertyChanged += OnPropertyChanged;
        }

        private void OnIsVisibleChanged(bool isVisible)
        {
            if (!IsLoaded)
            {
                return;
            }

            if (isVisible)
            {
                _focusToRestore.SetTarget(TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement());
                Dispatcher.UIThread.Post(() => OperationsList.Focus());
            }
            else
            {
                if (_focusToRestore.TryGetTarget(out var elementToFocus))
                {
                    Dispatcher.UIThread.Post(() => elementToFocus?.Focus());
                }
            }
        }

        private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == IsVisibleProperty && e.NewValue is bool isVisible)
            {
                OnIsVisibleChanged(isVisible);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                IsVisible = false;
            }
        }

        private void InitializeComponent()
            => AvaloniaXamlLoader.Load(this);
    }
}
