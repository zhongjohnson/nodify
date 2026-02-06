# Quick Fixes for Remaining Common Errors

## 1. Missing `using System;`

**Error**: `EventHandler<T>` not found

**Fix**: Add to top of file:
```csharp
using System;
```

**Files Affected**: Any file using `EventHandler<T>` for Avalonia routed events

---

## 2. RoutingStrategy Not Found

**Error**: `CS0103: The name 'RoutingStrategy' does not exist`

**Current Code**:
```csharp
using Avalonia.Interactivity;
```

**Issue**: Already have the right using, might need full namespace

**Fix**: Use full namespace or check Avalonia version:
```csharp
RoutedEvent.Register<T, TArgs>(name, Avalonia.Interactivity.RoutingStrategy.Bubble);
```

---

## 3. Template.FindName Doesn't Exist

**Error**: `CS0103: The name 'Template' does not exist`

**WPF Code**:
```csharp
_thumb = Template.FindName(ElementConnector, this) as Control ?? this;
```

**Avalonia Fix**:
```csharp
// Option 1: In OnApplyTemplate
protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
{
    _thumb = e.NameScope.Find<Control>(ElementConnector) ?? this;
}

// Option 2: Use this extension
_thumb = this.FindControl<Control>(ElementConnector) ?? this;
```

---

## 4. OnApplyTemplate Signature

**Error**: `CS0115: no suitable method found to override`

**WPF**:
```csharp
public override void OnApplyTemplate()
```

**Avalonia**:
```csharp
protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
```

---

## 5. RenderSize Property Missing

**Error**: `CS1061: 'Control' does not contain a definition for 'RenderSize'`

**WPF**:
```csharp
var size = control.RenderSize;
```

**Avalonia**:
```csharp
var size = control.Bounds.Size;
```

---

## 6. Rect.Inflate Signature

**Error**: `CS1501: No overload for method 'Inflate' takes 3 arguments`

**WPF**:
```csharp
Rect area = Rect.Inflate(viewport, offset, offset);
```

**Avalonia**:
```csharp
Rect area = viewport.Inflate(offset); // Inflates by offset on all sides
// OR
Rect area = viewport.Inflate(new Thickness(offset));
```

---

## 7. Mouse Events → Pointer Events

**Error**: `CS0115: 'OnMouseDown' no suitable method found to override`

**WPF**:
```csharp
protected override void OnMouseDown(MouseButtonEventArgs e)
{
    base.OnMouseDown(e);
    // ...
}
```

**Avalonia**:
```csharp
protected override void OnPointerPressed(PointerPressedEventArgs e)
{
    base.OnPointerPressed(e);
    // ...
}
```

**Common Conversions**:
- `OnMouseDown` → `OnPointerPressed`
- `OnMouseUp` → `OnPointerReleased`
- `OnMouseMove` → `OnPointerMoved`
- `OnMouseWheel` → `OnPointerWheelChanged`
- `OnLostMouseCapture` → `OnPointerCaptureLost`

---

## 8. Mouse Capture API

**WPF**:
```csharp
if (IsMouseCaptured)
    ReleaseMouseCapture();
CaptureMouse();
```

**Avalonia**:
```csharp
if (e.Pointer.Captured == this)
    e.Pointer.Capture(null);
e.Pointer.Capture(this);
```

---

## 9. MouseButtonState

**WPF**:
```csharp
if (e.LeftButton == MouseButtonState.Released)
```

**Avalonia**:
```csharp
// Different approach - check button properties
if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
```

---

## 10. TranslatePoint Returns Nullable

**Error**: `CS0266: Cannot implicitly convert type 'Point?' to 'Point'`

**Fix**:
```csharp
// WPF
Point point = element.TranslatePoint(relativePoint, target);

// Avalonia
Point point = element.TranslatePoint(relativePoint, target) ?? default(Point);
// OR
Point? point = element.TranslatePoint(relativePoint, target);
if (point.HasValue)
{
    // Use point.Value
}
```

---

## 11. Vector to Size Conversion

**Error**: `CS0030: Cannot convert type 'Size' to 'Vector'`

**WPF** (allowed):
```csharp
Vector v = (Vector)control.RenderSize;
```

**Avalonia**:
```csharp
// Create Vector from Size
Size size = control.Bounds.Size;
Vector v = new Vector(size.Width, size.Height);
```

---

## 12. InputProcessor.AddSharedHandlers Constraint

**Error**: Type constraint violation

**Issue**: Method expects `Control` but passing `Shape` or other `Visual`

**Fix**: Update InputProcessorExtensions:
```csharp
// Change constraint from Control to Visual
public static void AddSharedHandlers<TElement>(this InputProcessor processor, TElement element)
    where TElement : Visual
{
    // ...
}
```

---

## Batch Fix Script

Create a PowerShell script to apply common fixes:

```powershell
# Add using System to files using EventHandler<T>
Get-ChildItem -Path "Nodify" -Filter "*.cs" -Recurse | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    if ($content -match 'EventHandler<' -and $content -notmatch 'using System;') {
        $content = $content -replace '(using [\w.]+;)', "using System;`n`$1"
        Set-Content $_.FullName -Value $content -NoNewline
    }
}
```

---

**Use this guide as a quick reference when fixing remaining compilation errors!**
