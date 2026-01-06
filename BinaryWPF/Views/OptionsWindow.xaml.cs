using BinaryWPF.ViewModels.Settings;

using System.Windows;
using System.Windows.Interop;

namespace BinaryWPF.Views
{
    public partial class OptionsWindow : Window
    {
        public OptionsWindow()
        {
            InitializeComponent();
        }

        private void OnOk(object sender, RoutedEventArgs e)
        {
            if (DataContext is OptionsViewModel vm)
            {
                vm.Save();
            }

            if (ComponentDispatcher.IsThreadModal)
            {
                DialogResult = true;
            }
            else
            {
                Close();
            }
        }
    }
}
