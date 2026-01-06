using System.Collections.ObjectModel;

namespace BinaryWPF.ViewModels.Editor
{
    public sealed class EditorTreeNodeViewModel : ViewModelBase
    {
        private bool _isHighlighted;

        public string Name { get; set; }
        public string FullPath { get; }
        public int Level { get; }
        public int Index { get; }
        public EditorTreeNodeViewModel? Parent { get; }
        public ObservableCollection<EditorTreeNodeViewModel> Children { get; }

        public bool IsHighlighted
        {
            get => _isHighlighted;
            set => SetField(ref _isHighlighted, value);
        }

        public EditorTreeNodeViewModel(string name, string fullPath, int level, int index, EditorTreeNodeViewModel? parent)
        {
            Name = name;
            FullPath = fullPath;
            Level = level;
            Index = index;
            Parent = parent;
            Children = new ObservableCollection<EditorTreeNodeViewModel>();
        }
    }
}
