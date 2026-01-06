using Nikki.Core;
using Nikki.Reflection.Enum.CP;

using System;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace BinaryWPF.Views.UI
{
    public partial class AttribCreatorDialog : Window
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

        public uint KeyChosen { get; private set; }

        public AttribCreatorDialog(GameINT game)
        {
            InitializeComponent();
            PopulateAttribTypesBasedOnGame(game);
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

        private void PopulateAttribKeysBasedOnType()
        {
            if (AttribTypeComboBox.SelectedItem == null) return;

            string[] values = AttribTypeComboBox.SelectedItem.ToString() switch
            {
                Boolean => Enum.GetNames(typeof(eAttribBool)),
                Color => Enum.GetNames(typeof(eAttribColor)),
                Floating => Enum.GetNames(typeof(eAttribFloat)),
                Integer => Enum.GetNames(typeof(eAttribInt)),
                Key => Enum.GetNames(typeof(eAttribKey)),
                ModelTable => Enum.GetNames(typeof(eAttribModelTable)),
                PartID => Enum.GetNames(typeof(eAttribPartID)),
                String => Enum.GetNames(typeof(eAttribString)),
                TwoString => Enum.GetNames(typeof(eAttribTwoString)),
                _ => Array.Empty<string>(),
            };

            AttribKeyComboBox.Items.Clear();
            Array.Sort(values);
            foreach (var value in values)
            {
                AttribKeyComboBox.Items.Add(value);
            }

            if (AttribKeyComboBox.Items.Count > 0)
            {
                AttribKeyComboBox.SelectedIndex = 0;
            }
        }

        private void AttribTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            PopulateAttribKeysBasedOnType();
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Choose attribute type that is going to be applied to a car part. Then, based on the type chosen, select attribute key that describes what attribute is for.",
                "Help",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (AttribTypeComboBox.SelectedItem == null || AttribKeyComboBox.SelectedItem == null)
            {
                return;
            }

            var type = AttribTypeComboBox.SelectedItem.ToString() switch
            {
                Boolean => typeof(eAttribBool),
                Color => typeof(eAttribColor),
                Floating => typeof(eAttribFloat),
                Integer => typeof(eAttribInt),
                Key => typeof(eAttribKey),
                ModelTable => typeof(eAttribModelTable),
                PartID => typeof(eAttribPartID),
                String => typeof(eAttribString),
                TwoString => typeof(eAttribTwoString),
                _ => typeof(object),
            };

            KeyChosen = (uint)Enum.Parse(type, AttribKeyComboBox.SelectedItem.ToString() ?? string.Empty);
            DialogResult = true;
            Close();
        }
    }
}
