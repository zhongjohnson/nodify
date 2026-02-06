# Avalonia API Quick Reference - For Remaining Conversions

## StreamGeometryContext Differences

### WPF → Avalonia Patterns

**Pattern 1: Simple Line**
```csharp
// WPF
context.BeginFigure(start, false, false);
context.LineTo(end, true, true);

// Avalonia
context.BeginFigure(start, false);
context.LineTo(end);
context.EndFigure(false);
```

**Pattern 2: Filled Shape**
```csharp
// WPF
context.BeginFigure(start, true, true);
context.LineTo(p1, true, true);
context.LineTo(p2, true, true);

// Avalonia
context.BeginFigure(start, true);
context.LineTo(p1);
context.LineTo(p2);
context.EndFigure(true);
```

**Pattern 3: Bezier Curve**
```csharp
// WPF
context.BezierTo(cp1, cp2, end, true, true);

// Avalonia
context.CubicBezierTo(cp1, cp2, end);
```

## Vector Operations

### Immutable Vector Pattern
```csharp
// WPF - Direct assignment
vector.X = newValue;
(vector.X, vector.Y) = (vector.Y, vector.X);

// Avalonia - Create new Vector
vector = new Vector(newValue, vector.Y);
vector = new Vector(vector.Y, vector.X);
```

### Vector.LengthSquared
```csharp
// WPF
if (vector.LengthSquared > 0)

// Avalonia - Calculate manually
if (vector.X * vector.X + vector.Y * vector.Y > 0)

// Or create extension method:
public static double LengthSquared(this Vector vector)
    => vector.X * vector.X + vector.Y * vector.Y;
```

## Mouse/Pointer Events

### WPF Override Pattern
```csharp
protected override void OnMouseDown(MouseButtonEventArgs e)
{
    base.OnMouseDown(e);
    // handle
}
```

### Avalonia Patterns

**Option 1: Pointer Events**
```csharp
protected override void OnPointerPressed(PointerPressedEventArgs e)
{
    base.OnPointerPressed(e);
    // handle
}
```

**Option 2: Event Subscription (in constructor)**
```csharp
this.PointerPressed += OnPointerPressed;

private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
{
    // handle
}
```

## System Resources

### Colors
```csharp
// WPF
SystemColors.ControlTextBrush

// Avalonia - Use theme or hardcode
new SolidColorBrush(Colors.Black)
// Or from theme:
// (IBrush?)Application.Current?.FindResource("SystemControlForegroundBrush")
```

### Freezable Pattern
```csharp
// WPF
pen.Freeze();

// Avalonia - Not needed (already immutable)
// Just remove the Freeze() call
```

## Template Parts

### Attribute
```csharp
// Ensure using statement
using Avalonia.Metadata;

[TemplatePart(Name = "PART_Name", Type = typeof(Control))]
public class MyControl : Control
```

### OnApplyTemplate
```csharp
// WPF
public override void OnApplyTemplate()

// Avalonia
protected override void OnApplyTemplateCore()
// OR just OnApplyTemplate() without override keyword in some versions
```

## Other Common Differences

### Size Changed Event
```csharp
// WPF
protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)

// Avalonia
protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
{
    base.OnPropertyChanged(change);
    if (change.Property == BoundsProperty)
    {
        var newBounds = (Rect)change.NewValue!;
        // handle size change
    }
}
```

### Mouse Capture
```csharp
// WPF
if (IsMouseCaptured)
    ReleaseMouseCapture();
CaptureMouse();

// Avalonia
if (IsPointerCaptured)
    ReleasePointerCapture();
e.Pointer.Capture(this);
```

### Keyboard Focus
```csharp
// WPF
if (IsKeyboardFocusWithin)
    Keyboard.Focus(this);

// Avalonia
if (IsKeyboardFocusWithin)
    Focus();
```

## Quick Conversion Checklist

When converting a control file:

- [ ] Update all property declarations to `StyledProperty<T>`
- [ ] Add static constructor with property change handlers
- [ ] Convert StreamGeometryContext calls (BeginFigure, LineTo, etc.)
- [ ] Fix Vector operations (create new instead of modifying)
- [ ] Replace mouse overrides with pointer events
- [ ] Remove Freeze() calls
- [ ] Update template part attributes and OnApplyTemplate
- [ ] Fix system resource references
- [ ] Add required using statements (Avalonia.Metadata, Avalonia.Layout, etc.)

---

This guide covers the most common API differences encountered in the Nodify conversion.
Use it as a quick reference when converting remaining files!
