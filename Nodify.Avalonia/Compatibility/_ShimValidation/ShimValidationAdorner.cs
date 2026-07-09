// -----------------------------------------------------------------------------
//  Phase 5 adorner-shim smoke test (NOT a port; validation only)
// -----------------------------------------------------------------------------
//  The real adorners (BaseConnection.FocusVisualAdorner, PendingConnection.HotKeyAdorner) are
//  private nested classes of DEFERRED controls and additionally need Pen / WPF geometry ops /
//  HotKeyControl, so they cannot be compiled in this phase. This tiny local adorner instead
//  exercises the reusable WPF Adorner/AdornerLayer shim in isolation, mirroring the surface the
//  nested adorners use:
//    * derives from the WPF `System.Windows.Documents.Adorner` (ctor takes the adorned element),
//    * sets `IsHitTestVisible`/`IsEnabled`/`IsClipEnabled` (as FocusVisualAdorner does),
//    * overrides the WPF-style `OnRender(DrawingContext)` and draws with Avalonia-native primitives,
//    * is added to / removed from a layer via `AdornerLayer.GetAdornerLayer` + `Add`/`Remove`/`Update`.
//
//  This proves the adorner primitive compiles and links against the shim. It can be removed once
//  the real adorners are ported in the control phase.
// -----------------------------------------------------------------------------

using System.Windows.Documents;
using Avalonia;
using Avalonia.Media;

namespace Nodify.Avalonia.Compatibility
{
    internal sealed class ShimValidationAdorner : Adorner
    {
        public ShimValidationAdorner(Visual adornedElement) : base(adornedElement)
        {
            IsHitTestVisible = false;
            IsEnabled = false;
            IsClipEnabled = true;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var pen = new Pen(Brushes.Red, 1d);
            drawingContext.DrawRectangle(null, pen, new Rect(Bounds.Size));
        }

        public static void ExerciseLayerApi(Visual anchor, ShimValidationAdorner adorner)
        {
            AdornerLayer? layer = AdornerLayer.GetAdornerLayer(anchor);
            if (layer is null)
            {
                return;
            }

            layer.Add(adorner);
            layer.Update(anchor);
            layer.Remove(adorner);
        }
    }
}
