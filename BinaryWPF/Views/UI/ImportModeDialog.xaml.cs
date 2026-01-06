using Nikki.Reflection.Enum;

using System.Windows;

namespace BinaryWPF.Views.UI
{
    public partial class ImportModeDialog : Window
    {
        public SerializeType Mode { get; private set; } = SerializeType.Negate;

        public ImportModeDialog()
        {
            InitializeComponent();
        }

        private void OnOk(object sender, RoutedEventArgs e)
        {
            if (OverrideOption.IsChecked == true)
            {
                Mode = SerializeType.Override;
            }
            else if (SynchronizeOption.IsChecked == true)
            {
                Mode = SerializeType.Synchronize;
            }
            else
            {
                Mode = SerializeType.Negate;
            }
            DialogResult = true;
        }
    }
}
