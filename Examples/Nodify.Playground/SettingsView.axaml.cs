using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;

namespace Nodify.Playground
{
    public partial class SettingsView : UserControl
    {
        public static readonly StyledProperty<IEnumerable<ISettingViewModel>> ItemsProperty =
            AvaloniaProperty.Register<SettingsView, IEnumerable<ISettingViewModel>>(nameof(Items));

        public IEnumerable<ISettingViewModel> Items
        {
            get => GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        public SettingsView()
        {
            InitializeComponent();
        }
    }
}
