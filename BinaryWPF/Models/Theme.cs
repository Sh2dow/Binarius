using BinaryWPF.Properties;

using Endscript.Enums;

using Nikki.Core;

using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BinaryWPF.Models
{
    public class Theme
    {
        public string Name { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public bool DarkTheme { get; set; }
        public ColorSet Colors { get; set; } = new();

        public class ColorSet
        {
            public Color MainBackColor { get; set; }
            public Color MainForeColor { get; set; }
            public Color ButtonBackColor { get; set; }
            public Color ButtonForeColor { get; set; }
            public Color ButtonFlatColor { get; set; }
            public Color TextBoxBackColor { get; set; }
            public Color TextBoxForeColor { get; set; }
            public Color PrimBackColor { get; set; }
            public Color PrimForeColor { get; set; }
            public Color MenuItemBackColor { get; set; }
            public Color MenuItemForeColor { get; set; }
            public Color LabelTextColor { get; set; }
            public Color RegBorderColor { get; set; }
            public Color FocusedBackColor { get; set; }
            public Color FocusedForeColor { get; set; }
            public Color StatusStripGradientBegin { get; set; }
            public Color StatusStripGradientEnd { get; set; }
            public Color MenuStripGradientBegin { get; set; }
            public Color MenuStripGradientEnd { get; set; }
            public Color MenuBorder { get; set; }
            public Color MenuItemBorder { get; set; }
            public Color MenuItemPressedGradientBegin { get; set; }
            public Color MenuItemPressedGradientMiddle { get; set; }
            public Color MenuItemPressedGradientEnd { get; set; }
            public Color MenuItemSelected { get; set; }
            public Color MenuItemSelectedGradientBegin { get; set; }
            public Color MenuItemSelectedGradientEnd { get; set; }
        }

        private static readonly JsonSerializerOptions Options = new()
        {
            AllowTrailingCommas = true,
            IgnoreReadOnlyProperties = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true,
        };

        public class ColorJsonConverter : JsonConverter<Color>
        {
            private static readonly ColorConverter Converter = new();

            public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return (Color)Converter.ConvertFromInvariantString(reader.GetString() ?? string.Empty)!;
            }

            public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(Converter.ConvertToInvariantString(value));
            }
        }

        public static void AddColorJsonConverter()
        {
            foreach (JsonConverter converter in Options.Converters)
            {
                if (converter is ColorJsonConverter) return;
            }

            Options.Converters.Add(new ColorJsonConverter());
        }

        public static string GetThemeFile()
        {
            string filename = Configurations.Default.ThemeFile;

            if (Configurations.Default.ThemeFile == "Automatic")
            {
                filename = GetAutomaticThemeFile((GameINT)Configurations.Default.CurrentGame);
            }

            return Path.Combine("Themes", filename);
        }

        public static string GetAutomaticThemeFile(GameINT game)
        {
            return game switch
            {
                GameINT.Carbon => "Carbon.json",
                GameINT.MostWanted => "MostWanted.json",
                GameINT.Prostreet => "Prostreet.json",
                GameINT.Undercover => "Undercover.json",
                GameINT.Underground1 => "Underground1.json",
                GameINT.Underground2 => "Underground2.json",
                _ => "System"
            };
        }

        public Theme()
        {
            Name = string.Empty;
            Author = string.Empty;
            Version = string.Empty;
            DarkTheme = false;

            Colors.MainBackColor = SystemColors.Control;
            Colors.MainForeColor = SystemColors.ControlText;
            Colors.ButtonBackColor = SystemColors.Control;
            Colors.ButtonForeColor = SystemColors.ControlText;
            Colors.ButtonFlatColor = SystemColors.ButtonShadow;
            Colors.TextBoxBackColor = SystemColors.Control;
            Colors.TextBoxForeColor = SystemColors.ControlText;
            Colors.PrimBackColor = SystemColors.Control;
            Colors.PrimForeColor = SystemColors.ControlText;
            Colors.MenuItemBackColor = SystemColors.Control;
            Colors.MenuItemForeColor = SystemColors.ControlText;
            Colors.LabelTextColor = SystemColors.ControlText;
            Colors.RegBorderColor = SystemColors.Control;
            Colors.FocusedBackColor = SystemColors.Control;
            Colors.FocusedForeColor = SystemColors.ControlText;
            Colors.StatusStripGradientBegin = SystemColors.Control;
            Colors.StatusStripGradientEnd = SystemColors.Control;
            Colors.MenuStripGradientBegin = SystemColors.Control;
            Colors.MenuStripGradientEnd = SystemColors.Control;
            Colors.MenuBorder = SystemColors.Control;
            Colors.MenuItemBorder = SystemColors.Control;
            Colors.MenuItemPressedGradientBegin = SystemColors.Control;
            Colors.MenuItemPressedGradientMiddle = SystemColors.Control;
            Colors.MenuItemPressedGradientEnd = SystemColors.Control;
            Colors.MenuItemSelected = SystemColors.Control;
            Colors.MenuItemSelectedGradientBegin = SystemColors.Control;
            Colors.MenuItemSelectedGradientEnd = SystemColors.Control;
        }

        public static void Serialize(string filename, Theme theme)
        {
            if (filename == "System") return;

            AddColorJsonConverter();
            var settings = JsonSerializer.Serialize(theme, Options);

            using var sw = new StreamWriter(File.Open(filename, FileMode.Create));
            sw.Write(settings);
            sw.WriteLine();
        }

        public static void Deserialize(string filename, out Theme theme)
        {
            if (filename == "System" || !File.Exists(filename))
            {
                theme = new Theme
                {
                    Name = "System",
                    Author = "N/A",
                    Version = "1.0",
                    DarkTheme = false,
                };
                return;
            }

            var settings = File.ReadAllText(filename);
            AddColorJsonConverter();
            theme = JsonSerializer.Deserialize<Theme>(settings, Options) ?? new Theme();
        }
    }
}
