namespace BinaryWPF.Views.UI
{
    public sealed class TextInputDialogViewModel : BinaryWPF.ViewModels.ViewModelBase
    {
        public string Message { get; }
        public string Value { get; set; }

        public TextInputDialogViewModel(string message, string value)
        {
            Message = message;
            Value = value;
        }
    }
}
