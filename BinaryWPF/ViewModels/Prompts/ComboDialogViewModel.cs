using System.Collections.ObjectModel;

namespace BinaryWPF.ViewModels.Prompts
{
    public sealed class ComboDialogViewModel : ViewModelBase
    {
        private int _selectedIndex;

        public string Message { get; }
        public ObservableCollection<string> Options { get; }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set => SetField(ref _selectedIndex, value);
        }

        public ComboDialogViewModel(string message, string[] options, int selectedIndex)
        {
            Message = message;
            Options = new ObservableCollection<string>(options);
            _selectedIndex = selectedIndex;
        }
    }
}
