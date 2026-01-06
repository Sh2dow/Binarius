using BinaryWPF.ViewModels.Prompts;
using BinaryWPF.Views.Prompts;

using System.Windows;
using Application = System.Windows.Application;

namespace BinaryWPF.Services
{
    public interface IPromptService
    {
        void ShowInfo(string message);
        bool ShowCheckbox(string message, bool initial);
        int ShowCombo(string message, string[] options, int initialIndex);
    }

    public sealed class PromptService : IPromptService
    {
        public void ShowInfo(string message)
        {
            var dialog = new InfoDialog
            {
                Owner = Application.Current.MainWindow,
                DataContext = new InfoDialogViewModel(message)
            };

            _ = dialog.ShowDialog();
        }

        public bool ShowCheckbox(string message, bool initial)
        {
            var viewModel = new CheckDialogViewModel(message, initial);
            var dialog = new CheckDialog
            {
                Owner = Application.Current.MainWindow,
                DataContext = viewModel
            };

            return dialog.ShowDialog() == true && viewModel.Value;
        }

        public int ShowCombo(string message, string[] options, int initialIndex)
        {
            var viewModel = new ComboDialogViewModel(message, options, initialIndex);
            var dialog = new ComboDialog
            {
                Owner = Application.Current.MainWindow,
                DataContext = viewModel
            };

            return dialog.ShowDialog() == true ? viewModel.SelectedIndex : initialIndex;
        }
    }
}
