using BinaryWPF.Services;

using Endscript.Core;

using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace BinaryWPF.ViewModels.Settings
{
    public sealed class LanMakerViewModel : ViewModelBase
    {
        private readonly IUserInteractionService _interactionService;
        private string _directory = string.Empty;
        private string _endScriptFile = string.Empty;
        private string _selectedUsage = string.Empty;
        private string _selectedGame = string.Empty;

        public ObservableCollection<string> Usages { get; } = new() { "User", "Modder" };
        public ObservableCollection<string> Games { get; } = new() { "Carbon", "MostWanted", "Prostreet", "Undercover", "Underground1", "Underground2" };

        public string DirectoryPath
        {
            get => _directory;
            set => SetField(ref _directory, value);
        }

        public string EndScriptFile
        {
            get => _endScriptFile;
            set => SetField(ref _endScriptFile, value);
        }

        public string SelectedUsage
        {
            get => _selectedUsage;
            set
            {
                if (SetField(ref _selectedUsage, value))
                {
                    if (value == "User")
                    {
                        EndScriptFile = string.IsNullOrEmpty(EndScriptFile) ? "install.endscript" : EndScriptFile;
                    }
                    else
                    {
                        EndScriptFile = string.Empty;
                    }
                }
            }
        }

        public string SelectedGame
        {
            get => _selectedGame;
            set => SetField(ref _selectedGame, value);
        }

        public LanMakerViewModel()
        {
            _interactionService = new UserInteractionService();
            SelectedUsage = Usages[0];
            SelectedGame = Games[0];
        }

        public void BrowseDirectory()
        {
            if (_interactionService.TrySelectFolder("Select any Need for Speed game directory", out var path))
            {
                DirectoryPath = path;
            }
        }

        public void SaveLauncher(Window owner)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = ".endlauncher",
                Filter = "Binary/ius End Launcher Files|*.end;*.endlauncher|Binary End Launcher Files|*.end|Binarius End Launcher Files|*.endlauncher|All Files|*.*",
                OverwritePrompt = true,
                Title = "Save End Launcher"
            };

            if (dialog.ShowDialog(owner) != true) return;

            var launch = new Launch
            {
                Directory = DirectoryPath,
                Game = SelectedGame,
                Usage = SelectedUsage,
                Endscript = SelectedUsage == "User" ? EndScriptFile : string.Empty
            };

            if (SelectedUsage == "User" && !string.IsNullOrWhiteSpace(EndScriptFile))
            {
                var endPath = Path.Combine(Path.GetDirectoryName(dialog.FileName) ?? string.Empty, EndScriptFile);
                using var sw = new StreamWriter(File.Open(endPath, FileMode.Create));
                var ext = Path.GetExtension(EndScriptFile).ToLowerInvariant();
                if (ext != ".endscript")
                {
                    sw.WriteLine("[VERSN2]");
                }
                sw.WriteLine();
            }

            Launch.Serialize(dialog.FileName, launch);
            MessageBox.Show($"File {dialog.FileName} has been saved.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
