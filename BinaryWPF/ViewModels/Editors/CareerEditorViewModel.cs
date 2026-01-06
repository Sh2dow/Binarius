using BinaryWPF.Services;
using BinaryWPF.ViewModels.Editor;
using BinaryWPF.Views.UI;

using CoreExtensions.Management;

using Endscript.Enums;

using Nikki.Reflection.Abstract;
using Nikki.Support.Shared.Class;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Application = System.Windows.Application;

namespace BinaryWPF.ViewModels.Editors
{
    public sealed class CareerEditorViewModel : ViewModelBase
    {
        private readonly GCareer _career;
        private readonly string _careerPath;
        private readonly IUserInteractionService _interactionService;
        private readonly IPromptService _promptService;
        private readonly IWindowService _windowService;

        private EditorTreeNodeViewModel? _selectedNode;
        private object? _selectedObject;
        private string _findText = string.Empty;

        public ObservableCollection<EditorTreeNodeViewModel> Nodes { get; } = new();
        public List<string> Commands { get; } = new();

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

        public RelayCommand AddCollectionCommand { get; }
        public RelayCommand RemoveCollectionCommand { get; }
        public RelayCommand CopyCollectionCommand { get; }
        public RelayCommand ScriptCollectionCommand { get; }
        public RelayCommand HasherCommand { get; }
        public RelayCommand RaiderCommand { get; }

        public CareerEditorViewModel(GCareer career, string path)
        {
            _career = career;
            _careerPath = path;
            _interactionService = new UserInteractionService();
            _promptService = new PromptService();
            _windowService = new WindowService();

            AddCollectionCommand = new RelayCommand(AddCollection, () => SelectedNode?.Level == 0);
            RemoveCollectionCommand = new RelayCommand(RemoveCollection, () => SelectedNode?.Level == 1);
            CopyCollectionCommand = new RelayCommand(CopyCollection, () => SelectedNode?.Level == 1);
            ScriptCollectionCommand = new RelayCommand(ScriptCollection, () => SelectedNode?.Level == 1);
            HasherCommand = new RelayCommand(() => _windowService.ShowWindow<Views.HasherWindow>());
            RaiderCommand = new RelayCommand(() => _windowService.ShowWindow<Views.RaiderWindow>());

            LoadTree();
        }

        public void LoadTree(string? selected = null)
        {
            Nodes.Clear();

            for (int i = 0; i < _career.AllRootNames.Length; ++i)
            {
                string rootName = _career.AllRootNames[i];
                var rootNode = new EditorTreeNodeViewModel(rootName, rootName, 0, i, null);
                var root = _career.GetRoot(rootName);

                int collectionIndex = 0;
                foreach (Collectable collection in root)
                {
                    var collectionNode = new EditorTreeNodeViewModel(collection.CollectionName, $"{rootName}|{collection.CollectionName}", 1, collectionIndex++, rootNode);

                    int expandIndex = 0;
                    foreach (var expando in collection.GetAllNodes())
                    {
                        var expandNode = new EditorTreeNodeViewModel(expando.NodeName, $"{collectionNode.FullPath}|{expando.NodeName}", 2, expandIndex++, collectionNode);

                        int subIndex = 0;
                        foreach (var subpart in expando.SubNodes)
                        {
                            var subNode = new EditorTreeNodeViewModel(subpart.NodeName, $"{expandNode.FullPath}|{subpart.NodeName}", 3, subIndex++, expandNode);
                            expandNode.Children.Add(subNode);
                        }

                        collectionNode.Children.Add(expandNode);
                    }

                    rootNode.Children.Add(collectionNode);
                }

                Nodes.Add(rootNode);
            }

            if (!string.IsNullOrEmpty(selected))
            {
                SelectedNode = FindNode(selected, Nodes);
            }
        }

        public void OnPropertyValueChanged(string property, string value)
        {
            if (SelectedNode == null) return;

            if (string.IsNullOrEmpty(value))
            {
                value = "\"\"";
            }

            GenerateUpdateInCareerCommand(SelectedNode.FullPath, property, value);

            if (SelectedNode.Level == 1 && property == "CollectionName")
            {
                SelectedNode.Name = value.Trim('"');
                var parentPath = SelectedNode.Parent?.FullPath;
                if (!string.IsNullOrEmpty(parentPath))
                {
                    LoadTree($"{parentPath}|{SelectedNode.Name}");
                }
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

            if (SelectedNode.Level == 1 && SelectedNode.Parent != null)
            {
                SelectedObject = _career.GetCollection(SelectedNode.Name, SelectedNode.Parent.Name);
            }
            else if (SelectedNode.Level == 3 && SelectedNode.Parent?.Parent?.Parent != null)
            {
                string root = SelectedNode.Parent.Parent.Parent.Name;
                string collectionName = SelectedNode.Parent.Parent.Name;
                string expand = SelectedNode.Parent.Name;
                SelectedObject = _career.GetCollection(collectionName, root)?.GetSubPart(SelectedNode.Name, expand);
            }
            else
            {
                SelectedObject = null;
            }

            RaiseCanExecute();
        }

        private void RaiseCanExecute()
        {
            AddCollectionCommand.RaiseCanExecuteChanged();
            RemoveCollectionCommand.RaiseCanExecuteChanged();
            CopyCollectionCommand.RaiseCanExecuteChanged();
            ScriptCollectionCommand.RaiseCanExecuteChanged();
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

        private void AddCollection()
        {
            if (SelectedNode == null || SelectedNode.Level != 0) return;

            var input = new TextInputDialog("Enter name of the new collection") { Owner = Application.Current.MainWindow };
            if (input.ShowDialog() != true) return;

            try
            {
                _career.AddCollection(input.Value, SelectedNode.Name);
                GenerateAddInCareerCommand(SelectedNode.Name, input.Value);
                LoadTree($"{SelectedNode.Name}|{input.Value}");
            }
            catch (Exception ex)
            {
                _interactionService.ShowMessage(ex.GetLowestMessage(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveCollection()
        {
            if (SelectedNode?.Parent == null || SelectedNode.Level != 1) return;

            try
            {
                _career.RemoveCollection(SelectedNode.Name, SelectedNode.Parent.Name);
                GenerateRemoveInCareerCommand(SelectedNode.Parent.Name, SelectedNode.Name);
                SelectedObject = null;
                LoadTree(SelectedNode.Parent.FullPath);
            }
            catch (Exception ex)
            {
                _interactionService.ShowMessage(ex.GetLowestMessage(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CopyCollection()
        {
            if (SelectedNode?.Parent == null || SelectedNode.Level != 1) return;

            var input = new TextInputDialog("Enter name of the new collection") { Owner = Application.Current.MainWindow };
            if (input.ShowDialog() != true) return;

            try
            {
                _career.CloneCollection(input.Value, SelectedNode.Name, SelectedNode.Parent.Name);
                GenerateCopyInCareerCommand(SelectedNode.Parent.Name, SelectedNode.Name, input.Value);
                LoadTree($"{SelectedNode.Parent.Name}|{input.Value}");
            }
            catch (Exception ex)
            {
                _interactionService.ShowMessage(ex.GetLowestMessage(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ScriptCollection()
        {
            if (SelectedNode?.Parent == null || SelectedNode.Level != 1) return;

            var collection = _career.GetCollection(SelectedNode.Name, SelectedNode.Parent.Name);
            if (collection == null) return;

            const string empty = "\"\"";
            var properties = collection.GetAccessibles().ToList();
            properties.Sort();

            foreach (var property in properties)
            {
                if (property.Equals("CollectionName", StringComparison.InvariantCulture)) continue;
                var value = collection.GetValue(property);
                if (string.IsNullOrEmpty(value)) value = empty;
                GenerateUpdateInCareerCommand(SelectedNode.FullPath, property, value);
            }

            foreach (var node in SelectedNode.Children)
            {
                foreach (var subnode in node.Children)
                {
                    string path = subnode.FullPath;
                    string expand = node.Name;
                    string name = subnode.Name;
                    var part = collection.GetSubPart(name, expand);
                    if (part == null) continue;

                    foreach (var property in part.GetAccessibles())
                    {
                        var value = part.GetValue(property);
                        if (string.IsNullOrEmpty(value)) value = empty;
                        GenerateUpdateInCareerCommand(path, property, value);
                    }
                }
            }
        }

        private void GenerateUpdateInCareerCommand(string nodePath, string property, string value)
        {
            string[] splits = nodePath.Split('|', StringSplitOptions.RemoveEmptyEntries);

            if (property.Contains(' ')) property = $"\"{property}\"";
            if (value.Contains(' ')) value = $"\"{value}\"";
            if (string.IsNullOrEmpty(value)) value = "\"\"";

            string command = string.Empty;

            if (splits.Length == 2)
            {
                QuoteIfNeeded(splits);
                command = $"{eCommandType.update_incareer} {_careerPath} {splits[0]} {splits[1]} {property} {value}";
            }
            else if (splits.Length == 4)
            {
                QuoteIfNeeded(splits);
                command = $"{eCommandType.update_incareer} {_careerPath} {splits[0]} {splits[1]} {splits[2]} {splits[3]} {property} {value}";
            }

            if (!string.IsNullOrEmpty(command))
            {
                Commands.Add(command);
            }
        }

        private static void QuoteIfNeeded(string[] values)
        {
            for (int i = 0; i < values.Length; ++i)
            {
                if (values[i].Contains(' '))
                {
                    values[i] = $"\"{values[i]}\"";
                }
            }
        }

        private void GenerateAddInCareerCommand(string root, string name)
        {
            if (root.Contains(' ')) root = $"\"{root}\"";
            if (name.Contains(' ')) name = $"\"{name}\"";
            Commands.Add($"{eCommandType.add_incareer} {_careerPath} {root} {name}");
        }

        private void GenerateRemoveInCareerCommand(string root, string name)
        {
            if (root.Contains(' ')) root = $"\"{root}\"";
            if (name.Contains(' ')) name = $"\"{name}\"";
            Commands.Add($"{eCommandType.remove_incareer} {_careerPath} {root} {name}");
        }

        private void GenerateCopyInCareerCommand(string root, string copyName, string newName)
        {
            if (root.Contains(' ')) root = $"\"{root}\"";
            if (copyName.Contains(' ')) copyName = $"\"{copyName}\"";
            if (newName.Contains(' ')) newName = $"\"{newName}\"";
            Commands.Add($"{eCommandType.copy_incareer} {_careerPath} {root} {copyName} {newName}");
        }
    }
}
