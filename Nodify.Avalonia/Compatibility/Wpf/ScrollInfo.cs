// -----------------------------------------------------------------------------
//  WPF IScrollInfo / ScrollViewer shim (System.Windows.Controls.Primitives / .Controls)
// -----------------------------------------------------------------------------
//  NodifyEditor.Scrolling.cs implements WPF's IScrollInfo so a hosting WPF ScrollViewer can drive
//  the editor viewport. Avalonia's scrolling contract (ILogicalScrollable) differs in shape, and the
//  editor is not currently hosted in an Avalonia ScrollViewer, so this provides a WPF-shaped
//  IScrollInfo definition plus a minimal ScrollViewer placeholder purely so the linked upstream
//  scrolling partial compiles and its viewport math stays intact. Wiring the editor into Avalonia's
//  scroll infrastructure is deferred to the theme/behavior phase.
// -----------------------------------------------------------------------------

using Avalonia;
using Avalonia.Media;

namespace System.Windows.Controls.Primitives
{
    /// <summary>
    /// WPF-compatible <see cref="IScrollInfo"/>. Mirrors the members implemented by
    /// <c>NodifyEditor.Scrolling.cs</c> so the linked upstream source compiles unchanged.
    /// </summary>
    public interface IScrollInfo
    {
        /// <summary>Gets or sets whether horizontal scrolling is possible.</summary>
        bool CanHorizontallyScroll { get; set; }

        /// <summary>Gets or sets whether vertical scrolling is possible.</summary>
        bool CanVerticallyScroll { get; set; }

        /// <summary>Gets the horizontal size of the extent.</summary>
        double ExtentWidth { get; }

        /// <summary>Gets the vertical size of the extent.</summary>
        double ExtentHeight { get; }

        /// <summary>Gets the horizontal offset of the scrolled content.</summary>
        double HorizontalOffset { get; }

        /// <summary>Gets the vertical offset of the scrolled content.</summary>
        double VerticalOffset { get; }

        /// <summary>Gets the horizontal size of the viewport.</summary>
        double ViewportWidth { get; }

        /// <summary>Gets the vertical size of the viewport.</summary>
        double ViewportHeight { get; }

        /// <summary>Gets or sets the owning <see cref="System.Windows.Controls.ScrollViewer"/>.</summary>
        global::System.Windows.Controls.ScrollViewer? ScrollOwner { get; set; }

        /// <summary>Scrolls up one logical line.</summary>
        void LineUp();

        /// <summary>Scrolls down one logical line.</summary>
        void LineDown();

        /// <summary>Scrolls left one logical line.</summary>
        void LineLeft();

        /// <summary>Scrolls right one logical line.</summary>
        void LineRight();

        /// <summary>Scrolls up in response to a mouse wheel.</summary>
        void MouseWheelUp();

        /// <summary>Scrolls down in response to a mouse wheel.</summary>
        void MouseWheelDown();

        /// <summary>Scrolls left in response to a mouse wheel.</summary>
        void MouseWheelLeft();

        /// <summary>Scrolls right in response to a mouse wheel.</summary>
        void MouseWheelRight();

        /// <summary>Scrolls up one page.</summary>
        void PageUp();

        /// <summary>Scrolls down one page.</summary>
        void PageDown();

        /// <summary>Scrolls left one page.</summary>
        void PageLeft();

        /// <summary>Scrolls right one page.</summary>
        void PageRight();

        /// <summary>Brings the given rectangle of a visual into view.</summary>
        Rect MakeVisible(Visual visual, Rect rectangle);

        /// <summary>Sets the horizontal offset.</summary>
        void SetHorizontalOffset(double offset);

        /// <summary>Sets the vertical offset.</summary>
        void SetVerticalOffset(double offset);
    }
}

namespace System.Windows.Controls
{
    /// <summary>
    /// Minimal WPF-compatible <see cref="ScrollViewer"/> placeholder. Referenced only as the
    /// <c>IScrollInfo.ScrollOwner</c> type; hosting the editor in an Avalonia scroll container is
    /// deferred to the theme/behavior phase.
    /// </summary>
    public class ScrollViewer
    {
        /// <summary>Requests that the owner recompute its scroll information (no-op placeholder).</summary>
        public void InvalidateScrollInfo()
        {
        }
    }
}
