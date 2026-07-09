// -----------------------------------------------------------------------------
//  WPF -> Avalonia global type aliases
// -----------------------------------------------------------------------------
//  These map WPF value types and layout enums used throughout the linked Nodify
//  sources onto their Avalonia equivalents so the original code compiles unchanged.
//
//  IMPORTANT: A name provided here as a `global using` alias MUST NOT also be
//  declared as a shim type under the matching System.Windows.* namespace, or the
//  compiler reports CS0104 (ambiguous reference). A using alias always wins over a
//  type brought in by a `using <namespace>;` directive, so aliasing is conflict-free
//  as long as we never redeclare these simple names in the shim namespaces.
//
//  Reference types that need extra WPF-shaped API (Brush, Pen, DataTemplate, Style,
//  controls, ...) are intentionally NOT aliased here; they are provided as shim
//  classes inside the System.Windows.* namespaces in the Compatibility folder.
// -----------------------------------------------------------------------------

// Geometry / layout value types (structs) -> Avalonia primitives.
global using Point = Avalonia.Point;
global using Size = Avalonia.Size;
global using Rect = Avalonia.Rect;
global using Vector = Avalonia.Vector;
global using Thickness = Avalonia.Thickness;
global using CornerRadius = Avalonia.CornerRadius;
global using Matrix = Avalonia.Matrix;

// The WPF dependency base type maps onto Avalonia's object model root. It MUST be an alias
// (not a shim class) so that shim controls -- which derive from Avalonia controls and hence
// from AvaloniaObject -- still satisfy `where T : DependencyObject` constraints in the sources.
global using DependencyObject = Avalonia.AvaloniaObject;

// Media value types.
global using Color = Avalonia.Media.Color;
global using Colors = Avalonia.Media.Colors;

// Media reference types used by the ported utilities/controls. Avalonia's Geometry/Visual are
// the natural equivalents of WPF's System.Windows.Media.Geometry / System.Windows.Media.Visual,
// so they are aliased (they are used in signatures and generic constraints in the ported code).
global using Geometry = Avalonia.Media.Geometry;
global using Visual = Avalonia.Visual;

// Avalonia's DrawingContext is the render-time drawing surface, equivalent to WPF's
// System.Windows.Media.DrawingContext. It is aliased so the ported shapes' OnRender(DrawingContext)
// signatures match Avalonia's Render(DrawingContext); WPF-only draw helpers that Avalonia lacks
// (e.g. DrawRoundedRectangle) are provided as extension methods in Compatibility/Wpf/DrawingContext.cs.
global using DrawingContext = Avalonia.Media.DrawingContext;

// Layout enums (identical members between WPF and Avalonia).
global using HorizontalAlignment = Avalonia.Layout.HorizontalAlignment;
global using VerticalAlignment = Avalonia.Layout.VerticalAlignment;
global using Orientation = Avalonia.Layout.Orientation;

// Single-value converter: Avalonia's IValueConverter is signature-compatible with WPF's,
// so it is aliased rather than reshimmed (upstream converters that implement the WPF
// System.Windows.Data.IValueConverter compile verbatim against Avalonia's interface).
// NOTE: IMultiValueConverter is NOT aliased -- it has a different shape and is provided as a
// WPF-shaped shim in Compatibility/Wpf/Converters.cs.
global using IValueConverter = Avalonia.Data.Converters.IValueConverter;
