using System.Windows;

namespace BinaryWPF.Views.Prompts
{
    public partial class ComboDialog : Window
    {
        public ComboDialog()
        {
            InitializeComponent();
        }

        private void OnOk(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
