namespace BinaryWPF.ViewModels.Prompts
{
    public sealed class CheckDialogViewModel : ViewModelBase
    {
        private bool _value;

        public string Message { get; }

        public bool Value
        {
            get => _value;
            set => SetField(ref _value, value);
        }

        public CheckDialogViewModel(string message, bool initial)
        {
            Message = message;
            _value = initial;
        }
    }
}
