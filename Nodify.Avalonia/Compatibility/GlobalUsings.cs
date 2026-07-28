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
global using Nodify.Avalonia.Compatibility;

// Geometry / layout value types (structs) -> Avalonia primitives.
global using Point = System.Windows.Point;
global using Size = System.Windows.Size;
global using Rect = System.Windows.Rect;
global using Vector = System.Windows.Vector;
global using Matrix = System.Windows.Media.Matrix;

global using Thickness = Avalonia.Thickness;
global using CornerRadius = Avalonia.CornerRadius;

// The WPF dependency base type maps onto Avalonia's object model root. It MUST be an alias
// (not a shim class) so that shim controls -- which derive from Avalonia controls and hence
// from AvaloniaObject -- still satisfy `where T : DependencyObject` constraints in the sources.
global using DependencyObject = Avalonia.AvaloniaObject;

// Media value types.
global using Color = Avalonia.Media.Color;
global using Colors = Avalonia.Media.Colors;

// WPF's System.Windows.Media.Brush maps onto Avalonia's brush abstraction. Upstream Nodify uses
// Brush only as a dependency-property value type / property type (ContentBrush, HeaderBrush, ...),
// never through WPF-only Brush API, so the interface is the natural signature-compatible counterpart.
global using Brush = Avalonia.Media.IBrush;

// Media reference types used by the ported utilities/controls. Avalonia's Geometry/Visual are
// the natural equivalents of WPF's System.Windows.Media.Geometry / System.Windows.Media.Visual,
// so they are aliased (they are used in signatures and generic constraints in the ported code).
global using Geometry = Avalonia.Media.Geometry;
global using LineGeometry = Avalonia.Media.LineGeometry;
global using Visual = Avalonia.Visual;
global using Pen = Avalonia.Media.Pen;
global using DashStyle = Avalonia.Media.DashStyle;
global using Typeface = Avalonia.Media.Typeface;
global using FontFamily = Avalonia.Media.FontFamily;
global using FontStyle = Avalonia.Media.FontStyle;
global using FontWeight = Avalonia.Media.FontWeight;
global using FontStretch = Avalonia.Media.FontStretch;
global using FlowDirection = Avalonia.Media.FlowDirection;

// Avalonia's DrawingContext is the render-time drawing surface, equivalent to WPF's
// System.Windows.Media.DrawingContext. It is aliased so the ported shapes' OnRender(DrawingContext)
// signatures match Avalonia's Render(DrawingContext); WPF-only draw helpers that Avalonia lacks
// (e.g. DrawRoundedRectangle) are provided as extension methods in Compatibility/Wpf/DrawingContext.cs.
global using DrawingContext = Avalonia.Media.DrawingContext;
global using Transform = Avalonia.Media.Transform;
global using TransformGroup = Avalonia.Media.TransformGroup;
global using TranslateTransform = Avalonia.Media.TranslateTransform;
global using ScaleTransform = Avalonia.Media.ScaleTransform;
global using DragEventArgs = Avalonia.Input.DragEventArgs;

// Layout enums (identical members between WPF and Avalonia).
global using HorizontalAlignment = Avalonia.Layout.HorizontalAlignment;
global using VerticalAlignment = Avalonia.Layout.VerticalAlignment;
global using Orientation = Avalonia.Layout.Orientation;

// WPF's System.Windows.SizeChangedEventArgs maps onto Avalonia's equivalent. Upstream size-changed
// handlers (Connector/GroupingNode) use `(object sender, SizeChangedEventArgs e)` with `e.NewSize`/
// `e.PreviousSize`, all of which Avalonia's type provides, so a straight alias is sufficient.
global using SizeChangedEventArgs = Avalonia.Controls.SizeChangedEventArgs;

// WPF selection facade types. Upstream uses ItemCollection only as `.Count` + indexer, and
// SelectionChangedEventArgs only as the OnSelectionChanged parameter with AddedItems/RemovedItems --
// both signature-compatible with Avalonia's types, so straight aliases are sufficient.
global using ItemCollection = Avalonia.Controls.ItemCollection;
global using SelectionChangedEventArgs = Avalonia.Controls.SelectionChangedEventArgs;

// WPF's System.Windows.Input.KeyboardNavigationMode has the same members as Avalonia's, so the enum
// is aliased (upstream uses KeyboardNavigationMode.None in keyboard-navigation metadata overrides).
global using KeyboardNavigationMode = Avalonia.Input.KeyboardNavigationMode;

// Single-value converter: Avalonia's IValueConverter is signature-compatible with WPF's,
// so it is aliased rather than reshimmed (upstream converters that implement the WPF
// System.Windows.Data.IValueConverter compile verbatim against Avalonia's interface).
// NOTE: IMultiValueConverter is NOT aliased -- it has a different shape and is provided as a
// WPF-shaped shim in Compatibility/Wpf/Converters.cs.
global using IValueConverter = Avalonia.Data.Converters.IValueConverter;

// Templating / styling reference types. Upstream Nodify uses these only as dependency-property
// value types, cast targets, and opaque "template/style" handles (e.g. ContentTemplate,
// ResizeThumbTemplate, ContentContainerStyle) -- never through WPF-only API -- so they map
// straight onto Avalonia's equivalents:
//   * WPF DataTemplate/ControlTemplate  -> Avalonia's template INTERFACES (the concrete
//     Avalonia.Markup.Xaml.Templates.* live in a XAML assembly; the interfaces are the natural
//     signature-compatible counterparts and keep this core library free of a XAML dependency).
//   * WPF Style -> Avalonia.Styling.Style.
global using DataTemplate = Avalonia.Controls.Templates.IDataTemplate;
global using ControlTemplate = Avalonia.Controls.Templates.IControlTemplate;
global using Style = Avalonia.Styling.Style;

// WPF's UIElementCollection (Panel.InternalChildren) maps onto Avalonia's indexable child
// collection. Upstream panels only use it as `collection[i]` + `.Count`, both of which Avalonia's
// Controls type provides, so a straight alias is sufficient.
global using UIElementCollection = Avalonia.Controls.Controls;

global using UIElement = Avalonia.Controls.Control;
global using FrameworkElement = Avalonia.Controls.Control;