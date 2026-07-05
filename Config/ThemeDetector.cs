using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace PixelRigid.Config
{
    public static class ThemeDetector
    {
        public static bool IsDarkModeEnabled()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null)
                    {
                        var value = key.GetValue("AppsUseLightTheme");
                        if (value is int intVal)
                        {
                            return intVal == 0; // 0 = dark, 1 = light
                        }
                    }
                }
            }
            catch { }
            return false;
        }

        public static Icon GetTrayIcon()
        {
            bool isDark = IsDarkModeEnabled();
            string theme = isDark ? "Dark" : "Light";
            string prefix = isDark ? "dark" : "light";
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // Try 1: Resources\Icons\{Theme}\pixelrigid-{theme}.ico (new naming)
            string iconPath = Path.Combine(baseDir, "Resources", "Icons", theme, $"pixelrigid-{prefix}.ico");
            if (File.Exists(iconPath))
            {
                try { return new Icon(iconPath); }
                catch { }
            }

            // Try 2: Resources\Icons\{Theme}\pixelrigid-{theme}-mode.ico (old naming in new folder)
            iconPath = Path.Combine(baseDir, "Resources", "Icons", theme, $"pixelrigid-{prefix}-mode.ico");
            if (File.Exists(iconPath))
            {
                try { return new Icon(iconPath); }
                catch { }
            }

            // Fallback to old path: Resources\{Theme}\pixelrigid-{theme}-mode.ico
            iconPath = Path.Combine(baseDir, "Resources", theme, $"pixelrigid-{prefix}-mode.ico");
            if (File.Exists(iconPath))
            {
                try { return new Icon(iconPath); }
                catch { }
            }

            return Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }
    }
}