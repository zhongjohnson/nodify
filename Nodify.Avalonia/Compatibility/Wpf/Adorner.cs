// -----------------------------------------------------------------------------
//  WPF Adorner / AdornerLayer shim (System.Windows.Documents)
// -----------------------------------------------------------------------------
//  Upstream Nodify hosts a couple of *private nested* adorners:
//    * BaseConnection.FocusVisualAdorner : Adorner  (draws a focus outline in OnRender)
//    * PendingConnection.HotKeyAdorner   : Adorner  (hosts a HotKeyControl visual child)
//  and discovers/manages them through WPF's `System.Windows.Documents.AdornerLayer`:
//    * `AdornerLayer.GetAdornerLayer(this)` to find the layer,
//    * `layer.Add(adorner)` / `layer.Remove(adorner)` / `layer.Update(element)`.
//
//  Avalonia's `Avalonia.Controls.Primitives.AdornerLayer` has a DIFFERENT model:
//    * it is a concrete `Canvas`; adorners are plain `Control`s placed in `layer.Children`,
//    * the adorned element and clipping are set with the attached properties
//      `AdornerLayer.SetAdornedElement(adorner, adorned)` / `SetIsClipEnabled(adorner, bool)`,
//    * there is NO WPF-style `Adorner` base class and NO `Add`/`Remove`/`Update` instance API.
//
//  This shim provides the WPF surface those nested adorners compile against:
//    * `Adorner` — a `Control` base that records the adorned element (WPF ctor takes it),
//      routes Avalonia's virtual `Render` to a WPF-style `OnRender(DrawingContext)` hook,
//      exposes `IsClipEnabled` (mapped to the attached property when attached), the WPF instance
//      value accessors, and WPF visual/logical child helpers.
//    * `AdornerLayer` — a thin façade over the live Avalonia layer, exposing the WPF static
//      `GetAdornerLayer(Visual)` and instance `Add`/`Remove`/`Update`, mapped onto the Avalonia
//      layer's `Children` collection plus the `AdornedElement` attached property.
//
//  The nested adorners themselves stay DEFERRED to the control phase (they additionally need
//  `Pen`, WPF geometry ops, `DefaultStyleKey`/templates and `Connector.Thumb`). This shim only
//  guarantees the `Adorner`/`AdornerLayer` primitive compiles and is exercised by a smoke test.
// -----------------------------------------------------------------------------

using System.Windows;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Media;
using AvAdornerLayer = Avalonia.Controls.Primitives.AdornerLayer;

namespace System.Windows.Documents
{
    /// <summary>
    /// WPF-compatible <see cref="Adorner"/> base. Derives from Avalonia's <see cref="Control"/>,
    /// records the adorned element (as in WPF's <c>Adorner(UIElement adornedElement)</c>), and adapts
    /// Avalonia's virtual <c>Render</c> to a WPF-style <see cref="OnRender"/> hook.
    /// </summary>
    public abstract class Adorner : Control
    {
        /// <summary>Initializes a new instance of the <see cref="Adorner"/> class for the given element.</summary>
        /// <param name="adornedElement">The element the adorner decorates.</param>
        protected Adorner(Visual adornedElement)
        {
            AdornedElement = adornedElement;
        }

        /// <summary>The element this adorner is bound to (set through the WPF ctor).</summary>
        public Visual AdornedElement { get; }

        private bool _isClipEnabled;

        /// <summary>
        /// WPF's <c>Adorner.IsClipEnabled</c>. Mapped onto Avalonia's
        /// <see cref="AvAdornerLayer.SetIsClipEnabled(AvaloniaObject, bool)"/> attached property.
        /// </summary>
        public bool IsClipEnabled
        {
            get => _isClipEnabled;
            set
            {
                _isClipEnabled = value;
                AvAdornerLayer.SetIsClipEnabled(this, value);
            }
        }

        // -- WPF instance value accessors (shadow Avalonia's GetValue(AvaloniaProperty)) -----------

        /// <summary>WPF-style value accessor for a <see cref="DependencyProperty"/>.</summary>
        public object? GetValue(DependencyProperty property)
            => DependencyPropertyServices.GetValue(this, property);

        /// <summary>WPF-style value setter for a <see cref="DependencyProperty"/>.</summary>
        public void SetValue(DependencyProperty property, object? value)
            => DependencyPropertyServices.SetValue(this, property, value);

        /// <summary>WPF-style value setter for a read-only <see cref="DependencyPropertyKey"/>.</summary>
        public void SetValue(DependencyPropertyKey key, object? value)
            => DependencyPropertyServices.SetValue(this, key, value);

        /// <summary>WPF-style local-value setter for a <see cref="DependencyProperty"/>.</summary>
        public void SetCurrentValue(DependencyProperty property, object? value)
            => DependencyPropertyServices.SetCurrentValue(this, property, value);

        /// <summary>WPF-style value clear for a <see cref="DependencyProperty"/>.</summary>
        public void ClearValue(DependencyProperty property)
            => DependencyPropertyServices.ClearValue(this, property);

        // -- WPF visual/logical hosting helpers -----------------------------------------------------

        /// <summary>WPF's <c>Visual.AddVisualChild</c>. Adds the child to Avalonia's visual children.</summary>
        protected void AddVisualChild(Visual child)
        {
            if (!VisualChildren.Contains(child))
            {
                VisualChildren.Add(child);
            }
        }

        /// <summary>WPF's <c>FrameworkElement.AddLogicalChild</c>. Adds the child to Avalonia's logical children.</summary>
        protected void AddLogicalChild(object child)
        {
            if (child is ILogical logical && !LogicalChildren.Contains(logical))
            {
                LogicalChildren.Add(logical);
            }
        }

        /// <summary>WPF's <c>Visual.VisualChildrenCount</c>. Overridable no-op hook (Avalonia manages children directly).</summary>
        protected virtual int VisualChildrenCount => VisualChildren.Count;

        /// <summary>WPF's <c>Visual.GetVisualChild</c>. Overridable no-op hook (Avalonia manages children directly).</summary>
        /// <param name="index">The child index.</param>
        protected virtual Visual GetVisualChild(int index) => (Visual)VisualChildren[index];

        // -- Resource lookup ------------------------------------------------------------------------

        /// <summary>
        /// WPF's <c>FrameworkElement.TryFindResource</c>. Bridges to Avalonia's
        /// <see cref="Avalonia.Controls.ResourceNodeExtensions.TryFindResource(Avalonia.Controls.IResourceHost, object, out object?)"/>.
        /// </summary>
        /// <param name="resourceKey">The resource key to resolve.</param>
        /// <returns>The resource value, or <see langword="null"/> if not found.</returns>
        public object? TryFindResource(object resourceKey)
            => this.TryFindResource(resourceKey, out object? value) ? value : null;

        // -- Render bridge --------------------------------------------------------------------------

        /// <summary>
        /// Avalonia's render entry point, sealed to route to the WPF-style <see cref="OnRender"/> so
        /// derived adorners keep overriding <c>OnRender</c>.
        /// </summary>
        /// <param name="context">The drawing context to render into.</param>
        public sealed override void Render(DrawingContext context) => OnRender(context);

        /// <summary>
        /// WPF-style render callback. The base is a no-op; adorners override it to draw their content.
        /// </summary>
        /// <param name="drawingContext">The drawing context to render into.</param>
        protected virtual void OnRender(DrawingContext drawingContext)
        {
        }

        protected override Avalonia.Size ArrangeOverride(Avalonia.Size finalSize)
        {
            return ArrangeOverride((Size)finalSize);
        }

        protected override Avalonia.Size MeasureOverride(Avalonia.Size availableSize)
        {
            return MeasureOverride((Size)availableSize);
        }

        protected virtual Size ArrangeOverride(Size arrangeSize)
        {
            return (Size)base.ArrangeOverride((Avalonia.Size)arrangeSize);
        }

        /// <inheritdoc />
        protected virtual Size MeasureOverride(Size constraint)
        {
            return (Size)base.MeasureOverride((Avalonia.Size)constraint);
        }
    }

    /// <summary>
    /// WPF-compatible <see cref="AdornerLayer"/> façade over Avalonia's
    /// <see cref="AvAdornerLayer"/>. Exposes the WPF discovery/attach API (<see cref="GetAdornerLayer"/>,
    /// <see cref="Add"/>, <see cref="Remove"/>, <see cref="Update"/>) used by the ported adorner hosts.
    /// </summary>
    public sealed class AdornerLayer
    {
        private readonly AvAdornerLayer _layer;

        private AdornerLayer(AvAdornerLayer layer) => _layer = layer;

        /// <summary>
        /// WPF's <c>AdornerLayer.GetAdornerLayer(visual)</c>. Returns a façade over the Avalonia
        /// adorner layer for the visual, or <see langword="null"/> if none is available yet.
        /// </summary>
        /// <param name="element">The element whose adorner layer is requested.</param>
        public static AdornerLayer? GetAdornerLayer(Visual element)
        {
            AvAdornerLayer? layer = AvAdornerLayer.GetAdornerLayer(element);
            return layer is null ? null : new AdornerLayer(layer);
        }

        /// <summary>
        /// WPF's <c>AdornerLayer.Add(adorner)</c>. Adds the adorner to the Avalonia layer's children
        /// and binds it to its adorned element.
        /// </summary>
        /// <param name="adorner">The adorner to add.</param>
        public void Add(Adorner adorner)
        {
            if (!_layer.Children.Contains(adorner))
            {
                _layer.Children.Add(adorner);
            }

            AvAdornerLayer.SetAdornedElement(adorner, adorner.AdornedElement);
            AvAdornerLayer.SetIsClipEnabled(adorner, adorner.IsClipEnabled);
        }

        /// <summary>WPF's <c>AdornerLayer.Remove(adorner)</c>. Removes the adorner from the layer.</summary>
        /// <param name="adorner">The adorner to remove.</param>
        public void Remove(Adorner adorner) => _layer.Children.Remove(adorner);

        /// <summary>
        /// WPF's <c>AdornerLayer.Update(element)</c>. Avalonia repositions adorners automatically, so
        /// this invalidates the layer's visual to trigger a redraw.
        /// </summary>
        /// <param name="element">The adorned element whose adorners should be refreshed.</param>
        public void Update(Visual element) => _layer.InvalidateVisual();
    }
}
