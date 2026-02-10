using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Nodify.Shared;

namespace Nodify
{
    public partial class Swatches : Control
    {
        public static readonly StyledProperty<Color> SelectedColorProperty
            = AvaloniaProperty.Register<Swatches, Color>(nameof(SelectedColor), defaultValue: default, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        public static readonly StyledProperty<IEnumerable<Color>> ColorsProperty
            = AvaloniaProperty.Register<Swatches, IEnumerable<Color>>(nameof(Colors), Array.Empty<Color>());

        public Color SelectedColor
        {
            get => GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        public IEnumerable<Color> Colors
        {
            get => GetValue(ColorsProperty);
            set => SetValue(ColorsProperty, value);
        }

        static Swatches()
        {
            FocusableProperty.OverrideDefaultValue<Swatches>(true);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            var color = e.Source is Control control && control.DataContext is Color c ? c : (Color?)null;
            if (color.HasValue)
            {
                SelectedColor = color.Value;
            }

            e.Handled = true;
        }
    }
}
