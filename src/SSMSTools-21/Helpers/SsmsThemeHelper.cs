using Microsoft.VisualStudio.PlatformUI;
using System.Windows;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace SSMSTools_21.Helpers
{
    internal static class SsmsThemeHelper
    {
        internal static ApplicationTheme GetCurrentTheme()
        {
            // Prefer checking tool window TEXT color: light text means dark theme, dark text means light theme.
            var textColor = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowTextColorKey);
            if (!textColor.IsEmpty)
            {
                var luminance = (textColor.R * 0.299) + (textColor.G * 0.587) + (textColor.B * 0.114);
                return luminance > 128 ? ApplicationTheme.Dark : ApplicationTheme.Light;
            }

            // Fallback: background color key (dark background = dark theme)
            var bgColor = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);
            if (!bgColor.IsEmpty)
            {
                var luminance = (bgColor.R * 0.299) + (bgColor.G * 0.587) + (bgColor.B * 0.114);
                return luminance < 128 ? ApplicationTheme.Dark : ApplicationTheme.Light;
            }

            return ApplicationTheme.Light;
        }

        internal static void ApplyCurrentTheme(Window window)
        {
            var theme = GetCurrentTheme();
            ApplicationThemeManager.Apply(theme, WindowBackdropType.None, false);
            ApplicationThemeManager.Apply(window);
        }
    }
}


