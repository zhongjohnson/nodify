# Editor gestures

Nodify uses `EditorGestures` to map mouse and keyboard input to built-in editor operations. The default mappings live in `EditorGestures.Mappings`. A `NodifyEditor` can also receive its own mappings through `InputGestures`.

Use per-editor mappings when one editor needs different interaction rules than the rest of the application. Use `EditorGestures.Mappings` only when you want to change the application-wide defaults.

## Per-editor mappings

Create an `EditorGestures` instance in your view model and bind it to `NodifyEditor.InputGestures`.

```csharp
using Nodify.Interactivity;
using System.Windows.Input;

public class EditorViewModel
{
    public EditorGestures Gestures { get; } = new EditorGestures();

    public EditorViewModel()
    {
        Gestures.Editor.Pan.Value = new AnyGesture(
            new Nodify.Interactivity.MouseGesture(MouseAction.MiddleClick),
            new Nodify.Interactivity.MouseGesture(MouseAction.LeftClick, Key.Space));

        Gestures.Editor.ZoomModifierKey = ModifierKeys.Control;
        Gestures.Editor.PanWithMouseWheel = true;
    }
}
```

```xml
<nodify:NodifyEditor ItemsSource="{Binding Nodes}"
                     InputGestures="{Binding Gestures}" />
```

If `InputGestures` is not set, the editor uses `EditorGestures.Mappings`.

## Changing global defaults

Change `EditorGestures.Mappings` when every editor should use the same mappings.

```csharp
using Nodify.Interactivity;
using System.Windows.Input;

EditorGestures.Mappings.Editor.ZoomModifierKey = ModifierKeys.Control;
EditorGestures.Mappings.Editor.PanWithMouseWheel = true;
EditorGestures.Mappings.Connection.Disconnect.Unbind();
```

Global defaults can be changed whenever the application needs to switch its default editor interactions. Existing editors that use `EditorGestures.Mappings` pick up those changes through the shared mapping instance.

## Replacing a gesture

Most gesture properties are `InputGestureRef` values. Replace the `Value` property instead of replacing the reference.

```csharp
Gestures.Editor.Cutting.Value =
    new Nodify.Interactivity.MouseGesture(MouseAction.LeftClick, ModifierKeys.Alt);
```

Use `AnyGesture` when more than one input should trigger the same action.

```csharp
Gestures.Editor.Pan.Value = new AnyGesture(
    new Nodify.Interactivity.MouseGesture(MouseAction.RightClick),
    new Nodify.Interactivity.MouseGesture(MouseAction.MiddleClick));
```

Use `AllGestures` when every gesture in the list must match the same input event before the action is triggered.

Use `Unbind()` to disable an action.

```csharp
Gestures.Editor.Cutting.Unbind();
Gestures.Connector.Connect.Unbind();
Gestures.Connection.Split.Unbind();
```

## Selection gestures

Selection is made of several related gestures:

- `Replace`
- `Append`
- `Remove`
- `Invert`
- `Cancel`
- `Select`

Use `SelectionGestures` when you want to move the full selection group from one mouse button to another.

```csharp
Gestures.Editor.Selection.Apply(
    new EditorGestures.SelectionGestures(MouseAction.RightClick));
```

If item containers should use the same selection behavior, apply the same mapping to `ItemContainer.Selection` and update `ItemContainer.Drag`.

```csharp
Gestures.Editor.Selection.Apply(
    new EditorGestures.SelectionGestures(MouseAction.RightClick));

Gestures.ItemContainer.Selection.Apply(Gestures.Editor.Selection);
Gestures.ItemContainer.Drag.Value = new AnyGesture(
    Gestures.ItemContainer.Selection.Replace.Value,
    Gestures.ItemContainer.Selection.Remove.Value,
    Gestures.ItemContainer.Selection.Append.Value,
    Gestures.ItemContainer.Selection.Invert.Value);
```

This keeps selecting and dragging in sync.

## Read-only mode

For a read-only editor, unbind gestures that can modify graph state and leave navigation gestures enabled.

```csharp
public static void MakeReadOnly(EditorGestures gestures)
{
    gestures.Editor.Selection.Unbind();
    gestures.Editor.SelectAll.Unbind();
    gestures.Editor.Cutting.Unbind();
    gestures.Editor.PushItems.Unbind();

    gestures.Editor.Keyboard.ToggleSelected.Unbind();
    gestures.Editor.Keyboard.DragSelection.Unbind();
    gestures.Editor.Keyboard.DeselectAll.Unbind();

    gestures.ItemContainer.Selection.Unbind();
    gestures.ItemContainer.Drag.Unbind();

    gestures.Connection.Disconnect.Unbind();
    gestures.Connection.Split.Unbind();
    gestures.Connection.Selection.Unbind();

    gestures.Connector.Connect.Unbind();
    gestures.Connector.Disconnect.Unbind();
}
```

The exact list depends on what your editor is allowed to do. For example, a viewer may keep panning, zooming, keyboard navigation, and minimap navigation enabled.

## Copying and restoring mappings

Use `Apply()` to copy one gesture set into another. This is useful for editor modes such as "locked", "drawing", or "connection editing".

```csharp
private readonly EditorGestures _activeGestures = new EditorGestures();
private readonly EditorGestures _backupGestures = new EditorGestures();

public EditorGestures ActiveGestures => _activeGestures;

public void LockEditing()
{
    _backupGestures.Apply(_activeGestures);
    MakeReadOnly(_activeGestures);
}

public void UnlockEditing()
{
    _activeGestures.Apply(_backupGestures);
}
```

Bind `ActiveGestures` to `InputGestures`.

## Common gesture targets

The most common groups are:

- `Editor`: panning, zooming, selecting, cutting, pushing, keyboard navigation, fit to screen, reset viewport.
- `ItemContainer`: selecting and dragging node containers.
- `Connector`: starting pending connections and disconnecting connectors.
- `Connection`: selecting, splitting, and disconnecting connections.
- `GroupingNode`: grouped movement behavior and content selection.
- `Minimap`: panning, dragging the viewport, zooming, reset viewport.

For the full API, see [`EditorGestures`](api/Nodify_Interactivity_EditorGestures).
