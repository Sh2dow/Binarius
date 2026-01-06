using Nikki.Core;
using Nikki.Reflection.Enum;
using Nikki.Reflection.Enum.CP;

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace BinaryWPF.Views.UI
{
    public partial class CustomAttribCreatorDialog : Window
    {
        private const string Boolean = "Boolean";
        private const string Color = "Color";
        private const string Floating = "Floating";
        private const string Integer = "Integer";
        private const string Key = "Key";
        private const string ModelTable = "ModelTable";
        private const string PartID = "PartID";
        private const string String = "String";
        private const string TwoString = "TwoString";

        public CarPartAttribType Type { get; private set; }
        public string Value { get; private set; } = string.Empty;
        public List<string> CustomAttribKeys { get; } = new();

        public CustomAttribCreatorDialog(GameINT game)
        {
            InitializeComponent();
            PopulateAttribTypesBasedOnGame(game);
            LoadHashList();
        }

        private void PopulateAttribTypesBasedOnGame(GameINT game)
        {
            switch (game)
            {
                case GameINT.Carbon:
                case GameINT.Prostreet:
                case GameINT.Undercover:
                    AttribTypeComboBox.Items.Add(Boolean);
                    AttribTypeComboBox.Items.Add(Color);
                    AttribTypeComboBox.Items.Add(Floating);
                    AttribTypeComboBox.Items.Add(Integer);
                    AttribTypeComboBox.Items.Add(Key);
                    AttribTypeComboBox.Items.Add(ModelTable);
                    AttribTypeComboBox.Items.Add(PartID);
                    AttribTypeComboBox.Items.Add(String);
                    AttribTypeComboBox.Items.Add(TwoString);
                    break;
                case GameINT.Underground1:
                case GameINT.Underground2:
                case GameINT.MostWanted:
                    AttribTypeComboBox.Items.Add(Boolean);
                    AttribTypeComboBox.Items.Add(Floating);
                    AttribTypeComboBox.Items.Add(Integer);
                    AttribTypeComboBox.Items.Add(Key);
                    AttribTypeComboBox.Items.Add(String);
                    break;
                default:
                    break;
            }

            if (AttribTypeComboBox.Items.Count > 0)
            {
                AttribTypeComboBox.SelectedIndex = 0;
            }
        }

        private void LoadHashList()
        {
            if (File.Exists(Map.CustomAttribFile))
            {
                try
                {
                    var lines = File.ReadAllLines(Map.CustomAttribFile);
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("//") || line.StartsWith("#")) continue;
                        CustomAttribKeys.Add(line);
                    }
                }
                catch
                {
                    return;
                }
            }

            CustomAttribKeys.Sort(StringComparer.OrdinalIgnoreCase);
            foreach (var label in CustomAttribKeys)
            {
                AttribKeyComboBox.Items.Add(label);
            }
        }

        private void SaveHashList()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Map.CustomAttribFile) ?? ".");

            if (File.Exists(Map.CustomAttribFile))
            {
                var lines = File.ReadAllLines(Map.CustomAttribFile);
                var set = new HashSet<string>(lines.Length + 1, StringComparer.OrdinalIgnoreCase);
                foreach (var line in lines)
                {
                    if (line.StartsWith("//") || line.StartsWith("#")) continue;
                    set.Add(line);
                }

                set.Add(Value);
                using var sw = new StreamWriter(File.Open(Map.CustomAttribFile, FileMode.Create));
                foreach (var line in set) sw.WriteLine(line);
            }
            else
            {
                using var sw = new StreamWriter(File.Open(Map.CustomAttribFile, FileMode.Create));
                sw.WriteLine(Value);
            }
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Choose attribute type that is going to be applied to a car part. Then, based on the type chosen, type the attribute key that describes what attribute is for.",
                "Help",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (AttribTypeComboBox.SelectedItem == null)
            {
                return;
            }

            var type = AttribTypeComboBox.SelectedItem.ToString() switch
            {
                Boolean => CarPartAttribType.Boolean,
                Color => CarPartAttribType.Color,
                Floating => CarPartAttribType.Floating,
                Integer => CarPartAttribType.Integer,
                Key => CarPartAttribType.Key,
                ModelTable => CarPartAttribType.ModelTable,
                PartID => CarPartAttribType.CarPartID,
                String => CarPartAttribType.String,
                TwoString => CarPartAttribType.TwoString,
                _ => CarPartAttribType.Integer,
            };

            var value = AttribKeyComboBox.Text?.Trim();
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            Value = value;
            Type = type;
            SaveHashList();
            DialogResult = true;
            Close();
        }
    }
}
