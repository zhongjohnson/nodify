using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System.Windows.Input;

namespace Nodify
{
    public class TabControlEx : TabControl
    {
        private const string ElementScrollViewer = "PART_ScrollViewer";

        public static readonly StyledProperty<ICommand?> AddTabCommandProperty = AvaloniaProperty.Register<TabControlEx, ICommand?>(nameof(AddTabCommand));
        public static readonly StyledProperty<bool> AutoScrollToEndProperty = AvaloniaProperty.Register<TabControlEx, bool>(nameof(AutoScrollToEnd));

        public ICommand? AddTabCommand
        {
            get => GetValue(AddTabCommandProperty);
            set => SetValue(AddTabCommandProperty, value);
        }
        public bool AutoScrollToEnd
        {
            get => GetValue(AutoScrollToEndProperty);
            set => SetValue(AutoScrollToEndProperty, value);
        }

        protected ScrollViewer? ScrollViewer { get; private set; }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            ScrollViewer = e.NameScope.Find<ScrollViewer>(ElementScrollViewer);
            if (ScrollViewer != null)
            {
                ScrollViewer.ScrollChanged += OnScrollChanged;
            }
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentDelta.X > 0 && ScrollViewer != null && AutoScrollToEnd && ScrollViewer.Viewport.Width < ScrollViewer.Extent.Width)
            {
                ScrollViewer.Offset = new Vector(ScrollViewer.Extent.Width, ScrollViewer.Offset.Y);
            }
        }

        protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
            => new TabItemEx();
    }
}
