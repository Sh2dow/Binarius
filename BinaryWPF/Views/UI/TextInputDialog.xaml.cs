using System.Windows;

namespace BinaryWPF.Views.UI
{
    public partial class TextInputDialog : Window
    {
        public string Value { get; private set; } = string.Empty;

        public TextInputDialog(string message, string? initialValue = null)
        {
            InitializeComponent();
            DataContext = new TextInputDialogViewModel(message, initialValue ?? string.Empty);
        }

        private void OnOk(object sender, RoutedEventArgs e)
        {
            if (DataContext is TextInputDialogViewModel vm)
            {
                Value = vm.Value;
                DialogResult = true;
            }
        }
    }
}
