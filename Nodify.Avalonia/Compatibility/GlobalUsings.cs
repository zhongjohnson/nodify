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

// Layout enums (identical members between WPF and Avalonia).
global using HorizontalAlignment = Avalonia.Layout.HorizontalAlignment;
global using VerticalAlignment = Avalonia.Layout.VerticalAlignment;
global using Orientation = Avalonia.Layout.Orientation;
