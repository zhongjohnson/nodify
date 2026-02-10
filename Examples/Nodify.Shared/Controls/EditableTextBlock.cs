using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using Nodify.Shared;

namespace Nodify
{
    public class EditableTextBlock : TemplatedControl
    {
        private const string ElementTextBox = "PART_TextBox";

        public static readonly StyledProperty<bool> IsEditingProperty = AvaloniaProperty.Register<EditableTextBlock, bool>(nameof(IsEditing), defaultBindingMode: BindingMode.TwoWay);
        public static readonly StyledProperty<bool> IsEditableProperty = AvaloniaProperty.Register<EditableTextBlock, bool>(nameof(IsEditable), defaultValue: true);
        public static readonly StyledProperty<string?> TextProperty = AvaloniaProperty.Register<EditableTextBlock, string?>(nameof(Text), defaultBindingMode: BindingMode.TwoWay);
        public static readonly StyledProperty<bool> AcceptsReturnProperty = AvaloniaProperty.Register<EditableTextBlock, bool>(nameof(AcceptsReturn));
        public static readonly StyledProperty<TextWrapping> TextWrappingProperty = AvaloniaProperty.Register<EditableTextBlock, TextWrapping>(nameof(TextWrapping), TextWrapping.Wrap);
        public static readonly StyledProperty<TextTrimming> TextTrimmingProperty = AvaloniaProperty.Register<EditableTextBlock, TextTrimming>(nameof(TextTrimming), TextTrimming.CharacterEllipsis);
        public static readonly StyledProperty<int> MinLinesProperty = AvaloniaProperty.Register<EditableTextBlock, int>(nameof(MinLines));
        public static readonly StyledProperty<int> MaxLinesProperty = AvaloniaProperty.Register<EditableTextBlock, int>(nameof(MaxLines));
        public static readonly StyledProperty<int> MaxLengthProperty = AvaloniaProperty.Register<EditableTextBlock, int>(nameof(MaxLength));

        public string? Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public bool IsEditing
        {
            get => GetValue(IsEditingProperty);
            set => SetValue(IsEditingProperty, value);
        }

        public bool IsEditable
        {
            get => GetValue(IsEditableProperty);
            set => SetValue(IsEditableProperty, value);
        }

        public bool AcceptsReturn
        {
            get => GetValue(AcceptsReturnProperty);
            set => SetValue(AcceptsReturnProperty, value);
        }

        public int MaxLength
        {
            get => GetValue(MaxLengthProperty);
            set => SetValue(MaxLengthProperty, value);
        }

        public int MinLines
        {
            get => GetValue(MinLinesProperty);
            set => SetValue(MaxLinesProperty, value);
        }

        public int MaxLines
        {
            get => GetValue(MaxLinesProperty);
            set => SetValue(MaxLinesProperty, value);
        }

        public TextWrapping TextWrapping
        {
            get => GetValue(TextWrappingProperty);
            set => SetValue(TextWrappingProperty, value);
        }

        public TextTrimming TextTrimming
        {
            get => GetValue(TextTrimmingProperty);
            set => SetValue(TextTrimmingProperty, value);
        }

        protected TextBox? TextBox { get; private set; }

        static EditableTextBlock()
        {
            FocusableProperty.OverrideDefaultValue<EditableTextBlock>(true);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            if (TextBox != null)
            {
                TextBox.LostFocus -= OnLostFocus;
                TextBox.PropertyChanged -= OnTextBoxPropertyChanged;
            }

            TextBox = e.NameScope.Find<TextBox>(ElementTextBox);

            if (TextBox != null)
            {
                TextBox.LostFocus += OnLostFocus;
                TextBox.PropertyChanged += OnTextBoxPropertyChanged;

                if (IsEditing)
                {
                    TextBox.Focus();
                    TextBox.SelectAll();
                }
            }
        }

        private void OnTextBoxPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == Visual.IsVisibleProperty && IsEditing && TextBox != null)
            {
                if (TextBox.Focus())
                {
                    TextBox.SelectAll();
                }
                else
                {
                    IsEditing = false;
                }
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (IsEditing)
            {
                e.Handled = true;
            }
            else if (IsEditable && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && e.ClickCount == 2)
            {
                IsEditing = true;
                e.Handled = true;
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            if (IsEditing)
            {
                e.Handled = true;
            }
        }

        private void OnLostFocus(object? sender, RoutedEventArgs e)
        {
            IsEditing = false;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (IsEditing && e.Key == Key.Escape || !AcceptsReturn && e.Key == Key.Enter)
            {
                IsEditing = false;
            }

            if (e.Key == Key.Enter && IsFocused && !IsEditing)
            {
                IsEditing = true;
            }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == IsEditableProperty && !(bool)change.NewValue!)
            {
                IsEditing = false;
            }
        }
    }
}
