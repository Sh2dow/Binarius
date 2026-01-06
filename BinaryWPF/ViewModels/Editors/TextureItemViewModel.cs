using Nikki.Support.Shared.Class;

namespace BinaryWPF.ViewModels.Editors
{
    public sealed class TextureItemViewModel : ViewModelBase
    {
        private bool _isHighlighted;

        public int Index { get; }
        public string BinKey => $"0x{Texture.BinKey:X8}";
        public string CollectionName => Texture.CollectionName;
        public string Format { get; }
        public Texture Texture { get; }

        public bool IsHighlighted
        {
            get => _isHighlighted;
            set => SetField(ref _isHighlighted, value);
        }

        public TextureItemViewModel(int index, Texture texture, string format, bool isHighlighted)
        {
            Index = index;
            Texture = texture;
            Format = format;
            _isHighlighted = isHighlighted;
        }

        public void UpdateCollectionName(string name)
        {
            Texture.CollectionName = name;
            OnPropertyChanged(nameof(CollectionName));
            OnPropertyChanged(nameof(BinKey));
        }
    }
}
