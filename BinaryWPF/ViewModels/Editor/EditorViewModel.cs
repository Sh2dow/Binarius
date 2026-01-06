using BinaryWPF.Properties;
using BinaryWPF.Services;
using BinaryWPF.Views;

using CoreExtensions.Management;

using Endscript.Commands;
using Endscript.Core;
using Endscript.Enums;
using Endscript.Profiles;

using Nikki.Core;
using Nikki.Reflection.Abstract;
using Nikki.Reflection.Interface;
using Nikki.Support.Carbon.Framework;
using Nikki.Support.Shared.Class;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Application = System.Windows.Application;

namespace BinaryWPF.ViewModels.Editor
{
    public sealed class EditorViewModel : ViewModelBase
    {
        private readonly IUserInteractionService _interactionService;
        private readonly IPromptService _promptService;
        private readonly IWindowService _windowService;
        private readonly ThemeService _themeService;

        private BaseProfile? _profile;
        private EditorTreeNodeViewModel? _selectedNode;
        private object? _selectedObject;
        private string _statusText = "Waiting...";
        private string _nodeInfo = "| Ready";
        private string _commandText = string.Empty;
        private string _findText = string.Empty;
        private bool _edited;

        public ObservableCollection<EditorTreeNodeViewModel> Nodes { get; } = new();

        public EditorTreeNodeViewModel? SelectedNode
        {
            get => _selectedNode;
            set
            {
                if (SetField(ref _selectedNode, value))
                {
                    OnSelectedNodeChanged();
                }
            }
        }

        public object? SelectedObject
        {
            get => _selectedObject;
            private set => SetField(ref _selectedObject, value);
        }

        public string StatusText
        {
            get => _statusText;
            private set => SetField(ref _statusText, value);
        }

        public string NodeInfo
        {
            get => _nodeInfo;
            private set => SetField(ref _nodeInfo, value);
        }

        public string CommandText
        {
            get => _commandText;
            set => SetField(ref _commandText, value);
        }

        public string FindText
        {
            get => _findText;
            set
            {
                if (SetField(ref _findText, value))
                {
                    ApplyFindHighlight();
                }
            }
        }

        public RelayCommand LoadFilesCommand { get; }
        public RelayCommand ReloadFilesCommand { get; }
        public RelayCommand SaveFilesCommand { get; }
        public RelayCommand ImportEndscriptCommand { get; }
        public RelayCommand NewLauncherCommand { get; }
        public RelayCommand ExitCommand { get; }
        public RelayCommand RunCommandCommand { get; }
        public RelayCommand RunAllCommand { get; }
        public RelayCommand GenerateCommandCommand { get; }
        public RelayCommand ClearCommandCommand { get; }
        public RelayCommand OpenDirCommand { get; }
        public RelayCommand RunGameCommand { get; }
        public RelayCommand NewWindowCommand { get; }
        public RelayCommand AboutCommand { get; }
        public RelayCommand TutorialsCommand { get; }
        public RelayCommand ThemesCommand { get; }
        public RelayCommand SettingsCommand { get; }
        public RelayCommand HasherCommand { get; }
        public RelayCommand RaiderCommand { get; }
        public RelayCommand SwatcherCommand { get; }
        public RelayCommand CreateBackupsCommand { get; }
        public RelayCommand RestoreBackupsCommand { get; }
        public RelayCommand UnlockFilesCommand { get; }
        public RelayCommand InstallSpeedReflectCommand { get; }
        public RelayCommand AddNodeCommand { get; }
        public RelayCommand RemoveNodeCommand { get; }
        public RelayCommand CopyNodeCommand { get; }
        public RelayCommand ExportNodeCommand { get; }
        public RelayCommand ImportNodeCommand { get; }
        public RelayCommand ScriptNodeCommand { get; }
        public RelayCommand OpenEditorCommand { get; }
        public RelayCommand MoveNodeUpCommand { get; }
        public RelayCommand MoveNodeDownCommand { get; }
        public RelayCommand MoveNodeFirstCommand { get; }
        public RelayCommand MoveNodeLastCommand { get; }

        public EditorViewModel()
        {
            _interactionService = new UserInteractionService();
            _promptService = new PromptService();
            _windowService = new WindowService();
            _themeService = new ThemeService();

            LoadFilesCommand = new RelayCommand(LoadFiles);
            ReloadFilesCommand = new RelayCommand(ReloadFiles, () => _profile != null);
            SaveFilesCommand = new RelayCommand(SaveFiles, () => _profile != null);
            ImportEndscriptCommand = new RelayCommand(ImportEndscript, () => _profile != null);
            NewLauncherCommand = new RelayCommand(OpenNewLauncher);
            ExitCommand = new RelayCommand(() => Application.Current.MainWindow?.Close());
            RunCommandCommand = new RelayCommand(RunCommand, () => _profile != null);
            RunAllCommand = new RelayCommand(RunAllCommands, () => _profile != null);
            GenerateCommandCommand = new RelayCommand(GenerateCommand, () => _profile != null);
            ClearCommandCommand = new RelayCommand(() => CommandText = string.Empty);
            OpenDirCommand = new RelayCommand(OpenDirectory, () => _profile != null);
            RunGameCommand = new RelayCommand(RunGame, () => _profile != null);
            NewWindowCommand = new RelayCommand(OpenNewWindow);
            AboutCommand = new RelayCommand(() => _windowService.ShowWindow<AboutWindow>());
            TutorialsCommand = new RelayCommand(() => _interactionService.ShowMessage("Join Discord server at the start page to get help and full tool documentation!", "Info", MessageBoxButton.OK, MessageBoxImage.Information));
            ThemesCommand = new RelayCommand(ChangeTheme);
            SettingsCommand = new RelayCommand(() => _windowService.ShowDialog<OptionsWindow>());
            HasherCommand = new RelayCommand(() => _windowService.ShowWindow<HasherWindow>());
            RaiderCommand = new RelayCommand(() => _windowService.ShowWindow<RaiderWindow>());
            SwatcherCommand = new RelayCommand(() => _windowService.ShowWindow<SwatcherWindow>());
            CreateBackupsCommand = new RelayCommand(() => CreateBackupsForFiles(true), () => _profile != null);
            RestoreBackupsCommand = new RelayCommand(RestoreBackups, () => _profile != null);
            UnlockFilesCommand = new RelayCommand(UnlockMemoryFiles, () => _profile != null);
            InstallSpeedReflectCommand = new RelayCommand(InstallSpeedReflect, () => _profile != null);
            AddNodeCommand = new RelayCommand(AddNode, CanAddNode);
            RemoveNodeCommand = new RelayCommand(RemoveNode, CanRemoveNode);
            CopyNodeCommand = new RelayCommand(CopyNode, CanRemoveNode);
            ExportNodeCommand = new RelayCommand(ExportNode, CanRemoveNode);
            ImportNodeCommand = new RelayCommand(ImportNode, CanAddNode);
            ScriptNodeCommand = new RelayCommand(ScriptNode, CanScriptNode);
            OpenEditorCommand = new RelayCommand(OpenEditor, CanRemoveNode);
            MoveNodeUpCommand = new RelayCommand(MoveNodeUp, CanMoveNode);
            MoveNodeDownCommand = new RelayCommand(MoveNodeDown, CanMoveNode);
            MoveNodeFirstCommand = new RelayCommand(MoveNodeFirst, CanMoveNode);
            MoveNodeLastCommand = new RelayCommand(MoveNodeLast, CanMoveNode);
        }

        public void Initialize()
        {
            string file = Configurations.Default.LaunchFile;
            if (File.Exists(file))
            {
                LoadProfile(file, false);
            }
        }

        public void SetSelectedObject(object? value)
        {
            SelectedObject = value;
        }

        public void OnPropertyValueChanged(string property, string value)
        {
            if (SelectedNode == null) return;

            if (string.IsNullOrEmpty(value))
            {
                value = "\"\"";
            }

            string command = GenerateEndCommand(eCommandType.update_collection, SelectedNode.FullPath, property, value);
            AppendCommand(command);
            _edited = true;

            if (property == "CollectionName")
            {
                SelectedNode.Name = value.Trim('"');
                OnPropertyChanged(nameof(Nodes));
            }
        }

        public void AppendCommands(IEnumerable<string> lines)
        {
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    AppendCommand(line);
                }
            }

            _edited = true;
        }

        public bool CanClose()
        {
            if (!_edited) return true;

            var result = _interactionService.ShowMessage(
                "You have unsaved changes. Are you sure you want to quit the editor?",
                "Prompt",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            return result == MessageBoxResult.Yes;
        }

        private void LoadFiles()
        {
            if (_edited)
            {
                var result = _interactionService.ShowMessage(
                    "You have unsaved changes. Are you sure you want to load another database?",
                    "Prompt",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }

            if (!_interactionService.TryOpenFile(
                    "Binary/ius End Launcher Files|*.end;*.endlauncher|Binary End Launcher Files|*.end|Binarius End Launcher Files|*.endlauncher|All Files|*.*",
                    "Load End Launcher",
                    out var file))
            {
                return;
            }

            LoadProfile(file, true);
        }

        private void ReloadFiles()
        {
            string file = Configurations.Default.LaunchFile;

            if (!File.Exists(file))
            {
                _interactionService.ShowMessage($"Launch file {file} does not exist or was moved.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_edited)
            {
                var result = _interactionService.ShowMessage(
                    "You have unsaved changes. Are you sure you reload database?",
                    "Prompt",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }

            LoadProfile(file, true);
        }

        private void SaveFiles()
        {
            if (_profile == null) return;

            StatusText = "Saving... Please wait...";
            var watch = Stopwatch.StartNew();
            string[] exceptions = _profile.Save();
            watch.Stop();

            foreach (string exception in exceptions)
            {
                _interactionService.ShowMessage(exception, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            string filename = Configurations.Default.LaunchFile;
            StatusText = EditorHelpers.GetStatusString(_profile.Count, watch.ElapsedMilliseconds, filename, "Saving");
            _edited = false;
        }

        private void ImportEndscript()
        {
            if (_profile == null) return;

            if (!_interactionService.TryOpenFile(
                    "Binary/ius Endscript Files|*.end;*.endscript|Binary Endscript Files|*.end|Binarius Endscript Files|*.endscript|All Files|*.*",
                    "Import Endscript",
                    out var path))
            {
                return;
            }

            var parser = new EndScriptParser(path);
            BaseCommand[] commands;

            try
            {
                commands = parser.Read();
            }
            catch (Exception ex)
            {
                string error = $"Error has occured -> File: {parser.CurrentFile}, Line: {parser.CurrentIndex}" +
                    Environment.NewLine + $"Command: [{parser.CurrentLine}]" + Environment.NewLine +
                    $"Error: {ex.GetLowestMessage()}";

                _interactionService.ShowMessage(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var manager = new EndScriptManager(_profile, commands, path);

            try
            {
                manager.CommandChase();

                while (!manager.ProcessScript())
                {
                    var command = manager.CurrentCommand;

                    if (command is InfoboxCommand infobox)
                    {
                        _promptService.ShowInfo(infobox.Description);
                    }
                    else if (command is CheckboxCommand checkbox)
                    {
                        checkbox.Choice = _promptService.ShowCheckbox(checkbox.Description, true) ? 1 : 0;
                    }
                    else if (command is ComboboxCommand combobox)
                    {
                        string[] options = combobox.Options.Select(option => option.Name).ToArray();
                        combobox.Choice = _promptService.ShowCombo(combobox.Description, options, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                _interactionService.ShowMessage(ex.GetLowestMessage(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (manager.Errors.Any())
            {
                LaunchHelpers.WriteErrorsToLog(manager.Errors, path);
                _interactionService.ShowMessage(
                    $"Script {Path.GetFileName(path)} has been applied, however, {manager.Errors.Count()} errors have been detected. Check EndError.log for more information",
                    "Information",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            else
            {
                _interactionService.ShowMessage(
                    $"Script {Path.GetFileName(path)} has been successfully applied",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }

            LoadTreeView(SelectedNode?.FullPath);
        }

        private void OpenNewLauncher()
        {
            var window = new LanMakerWindow { Owner = Application.Current.MainWindow };
            _ = window.ShowDialog();
        }

        private void RunCommand()
        {
            if (_profile == null) return;

            if (string.IsNullOrWhiteSpace(CommandText)) return;

            string? line = CommandText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            if (string.IsNullOrWhiteSpace(line)) return;

            try
            {
                _ = EndScriptParser.ExecuteSingleCommand(line, _profile);
            }
            catch (Exception ex)
            {
                string error = $"Error has occured -> Command: [{line}]" + Environment.NewLine +
                    $"Error: {ex.GetLowestMessage()}";

                _interactionService.ShowMessage(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            LoadTreeView(SelectedNode?.FullPath);
        }

        private void RunAllCommands()
        {
            if (_profile == null) return;

            if (string.IsNullOrWhiteSpace(CommandText)) return;

            string command = string.Empty;
            int count = 0;

            try
            {
                foreach (string line in CommandText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    command = line;
                    _ = EndScriptParser.ExecuteSingleCommand(line, _profile);
                    ++count;
                }
            }
            catch (Exception ex)
            {
                string error = $"Error has occured -> Line: {count}" + Environment.NewLine +
                    $"Command: [{command}]" + Environment.NewLine + $"Error: {ex.GetLowestMessage()}";

                _interactionService.ShowMessage(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            LoadTreeView(SelectedNode?.FullPath);
        }

        private void GenerateCommand()
        {
            if (SelectedNode == null || SelectedObject == null) return;

            var type = SelectedObject.GetType();
            if (type != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
            {
                return;
            }

            string line = GenerateEndCommand(eCommandType.update_collection, SelectedNode.FullPath, "CollectionName", SelectedNode.Name);
            AppendCommand(line);
        }

        private void OpenDirectory()
        {
            if (_profile == null || _profile.Count == 0) return;

            string path = _profile[0].Folder;
            Process.Start(new ProcessStartInfo("explorer.exe", path));
        }

        private void RunGame()
        {
            if (_profile == null) return;

            try
            {
                ProcessHelpers.LaunchGame(_profile.Directory, _profile.GameINT);
            }
            catch (Exception e)
            {
                _interactionService.ShowMessage(e.GetLowestMessage(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenNewWindow()
        {
            string? path = Process.GetCurrentProcess().MainModule?.FileName;
            if (string.IsNullOrEmpty(path)) return;
            _ = Process.Start(new ProcessStartInfo() { FileName = path });
        }

        private void ChangeTheme()
        {
            _windowService.ShowDialog<ThemeSelectorWindow>();
            _themeService.ApplyThemeResources();
        }

        private void RestoreBackups()
        {
            if (_profile == null || _profile.Count == 0)
            {
                _interactionService.ShowMessage("No files are open and directory is not chosen", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int count = 0;
            foreach (var sdb in _profile)
            {
                string from = $"{sdb.FullPath}.bacc";
                string to = sdb.FullPath;
                if (File.Exists(from))
                {
                    File.Copy(from, to, true);
                    ++count;
                }
            }

            if (count == 0)
            {
                _interactionService.ShowMessage("No backup files were found.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            LoadProfile(Configurations.Default.LaunchFile, true);
            _interactionService.ShowMessage("All backups have been successfully restored.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UnlockMemoryFiles()
        {
            if (_profile == null || _profile.Count == 0)
            {
                _interactionService.ShowMessage("No files are open and directory is not chosen", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string path = _profile[0].Folder;

            Nikki.Utils.MemoryUnlock.FastUnlock(path + @"\\GLOBAL\\CARHEADERSMEMORYFILE.BIN");
            Nikki.Utils.MemoryUnlock.FastUnlock(path + @"\\GLOBAL\\FRONTENDMEMORYFILE.BIN");
            Nikki.Utils.MemoryUnlock.FastUnlock(path + @"\\GLOBAL\\INGAMEMEMORYFILE.BIN");
            Nikki.Utils.MemoryUnlock.FastUnlock(path + @"\\GLOBAL\\PERMANENTMEMORYFILE.BIN");
            Nikki.Utils.MemoryUnlock.LongUnlock(path + @"\\GLOBAL\\GLOBALMEMORYFILE.BIN");

            _interactionService.ShowMessage("Memory files were successfully unlocked for modding.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void InstallSpeedReflect()
        {
            if (_profile == null || _profile.Count == 0)
            {
                _interactionService.ShowMessage("No files are open and directory is not chosen", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string dir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty) ?? string.Empty;
            string speedfrom = Path.Combine(dir, "SpeedReflect.asi");

            if (!File.Exists(speedfrom))
            {
                _interactionService.ShowMessage("SpeedReflect.asi was not found in the Binary directory.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string speedto = Path.Combine(_profile.Directory, @"scripts\\SpeedReflect.asi");
            Directory.CreateDirectory(Path.Combine(_profile.Directory, "scripts"));
            File.Copy(speedfrom, speedto, true);

            _interactionService.ShowMessage("Successfully installed SpeedReflect.asi.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LoadProfile(string filename, bool showErrors)
        {
            try
            {
                _edited = false;
                Launch.Deserialize(filename, out var launch);
                launch.ThisDir = Path.GetDirectoryName(filename);

                LaunchHelpers.FixLaunchDirectory(launch, filename);

                if (launch.UsageID != eUsage.Modder)
                {
                    throw new Exception($"Usage type of the endscript is stated to be {launch.Usage}, while should be Modder");
                }

                if (launch.GameID == GameINT.None)
                {
                    throw new Exception($"Invalid stated game type named {launch.Game}");
                }

                if (!Directory.Exists(launch.Directory))
                {
                    throw new DirectoryNotFoundException($"Directory named {launch.Directory} does not exist");
                }

                SelectedObject = null;
                _profile = BaseProfile.NewProfile(launch.GameID, launch.Directory);
                StatusText = "Loading... Please wait...";

                var watch = Stopwatch.StartNew();
                string[] exceptions = _profile.Load(launch);
                watch.Stop();

                foreach (string exception in exceptions)
                {
                    _interactionService.ShowMessage(exception, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                StatusText = EditorHelpers.GetStatusString(launch.Files.Count, watch.ElapsedMilliseconds, filename, "Loading");
                LoadTreeView();

                if (Configurations.Default.AutoBackups)
                {
                    CreateBackupsForFiles(false);
                }

                Configurations.Default.LaunchFile = filename;
                Configurations.Default.CurrentGame = (int)_profile.GameINT;
                Configurations.Default.Save();

                OnPropertyChanged(nameof(SelectedObject));
            }
            catch (Exception e)
            {
                if (showErrors)
                {
                    _interactionService.ShowMessage(e.GetLowestMessage(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                Nodes.Clear();
            }
        }

        private void CreateBackupsForFiles(bool forced)
        {
            if (_profile == null) return;

            if (_profile.Count == 0)
            {
                if (forced)
                {
                    _interactionService.ShowMessage("No files are open and directory is not chosen", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return;
            }

            foreach (var sdb in _profile)
            {
                string from = sdb.FullPath;
                string to = $"{sdb.FullPath}.bacc";

                if (forced || (!forced && !File.Exists(to)))
                {
                    File.Copy(from, to, true);
                }
            }

            if (forced)
            {
                _interactionService.ShowMessage("All files have been successfully backed up.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LoadTreeView(string? selectedPath = null)
        {
            Nodes.Clear();
            if (_profile == null) return;

            _profile.Sort();

            foreach (var sdb in _profile)
            {
                var root = new EditorTreeNodeViewModel(sdb.Filename, sdb.Filename, 0, Nodes.Count, null);

                int managerIndex = 0;
                foreach (var manager in sdb.Database.Managers)
                {
                    if (manager.Count == 0 && Configurations.Default.HideEmptyManagers)
                    {
                        continue;
                    }

                    var managerNode = new EditorTreeNodeViewModel(manager.Name, root.FullPath + "|" + manager.Name, 1, managerIndex++, root);

                    int collectionIndex = 0;
                    foreach (Collectable collection in manager)
                    {
                        var collectionNode = new EditorTreeNodeViewModel(collection.CollectionName, managerNode.FullPath + "|" + collection.CollectionName, 2, collectionIndex++, managerNode);

                        int expandIndex = 0;
                        foreach (var expando in collection.GetAllNodes())
                        {
                            var expandNode = new EditorTreeNodeViewModel(expando.NodeName, collectionNode.FullPath + "|" + expando.NodeName, 3, expandIndex++, collectionNode);

                            int subIndex = 0;
                            foreach (var subpart in expando.SubNodes)
                            {
                                var subNode = new EditorTreeNodeViewModel(subpart.NodeName, expandNode.FullPath + "|" + subpart.NodeName, 4, subIndex++, expandNode);
                                expandNode.Children.Add(subNode);
                            }

                            collectionNode.Children.Add(expandNode);
                        }

                        managerNode.Children.Add(collectionNode);
                    }

                    root.Children.Add(managerNode);
                }

                Nodes.Add(root);
            }

            if (!string.IsNullOrEmpty(selectedPath))
            {
                SelectedNode = FindNodeByPath(selectedPath, Nodes);
            }
        }

        private EditorTreeNodeViewModel? FindNodeByPath(string path, IEnumerable<EditorTreeNodeViewModel> nodes)
        {
            foreach (var node in nodes)
            {
                if (node.FullPath == path)
                {
                    return node;
                }

                var found = FindNodeByPath(path, node.Children);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private void OnSelectedNodeChanged()
        {
            if (_profile == null || SelectedNode == null)
            {
                SelectedObject = null;
                NodeInfo = "| Ready";
                RaiseContextCanExecute();
                return;
            }

            var reflective = EditorHelpers.GetReflective(SelectedNode.FullPath, "|", _profile);
            SelectedObject = reflective;
            NodeInfo = $"| Index: {SelectedNode.Index} | {SelectedNode.Children.Count} subnodes";
            RaiseContextCanExecute();
        }

        private void RaiseContextCanExecute()
        {
            AddNodeCommand.RaiseCanExecuteChanged();
            RemoveNodeCommand.RaiseCanExecuteChanged();
            CopyNodeCommand.RaiseCanExecuteChanged();
            ExportNodeCommand.RaiseCanExecuteChanged();
            ImportNodeCommand.RaiseCanExecuteChanged();
            ScriptNodeCommand.RaiseCanExecuteChanged();
            OpenEditorCommand.RaiseCanExecuteChanged();
            MoveNodeUpCommand.RaiseCanExecuteChanged();
            MoveNodeDownCommand.RaiseCanExecuteChanged();
            MoveNodeFirstCommand.RaiseCanExecuteChanged();
            MoveNodeLastCommand.RaiseCanExecuteChanged();
        }

        private bool CanAddNode() => _profile != null && SelectedNode?.Level == 1;
        private bool CanRemoveNode() => _profile != null && SelectedNode?.Level == 2;
        private bool CanScriptNode()
        {
            if (_profile == null || SelectedNode?.Level != 2) return false;
            var manager = GetManagerForSelectedCollection();
            return manager != null &&
                   !typeof(DBModelPart).IsAssignableFrom(manager.CollectionType) &&
                   !typeof(FNGroup).IsAssignableFrom(manager.CollectionType) &&
                   !typeof(STRBlock).IsAssignableFrom(manager.CollectionType);
        }
        private bool CanMoveNode() => _profile != null && SelectedNode?.Level == 2;

        private void AddNode()
        {
            if (_profile == null || SelectedNode?.Level != 1) return;

            var manager = GetManagerForSelectedManager();
            if (manager == null || manager.IsReadOnly) return;

            var input = new Views.UI.TextInputDialog("Enter name of the new collection") { Owner = Application.Current.MainWindow };
            if (input.ShowDialog() != true) return;

            try
            {
                manager.Add(input.Value);
                string path = SelectedNode.FullPath;
                AppendCommand(GenerateEndCommand(eCommandType.add_collection, path, input.Value));
                LoadTreeView(SelectedNode.FullPath);
                _edited = true;
            }
            catch (Exception ex)
            {
                _interactionService.ShowMessage(ex.GetLowestMessage(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveNode()
        {
            if (_profile == null || SelectedNode?.Level != 2) return;

            var manager = GetManagerForSelectedCollection();
            if (manager == null || manager.IsReadOnly) return;

            try
            {
                manager.Remove(SelectedNode.Name);
                AppendCommand(GenerateEndCommand(eCommandType.remove_collection, SelectedNode.FullPath));
                LoadTreeView(SelectedNode.Parent?.FullPath);
                _edited = true;
            }
            catch (Exception ex)
            {
                _interactionService.ShowMessage(ex.GetLowestMessage(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CopyNode()
        {
            if (_profile == null || SelectedNode?.Level != 2) return;

            var manager = GetManagerForSelectedCollection();
            if (manager == null || manager.IsReadOnly) return;

            var input = new Views.UI.TextInputDialog("Enter name of the new collection") { Owner = Application.Current.MainWindow };
            if (input.ShowDialog() != true) return;

            try
            {
                manager.Clone(input.Value, SelectedNode.Name);
                AppendCommand(GenerateEndCommand(eCommandType.copy_collection, SelectedNode.FullPath, input.Value));
                LoadTreeView(SelectedNode.Parent?.FullPath);
                _edited = true;
            }
            catch (Exception ex)
            {
                _interactionService.ShowMessage(ex.GetLowestMessage(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportNode()
        {
            if (_profile == null || SelectedNode?.Level != 2) return;

            var manager = GetManagerForSelectedCollection();
            if (manager == null) return;

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = ".bin",
                Filter = "Binary Files|*.bin|All Files|*.*",
                FileName = SelectedNode.Name,
                OverwritePrompt = true,
                Title = "Export Collection"
            };

            if (typeof(FNGroup).IsAssignableFrom(manager.CollectionType))
            {
                dialog.DefaultExt = ".fng";
                dialog.Filter = "FNG Files|*.fng|" + dialog.Filter;
            }
            else if (typeof(TPKBlock).IsAssignableFrom(manager.CollectionType))
            {
                dialog.DefaultExt = ".tpk";
                dialog.Filter = "Texture Packages|*.tpk|" + dialog.Filter;
            }

            if (dialog.ShowDialog() != true) return;

            try
            {
                bool serialized = !manager.AllowsNoSerialization;
                using var bw = new BinaryWriter(File.Open(dialog.FileName, FileMode.Create));
                manager.Export(SelectedNode.Name, bw, serialized);
                _interactionService.ShowMessage($"Collection {SelectedNode.Name} has been exported to path {dialog.FileName}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _interactionService.ShowMessage(ex.GetLowestMessage(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportNode()
        {
            if (_profile == null || SelectedNode?.Level != 1) return;

            var manager = GetManagerForSelectedManager();
            if (manager == null) return;

            var importMode = new Views.UI.ImportModeDialog { Owner = Application.Current.MainWindow };
            if (importMode.ShowDialog() != true) return;

            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".bin",
                Filter = "Binary Files|*.bin|All Files|*.*",
                Title = "Import Collection from File"
            };

            if (typeof(FNGroup).IsAssignableFrom(manager.CollectionType))
            {
                dialog.DefaultExt = ".fng";
                dialog.Filter = "FNG Files|*.fng|" + dialog.Filter;
            }
            else if (typeof(TPKBlock).IsAssignableFrom(manager.CollectionType))
            {
                dialog.DefaultExt = ".tpk";
                dialog.Filter = "Texture Packages|*.tpk|" + dialog.Filter;
            }

            if (dialog.ShowDialog() != true) return;

            try
            {
                using var br = new BinaryReader(File.Open(dialog.FileName, FileMode.Open));
                manager.Import(importMode.Mode, br);
                _interactionService.ShowMessage($"File {dialog.FileName} has been imported with type {importMode.Mode}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadTreeView(SelectedNode.FullPath);
                _edited = true;
            }
            catch (Exception ex)
            {
                _interactionService.ShowMessage(ex.GetLowestMessage(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ScriptNode()
        {
            if (_profile == null || SelectedNode?.Level != 2) return;

            var manager = GetManagerForSelectedCollection();
            if (manager == null) return;

            var collection = manager[manager.IndexOf(SelectedNode.Name)] as Collectable;
            if (collection == null) return;

            const string empty = "\"\"";
            var properties = collection.GetAccessibles().ToList();
            properties.Sort();

            var lines = new List<string>(properties.Count);
            string path = SelectedNode.FullPath;

            foreach (string property in properties)
            {
                if (property.Equals("CollectionName", StringComparison.InvariantCulture)) continue;
                string value = collection.GetValue(property);
                if (string.IsNullOrEmpty(value)) value = empty;
                lines.Add(GenerateEndCommand(eCommandType.update_collection, path, property, value));
            }

            foreach (var node in SelectedNode.Children)
            {
                foreach (var subnode in node.Children)
                {
                    path = subnode.FullPath;
                    string expand = node.Name;
                    string name = subnode.Name;
                    var part = collection.GetSubPart(name, expand);
                    if (part == null) continue;

                    foreach (string property in part.GetAccessibles())
                    {
                        string value = part.GetValue(property);
                        if (string.IsNullOrEmpty(value)) value = empty;
                        lines.Add(GenerateEndCommand(eCommandType.update_collection, path, property, value));
                    }
                }
            }

            AppendCommands(lines);
        }

        private void OpenEditor()
        {
            if (_profile == null || SelectedNode?.Level != 2 || SelectedObject == null) return;

            var path = EditorHelpers.GetSeparatedPath(SelectedNode.FullPath, '|');
            var owner = Application.Current.MainWindow;

            switch (SelectedObject)
            {
                case TPKBlock tpk:
                {
                    var editor = new TextureEditorWindow(tpk, path) { Owner = owner };
                    _ = editor.ShowDialog();
                    AppendCommands(editor.Commands);
                    break;
                }
                case STRBlock str:
                {
                    var editor = new StringEditorWindow(str, path) { Owner = owner };
                    _ = editor.ShowDialog();
                    AppendCommands(editor.Commands);
                    break;
                }
                case DBModelPart model:
                {
                    var editor = new CarPartsEditorWindow(model) { Owner = owner };
                    _ = editor.ShowDialog();
                    break;
                }
                case GCareer career:
                {
                    var editor = new CareerEditorWindow(career, path) { Owner = owner };
                    _ = editor.ShowDialog();
                    AppendCommands(editor.Commands);
                    break;
                }
                case VectorVinyl vinyl:
                {
                    var editor = new VectorEditorWindow(vinyl) { Owner = owner };
                    _ = editor.ShowDialog();
                    break;
                }
                case FNGroup:
                    _interactionService.ShowMessage("This editor is not yet available in the WPF rewrite.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
            }
        }

        private void MoveNodeUp()
        {
            MoveSelectedNode(-1, eCommandType.move_collection_up, "Unable to move up because selected node is the up most node");
        }

        private void MoveNodeDown()
        {
            MoveSelectedNode(1, eCommandType.move_collection_down, "Unable to move down because selected node is the down most node");
        }

        private void MoveNodeFirst()
        {
            if (_profile == null || SelectedNode?.Level != 2) return;
            var manager = GetManagerForSelectedCollection();
            if (manager == null) return;

            int index1 = SelectedNode.Index;
            for (int index2 = index1 - 1; index2 >= 0; index1--, index2--)
            {
                manager.Switch(index1, index2);
            }

            AppendCommand(GenerateEndCommand(eCommandType.move_collection_first, SelectedNode.FullPath));
            LoadTreeView(SelectedNode.Parent?.FullPath);
            _edited = true;
        }

        private void MoveNodeLast()
        {
            if (_profile == null || SelectedNode?.Level != 2) return;
            var manager = GetManagerForSelectedCollection();
            if (manager == null) return;

            int index1 = SelectedNode.Index;
            int count = SelectedNode.Parent?.Children.Count ?? 0;
            for (int index2 = index1 + 1; index2 < count; index1++, index2++)
            {
                manager.Switch(index1, index2);
            }

            AppendCommand(GenerateEndCommand(eCommandType.move_collection_last, SelectedNode.FullPath));
            LoadTreeView(SelectedNode.Parent?.FullPath);
            _edited = true;
        }

        private void MoveSelectedNode(int delta, eCommandType commandType, string edgeMessage)
        {
            if (_profile == null || SelectedNode?.Level != 2) return;
            var manager = GetManagerForSelectedCollection();
            if (manager == null) return;

            int index1 = SelectedNode.Index;
            int index2 = index1 + delta;
            int count = SelectedNode.Parent?.Children.Count ?? 0;

            if (index2 < 0 || index2 >= count)
            {
                _interactionService.ShowMessage(edgeMessage, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                manager.Switch(index1, index2);
                AppendCommand(GenerateEndCommand(commandType, SelectedNode.FullPath));
                LoadTreeView(SelectedNode.Parent?.FullPath);
                _edited = true;
            }
            catch (Exception ex)
            {
                _interactionService.ShowMessage(ex.GetLowestMessage(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private IManager? GetManagerForSelectedManager()
        {
            if (_profile == null || SelectedNode?.Level != 1 || SelectedNode.Parent == null) return null;
            var sdb = _profile.Find(_ => _.Filename == SelectedNode.Parent.Name);
            return sdb?.Database.GetManager(SelectedNode.Name);
        }

        private IManager? GetManagerForSelectedCollection()
        {
            if (_profile == null || SelectedNode?.Level != 2 || SelectedNode.Parent?.Parent == null) return null;
            var sdb = _profile.Find(_ => _.Filename == SelectedNode.Parent.Parent.Name);
            return sdb?.Database.GetManager(SelectedNode.Parent.Name);
        }

        private void ApplyFindHighlight()
        {
            foreach (var node in Nodes)
            {
                ApplyFindHighlight(node, FindText);
            }
        }

        private void ApplyFindHighlight(EditorTreeNodeViewModel node, string match)
        {
            node.IsHighlighted = !string.IsNullOrEmpty(match) && node.Name.Contains(match, StringComparison.OrdinalIgnoreCase);

            foreach (var child in node.Children)
            {
                ApplyFindHighlight(child, match);
            }
        }

        private void AppendCommand(string line)
        {
            if (string.IsNullOrEmpty(CommandText) || CommandText.EndsWith(Environment.NewLine, StringComparison.Ordinal))
            {
                CommandText += line;
            }
            else
            {
                CommandText += Environment.NewLine + line;
            }

            if (!CommandText.EndsWith(Environment.NewLine, StringComparison.Ordinal))
            {
                CommandText += Environment.NewLine;
            }
        }

        private static string GenerateEndCommand(eCommandType type, string nodepath, params string[] args)
        {
            const string space = " ";
            string line = type + space;
            string[] splits = nodepath.Split('|');

            foreach (string split in splits)
            {
                line += split.Contains(' ') ? $"\"{split}\"" + space : split + space;
            }

            for (int loop = 0; loop < args.Length - 1; ++loop)
            {
                string arg = args[loop];
                line += arg.Contains(' ') ? $"\"{arg}\"" + space : arg + space;
            }

            string last = args[^1];
            line += last.Contains(' ') ? $"\"{last}\"" : last;
            return line;
        }
    }
}
