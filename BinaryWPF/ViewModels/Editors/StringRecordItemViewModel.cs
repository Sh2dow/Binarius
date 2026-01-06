using Nikki.Support.Shared.Parts.STRParts;

namespace BinaryWPF.ViewModels.Editors
{
    public sealed class StringRecordItemViewModel : ViewModelBase
    {
        private bool _isHighlighted;
        private bool _isModified;

        public int Index { get; }
        public StringRecord Record { get; }

        public string Key => $"0x{Record.Key:X8}";
        public string Label => Record.Label;
        public string Text => Record.Text;

        public bool IsHighlighted
        {
            get => _isHighlighted;
            set => SetField(ref _isHighlighted, value);
        }

        public bool IsModified
        {
            get => _isModified;
            set => SetField(ref _isModified, value);
        }

        public StringRecordItemViewModel(int index, StringRecord record)
        {
            Index = index;
            Record = record;
        }

        public void Refresh()
        {
            OnPropertyChanged(nameof(Key));
            OnPropertyChanged(nameof(Label));
            OnPropertyChanged(nameof(Text));
        }
    }
}
