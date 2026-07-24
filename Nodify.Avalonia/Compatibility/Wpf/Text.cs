using System.Windows;

namespace System.Windows.Controls
{
    /// <summary>WPF-compatible text block dependency-property owner.</summary>
    public class TextBlock : Control
    {
        /// <summary>Foreground brush dependency property.</summary>
        public static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register("Foreground", typeof(Avalonia.Media.IBrush), typeof(TextBlock), new FrameworkPropertyMetadata(null));

        /// <summary>Text dependency property.</summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TextBlock), new FrameworkPropertyMetadata(string.Empty));
    }
}

namespace System.Windows.Controls.Primitives
{
    /// <summary>WPF-compatible text box base dependency-property owner.</summary>
    public static class TextBoxBase
    {
        /// <summary>Read-only state dependency property.</summary>
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.RegisterAttached("IsReadOnly", typeof(bool), typeof(TextBoxBase), new FrameworkPropertyMetadata(false));
    }
}

namespace System.Windows.Documents
{
    /// <summary>WPF-compatible text element dependency-property owner.</summary>
    public static class TextElement
    {
        /// <summary>Font size dependency property.</summary>
        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.RegisterAttached("FontSize", typeof(double), typeof(TextElement), new FrameworkPropertyMetadata(12d));

        /// <summary>Font family dependency property.</summary>
        public static readonly DependencyProperty FontFamilyProperty =
            DependencyProperty.RegisterAttached("FontFamily", typeof(Avalonia.Media.FontFamily), typeof(TextElement), new FrameworkPropertyMetadata(Avalonia.Media.FontFamily.Default));

        /// <summary>Font weight dependency property.</summary>
        public static readonly DependencyProperty FontWeightProperty =
            DependencyProperty.RegisterAttached("FontWeight", typeof(Avalonia.Media.FontWeight), typeof(TextElement), new FrameworkPropertyMetadata(Avalonia.Media.FontWeight.Normal));

        /// <summary>Font style dependency property.</summary>
        public static readonly DependencyProperty FontStyleProperty =
            DependencyProperty.RegisterAttached("FontStyle", typeof(Avalonia.Media.FontStyle), typeof(TextElement), new FrameworkPropertyMetadata(Avalonia.Media.FontStyle.Normal));

        /// <summary>Font stretch dependency property.</summary>
        public static readonly DependencyProperty FontStretchProperty =
            DependencyProperty.RegisterAttached("FontStretch", typeof(Avalonia.Media.FontStretch), typeof(TextElement), new FrameworkPropertyMetadata(Avalonia.Media.FontStretch.Normal));
    }
}
