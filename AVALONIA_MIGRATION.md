# WPF to Avalonia Migration Guide for Nodify

This document tracks the migration from WPF to Avalonia for the Nodify project.

## Migration Progress

### ✅ Phase 1 Complete - Foundation (17 files)
- Project configuration updated
- Utility classes converted
- Event arguments converted  
- Command infrastructure converted
- **Namespace replacements applied globally** ✅

### 🔄 Phase 2 In Progress - Control Conversion
**Current Status**: Namespace conversions complete, DependencyProperty conversions needed

The bulk namespace replacements have been applied:
- `System.Windows` → `Avalonia`
- `System.Windows.Controls` → `Avalonia.Controls`
- `System.Windows.Media` → `Avalonia.Media`
- `System.Windows.Input` → `Avalonia.Input`  
- `System.Windows.Shapes` → `Avalonia.Controls.Shapes`
- `System.Windows.Documents` → `Avalonia.Controls`
- `DependencyObject` → `AvaloniaObject`
- `UIElement/FrameworkElement` → `Control`

### ⚠️ Known Conversion Challenges

The following patterns cannot be auto-converted and require manual work:

#### 1. Dependency Properties → Styled Properties
**WPF Pattern**:
```csharp
public static readonly DependencyProperty SourceProperty = 
    DependencyProperty.Register(nameof(Source), typeof(Point), typeof(BaseConnection), 
        new FrameworkPropertyMetadata(default(Point), FrameworkPropertyMetadataOptions.AffectsRender));
```

**Avalonia Pattern**:
```csharp
public static readonly StyledProperty<Point> SourceProperty =
    AvaloniaProperty.Register<BaseConnection, Point>(nameof(Source), defaultValue: default(Point));
```

Key Differences:
- `StyledProperty` is generic: `StyledProperty<TValue>`
- Registration uses `AvaloniaProperty.Register<TOwner, TValue>()`
- Metadata is handled differently (coercion, validation as separate parameters)
- `AffectsRender`, `AffectsMeasure`, etc. are handled via `PropertyChanged` callbacks or rendering flags

#### 2. Attached Properties
**WPF Pattern**:
```csharp
public static readonly DependencyProperty IsSelectedProperty = 
    DependencyProperty.RegisterAttached("IsSelected", typeof(bool), typeof(BaseConnection), ...);
```

**Avalonia Pattern**:
```csharp
public static readonly AttachedProperty<bool> IsSelectedProperty =
    AvaloniaProperty.RegisterAttached<BaseConnection, Control, bool>("IsSelected");
```

#### 3. Property Changed Callbacks
**WPF Pattern**:
```csharp
new PropertyChangedCallback(OnPropertyChanged)
private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) { }
```

**Avalonia Pattern**:
```csharp
// In property registration, no callback parameter
// Instead, override OnPropertyChanged or subscribe to property changes
static BaseConnection()
{
    SourceProperty.Changed.AddClassHandler<BaseConnection>((x, e) => x.OnSourceChanged(e));
}
```

#### 4. Types Without Avalonia Equivalents

These WPF types don't have direct Avalonia equivalents and need custom solutions:

- **`TextElement`** - No equivalent. Font properties are on `Control` directly or use `TextBlock`
- **`AdornerLayer`** - No adorner system. Use overlays, popups, or custom rendering
- **`ResourceKey`, `ComponentResourceKey`** - Use strings or custom key types
- **`FontSizeConverter`** - Avalonia has different type converters
- **`Freezable`** - Not needed in Avalonia
- **`VisualBrush`**, **`DrawingBrush`** - Limited or different implementations
- **`Geometry.Combine`** - Different geometry APIs
- **`RoutedUICommand`** - Use `ICommand` implementations (ReactiveCommand, etc.)
- **`CommandManager`** - No equivalent, commands are simpler

#### 5. Shape Base Class
WPF's `Shape` class has abstract `DefiningGeometry` property marked `override`.
Avalonia's `Shape` class has different structure - check Avalonia.Controls.Shapes API.

#### 6. Event System Differences
- **WPF**: `RoutedEvent` with `EventManager.RegisterRoutedEvent()`
- **Avalonia**: `RoutedEvent<TEventArgs>` generic, different registration

**WPF**:
```csharp
public static readonly RoutedEvent DisconnectEvent = 
    EventManager.RegisterRoutedEvent(nameof(Disconnect), RoutingStrategy.Bubble, 
        typeof(ConnectionEventHandler), typeof(BaseConnection));
```

**Avalonia**:
```csharp
public static readonly RoutedEvent<ConnectionEventArgs> DisconnectEvent =
    RoutedEvent.Register<BaseConnection, ConnectionEventArgs>(
        nameof(Disconnect), RoutingStrategy.Bubble);
```

#### 7. Input and Gestures
- Mouse/Keyboard event args are different
- Gesture system is different (no `InputGestureCollection`)
- Focus system works differently

#### 8. Orientation Enum
`System.Windows.Controls.Orientation` → `Avalonia.Layout.Orientation`

Needs additional using:
```csharp
using Avalonia.Layout;
```

## Migration Strategy - Revised

Given the complexity discovered, the strategy is:

### Manual Conversion Required For Each Control File:
1. ✅ Update namespaces (DONE via scripts)
2. ⏳ Convert each `DependencyProperty` to `StyledProperty<T>` or `Attached Property<T>` 
3. ⏳ Update property changed callbacks to use Avalonia's pattern
4. ⏳ Handle types without equivalents (TextElement, AdornerLayer, etc.)
5. ⏳ Fix Shape rendering (DefiningGeometry, OnRender, etc.)
6. ⏳ Convert routed events
7. ⏳ Update input handling
8. ⏳ Test and fix behavioral differences

### Recommended Approach:
1. **Create helper/extension classes** for missing WPF types
2. **Start with simplest controls** and work up to complex ones
3. **Test incrementally** as each control is converted
4. **Consider creating Avalonia-specific versions** rather than 1:1 ports for complex features

## Current Errors Summary

From latest build (~500 total errors):
- StyledProperty needs generic type arguments  
- Property metadata differs
- Missing Avalonia equivalents for: TextElement, AdornerLayer, ResourceKey, FontSizeConverter
- Shape.DefiningGeometry signature difference
- Orientation enum namespace
- Routed event registration differences

## Files Needing Manual Conversion

Priority order based on dependencies:

### Tier 1 - Base Types & Utilities
- ✅ BoxValue.cs
- ✅ MathExtensions.cs
- ✅ DependencyObjectExtensions.cs → AvaloniaObjectExtensions.cs
- ✅ Event args files
- ⏳ EditorGestures.cs (input gestures)
- ⏳ InputProcessor.cs (input handling)

### Tier 2 - Core Infrastructure  
- ⏳ ItemContainer.cs
- ⏳ DecoratorContainer.cs
- ⏳ ConnectionContainer.cs

### Tier 3 - Shapes & Connections
- ⏳ BaseConnection.cs (882 lines, complex)
- ⏳ LineConnection.cs
- ⏳ CircuitConnection.cs
- ⏳ StepConnection.cs
- ⏳ Connection.cs

### Tier 4 - Nodes
- ⏳ Node.cs
- ⏳ StateNode.cs
- ⏳ GroupingNode.cs
- ⏳ KnotNode.cs
- ⏳ NodeInput.cs, NodeOutput.cs

### Tier 5 - Connectors
- ⏳ Connector.cs
- ⏳ PendingConnection.cs
- ⏳ HotKeyControl.cs

### Tier 6 - Editor
- ⏳ NodifyEditor.cs (main control, largest file)
- ⏳ NodifyCanvas.cs
- ⏳ Minimap.cs
- ⏳ CuttingLine.cs

### Tier 7 - State Management
- ⏳ All state files in States/ folders

## Automated Steps Completed

✅ Namespace replacement across all .cs files
✅ Basic type replacements (DependencyObject → AvaloniaObject, etc.)

## Next Steps

**Option A - Continue Manual Conversion** (Recommended)
- Pick one simple control file
- Manually convert all DependencyProperties to StyledProperty<T>
- Establish pattern for others
- Create helpers for missing WPF types

**Option B - Hybrid Approach**
- Create PowerShell script to generate StyledProperty declarations
- Manually fix complex cases
- Build incrementally

**Option C - Pause and Assess**
- Evaluate if full 1:1 port is feasible  
- Consider redesigning some features for Avalonia
- Create proof-of-concept with subset of features

## Estimated Effort

- **Remaining .cs file conversions**: 85-90 files
- **Estimated time per file**: 30-120 minutes (depending on complexity)
- **Total estimated effort**: 40-100 hours of manual conversion work
- **XAML conversions**: Additional 20-40 hours
- **Testing & refinement**: 20-40 hours

**Total project estimate**: 80-180 hours

This is a **major migration project** requiring significant manual effort.
