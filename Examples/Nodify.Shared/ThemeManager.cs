using System;
using System.Collections.Generic;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using System.Windows.Input;

namespace Nodify
{
    public static class ThemeManager
    {
        private static readonly string? _assemblyName = Assembly.GetEntryAssembly()?.GetName().Name;

        private static readonly Dictionary<string, List<Uri>> _themesUris = new Dictionary<string, List<Uri>>();
        public static string? ActiveTheme { get; private set; }

        private static readonly List<string> _availableThemes = new List<string>();
        public static IReadOnlyCollection<string> AvailableThemes => _availableThemes;

        public static ICommand SetNextThemeCommand { get; }

        static ThemeManager()
        {
            PreloadTheme("Dark");
            PreloadTheme("Light");
            PreloadTheme("Nodify");

            SetNextThemeCommand = new DelegateCommand(SetNextTheme);
        }

        private static void EnsureThemeUris(string themeName)
        {
        }

        private static void PreloadTheme(string themeName)
        {
            _availableThemes.Add(themeName);
        }

        public static void SetNextTheme()
        {
            if (ActiveTheme != null)
            {
                var i = _availableThemes.IndexOf(ActiveTheme);
                var next = i + 1 == _availableThemes.Count ? 0 : i + 1;

                SetTheme(_availableThemes[next]);
            }
            else if (_availableThemes.Count > 0)
            {
                SetTheme(_availableThemes[0]);
            }
        }

        public static void SetTheme(string themeName)
        {
            ActiveTheme = themeName;
        }
    }
}
