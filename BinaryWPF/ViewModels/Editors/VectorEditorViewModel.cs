using BinaryWPF.Services;
using BinaryWPF.ViewModels.Editor;

using CoreExtensions.Management;

using Nikki.Support.Shared.Class;

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace BinaryWPF.ViewModels.Editors
{
    public sealed class VectorEditorViewModel : ViewModelBase
    {
        private readonly VectorVinyl _vector;
        private readonly IUserInteractionService _interactionService;
        private readonly IPromptService _promptService;

        private EditorTreeNodeViewModel? _selectedNode;
        private object? _selectedObject;

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

        public RelayCommand ImportSvgCommand { get; }
        public RelayCommand ExportSvgCommand { get; }
        public RelayCommand PreviewCommand { get; }
        public RelayCommand AddPathSetCommand { get; }
        public RelayCommand RemovePathSetCommand { get; }
        public RelayCommand MoveUpPathSetCommand { get; }
        public RelayCommand MoveDownPathSetCommand { get; }

        public VectorEditorViewModel(VectorVinyl vinyl)
        {
            _vector = vinyl;
            _interactionService = new UserInteractionService();
            _promptService = new PromptService();

            ImportSvgCommand = new RelayCommand(ImportSvg);
            ExportSvgCommand = new RelayCommand(ExportSvg);
            PreviewCommand = new RelayCommand(Preview);
            AddPathSetCommand = new RelayCommand(AddPathSet);
            RemovePathSetCommand = new RelayCommand(RemovePathSet, () => SelectedNode != null);
            MoveUpPathSetCommand = new RelayCommand(() => MovePathSet(-1), () => SelectedNode != null);
            MoveDownPathSetCommand = new RelayCommand(() => MovePathSet(1), () => SelectedNode != null);

            LoadTree();
        }

        public void LoadTree(int? selectIndex = null)
        {
            Nodes.Clear();

            for (int i = 0; i < _vector.NumberOfPaths; ++i)
            {
                _ = _vector.GetPathSet(i);
                Nodes.Add(new EditorTreeNodeViewModel($"PathSet{i}", $"PathSet{i}", 0, i, null));
            }

            if (selectIndex.HasValue && selectIndex.Value >= 0 && selectIndex.Value < Nodes.Count)
            {
                SelectedNode = Nodes[selectIndex.Value];
            }
        }

        private void UpdateSelectedObject()
        {
            if (SelectedNode == null)
            {
                SelectedObject = null;
            }
            else
            {
                SelectedObject = _vector.GetPathSet(SelectedNode.Index);
            }

            RemovePathSetCommand.RaiseCanExecuteChanged();
            MoveUpPathSetCommand.RaiseCanExecuteChanged();
            MoveDownPathSetCommand.RaiseCanExecuteChanged();
        }

        private void ImportSvg()
        {
            if (!_interactionService.TryOpenFile("Scalable Vector Graphics Files|*.svg", "Import .svg File", out var file))
            {
                return;
            }

            try
            {
                _vector.ReadFromFile(file);
                SelectedObject = null;
                LoadTree();
                _interactionService.ShowMessage($"File {file} has been successfully imported.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _interactionService.ShowMessage(ex.GetLowestMessage(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportSvg()
        {
            string[] options =
            {
                "512x512",
                "1024x1024",
                "2048x2048",
                "4096x4096",
                "8192x8192",
                "16384x16384",
                "32768x32768",
                "65536x65536",
            };

            int[] resolutions =
            {
                512,
                1024,
                2048,
                4096,
                8192,
                16384,
                32768,
                65536,
            };

            int choice = _promptService.ShowCombo("Select resolution in which vector vinyl should be exported", options, 0);

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                AddExtension = true,
                CheckPathExists = true,
                DefaultExt = ".svg",
                Filter = "Scalable Vector Graphics Files|*.svg|All Files|*.*",
                FileName = _vector.CollectionName,
                OverwritePrompt = true,
                Title = "Export .svg File"
            };

            if (dialog.ShowDialog() != true) return;

            try
            {
                string svg = _vector.GetSVGString(resolutions[choice]);
                File.WriteAllText(dialog.FileName, svg);
                _interactionService.ShowMessage($"Vector {_vector.CollectionName} has been successfully exported.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _interactionService.ShowMessage(ex.GetLowestMessage(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Preview()
        {
            try
            {
                string svg = _vector.GetSVGString(1024);
                string? dir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName);
                if (string.IsNullOrEmpty(dir)) return;
                string file = Path.Combine(dir, "vectordev.html");
                File.WriteAllText(file, svg);
                _ = Process.Start(new ProcessStartInfo($"\"{file}\"") { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                _interactionService.ShowMessage(ex.GetLowestMessage(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddPathSet()
        {
            int count = _vector.NumberOfPaths;
            _vector.AddPathSet();
            LoadTree(count);
        }

        private void RemovePathSet()
        {
            if (SelectedNode == null) return;

            _vector.RemovePathSet(SelectedNode.Index);
            SelectedObject = null;
            LoadTree(Math.Min(SelectedNode.Index, _vector.NumberOfPaths - 1));
        }

        private void MovePathSet(int delta)
        {
            if (SelectedNode == null) return;

            int index1 = SelectedNode.Index;
            int index2 = index1 + delta;

            if (index2 < 0 || index2 >= _vector.NumberOfPaths)
            {
                _interactionService.ShowMessage("Unable to move because selected node is out of range", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _vector.SwitchPaths(index1, index2);
            LoadTree(index2);
        }
    }
}
