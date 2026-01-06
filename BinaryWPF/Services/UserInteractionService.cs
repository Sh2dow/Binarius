using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace BinaryWPF.Services
{
    public interface IUserInteractionService
    {
        bool TryOpenFile(string filter, string title, out string path);
        bool TrySelectFolder(string description, out string path);
        MessageBoxResult ShowMessage(string message, string title, MessageBoxButton buttons, MessageBoxImage icon);
    }

    public sealed class UserInteractionService : IUserInteractionService
    {
        public bool TryOpenFile(string filter, string title, out string path)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = filter,
                Title = title,
                Multiselect = false
            };

            var result = dialog.ShowDialog();
            path = dialog.FileName;
            return result == true;
        }

        public bool TrySelectFolder(string description, out string path)
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = description,
                UseDescriptionForTitle = true,
                ShowNewFolderButton = false
            };

            var result = dialog.ShowDialog();
            path = dialog.SelectedPath;
            return result == System.Windows.Forms.DialogResult.OK;
        }

        public MessageBoxResult ShowMessage(string message, string title, MessageBoxButton buttons, MessageBoxImage icon)
        {
            return MessageBox.Show(message, title, buttons, icon);
        }
    }
}
