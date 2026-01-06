using System.Windows;

namespace BinaryWPF.Views.Prompts
{
    public partial class InfoDialog : Window
    {
        public InfoDialog()
        {
            InitializeComponent();
        }

        private void OnOk(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
