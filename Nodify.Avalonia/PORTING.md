# Nodify → Avalonia Port (`Nodify.Avalonia`)

This project ports [Nodify](https://github.com/miroiu/nodify) (a WPF node-editor
control library, currently **v7.3.0** in this repo) to **Avalonia 11.3.x** so the
controls can be used cross-platform.

## Strategy: source-fork port (Option A)

The port follows the same approach as the established
[BAndysc/nodify-avalonia](https://github.com/BAndysc/nodify-avalonia) project
(*"a direct port… keep the codebase as similar to the original as possible, to
the point where merges from the upstream are not a problem"*).

**Why not a pure "link upstream + shim" approach?**
An earlier attempt tried to link the upstream `Nodify\**\*.cs` **unchanged** and
satisfy WPF purely through a compatibility shim. That is not viable: the upstream
controls hard-depend on WPF subsystems whose Avalonia equivalents have a
*different shape* and cannot be provided by type aliases or thin shims:

| WPF subsystem (upstream) | Avalonia equivalent | Shimmable? |
| --- | --- | --- |
| `EventManager.RegisterRoutedEvent` / `RoutingStrategy` / `RoutedEvent` / `AddHandler` / `RaiseEvent` / `RoutedEventArgs` | `RoutedEvent.Register<TArgs>` / `RoutingStrategies` / different args base | ❌ different API + args shape |
| `OnMouseDown/Move/Up(MouseButtonEventArgs)` overrides, `Mouse.Capture`, `CaptureMouse` | `OnPointerPressed/Moved/Released(PointerEventArgs)`, `PointerPressedEventArgs`, `e.Pointer.Capture` | ❌ different names + signatures |
| `Shape` + `OnRender(DrawingContext)` + `DefiningGeometry` | `Avalonia.Controls.Shapes.Shape`, `Render(DrawingContext)`, different `DrawingContext` API | ⚠️ partial (custom base + adapter) |
| `Adorner` / `AdornerLayer` | `Avalonia.Controls.Primitives.AdornerLayer` (different model) | ⚠️ needs reimplementation |
| `VisualTreeHelper` navigation (`GetParent/GetChild/GetChildrenCount`) | `Avalonia.VisualTree` extensions | ✅ done (shim) |
| `VisualTreeHelper.HitTest(filter, result, params)` callback traversal | `InputHitTest` / `GetVisualsAt` (no callback model) | ❌ needs hand-written walk |
| `DependencyProperty` / `FrameworkPropertyMetadata` / DP accessors | `StyledProperty<T>` / `AttachedProperty<T>` | ✅ done (shim) |

Therefore the ported `.cs` files live under this project's **own** folders and are
edited to be Avalonia-native. A **small compatibility layer** is retained under
`Compatibility\` to keep the ported files textually close to upstream (minimizing
future merge conflicts):

- `Compatibility/GlobalUsings.cs` — aliases WPF value types/enums (`Point`, `Size`,
  `Rect`, `Vector`, `Thickness`, `Color`, layout enums) and `DependencyObject` to
  Avalonia types.
- `Compatibility/Wpf/DependencyProperty*.cs`, `FrameworkPropertyMetadata.cs` — a
  working WPF dependency-property system (`Register` / `RegisterAttached` /
  `RegisterReadOnly` / `AddOwner` / `OverrideMetadata`, `GetValue` / `SetValue` /
  `CoerceValue`, property-changed & coerce callbacks) bridged onto Avalonia's
  strongly-typed properties.
- `Compatibility/Wpf/VisualTreeHelper.cs` — visual-tree navigation over Avalonia.

**Consequence:** upstream sync is a **periodic manual merge** (as in
nodify-avalonia), not automatic. This is an inherent trade-off of porting between
two different UI frameworks.

## Status

### ✅ Done (compiles and verified)
- New `Nodify.Avalonia` project (`net8.0`, Avalonia `11.3.18`), added to `Nodify.sln`.
- Dependency-property compatibility layer.
- `VisualTreeHelper` navigation shim.
- **Routed-event compatibility layer** (`Compatibility/Wpf/RoutedEvents.cs`,
  `RoutedEventServices.cs`) — `EventManager.RegisterRoutedEvent`, `RoutingStrategy`,
  `RoutedEvent` (+ `AddOwner`) and `RoutedEventArgs` mapped onto Avalonia's
  `Avalonia.Interactivity.RoutedEvent`/`RoutedEventArgs`; WPF `AddHandler`
  (2-arg / 3-arg) bridged to Avalonia's `Interactive`. `RaiseEvent` and 2-arg
  `RemoveHandler` are used from Avalonia natively.
- **Framework-agnostic utilities** ported verbatim: `Utilities/MathExtensions.cs`,
  `Utilities/WeakReferenceCollection.cs`, `Utilities/BoxValue.cs`.
- **Event-argument classes** ported verbatim into `Events/`: `ConnectorEventArgs`,
  `ConnectionEventArgs`, `PendingConnectionEventArgs` (with their handler delegates).

### 🚧 Remaining work (per subsystem)

Port order is bottom-up so lower layers compile before the controls that use them.

1. **Utilities & value helpers** — ✅ `BoxValue`, `MathExtensions`,
   `WeakReferenceCollection` ported. ⏳ Still to do: `DependencyObjectExtensions`
   (rewrite `HitTest` usages — see "Hit testing" below), `SelectionHelper`,
   converters (`UnscaleTransformConverter`), `EditorGesturesExtensions`.
2. **Routed-event compatibility** — ✅ Done. Chose option (a): a shim
   `EventManager`/`RoutingStrategy`/`RoutedEvent`/`RoutedEventArgs` mapping onto
   Avalonia's `Avalonia.Interactivity` routed events (the shim `RoutedEvent`/
   `RoutedEventArgs` *derive from* the Avalonia types, so `RaiseEvent`/`AddHandler`/
   `RemoveHandler` work natively; only WPF's shorter `AddHandler` overloads are
   bridged). The `RoutedEventArgs`-derived event-arg classes are ported verbatim.
3. **Input / interactivity** — ⏳ Next up. Port `Interactivity\` (InputProcessor,
   gestures, states, keyboard navigation) from `Mouse`/`Keyboard`/`MouseButtonEventArgs`/
   `InputGesture`/`ModifierKeys` to Avalonia pointer/key APIs and `e.Pointer.Capture`.
   This is the largest remaining foundational piece; it likely needs a small input
   shim (`Mouse`/`Keyboard`/`ModifierKeys`/`Key`/`MouseButton` + pointer/key event-arg
   adapters) plus per-file porting of the `OnMouseDown/Move/Up` overrides to the
   Avalonia `OnPointer*` methods.
4. **Shapes & rendering** — `BaseConnection`, `LineConnection`, `CircuitConnection`,
   `StepConnection`, `CuttingLine`: port `Shape`/`OnRender(DrawingContext)`/`DefiningGeometry`
   to Avalonia's `Shape`/`Render`. (`CuttingLine` may be blocked — see nodify-avalonia,
   which lists Cutting Lines as unsupported pending an Avalonia fix.)
5. **Adorners** — reimplement `FocusVisualAdorner` / `HotKeyAdorner` on Avalonia's
   `AdornerLayer`.
6. **Containers & core controls** — `ItemContainer`, `DecoratorContainer`,
   `ConnectionContainer`, `Node`, `KnotNode`, `GroupingNode`, `StateNode`, `Connector`,
   `PendingConnection`.
7. **Editor** — `NodifyEditor` (+ partials), `NodifyCanvas`, `EditorCommands`.
8. **Minimap** — `Minimap`, `MinimapItem`, `MinimapPanel`.
9. **Theme** — rewrite `Themes\**\*.xaml` (WPF `ControlTemplate`s) into Avalonia
   `.axaml` control themes/`ResourceDictionary`s; expose an includable `Theme.axaml`.
10. **Example app** — one runnable Avalonia desktop sample validating the controls
    (e.g., a minimal editor with a few nodes + connections).

## Conventions for keeping merges tractable

- Keep file/namespace/type/member names identical to upstream wherever possible.
- Prefer the compatibility shims over rewriting DP/tree code, so diffs vs. upstream
  stay minimal.
- Where a WPF construct has no Avalonia analogue, isolate the change and leave a
  `// PORT:` comment referencing the upstream construct, so the next upstream merge
  is easy to reconcile.
