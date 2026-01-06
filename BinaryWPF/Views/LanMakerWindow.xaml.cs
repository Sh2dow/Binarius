using BinaryWPF.ViewModels.Settings;

using System.Windows;

namespace BinaryWPF.Views
{
    public partial class LanMakerWindow : Window
    {
        public LanMakerWindow()
        {
            InitializeComponent();
        }

        private void OnBrowse(object sender, RoutedEventArgs e)
        {
            if (DataContext is LanMakerViewModel vm)
            {
                vm.BrowseDirectory();
            }
        }

        private void OnSave(object sender, RoutedEventArgs e)
        {
            if (DataContext is LanMakerViewModel vm)
            {
                vm.SaveLauncher(this);
            }
        }
    }
}
