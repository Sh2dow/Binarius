using System.Windows;

namespace BinaryWPF.Views.Prompts
{
    public partial class CheckDialog : Window
    {
        public CheckDialog()
        {
            InitializeComponent();
        }

        private void OnOk(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
