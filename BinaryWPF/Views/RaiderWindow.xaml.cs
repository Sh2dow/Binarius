using BinaryWPF.ViewModels.Tools;

using System.Windows;
using Clipboard = System.Windows.Clipboard;

namespace BinaryWPF.Views
{
    public partial class RaiderWindow : Window
    {
        public RaiderWindow()
        {
            InitializeComponent();
        }

        private void CopyBinHash(object sender, RoutedEventArgs e)
        {
            if (DataContext is RaiderViewModel vm && !string.IsNullOrEmpty(vm.BinHash))
            {
                Clipboard.SetText(vm.BinHash);
            }
        }

        private void CopyBinFile(object sender, RoutedEventArgs e)
        {
            if (DataContext is RaiderViewModel vm && !string.IsNullOrEmpty(vm.BinFile))
            {
                Clipboard.SetText(vm.BinFile);
            }
        }

        private void CopyResult(object sender, RoutedEventArgs e)
        {
            if (DataContext is RaiderViewModel vm && !string.IsNullOrEmpty(vm.Result))
            {
                Clipboard.SetText(vm.Result);
            }
        }
    }
}
