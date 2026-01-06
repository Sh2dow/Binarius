using BinaryWPF.ViewModels.Tools;

using System.Windows;
using Clipboard = System.Windows.Clipboard;

namespace BinaryWPF.Views
{
    public partial class SwatcherWindow : Window
    {
        public SwatcherWindow()
        {
            InitializeComponent();
        }

        private void CopySwatch(object sender, RoutedEventArgs e)
        {
            if (DataContext is SwatcherViewModel vm)
            {
                Clipboard.SetText(vm.PaintSwatch);
            }
        }

        private void CopySaturation(object sender, RoutedEventArgs e)
        {
            if (DataContext is SwatcherViewModel vm)
            {
                Clipboard.SetText(vm.Saturation);
            }
        }

        private void CopyBrightness(object sender, RoutedEventArgs e)
        {
            if (DataContext is SwatcherViewModel vm)
            {
                Clipboard.SetText(vm.Brightness);
            }
        }
    }
}
