using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia;
using Avalonia.Media;
using AvFormattedText = Avalonia.Media.FormattedText;

namespace System.Windows.Media
{
    /// <summary>WPF-compatible bitmap-cache configuration carrier.</summary>
    public sealed class BitmapCache
    {
        /// <summary>Initializes a cache with the requested render scale.</summary>
        public BitmapCache(double renderAtScale)
        {
            RenderAtScale = renderAtScale;
        }

        /// <summary>Gets the requested cache render scale.</summary>
        public double RenderAtScale { get; }
    }

    /// <summary>WPF-compatible collection of double values.</summary>
    public class DoubleCollection : Collection<double>
    {
    }

    /// <summary>WPF-compatible formatted text wrapper over Avalonia formatted text.</summary>
    public sealed class FormattedText
    {
        internal AvFormattedText Inner { get; }

        /// <summary>Initializes formatted text. Avalonia derives DPI from the render target.</summary>
        public FormattedText(
            string text,
            CultureInfo culture,
            FlowDirection flowDirection,
            Typeface typeface,
            double emSize,
            IBrush? foreground,
            double pixelsPerDip)
        {
            Inner = new AvFormattedText(text, culture, flowDirection, typeface, emSize, foreground);
        }

        /// <summary>Gets the text width.</summary>
        public double Width => Inner.Width;

        /// <summary>Gets the text height.</summary>
        public double Height => Inner.Height;
    }

    /// <summary>WPF media compatibility helpers.</summary>
    public static class MediaCompatibilityExtensions
    {
        /// <summary>WPF Freezable compatibility no-op for Avalonia pens.</summary>
        public static void Freeze(this Pen pen)
        {
        }

        /// <summary>Draws WPF-compatible formatted text.</summary>
        public static void DrawText(this DrawingContext context, FormattedText text, Point origin)
            => context.DrawText(text.Inner, origin);

        /// <summary>Draws a rounded rectangle using WPF's argument order.</summary>
        public static void DrawRoundedRectangle(
            this DrawingContext context,
            IBrush? brush,
            IPen? pen,
            Rect rectangle,
            double radiusX,
            double radiusY)
            => context.DrawRectangle(brush, pen, new RoundedRect(rectangle, radiusX, radiusY));
    }
}

namespace System.Windows
{
    /// <summary>WPF-compatible system color resources.</summary>
    public static class SystemColors
    {
        /// <summary>Gets the default control text brush.</summary>
        public static Avalonia.Media.IBrush ControlTextBrush { get; } = Avalonia.Media.Brushes.Black;
    }
}
