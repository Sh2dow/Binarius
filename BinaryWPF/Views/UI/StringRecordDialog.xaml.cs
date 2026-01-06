using Nikki.Support.Shared.Parts.STRParts;

using System.Windows;

namespace BinaryWPF.Views.UI
{
    public partial class StringRecordDialog : Window
    {
        public string Key => ((StringRecordDialogViewModel)DataContext).Key;
        public string Label => ((StringRecordDialogViewModel)DataContext).Label;
        public string TextValue => ((StringRecordDialogViewModel)DataContext).TextValue;

        public StringRecordDialog()
        {
            InitializeComponent();
            DataContext = new StringRecordDialogViewModel();
        }

        public StringRecordDialog(StringRecord record)
        {
            InitializeComponent();
            DataContext = new StringRecordDialogViewModel
            {
                Key = $"0x{record.Key:X8}",
                Label = record.Label,
                TextValue = record.Text
            };
        }

        private void OnOk(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
