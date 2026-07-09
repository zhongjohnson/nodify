# Theming

Nodify ships with three themes:

- `Dark.xaml`
- `Light.xaml`
- `Nodify.xaml`

Each theme merges the default control styles and defines the color resources used by those styles. The default brushes in `Nodify/Themes/Brushes.xaml` use `DynamicResource` references, so color changes can be applied after the selected theme is merged.

## Using a built-in theme

Merge one built-in theme in `App.xaml`.

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="pack://application:,,,/Nodify;component/Themes/Dark.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

Use `Light.xaml` or `Nodify.xaml` when you want one of the other palettes.

## Customizing an existing theme

To keep the default templates and change only a few colors, merge your overrides after the selected built-in theme.

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="pack://application:,,,/Nodify;component/Themes/Dark.xaml" />

            <ResourceDictionary>
                <Color x:Key="NodifyEditor.BackgroundColor">#20242A</Color>
                <Color x:Key="Node.BackgroundColor">#2F3540</Color>
                <Color x:Key="Node.HeaderColor">#1D222A</Color>
                <Color x:Key="Connection.StrokeColor">#4DA3FF</Color>
                <Color x:Key="PendingConnection.StrokeColor">#4DA3FF</Color>
                <Color x:Key="ItemContainer.SelectedColor">#FFB020</Color>
            </ResourceDictionary>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

The most commonly customized color keys are:

- `NodifyEditor.BackgroundColor`
- `NodifyEditor.ForegroundColor`
- `NodifyEditor.SelectionRectangleColor`
- `ItemContainer.BorderColor`
- `ItemContainer.SelectedColor`
- `Node.BackgroundColor`
- `Node.ContentColor`
- `Node.ForegroundColor`
- `Node.HeaderForegroundColor`
- `Node.HeaderColor`
- `Node.FooterColor`
- `Node.BorderColor`
- `Connector.BackgroundColor`
- `Connector.ForegroundColor`
- `Connector.BorderColor`
- `NodeInput.BackgroundColor`
- `NodeInput.ForegroundColor`
- `NodeInput.BorderColor`
- `NodeOutput.BackgroundColor`
- `NodeOutput.ForegroundColor`
- `NodeOutput.BorderColor`
- `Connection.StrokeColor`
- `LineConnection.StrokeColor`
- `CircuitConnection.StrokeColor`
- `StepConnection.StrokeColor`
- `PendingConnection.StrokeColor`
- `Minimap.BackgroundColor`
- `Minimap.ViewportStrokeColor`
- `Minimap.ViewportBackgroundColor`
- `MinimapItem.BackgroundColor`
- `HotKey.BackgroundColor`
- `HotKey.ForegroundColor`
- `HotKey.BorderColor`

## Creating a custom theme

A complete custom theme should merge Nodify's default control styles and define the color keys consumed by those styles.

Create a resource dictionary in your application, for example `Themes/MyTheme.xaml`:

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <ResourceDictionary.MergedDictionaries>
        <ResourceDictionary Source="pack://application:,,,/Nodify;component/Themes/Controls.xaml" />
    </ResourceDictionary.MergedDictionaries>

    <Color x:Key="NodifyEditor.BackgroundColor">#181B20</Color>
    <Color x:Key="NodifyEditor.ForegroundColor">#F4F7FB</Color>
    <Color x:Key="NodifyEditor.SelectionRectangleColor">#4DA3FF</Color>
    <Color x:Key="NodifyEditor.PushedAreaColor">#637083</Color>
    <Color x:Key="NodifyEditor.CuttingLineColor">#FF4D4D</Color>

    <Color x:Key="ItemContainer.BorderColor">#4DA3FF</Color>
    <Color x:Key="ItemContainer.SelectedColor">#FFB020</Color>

    <Color x:Key="Node.BackgroundColor">#2A3038</Color>
    <Color x:Key="Node.ContentColor">#2A3038</Color>
    <Color x:Key="Node.ForegroundColor">#F4F7FB</Color>
    <Color x:Key="Node.HeaderForegroundColor">#F4F7FB</Color>
    <Color x:Key="Node.HeaderColor">#1F252D</Color>
    <Color x:Key="Node.FooterColor">#1F252D</Color>
    <Color x:Key="Node.BorderColor">Transparent</Color>

    <Color x:Key="StateNode.BackgroundColor">#1F252D</Color>
    <Color x:Key="StateNode.ForegroundColor">#F4F7FB</Color>
    <Color x:Key="StateNode.BorderColor">#3D4654</Color>
    <Color x:Key="StateNode.HighlightColor">#BFD7FF</Color>

    <Color x:Key="GroupingNode.BackgroundColor">#252B33</Color>
    <Color x:Key="GroupingNode.ForegroundColor">#F4F7FB</Color>
    <Color x:Key="GroupingNode.HeaderColor">#1F252D</Color>
    <Color x:Key="GroupingNode.BorderColor">Transparent</Color>

    <Color x:Key="KnotNode.BackgroundColor">Transparent</Color>
    <Color x:Key="KnotNode.ForegroundColor">#4DA3FF</Color>
    <Color x:Key="KnotNode.BorderColor">Transparent</Color>

    <Color x:Key="Connector.BackgroundColor">Transparent</Color>
    <Color x:Key="Connector.ForegroundColor">#F4F7FB</Color>
    <Color x:Key="Connector.BorderColor">#4DA3FF</Color>

    <Color x:Key="NodeInput.BackgroundColor">#2A3038</Color>
    <Color x:Key="NodeInput.ForegroundColor">#F4F7FB</Color>
    <Color x:Key="NodeInput.BorderColor">#4DA3FF</Color>

    <Color x:Key="NodeOutput.BackgroundColor">#2A3038</Color>
    <Color x:Key="NodeOutput.ForegroundColor">#F4F7FB</Color>
    <Color x:Key="NodeOutput.BorderColor">#4DA3FF</Color>

    <Color x:Key="Connection.StrokeColor">#4DA3FF</Color>
    <Color x:Key="LineConnection.StrokeColor">#4DA3FF</Color>
    <Color x:Key="CircuitConnection.StrokeColor">#4DA3FF</Color>
    <Color x:Key="StepConnection.StrokeColor">#4DA3FF</Color>

    <Color x:Key="PendingConnection.StrokeColor">#4DA3FF</Color>
    <Color x:Key="PendingConnection.BorderColor">#111418</Color>
    <Color x:Key="PendingConnection.ForegroundColor">#F4F7FB</Color>
    <Color x:Key="PendingConnection.BackgroundColor">#181B20</Color>

    <Color x:Key="Minimap.BackgroundColor">#111418</Color>
    <Color x:Key="Minimap.ViewportStrokeColor">#637083</Color>
    <Color x:Key="Minimap.ViewportBackgroundColor">#637083</Color>
    <Color x:Key="MinimapItem.BackgroundColor">#4DA3FF</Color>

    <Color x:Key="HotKey.BackgroundColor">#F4F7FB</Color>
    <Color x:Key="HotKey.ForegroundColor">#111418</Color>
    <Color x:Key="HotKey.BorderColor">#4DA3FF</Color>
</ResourceDictionary>
```

Then merge it in `App.xaml`:

```xml
<ResourceDictionary Source="pack://application:,,,/MyApplication;component/Themes/MyTheme.xaml" />
```

## Overriding a control style

Use a style override when a color resource is not enough. Base the style on the default style so the existing template and behavior are preserved.

Add the Nodify XML namespace to the file that contains the style: `xmlns:nodify="https://miroiu.github.io/nodify"`.

```xml
<Style TargetType="{x:Type nodify:HotKeyControl}"
       BasedOn="{StaticResource {x:Type nodify:HotKeyControl}}">
    <Setter Property="Width" Value="18" />
    <Setter Property="Height" Value="18" />
    <Setter Property="Background" Value="#FFB020" />
</Style>
```

This pattern is useful for small visual changes such as connector size, hot key badges, minimap viewport styling, or focus visuals.

## Switching themes at runtime

Themes are just resource dictionaries, so runtime switching is done by replacing the active theme dictionary.

```csharp
private ResourceDictionary? _activeTheme;

public void SetNodifyTheme(string themeName)
{
    if (_activeTheme != null)
    {
        Application.Current.Resources.MergedDictionaries.Remove(_activeTheme);
    }

    _activeTheme = new ResourceDictionary
    {
        Source = new Uri(
            $"pack://application:,,,/Nodify;component/Themes/{themeName}.xaml",
            UriKind.Absolute)
    };

    Application.Current.Resources.MergedDictionaries.Insert(0, _activeTheme);
}
```

If you also use application-specific resource dictionaries, keep them merged after the selected Nodify theme so your overrides continue to win.
