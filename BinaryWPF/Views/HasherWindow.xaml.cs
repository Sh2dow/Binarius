using BinaryWPF.ViewModels.Tools;

using System.Windows;
using Clipboard = System.Windows.Clipboard;

namespace BinaryWPF.Views
{
    public partial class HasherWindow : Window
    {
        public HasherWindow()
        {
            InitializeComponent();
        }

        private void CopyInput(object sender, RoutedEventArgs e)
        {
            if (DataContext is HasherViewModel vm && !string.IsNullOrEmpty(vm.Input))
            {
                Clipboard.SetText(vm.Input);
            }
        }

        private void CopyBinHash(object sender, RoutedEventArgs e)
        {
            if (DataContext is HasherViewModel vm && !string.IsNullOrEmpty(vm.BinHash))
            {
                Clipboard.SetText(vm.BinHash);
            }
        }

        private void CopyBinFile(object sender, RoutedEventArgs e)
        {
            if (DataContext is HasherViewModel vm && !string.IsNullOrEmpty(vm.BinFile))
            {
                Clipboard.SetText(vm.BinFile);
            }
        }

        private void CopyVltHash(object sender, RoutedEventArgs e)
        {
            if (DataContext is HasherViewModel vm && !string.IsNullOrEmpty(vm.VltHash))
            {
                Clipboard.SetText(vm.VltHash);
            }
        }

        private void CopyVltFile(object sender, RoutedEventArgs e)
        {
            if (DataContext is HasherViewModel vm && !string.IsNullOrEmpty(vm.VltFile))
            {
                Clipboard.SetText(vm.VltFile);
            }
        }
    }
}
