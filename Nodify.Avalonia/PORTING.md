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
| `VisualTreeHelper.HitTest(filter, result, params)` callback traversal | `InputHitTest` / `GetVisualsAt` (no callback model) | ✅ done (hand-written recursive walk in the shim) |
| `Transform.Inverse` / `DoubleAnimation`/`PointAnimation` + `BeginAnimation` | `Matrix.TryInvert` / no `BeginAnimation(DP, timeline)` API | ✅ done (adapted converter + `DispatcherTimer` animator shim) |
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
- `Compatibility/Wpf/VisualTreeHelper.cs` — visual-tree navigation **and** a WPF-compatible
  callback-based `HitTest(filter, result, params)` reimplemented as a recursive Avalonia
  visual-tree walk (top-most-first; honors filter skip/stop; point-contains + geometry-bounds
  intersection in root coordinates).
- `Compatibility/Wpf/HitTesting.cs` — WPF hit-test vocabulary (`HitTestFilterBehavior`,
  `HitTestResultBehavior`, `PointHitTestParameters`/`GeometryHitTestParameters`,
  `HitTestResult`/`PointHitTestResult`/`GeometryHitTestResult`, callback delegates).
- `Compatibility/Wpf/Converters.cs` — WPF-shaped `IMultiValueConverter` (`object[]` +
  `ConvertBack`) that also implements Avalonia's `IMultiValueConverter` (bridges `IList<object?>`);
  `IValueConverter` is aliased directly to Avalonia's signature-compatible interface.
- `Compatibility/Wpf/Animation.cs` — the small WPF animation slice Nodify needs: `Duration`,
  `RepeatBehavior` (`Forever`), `AnimationTimeline`/`DoubleAnimation`/`PointAnimation` (linear),
  and a `DispatcherTimer`-driven `BeginAnimation`(cancel via `null`) property animator.
- `Compatibility/Wpf/RectExtensions.cs` — WPF `Rect.IntersectsWith` mapped to Avalonia
  `Rect.Intersects`.

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
- **Input compatibility layer** (Phase 3a, `Compatibility/Wpf/Input/*.cs`) — WPF
  `System.Windows.Input` surface mapped onto Avalonia's pointer/key system:
  - `InputEnums.cs` — `Key`/`MouseButton` aliased to `Avalonia.Input` enums; WPF-shaped
    `ModifierKeys` (`[Flags]`), `MouseAction`, `MouseButtonState` + `ModifierKeys`↔
    `KeyModifiers` conversions.
  - `InputEventArgs.cs` — `InputEventArgs`/`MouseEventArgs`/`MouseButtonEventArgs`/
    `MouseWheelEventArgs`/`KeyEventArgs` (deriving from the routed-event args shim),
    wrapping Avalonia `PointerEventArgs`/`KeyEventArgs` for `GetPosition`/`Pointer`/
    `Key`/`Delta`, + handler delegates.
  - `InputState.cs` — `Keyboard` (`Modifiers`, `IsKeyDown`) and `Mouse`
    (`Left/Right/MiddleButton`, `MouseWheelDeltaForOneLine`, `GetPosition`) over an
    `InputStateTracker` (fed by the ported control overrides in Phase 3b).
  - `InputGestures.cs` — `InputGesture` base + `KeyGesture`/`MouseGesture` with WPF
    gesture-matching semantics reimplemented (Avalonia has no `InputGesture.Matches`).
  - `Commands.cs` — WPF-shaped `RoutedCommand`/`RoutedUICommand` (over the BCL
    `ICommand`), `InputGestureCollection`, and an `ApplicationCommands` subset
    (`SelectAll` seeded with Ctrl+A) consumed by `EditorGestures` (and later
    `EditorCommands`).
- **Interactivity primitives (control-independent, Phase 3b)** — **linked** directly
  from the upstream sources (via `<Compile Include="..\Nodify\..." Link=... />`) so they
  stay byte-for-byte in sync on future merges. They compile unchanged against the
  compatibility shim:
  - Core: `Interactivity/IInputHandler.cs`, `InputProcessor.cs`,
    `InputElementState.cs`, `InputElementStateStack.cs` (+ `.InputElementState` partial).
  - Gestures: `AllGestures`, `AnyGesture`, `MultiGesture`, `InputGestureRef`,
    `KeyComboGesture`, `MouseGesture`, `EditorGestures`.
  - Framework shim backing these: `Compatibility/Wpf/FrameworkElement.cs` —
    `UIElement`/`FrameworkElement` (over Avalonia `Control`), the WPF input
    `RoutedEvent` identities used for state dispatch, `EventManager.RegisterClassHandler`
    (currently record-only; runtime bridge wired in the control phase),
    `FocusNavigationDirection`, and `KeyboardFocusChangedEventArgs`/handler. The WPF
    instance value accessors (`GetValue(DependencyProperty)`/`SetValue`/`SetCurrentValue`/
    `ClearValue`) that shadow Avalonia's `GetValue(AvaloniaProperty)` for unqualified
    `GetValue(dp)` calls also live here (added in Phase 4b).
- **Rendering primitives (control-independent, Phase 4b)** — the reusable WPF-shaped
  geometry/shape surface, provided as shims (globbed, not linked):
  - `Compatibility/Wpf/StreamGeometry.cs` — `System.Windows.Media.StreamGeometry`
    (over `Avalonia.Media.StreamGeometry`) with the WPF `FillRule`, plus a
    `StreamGeometryContext` wrapper that re-exposes WPF `BeginFigure(pt, isFilled, isClosed)`/
    `LineTo`/`BezierTo`/`QuadraticBezierTo`/`ArcTo` and defers Avalonia's required
    `EndFigure(isClosed)` until the next figure or `Dispose`. Also `FillRule`/`SweepDirection`.
  - `Compatibility/Wpf/Shape.cs` — `System.Windows.Shapes.Shape` (over
    `Avalonia.Controls.Shapes.Shape`). Bridges the API-shape mismatch: Avalonia's
    `DefiningGeometry` is non-virtual and `Render` is sealed, so the shim shadows a WPF-style
    virtual `DefiningGeometry`, implements Avalonia's abstract `CreateDefiningGeometry()` to
    return it, and exposes a WPF-style virtual `OnRender(DrawingContext)` hook (base no-op;
    runtime wiring of `OnRender` extras deferred to the control/adorner phase). Carries the
    same WPF instance value accessors as `UIElement`.
  - `GlobalUsings.cs` — `DrawingContext = Avalonia.Media.DrawingContext` alias.
  - Verified by `Compatibility/_ShimValidation/ShimValidationShape.cs`, a local shape that
    mirrors `CuttingLine`'s render surface (`StreamGeometry` + `StreamGeometryContext` +
    `Shape.OnRender` + `DrawingContext.DrawEllipse`) and compiles/links against the shims.
- **Adorner primitives (control-independent, Phase 5)** — the reusable WPF adorner surface,
  provided as a shim (globbed, not linked):
  - `Compatibility/Wpf/Adorner.cs` — `System.Windows.Documents.Adorner` (over Avalonia
    `Control`): records the adorned element (WPF ctor), routes Avalonia's virtual `Render` to a
    WPF-style `OnRender(DrawingContext)` hook, exposes `IsClipEnabled` (→ `AdornerLayer.SetIsClipEnabled`),
    the WPF instance value accessors, `AddVisualChild`/`AddLogicalChild` (over `VisualChildren`/
    `LogicalChildren`), overridable `VisualChildrenCount`/`GetVisualChild`, and `TryFindResource`.
    Plus `System.Windows.Documents.AdornerLayer` — a façade over
    `Avalonia.Controls.Primitives.AdornerLayer` exposing WPF `GetAdornerLayer`/`Add`/`Remove`/`Update`
    (mapped onto the layer's `Children` + the `AdornedElement`/`IsClipEnabled` attached properties).
  - Verified by `Compatibility/_ShimValidation/ShimValidationAdorner.cs`, a local adorner that
    mirrors `FocusVisualAdorner`'s surface (adorned-element ctor, `IsHitTestVisible`/`IsEnabled`/
    `IsClipEnabled`, `OnRender`) and exercises the layer `Add`/`Update`/`Remove` API.
- **Control bases + first foundational control (Phase 6a)** — the reusable WPF templated-control
  bases, provided as a shim (globbed, not linked):
  - `Compatibility/Wpf/Controls.cs` — `System.Windows.Controls.ContentControl` (over Avalonia
    `ContentControl`) and `HeaderedContentControl` (over Avalonia
    `Primitives.HeaderedContentControl`). Each exposes the WPF control statics that upstream static
    ctors use — `DefaultStyleKeyProperty` (**record-only**; Avalonia themes by type, so overriding
    it is a no-op today and the style key is just captured for the theme phase) and
    `FocusableProperty` (wraps Avalonia's real `InputElement.FocusableProperty`, so
    `OverrideMetadata(type, new FrameworkPropertyMetadata(false))` actually flips the focus default)
    — plus the WPF value accessors and an `OnPropertyChanged` override that runs WPF coercion/change
    routing via `DependencyPropertyServices`.
  - `Compatibility/Wpf/DependencyProperty.cs` — added `DependencyProperty.FromExisting(...)` to wrap
    an already-existing Avalonia property (e.g. `InputElement.FocusableProperty`) in a WPF DP so
    `OverrideMetadata`/`AddOwner` work against built-in Avalonia properties.
  - **First control ported, linked verbatim from upstream:** `Nodes/KnotNode.cs`
    (`ContentControl` + `DefaultStyleKeyProperty`/`FocusableProperty` overrides only) — the
    least-coupled control, compiles unchanged against the new bases.
- **Template / style / presenter primitives (Phase 6b)** — the reusable templating surface the
  deferred templated controls (`Node`/`GroupingNode`/`StateNode`) consume:
  - `GlobalUsings.cs` — aliases for the types used only as DP value types / cast targets:
    `DataTemplate` -> `Avalonia.Controls.Templates.IDataTemplate`, `ControlTemplate` ->
    `IControlTemplate`, `Style` -> `Avalonia.Styling.Style`. (Interfaces are used for the templates
    so this core library stays free of a XAML-assembly dependency.)
  - `Compatibility/Wpf/Controls.cs` — `System.Windows.Controls.ContentPresenter` (over Avalonia
    `Presenters.ContentPresenter`) and `Border` (over Avalonia `Border`), each re-exposing the WPF
    DP statics upstream reuses via `AddOwner(...)` — `ContentPresenter.ContentProperty`/
    `ContentTemplateProperty` and `Border.CornerRadiusProperty` — wrapping the real Avalonia
    `StyledProperty` fields through `DependencyProperty.FromExisting`.
  - Verified by `Compatibility/_ShimValidation/ShimValidationTemplatedControl.cs`, a local control
    that mirrors how `StateNode`/`Node`/`GroupingNode` consume these primitives (AddOwner of
    `ContentTemplate`/`CornerRadius`, `DataTemplate`/`ControlTemplate`/`Style`-typed DPs,
    `DefaultStyleKeyProperty`/`FocusableProperty` overrides).

### 🚧 Remaining work (per subsystem)

Port order is bottom-up so lower layers compile before the controls that use them.

1. **Utilities & value helpers** — ✅ **Done (Phase 4a).** `BoxValue`, `MathExtensions`,
   `WeakReferenceCollection` ported; `DependencyObjectExtensions` **linked verbatim** from
   upstream (its hit-test + animation call sites now resolve through the
   `VisualTreeHelper.HitTest`/`HitTesting`/`Animation`/`RectExtensions` shims);
   `UnscaleTransformConverter` **copied & adapted** to `Utilities/` (WPF `Transform.Inverse` →
   `Matrix.TryInvert`), with its `Scale*Converter`s over the multi-value converter shim.
   ⏳ `SelectionHelper` is **deferred to the control phase** (control-coupled: references
   `ItemContainer` and `SelectionType`), as is `EditorGesturesExtensions` (needs `SelectionType`
   declared on `NodifyEditor`).
2. **Routed-event compatibility** — ✅ Done. Chose option (a): a shim
   `EventManager`/`RoutingStrategy`/`RoutedEvent`/`RoutedEventArgs` mapping onto
   Avalonia's `Avalonia.Interactivity` routed events (the shim `RoutedEvent`/
   `RoutedEventArgs` *derive from* the Avalonia types, so `RaiseEvent`/`AddHandler`/
   `RemoveHandler` work natively; only WPF's shorter `AddHandler` overloads are
   bridged). The `RoutedEventArgs`-derived event-arg classes are ported verbatim.
3. **Input / interactivity** — 🔶 In progress.
   - **Phase 3a (input shim) — ✅ Done.** WPF `System.Windows.Input` surface
     (`Mouse`/`Keyboard`/`ModifierKeys`/`Key`/`MouseButton`/`MouseAction`/
     `MouseButtonState`, the mouse/key event-arg hierarchy, and the
     `InputGesture`/`MouseGesture`/`KeyGesture` matching model) is implemented under
     `Compatibility/Wpf/Input/` (see "Done" above). Builds clean.
   - **Phase 3b (consumer ports) — 🔶 In progress.**
     - ✅ **Control-independent primitives linked and building:** the `Interactivity\`
       core (`InputProcessor`, `IInputHandler`, `InputElementState`,
       `InputElementStateStack` + its `InputElementState` partial) and the full gesture
       set (`MouseGesture`, `KeyComboGesture`, `MultiGesture`, `AllGestures`,
       `AnyGesture`, `InputGestureRef`, `EditorGestures`) are linked from upstream and
       compile against the shim (incl. the new `FrameworkElement.cs`/`Commands.cs`
       shims). `EventManager.RegisterClassHandler` used by `KeyComboGesture` is
       currently record-only.
     - ⏳ **Deferred to the control phase (control/device coupled):**
       `InputProcessor.Shared.cs` (hard-refs `NodifyEditor`/`ItemContainer`/`Connector`/
       `Minimap`/`BaseConnection`), `DragState.cs` and
       `InputElementStateStack.DragState.cs` (`Mouse`/`Stylus` devices,
       `NodifyEditor.ViewportUpdatedEvent`, `Element.ContextMenu`), the
       `Interactivity\KeyboardNavigation\` navigators, `EditorGesturesExtensions.cs`
       (needs `SelectionType` declared on `NodifyEditor`), and every control `*State`
       class. These arrive with the control `OnPointer*`/`OnKey*` overrides that feed
       `InputStateTracker` and with the `KeyComboGesture` runtime class-handler bridge.
4. **Shapes & rendering** — 🔶 **Shim layer done (Phase 4b); shape files deferred.**
   The reusable WPF-shaped rendering surface (`StreamGeometry`/`StreamGeometryContext`,
   `Shape` with the `CreateDefiningGeometry`/`OnRender` bridge, `DrawingContext` alias) is
   implemented and build-verified (see the Phase 4b entry under **Done**). The upstream shape
   `.cs` files themselves are **not linked yet** because every one turned out to be
   control-coupled, so they stay with the control/adorner phase to preserve bottom-up order:
   - `BaseConnection` (and the derived `Connection`/`LineConnection`/`CircuitConnection`/
     `StepConnection`) — implements `IKeyboardFocusTarget<FrameworkElement>`, calls
     `InputProcessor.AddSharedHandlers`, resolves a `ConnectionContainer`, drives an
     `AdornerLayer`/`FocusVisualAdorner`, and uses WPF-only geometry ops (`Geometry.Combine`,
     `GetWidenedPathGeometry`, `GetOutlinedPathGeometry`) plus `FormattedText`.
   - `CuttingLine` — its `IsOverElement` is `PendingConnection.IsOverElementProperty.AddOwner(...)`,
     i.e. it depends on the deferred `PendingConnection` connector control.
   The `Pen`/`Brush`/`FormattedText` draw shims and the WPF `DrawingContext.DrawRoundedRectangle`
   helper are only needed by the above, so they are also deferred to the control/adorner phase.
5. **Adorners** — 🔶 **Shim layer done (Phase 5); nested adorners deferred.**
   The reusable WPF `Adorner`/`AdornerLayer` surface is implemented over Avalonia's
   `Avalonia.Controls.Primitives.AdornerLayer` and build-verified (see the Phase 5 entry under
   **Done**). The actual adorners are **private nested classes of deferred controls** and
   additionally need `Pen`/WPF geometry ops/`HotKeyControl`/`Connector.Thumb`, so they arrive with
   their hosts in the control phase:
   - `FocusVisualAdorner` (nested in `BaseConnection`) — draws the focus outline via `Pen` +
     `Geometry.Combine`/`GetWidenedPathGeometry`/`GetOutlinedPathGeometry`.
   - `HotKeyAdorner` (nested in `PendingConnection`) — hosts a `HotKeyControl` visual child and
     positions it with `Connector.Thumb.TranslatePoint(...)`. (`HotKeyControl` itself is a small
     standalone `Control` but uses `DefaultStyleKey`/a template, so it too is control-phase.)
6. **Containers & core controls** — 🔶 **In progress (Phase 6a–6b).**
   The reusable WPF templated-control bases (`ContentControl`/`HeaderedContentControl` with
   `DefaultStyleKeyProperty` [record-only] + `FocusableProperty` + WPF accessors) and the
   template/style/presenter primitives (`DataTemplate`/`ControlTemplate`/`Style` aliases,
   `ContentPresenter`/`Border` DP-owner shims) are implemented and build-verified (see the Phase
   6a/6b entries under **Done**), and `KnotNode` is ported. Remaining controls each need their own
   increment for their still-missing dependencies:
   - `Node` — still needs `GroupStyle` + `ItemsControl.GroupStyle` (Avalonia has no `GroupStyle`);
     the presenter/template/style primitives it uses are now available.
   - `GroupingNode` — editor-coupled (`NodifyEditor`, `ItemContainer`, `Panel.ZIndex`,
     `Thumb.Drag*` events/`DragDeltaEventHandler`, `EditorGestures`).
   - `ItemContainer`, `DecoratorContainer`, `ConnectionContainer` — editor-coupled
     (`NodifyEditor`, `INodifyCanvasItem`, `IKeyboardFocusTarget<T>`, container `*State` classes,
     `KeyboardNavigation` attached properties, `Selector.IsSelectedProperty`).
   - `Connector`, `PendingConnection`, `StateNode` (derives from `Connector`) — connector stack;
     `PendingConnection` also hosts the deferred `HotKeyAdorner`/`HotKeyControl`.
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
