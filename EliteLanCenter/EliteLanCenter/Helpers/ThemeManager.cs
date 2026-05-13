using System.IO;
using System.Windows;
using System.Windows.Media;

namespace EliteLanCenter.Helpers
{
    public static class ThemeManager
    {
        private const string ConfigFile = "elite_theme.cfg";

        public static bool IsDarkMode { get; private set; } = true;

        private static readonly Dictionary<string, Color> DarkColors = new()
        {
            ["BgPrincipalBrush"] = Color.FromRgb(0x11, 0x18, 0x27),
            ["BgPanelBrush"] = Color.FromRgb(0x1A, 0x1F, 0x35),
            ["BgCardBrush"] = Color.FromRgb(0x2D, 0x32, 0x50),
            ["TextPrimaryBrush"] = Color.FromRgb(0xE8, 0xEA, 0xF6),
            ["TextSecondaryBrush"] = Color.FromRgb(0x88, 0x92, 0xB0),
            ["InputBgBrush"] = Color.FromRgb(0x2D, 0x32, 0x50),
            ["BorderBrush"] = Color.FromRgb(0x2D, 0x32, 0x50),
        };

        private static readonly Dictionary<string, Color> LightColors = new()
        {
            ["BgPrincipalBrush"] = Color.FromRgb(0xF0, 0xFA, 0xFA),
            ["BgPanelBrush"] = Color.FromRgb(0xFF, 0xFF, 0xFF),
            ["BgCardBrush"] = Color.FromRgb(0xFF, 0xFF, 0xFF),
            ["TextPrimaryBrush"] = Color.FromRgb(0x1A, 0x1A, 0x2E),
            ["TextSecondaryBrush"] = Color.FromRgb(0x66, 0x68, 0x88),
            ["InputBgBrush"] = Color.FromRgb(0xF0, 0xF4, 0xF8),
            ["BorderBrush"] = Color.FromRgb(0xE0, 0xE4, 0xE8),
        };

        public static void Inicializar()
        {
            foreach (var kvp in DarkColors)
                Application.Current.Resources[kvp.Key] = new SolidColorBrush(kvp.Value);

            LoadSavedTheme();
        }

        public static void LoadSavedTheme()
        {
            try
            {
                if (File.Exists(ConfigFile))
                {
                    var saved = File.ReadAllText(ConfigFile).Trim();
                    if (saved == "Light")
                        CambiarColores(false);
                }
            }
            catch { }
        }

        public static void ToggleTheme()
        {
            CambiarColores(!IsDarkMode);
            try { File.WriteAllText(ConfigFile, IsDarkMode ? "Dark" : "Light"); } catch { }
        }

        private static void CambiarColores(bool darkMode)
        {
            IsDarkMode = darkMode;
            var colores = darkMode ? DarkColors : LightColors;

            foreach (var kvp in colores)
                Application.Current.Resources[kvp.Key] = new SolidColorBrush(kvp.Value);
        }
    }
}
