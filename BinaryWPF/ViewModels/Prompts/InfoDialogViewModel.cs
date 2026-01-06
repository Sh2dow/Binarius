using System.Collections.ObjectModel;

namespace BinaryWPF.ViewModels.Prompts
{
    public sealed class InfoDialogViewModel : ViewModelBase
    {
        public string Message { get; }

        public InfoDialogViewModel(string message)
        {
            Message = message;
        }
    }
}
