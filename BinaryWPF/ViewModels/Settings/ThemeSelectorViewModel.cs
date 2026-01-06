using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Media;

using BinaryWPF.Models;
using BinaryWPF.Properties;

namespace BinaryWPF.ViewModels.Settings
{
    public sealed class ThemeColorItemViewModel : ViewModelBase
    {
        private System.Drawing.Color _color;

        public string Name { get; }

        public System.Drawing.Color Color
        {
            get => _color;
            set
            {
                if (SetField(ref _color, value))
                {
                    OnPropertyChanged(nameof(Brush));
                }
            }
        }

        public SolidColorBrush Brush => new SolidColorBrush(System.Windows.Media.Color.FromArgb(Color.A, Color.R, Color.G, Color.B));

        public ThemeColorItemViewModel(string name, System.Drawing.Color color)
        {
            Name = name;
            _color = color;
        }
    }

    public sealed class ThemeSelectorViewModel : ViewModelBase
    {
        private string _selectedTheme = string.Empty;
        private Theme _currentTheme = new();
        private string _themeName = string.Empty;
        private string _themeAuthor = string.Empty;
        private string _themeVersion = string.Empty;
        private bool _darkTheme;

        public ObservableCollection<string> Themes { get; } = new();
        public ObservableCollection<ThemeColorItemViewModel> Colors { get; } = new();

        public string SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (SetField(ref _selectedTheme, value))
                {
                    LoadTheme(value);
                }
            }
        }

        public string ThemeName
        {
            get => _themeName;
            set => SetField(ref _themeName, value);
        }

        public string ThemeAuthor
        {
            get => _themeAuthor;
            set => SetField(ref _themeAuthor, value);
        }

        public string ThemeVersion
        {
            get => _themeVersion;
            set => SetField(ref _themeVersion, value);
        }

        public bool DarkTheme
        {
            get => _darkTheme;
            set => SetField(ref _darkTheme, value);
        }

        public ThemeSelectorViewModel()
        {
            LoadThemes();
        }

        public void ApplySelection()
        {
            Configurations.Default.ThemeFile = SelectedTheme;
            Configurations.Default.Save();
        }

        public void SaveAs(string path)
        {
            _currentTheme.Name = ThemeName;
            _currentTheme.Author = ThemeAuthor;
            _currentTheme.Version = ThemeVersion;
            _currentTheme.DarkTheme = DarkTheme;

            Theme.Serialize(path, _currentTheme);
            LoadThemes();
        }

        public void UpdateColor(ThemeColorItemViewModel item, System.Drawing.Color color)
        {
            item.Color = color;

            var prop = _currentTheme.Colors.GetType().GetProperties()
                .FirstOrDefault(p => p.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase));

            if (prop != null)
            {
                prop.SetValue(_currentTheme.Colors, color);
            }
        }

        private void LoadThemes()
        {
            Themes.Clear();
            Themes.Add("System");
            Themes.Add("Automatic");

            if (!Directory.Exists("Themes"))
            {
                Directory.CreateDirectory("Themes");
            }

            foreach (var theme in Directory.GetFiles("Themes", "*.json", SearchOption.TopDirectoryOnly))
            {
                Themes.Add(Path.GetFileName(theme));
            }

            var selected = Configurations.Default.ThemeFile;
            SelectedTheme = Themes.Contains(selected) ? selected : Themes.First();
        }

        private void LoadTheme(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename)) return;

            Theme.Deserialize(Path.Combine("Themes", filename), out var theme);
            _currentTheme = theme;

            if (Configurations.Default.ThemeFile == "Automatic" || filename == "Automatic")
            {
                ThemeName = "Automatic";
                ThemeAuthor = "N/A";
                ThemeVersion = "N/A";
                DarkTheme = false;
                Colors.Clear();
                return;
            }

            ThemeName = _currentTheme.Name;
            ThemeAuthor = _currentTheme.Author;
            ThemeVersion = _currentTheme.Version;
            DarkTheme = _currentTheme.DarkTheme;

            Colors.Clear();
            foreach (var colorProp in _currentTheme.Colors.GetType().GetProperties())
            {
                if (colorProp.GetValue(_currentTheme.Colors) is System.Drawing.Color color)
                {
                    Colors.Add(new ThemeColorItemViewModel(colorProp.Name, color));
                }
            }
        }
    }
}
