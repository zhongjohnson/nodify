# .NET 10 + WPF-to-Avalonia Migration Plan (Examples)

## Table of Contents
- [Executive Summary](#executive-summary)
- [Migration Strategy](#migration-strategy)
- [Detailed Dependency Analysis](#detailed-dependency-analysis)
- [Project-by-Project Plans](#project-by-project-plans)
- [Package Update Reference](#package-update-reference)
- [Breaking Changes Catalog](#breaking-changes-catalog)
- [Testing & Validation Strategy](#testing--validation-strategy)
- [Risk Management](#risk-management)
- [Complexity & Effort Assessment](#complexity--effort-assessment)
- [Source Control Strategy](#source-control-strategy)
- [Success Criteria](#success-criteria)

## Executive Summary
The goal is to port all `Examples` projects from WPF to Avalonia while upgrading them to `.NET 10` and continuing to use the Avalonia-ported `Nodify` library in this solution.

### Scope & Metrics
- **Projects in scope**: 6 total (5 `Examples` apps + `Nodify.Shared`), plus dependency on `Nodify`.
- **Target framework**: `net10.0` (apps to `net10.0`/`net10.0-windows` as applicable for Avalonia).
- **Total code files**: 248
- **Total LOC**: 23,626
- **Estimated LOC to modify**: 489+ (primarily in `Nodify.Shared`)
- **Package updates**: 0 required (all compatible per assessment)
- **Security vulnerabilities**: none reported

### Selected Strategy
**All-At-Once Strategy** — Upgrade and port all `Examples` projects simultaneously to avoid mixed WPF/Avalonia states and ensure consistent UI behavior with the Avalonia-based `Nodify` library.

**Rationale**:
- Simple dependency structure (depth 2, no cycles)
- Single shared UI library (`Nodify.Shared`) used by all apps
- Primary risk concentrated in one project (`Nodify.Shared`)

### Complexity Classification
**Medium complexity**: 6 projects, shallow dependencies, one medium-risk project with 489 binary-incompatible API hits.

### Iteration Strategy
Phase-based detailing with a single atomic upgrade phase (All-At-Once) followed by testing and validation.

## Migration Strategy
### Approach Selection
**All-At-Once Strategy**: Upgrade and port all `Examples` projects simultaneously to `.NET 10` and Avalonia. This avoids a mixed WPF/Avalonia UI state and ensures shared styles/themes (`Nodify.Shared`) remain consistent across applications.

### Framework & UI Porting Strategy
- **Target framework**: consolidate the `Examples` projects to **`net10.0`** (Avalonia is cross-platform; avoid `-windows` TFM unless Windows-only is required).
- **Remove WPF-specific settings**: remove `<UseWPF>` and `Microsoft.NET.Sdk.WindowsDesktop` usage from `Examples` projects.
- **Adopt Avalonia app model**:
  - Replace `App.xaml`/`MainWindow.xaml` WPF artifacts with Avalonia `App.axaml`/`MainWindow.axaml` equivalents.
  - Replace WPF `ResourceDictionary` and `pack://application` URI usage with Avalonia `avares://` URIs.
  - Update styles/themes to Avalonia equivalents (include Avalonia Fluent or Simple theme as needed).
- **Use `Nodify` (Avalonia port)** for node editor controls and styling. Ensure `Examples` reference the Avalonia-compatible `Nodify` project already in the solution.

### Dependency Ordering (Within the Atomic Operation)
- Update `Nodify.Shared` first (shared styles and controls), then port application views and windows (`Calculator`, `Playground`, `Shapes`, `StateMachine`) that depend on it.
- `Nodify` is already on `net10.0` and should remain the dependency root.

### Parallel vs Sequential Decisions
- **Parallel**: UI view conversion per app can be done in parallel once shared Avalonia styles/utilities are updated.
- **Sequential**: Shared UI primitives (`Nodify.Shared`) must be updated before app views to avoid cascading rework.

## Detailed Dependency Analysis
### Dependency Graph Summary
- **Leaf dependencies**: `Nodify` (Avalonia port, `net10.0`), `Examples/Nodify.Shared` (shared UI helpers)
- **Applications** (depend on both `Nodify` and `Nodify.Shared`):
  - `Examples/Nodify.Calculator`
  - `Examples/Nodify.Playground`
  - `Examples/Nodify.Shapes`
  - `Examples/Nodify.StateMachine`
- **No circular dependencies** detected.
- **Dependency depth**: 2

### Migration Groupings (All-At-Once)
All projects are upgraded in a single coordinated operation, but the internal ordering should still respect dependencies:
- **Foundation set (libraries)**: `Nodify`, `Examples/Nodify.Shared`
- **Application set**: `Examples/Nodify.Calculator`, `Examples/Nodify.Playground`, `Examples/Nodify.Shapes`, `Examples/Nodify.StateMachine`

### Critical Path
`Nodify.Shared` is the most impacted project (489 binary incompatible APIs) and is the primary blocker for application-level Avalonia UI ports.

## Project-by-Project Plans
### Examples/Nodify.Shared
**Current State**: WPF library, TFMs `net9-windows;net8-windows;net6-windows;net5-windows;netcoreapp3.1;net48;net472`, 30 files, 1,935 LOC, 489 binary-incompatible API hits.
**Target State**: Avalonia-compatible shared library targeting `net10.0`.
**Migration Steps**:
1. **Project file**: switch to `Microsoft.NET.Sdk`, set `TargetFramework` to `net10.0`, remove `<UseWPF>` and WindowsDesktop SDK usage.
2. **Package references**: add Avalonia packages aligned to `Nodify`:
   - `Avalonia` `11.3.11`
   - `Avalonia.Themes.Fluent` `11.3.11`
3. **XAML resources**: convert WPF `ResourceDictionary` files to Avalonia styles and update URIs (`pack://application:,,,/` → `avares://`).
   - Files: `Themes/Brushes.xaml`, `Themes/Controls.xaml`, `Themes/Dark.xaml`, `Themes/Generic.xaml`, `Themes/Icons.xaml`, `Themes/Light.xaml`, `Themes/Nodify.xaml`.
4. **DependencyProperty → AvaloniaProperty**: replace WPF `DependencyProperty` with Avalonia `StyledProperty`/`DirectProperty` in custom controls.
   - Target classes: `EditableTextBlock`, `ResizablePanel`, `ResizableContainer`, `TabControlEx`, `TabItemEx`, `Swatches`, `ThemeManager`.
5. **Events and input**: replace WPF routed events and `CommandManager.RequerySuggested` usage with Avalonia routed events and explicit command requery logic.
6. **Converters and bindings**: move from WPF `IValueConverter`/`IMultiValueConverter` to Avalonia `Avalonia.Data.Converters` equivalents.
7. **Color/Brush/Visibility**: update `System.Windows.Media.Color`, `SolidColorBrush`, `Visibility` to Avalonia equivalents.

**Testing Strategy**:
- Build `Nodify.Shared` against `net10.0`.
- Validate style dictionaries load in at least one app (`Playground`) without resource errors.

**Validation Checklist**:
- [ ] `Nodify.Shared` builds on `net10.0`
- [ ] All resource dictionaries resolve via `avares://`
- [ ] Custom control templates render without missing styles

### Examples/Nodify.Calculator
**Current State**: WPF app, TFMs `net9-windows;net8-windows;net6-windows;net5-windows;netcoreapp3.1;net48;net472`, 30 files, 1,400 LOC.
**Target State**: Avalonia app targeting `net10.0`.
**Migration Steps**:
1. **Project file**: use `Microsoft.NET.Sdk`, set `TargetFramework` to `net10.0`, remove `<UseWPF>`.
2. **Package references**: add Avalonia desktop packages (align to `11.3.11`): `Avalonia`, `Avalonia.Desktop`, `Avalonia.Themes.Fluent`.
3. **Application bootstrap**: replace `App.xaml`/`App.xaml.cs` with Avalonia `App.axaml`/`App.axaml.cs` and `Program.cs` using `ClassicDesktopStyleApplicationLifetime`.
4. **View conversion**: port WPF XAML to Avalonia:
   - `MainWindow.xaml` → `MainWindow.axaml`
   - `EditorView.xaml` → `EditorView.axaml`
   - `OperationsMenuView.xaml` → `OperationsMenuView.axaml`
5. **Code-behind**: update code-behind namespaces to Avalonia controls and events.
6. **Resource URIs**: replace `pack://application` with `avares://` for `Nodify` and shared styles.

**Testing Strategy**:
- Build app and verify window loads with node editor content.

**Validation Checklist**:
- [ ] Application launches without XAML parse errors
- [ ] Editor view renders and responds to input

### Examples/Nodify.Playground
**Current State**: WPF app, TFMs `net9-windows;net8-windows;net6-windows;net5-windows;netcoreapp3.1;net48;net472`, 31 files, 2,961 LOC.
**Target State**: Avalonia app targeting `net10.0`.
**Migration Steps**:
1. **Project file**: update to `net10.0`, remove WPF settings.
2. **Package references**: add `Avalonia`, `Avalonia.Desktop`, `Avalonia.Themes.Fluent` `11.3.11`.
3. **Application bootstrap**: migrate `App.xaml`/`App.xaml.cs` to Avalonia `App.axaml`/`App.axaml.cs` and `Program.cs`.
4. **View conversion**:
   - `MainWindow.xaml` → `MainWindow.axaml`
   - `Editor/NodifyEditorView.xaml` → `Editor/NodifyEditorView.axaml`
   - `EditorSettingsView.xaml` → `EditorSettingsView.axaml`
   - `PointEditorView.xaml` → `PointEditorView.axaml`
   - `SettingsView.xaml` → `SettingsView.axaml`
5. **Theme conversion**:
   - `Themes/Brushes.xaml`, `Dark.xaml`, `Light.xaml`, `Nodify.xaml` → Avalonia style includes.
6. **Input and commands**: update WPF input bindings and command requery patterns to Avalonia equivalents.

**Testing Strategy**:
- Build and open the editor window, verify node editor interactions and settings panels.

**Validation Checklist**:
- [ ] Styles load without missing resource keys
- [ ] Editor and settings views render and respond to mouse/keyboard

### Examples/Nodify.Shapes
**Current State**: WPF app, TFMs `net9-windows;net8-windows;net6-windows;net5-windows;netcoreapp3.1;net48;net472`, 23 files, 1,008 LOC.
**Target State**: Avalonia app targeting `net10.0`.
**Migration Steps**:
1. **Project file**: update to `net10.0`, remove WPF settings.
2. **Package references**: add `Avalonia`, `Avalonia.Desktop`, `Avalonia.Themes.Fluent` `11.3.11`.
3. **Application bootstrap**: migrate `App.xaml`/`App.xaml.cs` to Avalonia `App.axaml`/`App.axaml.cs` and `Program.cs`.
4. **View conversion**:
   - `MainWindow.xaml` → `MainWindow.axaml`
   - `Canvas/CanvasView.xaml` → `Canvas/CanvasView.axaml`
5. **Code-behind**: update control references to Avalonia controls and input events.

**Testing Strategy**:
- Build and verify canvas rendering and shape interactions.

**Validation Checklist**:
- [ ] Canvas loads with shapes
- [ ] Dragging/resizing works as expected

### Examples/Nodify.StateMachine
**Current State**: WPF app, TFMs `net9-windows;net8-windows;net6-windows;net5-windows;netcoreapp3.1;net48;net472`, 36 files, 1,893 LOC.
**Target State**: Avalonia app targeting `net10.0`.
**Migration Steps**:
1. **Project file**: update to `net10.0`, remove WPF settings.
2. **Package references**: add `Avalonia`, `Avalonia.Desktop`, `Avalonia.Themes.Fluent` `11.3.11`.
3. **Application bootstrap**: migrate `App.xaml`/`App.xaml.cs` to Avalonia `App.axaml`/`App.axaml.cs` and `Program.cs`.
4. **View conversion**:
   - `MainWindow.xaml` → `MainWindow.axaml`
   - `BlackboardKeyEditorView.xaml` → `BlackboardKeyEditorView.axaml`
5. **Theme conversion**:
   - `Themes/Brushes.xaml`, `Dark.xaml`, `Light.xaml`, `Nodify.xaml` → Avalonia style includes.

**Testing Strategy**:
- Build and verify state machine editor and blackboard views.

**Validation Checklist**:
- [ ] State machine editor renders
- [ ] Blackboard editor bindings update correctly

### Nodify (Dependency)
**Current State**: Avalonia-port library, `net10.0`, 14,429 LOC.
**Target State**: No change; ensure compatibility with Examples after port.
**Migration Steps**:
1. Confirm `Examples` projects reference `Nodify` project directly (no WPF package references).
2. Align Avalonia package versions to `Nodify` (`11.3.11`) to avoid conflicts.

**Testing Strategy**:
- Build `Nodify` with `Examples` to ensure consistent API surface.

**Validation Checklist**:
- [ ] No package version conflicts in restore

## Package Update Reference
### Common Package Additions (Avalonia UI)
| Package | Current | Target | Projects Affected | Reason |
| --- | --- | --- | --- | --- |
| `Avalonia` | (new) | `11.3.11` | All `Examples` apps + `Nodify.Shared` | Required for Avalonia UI framework |
| `Avalonia.Desktop` | (new) | `11.3.11` | `Nodify.Calculator`, `Nodify.Playground`, `Nodify.Shapes`, `Nodify.StateMachine` | Desktop lifetime for Avalonia apps |
| `Avalonia.Themes.Fluent` | (new) | `11.3.11` | All `Examples` apps + `Nodify.Shared` | Fluent theme resources |

### Existing Packages (No Update Required)
| Package | Current | Target | Projects Affected | Reason |
| --- | --- | --- | --- | --- |
| `StringMath` | `4.1.3` | `4.1.3` | `Nodify.Calculator` | Compatible per assessment |

## Breaking Changes Catalog
### WPF → Avalonia API Replacements (Primary)
Based on assessment, `Nodify.Shared` accounts for **479 binary incompatible** and **10 behavioral** API changes. Key replacements include:
- `System.Windows.DependencyProperty` → `AvaloniaProperty` (`StyledProperty`/`DirectProperty`).
- `FrameworkPropertyMetadata` → `AvaloniaPropertyMetadata`/`StyledPropertyMetadata`.
- `System.Windows.Visibility` → `Avalonia.Controls.Visibility`.
- `System.Windows.ResourceDictionary` → `Avalonia.Styling.Styles` / `StyleInclude`.
- `System.Windows.RoutedEvent` and `UIElement.AddHandler` → Avalonia routed events (`AddHandler` overloads differ).
- `System.Windows.Input.Key`/`KeyEventArgs` → `Avalonia.Input.Key`/`KeyEventArgs`.
- `System.Windows.Media.Color`/`SolidColorBrush` → `Avalonia.Media.Color`/`SolidColorBrush`.
- `CommandManager.RequerySuggested` → explicit command requery or `INotifyCanExecuteChanged` pattern.

### Behavioral Changes (.NET 10)
- `System.Uri` behavior changes: review any parsing or absolute/relative URI handling used for resource resolution.

### XAML/Resource Loading Changes
- Replace WPF `pack://application:,,,/` URIs with Avalonia `avares://` URIs.
- WPF `ControlTemplate` and `Style` syntax must be converted to Avalonia `Styles` and `ControlTheme` where required.

## Testing & Validation Strategy
### Build Validation (All-At-Once)
- Restore and build the entire solution after all project and package updates.
- Ensure no compilation errors and no missing Avalonia resources.

### Application Validation (Per App)
Because there are no listed automated test projects, validate through build and basic UI smoke checks:
- `Nodify.Calculator`: launch and verify editor renders, operations list loads.
- `Nodify.Playground`: launch, verify editor, settings, and node interactions.
- `Nodify.Shapes`: launch, verify canvas rendering and interaction.
- `Nodify.StateMachine`: launch, verify state editor and blackboard editor.

### Validation Checklist (Per App)
- [ ] Application builds with no warnings or errors
- [ ] Main window loads without XAML parse errors
- [ ] Resource dictionaries load successfully
- [ ] Core editor interactions (selection, drag, zoom) function

## Risk Management
### High-Risk Changes
| Area | Risk Level | Description | Mitigation |
| --- | --- | --- | --- |
| `Nodify.Shared` UI primitives | Medium | 489 binary-incompatible WPF APIs need Avalonia replacements | Convert properties/events first; validate in a single app before porting others |
| Resource dictionaries & styles | Medium | WPF style/resource model differs from Avalonia | Convert styles into `Styles`/`StyleInclude`; verify in `Playground` |
| Input/command routing | Medium | WPF `CommandManager` and routed events differ | Replace with Avalonia input bindings and explicit `CanExecute` triggers |
| App bootstrap changes | Low | New Avalonia startup pipeline | Use standard Avalonia `Program.cs`/`App.axaml` patterns |

### Security Vulnerabilities
- No security vulnerabilities reported in assessment.

### Contingency & Rollback
- If Avalonia styling or control templates fail broadly, isolate in `Nodify.Shared` and revert to a minimal set of styles to restore app startup.
- Keep a single upgrade branch (`upgrade-to-NET10`) to simplify rollback via branch reset if needed.

## Complexity & Effort Assessment
### Per-Project Complexity
| Project | Complexity | Key Drivers |
| --- | --- | --- |
| `Examples/Nodify.Shared` | Medium | High API delta (DependencyProperty, styles, resources) |
| `Examples/Nodify.Calculator` | Low | Limited view surface |
| `Examples/Nodify.Playground` | Medium | Largest view count and theme surface |
| `Examples/Nodify.Shapes` | Low | Smallest view surface |
| `Examples/Nodify.StateMachine` | Medium | Multiple specialized views |
| `Nodify` (dependency) | Low | Already Avalonia + `net10.0` |

### Phase Complexity (All-At-Once)
- **Atomic upgrade phase**: Medium (UI porting across 5 apps + shared library)
- **Testing/validation phase**: Medium (manual UI validation across apps)

## Source Control Strategy
- **Branching**: Perform all work on `upgrade-to-NET10`.
- **Commit Strategy**: Prefer a **single atomic commit** for the entire WPF→Avalonia and `.NET 10` port to align with All-At-Once strategy.
- **Review**: Single PR targeting the main branch, with checklist:
  - All `Examples` projects target `net10.0`
  - WPF dependencies removed
  - Avalonia resources load correctly

## Success Criteria
### Technical Criteria
- All `Examples` projects target `net10.0` and build successfully.
- All WPF-specific references removed from `Examples` projects.
- Avalonia packages aligned to `11.3.11`.
- All resource dictionaries resolve via `avares://` URIs.
- No compilation errors or missing resources.

### Quality Criteria
- Each application launches and renders its primary editor view without exceptions.
- Core interactions (selection, drag, zoom, keyboard input) function in each app.

### Process Criteria
- All changes performed in a single coordinated upgrade on `upgrade-to-NET10`.
- One atomic commit represents the upgrade.
