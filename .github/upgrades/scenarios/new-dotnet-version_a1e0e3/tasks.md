# Nodify Examples .NET 10 + Avalonia Migration Tasks

## Overview

This document tracks the migration of all Examples projects from WPF to Avalonia while upgrading to .NET 10. All 6 projects will be upgraded and ported simultaneously in a single atomic operation.

**Progress**: 1/2 tasks complete (50%) ![0%](https://progress-bar.xyz/50)

---

## Tasks

### [✓] TASK-001: Atomic WPF-to-Avalonia migration and .NET 10 upgrade *(Completed: 2026-02-10 14:55)*
**References**: Plan §Project-by-Project Plans, Plan §Package Update Reference, Plan §Breaking Changes Catalog

- [✓] (1) Update project files for all 6 projects per Plan §Project-by-Project Plans (Nodify.Shared, Nodify.Calculator, Nodify.Playground, Nodify.Shapes, Nodify.StateMachine): remove `<UseWPF>`, remove WindowsDesktop SDK references, set `TargetFramework` to `net10.0`
- [✓] (2) All project files updated to `net10.0` (**Verify**)
- [✓] (3) Add Avalonia package references per Plan §Package Update Reference: `Avalonia` 11.3.11, `Avalonia.Desktop` 11.3.11, `Avalonia.Themes.Fluent` 11.3.11 to appropriate projects
- [✓] (4) All Avalonia package references added (**Verify**)
- [✓] (5) Convert Nodify.Shared WPF resources to Avalonia per Plan §Nodify.Shared Migration Steps: replace DependencyProperty with AvaloniaProperty, convert resource dictionaries (Themes/*.xaml) to Avalonia styles, update URIs from `pack://application` to `avares://`
- [✓] (6) Convert application XAML files per Plan §Project-by-Project Plans: App.xaml → App.axaml, MainWindow.xaml → MainWindow.axaml, view files to .axaml equivalents for all 5 apps
- [✓] (7) Update application bootstrap per Plan §Project-by-Project Plans: replace WPF App.xaml.cs with Avalonia App.axaml.cs and Program.cs using `ClassicDesktopStyleApplicationLifetime` for all apps
- [✓] (8) Update code-behind namespaces and API calls per Plan §Breaking Changes Catalog: replace `System.Windows.*` with `Avalonia.*` equivalents, update routed events, converters, input handling
- [✓] (9) Restore all dependencies
- [✓] (10) All dependencies restored successfully (**Verify**)
- [✓] (11) Build solution and fix all compilation errors per Plan §Breaking Changes Catalog (WPF→Avalonia API replacements, XAML syntax differences, resource loading changes)
- [✓] (12) Solution builds with 0 errors (**Verify**)

---

### [▶] TASK-002: Final commit
**References**: Plan §Source Control Strategy

- [ ] (1) Commit all changes with message: "TASK-002: Complete WPF-to-Avalonia migration and .NET 10 upgrade"

---


