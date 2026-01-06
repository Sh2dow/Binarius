using BinaryWPF.ViewModels.Settings;

using System.Windows;
using MessageBox = System.Windows.MessageBox;
using System.Windows.Input;

namespace BinaryWPF.Views
{
    public partial class ThemeSelectorWindow : Window
    {
        public ThemeSelectorWindow()
        {
            InitializeComponent();
        }

        private void OnOk(object sender, RoutedEventArgs e)
        {
            if (DataContext is ThemeSelectorViewModel vm)
            {
                vm.ApplySelection();
            }

            DialogResult = true;
        }

        private void OnSaveAs(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ThemeSelectorViewModel vm) return;

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = ".json",
                Filter = "json Files|*.json",
                OverwritePrompt = true,
                Title = "Save Theme"
            };

            if (dialog.ShowDialog() == true)
            {
                vm.SaveAs(dialog.FileName);
                MessageBox.Show($"File {dialog.FileName} has been saved.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OnColorDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not ThemeSelectorViewModel vm) return;
            if ((sender as System.Windows.Controls.ListView)?.SelectedItem is not ThemeColorItemViewModel item) return;

            using var dialog = new System.Windows.Forms.ColorDialog
            {
                Color = item.Color
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                vm.UpdateColor(item, dialog.Color);
            }
        }
    }
}
