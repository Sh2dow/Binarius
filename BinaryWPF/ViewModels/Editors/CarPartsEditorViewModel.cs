using BinaryWPF.ViewModels.Editor;
using BinaryWPF.Views.UI;
using BinaryWPF.Services;

using CoreExtensions.Management;

using Nikki.Support.Shared.Class;
using Nikki.Support.Shared.Parts.CarParts;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Application = System.Windows.Application;

namespace BinaryWPF.ViewModels.Editors
{
    public sealed class CarPartsEditorViewModel : ViewModelBase
    {
        private readonly DBModelPart _model;
        private readonly IUserInteractionService _interactionService;
        private readonly IPromptService _promptService;
        private readonly IWindowService _windowService;
        private EditorTreeNodeViewModel? _selectedNode;
        private object? _selectedObject;
        private string _findText = string.Empty;

        public ObservableCollection<EditorTreeNodeViewModel> Nodes { get; } = new();

        public EditorTreeNodeViewModel? SelectedNode
        {
            get => _selectedNode;
            set
            {
                if (SetField(ref _selectedNode, value))
                {
                    UpdateSelectedObject();
                }
            }
        }

        public object? SelectedObject
        {
            get => _selectedObject;
            private set => SetField(ref _selectedObject, value);
        }

        public string FindText
        {
            get => _findText;
            set
            {
                if (SetField(ref _findText, value))
                {
                    ApplyHighlight();
                }
            }
        }

        public RelayCommand AddPartCommand { get; }
        public RelayCommand RemovePartCommand { get; }
        public RelayCommand CopyPartCommand { get; }
        public RelayCommand MovePartUpCommand { get; }
        public RelayCommand MovePartDownCommand { get; }
        public RelayCommand MovePartFirstCommand { get; }
        public RelayCommand MovePartLastCommand { get; }
        public RelayCommand ReversePartsCommand { get; }
        public RelayCommand SortPartsByNameCommand { get; }
        public RelayCommand FindReplaceCommand { get; }
        public RelayCommand AddAttributeCommand { get; }
        public RelayCommand RemoveAttributeCommand { get; }
        public RelayCommand MoveAttributeUpCommand { get; }
        public RelayCommand MoveAttributeDownCommand { get; }
        public RelayCommand ReverseAttributesCommand { get; }
        public RelayCommand SortAttributesCommand { get; }
        public RelayCommand AddCustomAttributeCommand { get; }
        public RelayCommand HasherCommand { get; }
        public RelayCommand RaiderCommand { get; }

        public CarPartsEditorViewModel(DBModelPart model)
        {
            _model = model;
            _interactionService = new UserInteractionService();
            _promptService = new PromptService();
            _windowService = new WindowService();

            AddPartCommand = new RelayCommand(AddPart);
            RemovePartCommand = new RelayCommand(RemovePart, () => SelectedNode?.Level == 0);
            CopyPartCommand = new RelayCommand(CopyPart, () => SelectedNode?.Level == 0);
            MovePartUpCommand = new RelayCommand(() => MovePart(-1), () => SelectedNode?.Level == 0);
            MovePartDownCommand = new RelayCommand(() => MovePart(1), () => SelectedNode?.Level == 0);
            MovePartFirstCommand = new RelayCommand(MovePartFirst, () => SelectedNode?.Level == 0);
            MovePartLastCommand = new RelayCommand(MovePartLast, () => SelectedNode?.Level == 0);
            ReversePartsCommand = new RelayCommand(ReverseParts, () => _model.CarPartsCount > 0);
            SortPartsByNameCommand = new RelayCommand(SortPartsByName, () => _model.CarPartsCount > 0);
            FindReplaceCommand = new RelayCommand(FindAndReplace, () => _model.CarPartsCount > 0);
            AddAttributeCommand = new RelayCommand(AddAttribute, () => SelectedNode?.Level == 0);
            RemoveAttributeCommand = new RelayCommand(RemoveAttribute, () => SelectedNode?.Level == 1);
            MoveAttributeUpCommand = new RelayCommand(() => MoveAttribute(-1), () => SelectedNode?.Level == 1);
            MoveAttributeDownCommand = new RelayCommand(() => MoveAttribute(1), () => SelectedNode?.Level == 1);
            ReverseAttributesCommand = new RelayCommand(ReverseAttributes, () => SelectedNode?.Level == 0);
            SortAttributesCommand = new RelayCommand(SortAttributes, () => SelectedNode?.Level == 0);
            AddCustomAttributeCommand = new RelayCommand(AddCustomAttribute, () => SelectedNode?.Level == 0);
            HasherCommand = new RelayCommand(() => _windowService.ShowWindow<Views.HasherWindow>());
            RaiderCommand = new RelayCommand(() => _windowService.ShowWindow<Views.RaiderWindow>());

            LoadTree();
        }

        public void LoadTree(string? selected = null)
        {
            Nodes.Clear();
            for (int i = 0; i < _model.CarPartsCount; ++i)
            {
                var part = _model.ModelCarParts[i];
                var node = new EditorTreeNodeViewModel(part.PartName, part.PartName, 0, i, null);
                int attrIndex = 0;
                foreach (var attribute in part.Attributes)
                {
                    var child = new EditorTreeNodeViewModel(attribute.ToString(), part.PartName + "|" + attribute, 1, attrIndex++, node);
                    node.Children.Add(child);
                }
                Nodes.Add(node);
            }

            if (!string.IsNullOrEmpty(selected))
            {
                SelectedNode = FindNode(selected, Nodes);
            }
        }

        private void UpdateSelectedObject()
        {
            if (SelectedNode == null)
            {
                SelectedObject = null;
                RaiseCanExecute();
                return;
            }

            if (SelectedNode.Level == 0)
            {
                SelectedObject = _model.GetRealPart(SelectedNode.Index);
            }
            else
            {
                var part = _model.GetRealPart(SelectedNode.Parent?.Index ?? 0);
                SelectedObject = part.GetAttribute(SelectedNode.Index);
            }

            RaiseCanExecute();
        }

        public void OnPropertyValueChanged()
        {
            if (SelectedNode == null || SelectedObject == null) return;

            string path = GetCurrentPath();
            LoadTree(path);
        }

        private void ApplyHighlight()
        {
            foreach (var node in Nodes)
            {
                ApplyHighlight(node, FindText);
            }
        }

        private static void ApplyHighlight(EditorTreeNodeViewModel node, string match)
        {
            node.IsHighlighted = !string.IsNullOrEmpty(match) && node.Name.Contains(match, StringComparison.OrdinalIgnoreCase);
            foreach (var child in node.Children)
            {
                ApplyHighlight(child, match);
            }
        }

        private static EditorTreeNodeViewModel? FindNode(string path, ObservableCollection<EditorTreeNodeViewModel> nodes)
        {
            foreach (var node in nodes)
            {
                if (node.FullPath == path) return node;
                var found = FindNode(path, node.Children);
                if (found != null) return found;
            }

            return null;
        }

        private void AddPart()
        {
            _model.AddRealPart();
            LoadTree();
        }

        private void RemovePart()
        {
            if (SelectedNode == null) return;
            _model.RemovePart(SelectedNode.Index);
            LoadTree();
        }

        private void CopyPart()
        {
            if (SelectedNode == null) return;

            int index = SelectedNode.Index;
            _model.ClonePart(index);
            LoadTree();
        }

        private void MovePart(int delta)
        {
            if (SelectedNode == null) return;

            int index1 = SelectedNode.Index;
            int index2 = index1 + delta;

            if (index2 < 0 || index2 >= _model.CarPartsCount)
            {
                _interactionService.ShowMessage("Unable to move because selected node is out of range", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var temp = _model.ModelCarParts[index1];
            _model.ModelCarParts[index1] = _model.ModelCarParts[index2];
            _model.ModelCarParts[index2] = temp;
            LoadTree();
            SelectedNode = Nodes[index2];
        }

        private void MovePartFirst()
        {
            if (SelectedNode == null || _model.CarPartsCount < 2) return;
            int index = SelectedNode.Index;
            var part = _model.ModelCarParts[index];
            _model.RemovePart(index);
            _model.ModelCarParts.Insert(0, part);
            LoadTree();
            SelectedNode = Nodes[0];
        }

        private void MovePartLast()
        {
            if (SelectedNode == null || _model.CarPartsCount < 2) return;
            int index = SelectedNode.Index;
            var part = _model.ModelCarParts[index];
            _model.RemovePart(index);
            _model.ModelCarParts.Add(part);
            LoadTree();
            SelectedNode = Nodes[^1];
        }

        private void ReverseParts()
        {
            _model.ReverseParts();
            LoadTree();
        }

        private void SortPartsByName()
        {
            _model.SortByProperty(nameof(RealCarPart.PartName));
            LoadTree();
        }

        private void FindAndReplace()
        {
            var search = new TextInputDialog("Enter string to search for") { Owner = Application.Current.MainWindow };
            if (search.ShowDialog() != true) return;

            var replace = new TextInputDialog("Enter string to replace with") { Owner = Application.Current.MainWindow };
            if (replace.ShowDialog() != true) return;

            bool caseSensitive = _promptService.ShowCheckbox("Make case-sensitive replace?", false);
            bool onlyLabel = _promptService.ShowCheckbox("Make replacement only in PartLabel?", true);

            var options = caseSensitive
                ? RegexOptions.Multiline | RegexOptions.CultureInvariant
                : RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;

            for (int i = 0; i < _model.CarPartsCount; ++i)
            {
                var part = _model.GetRealPart(i);
                part.MakeReplace(onlyLabel, search.Value, replace.Value, options);
            }

            LoadTree();
        }

        private void AddAttribute()
        {
            if (SelectedNode == null) return;

            var creator = new AttribCreatorDialog(_model.GameINT) { Owner = Application.Current.MainWindow };
            if (creator.ShowDialog() != true) return;

            var part = _model.GetRealPart(SelectedNode.Index);
            part.AddAttribute(creator.KeyChosen);
            LoadTree(GetCurrentPath());
        }

        private void RemoveAttribute()
        {
            if (SelectedNode?.Parent == null) return;

            var part = _model.GetRealPart(SelectedNode.Parent.Index);
            part.Attributes.RemoveAt(SelectedNode.Index);
            LoadTree(GetCurrentPath());
        }

        private void MoveAttribute(int delta)
        {
            if (SelectedNode?.Parent == null) return;

            int index1 = SelectedNode.Index;
            int index2 = index1 + delta;
            var part = _model.GetRealPart(SelectedNode.Parent.Index);

            if (index2 < 0 || index2 >= part.Length)
            {
                _interactionService.ShowMessage("Unable to move because selected node is out of range", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var temp = part.Attributes[index1];
            part.Attributes[index1] = part.Attributes[index2];
            part.Attributes[index2] = temp;
            LoadTree(GetCurrentPath());
        }

        private void ReverseAttributes()
        {
            if (SelectedNode == null) return;
            var part = _model.GetRealPart(SelectedNode.Index);
            part.Attributes.Reverse();
            LoadTree(GetCurrentPath());
        }

        private void SortAttributes()
        {
            if (SelectedNode == null) return;
            var part = _model.GetRealPart(SelectedNode.Index);
            part.Attributes.Sort((x, y) => x.Key.CompareTo(y.Key));
            LoadTree(GetCurrentPath());
        }

        private void AddCustomAttribute()
        {
            if (SelectedNode == null) return;

            var creator = new CustomAttribCreatorDialog(_model.GameINT) { Owner = Application.Current.MainWindow };
            if (creator.ShowDialog() != true) return;

            var part = _model.GetRealPart(SelectedNode.Index);
            part.AddCustomAttribute(creator.Value, creator.Type);
            LoadTree(GetCurrentPath());
        }

        private void RaiseCanExecute()
        {
            RemovePartCommand.RaiseCanExecuteChanged();
            CopyPartCommand.RaiseCanExecuteChanged();
            MovePartUpCommand.RaiseCanExecuteChanged();
            MovePartDownCommand.RaiseCanExecuteChanged();
            MovePartFirstCommand.RaiseCanExecuteChanged();
            MovePartLastCommand.RaiseCanExecuteChanged();
            ReversePartsCommand.RaiseCanExecuteChanged();
            SortPartsByNameCommand.RaiseCanExecuteChanged();
            FindReplaceCommand.RaiseCanExecuteChanged();
            AddAttributeCommand.RaiseCanExecuteChanged();
            RemoveAttributeCommand.RaiseCanExecuteChanged();
            MoveAttributeUpCommand.RaiseCanExecuteChanged();
            MoveAttributeDownCommand.RaiseCanExecuteChanged();
            ReverseAttributesCommand.RaiseCanExecuteChanged();
            SortAttributesCommand.RaiseCanExecuteChanged();
            AddCustomAttributeCommand.RaiseCanExecuteChanged();
        }

        private string GetCurrentPath()
        {
            if (SelectedNode == null) return string.Empty;

            if (SelectedNode.Level == 0)
            {
                var part = _model.GetRealPart(SelectedNode.Index);
                return part.PartName;
            }

            var parent = SelectedNode.Parent != null
                ? _model.GetRealPart(SelectedNode.Parent.Index)
                : _model.GetRealPart(SelectedNode.Index);

            if (SelectedNode.Level == 1 && parent != null)
            {
                var attribute = parent.GetAttribute(SelectedNode.Index);
                return $"{parent.PartName}|{attribute}";
            }

            return SelectedNode.FullPath;
        }
    }
}
