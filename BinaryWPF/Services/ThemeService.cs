using BinaryWPF.Models;

using System.Windows;
using System.Windows.Media;
using Application = System.Windows.Application;
using MediaColor = System.Windows.Media.Color;

namespace BinaryWPF.Services
{
    public sealed class ThemeService
    {
        public Theme CurrentTheme { get; private set; } = new();

        public void ApplyThemeResources()
        {
            var filename = Theme.GetThemeFile();
            Theme.Deserialize(filename, out var theme);
            CurrentTheme = theme;

            ApplyColor("MainBackColor", theme.Colors.MainBackColor);
            ApplyColor("MainForeColor", theme.Colors.MainForeColor);
            ApplyColor("PanelBackColor", theme.Colors.ButtonBackColor);
            ApplyColor("AccentColor", theme.Colors.ButtonFlatColor);
            ApplyColor("TextBoxBackColor", theme.Colors.TextBoxBackColor);
            ApplyColor("TextBoxForeColor", theme.Colors.TextBoxForeColor);
            ApplyColor("PrimBackColor", theme.Colors.PrimBackColor);
            ApplyColor("PrimForeColor", theme.Colors.PrimForeColor);
            ApplyColor("MenuItemBackColor", theme.Colors.MenuItemBackColor);
            ApplyColor("MenuItemForeColor", theme.Colors.MenuItemForeColor);
            ApplyColor("LabelTextColor", theme.Colors.LabelTextColor);
            ApplyColor("RegBorderColor", theme.Colors.RegBorderColor);
            ApplyColor("FocusedBackColor", theme.Colors.FocusedBackColor);
            ApplyColor("FocusedForeColor", theme.Colors.FocusedForeColor);
            ApplyColor("StatusStripGradientBegin", theme.Colors.StatusStripGradientBegin);
            ApplyColor("StatusStripGradientEnd", theme.Colors.StatusStripGradientEnd);
            ApplyColor("MenuStripGradientBegin", theme.Colors.MenuStripGradientBegin);
            ApplyColor("MenuStripGradientEnd", theme.Colors.MenuStripGradientEnd);
            ApplyColor("MenuBorder", theme.Colors.MenuBorder);
            ApplyColor("MenuItemBorder", theme.Colors.MenuItemBorder);
            ApplyColor("MenuItemSelected", theme.Colors.MenuItemSelected);
            ApplyColor("MenuItemSelectedGradientBegin", theme.Colors.MenuItemSelectedGradientBegin);
            ApplyColor("MenuItemSelectedGradientEnd", theme.Colors.MenuItemSelectedGradientEnd);

            var isDark = theme.DarkTheme;
            Application.Current.Resources["ThemeImageUser"] = GetThemeImage(isDark ? "DarkUser.png" : "LightUser.png");
            Application.Current.Resources["ThemeImageModder"] = GetThemeImage(isDark ? "DarkModder.png" : "LightModder.png");
            Application.Current.Resources["ThemeImageTools"] = GetThemeImage(isDark ? "DarkTools.png" : "LightTools.png");
            Application.Current.Resources["ThemeImageTheme"] = GetThemeImage(isDark ? "DarkTheme.png" : "LightTheme.png");
            Application.Current.Resources["ThemeImageUpdates"] = GetThemeImage("binarius.png");
        }

        public void ApplyPropertyGridTheme(System.Windows.Forms.PropertyGrid grid)
        {
            if (grid == null) return;

            var filename = Theme.GetThemeFile();
            Theme.Deserialize(filename, out var theme);

            grid.BackColor = theme.Colors.PrimBackColor;
            grid.CategorySplitterColor = theme.Colors.ButtonBackColor;
            grid.CategoryForeColor = theme.Colors.TextBoxForeColor;
            grid.CommandsBackColor = theme.Colors.PrimBackColor;
            grid.CommandsForeColor = theme.Colors.PrimForeColor;
            grid.CommandsBorderColor = theme.Colors.PrimBackColor;
            grid.DisabledItemForeColor = theme.Colors.LabelTextColor;
            grid.LineColor = theme.Colors.ButtonBackColor;
            grid.SelectedItemWithFocusBackColor = theme.Colors.FocusedBackColor;
            grid.SelectedItemWithFocusForeColor = theme.Colors.FocusedForeColor;
            grid.ViewBorderColor = theme.Colors.RegBorderColor;
            grid.ViewBackColor = theme.Colors.PrimBackColor;
            grid.ViewForeColor = theme.Colors.PrimForeColor;
            grid.HelpBackColor = theme.Colors.PrimBackColor;
            grid.HelpForeColor = theme.Colors.PrimForeColor;
            grid.HelpBorderColor = theme.Colors.RegBorderColor;
        }

        private static void ApplyColor(string key, System.Drawing.Color color)
        {
            Application.Current.Resources[key] = MediaColor.FromArgb(color.A, color.R, color.G, color.B);
            Application.Current.Resources[$"{key}Brush"] = new SolidColorBrush(MediaColor.FromArgb(color.A, color.R, color.G, color.B));
        }

        private static Uri GetThemeImage(string filename)
        {
            return new Uri($"pack://application:,,,/Resources/{filename}", UriKind.Absolute);
        }
    }
}
