using System;
using System.Windows;

namespace BinaryWPF.Services
{
    public interface IWindowService
    {
        void ShowWindow<T>() where T : Window, new();
        bool? ShowDialog<T>() where T : Window, new();
    }

    public sealed class WindowService : IWindowService
    {
        public void ShowWindow<T>() where T : Window, new()
        {
            var window = new T { WindowStartupLocation = WindowStartupLocation.CenterScreen };
            window.Show();
        }

        public bool? ShowDialog<T>() where T : Window, new()
        {
            var window = new T { WindowStartupLocation = WindowStartupLocation.CenterScreen };
            return window.ShowDialog();
        }
    }
}
