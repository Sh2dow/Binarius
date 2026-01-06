using BinaryWPF.Services;

using CoreExtensions.Management;

using Endscript.Commands;
using Endscript.Core;
using Endscript.Enums;
using Endscript.Profiles;

using Nikki.Core;

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Application = System.Windows.Application;

namespace BinaryWPF.ViewModels
{
    public sealed class IntroViewModel : ViewModelBase
    {
        private readonly IUserInteractionService _interactionService;
        private readonly IPromptService _promptService;
        private readonly IWindowService _windowService;
        private readonly ThemeService _themeService;

        public string Title => $"Binarius - v{GetType().Assembly.GetName().Version}";

        public Uri UserImage => (Uri)Application.Current.Resources["ThemeImageUser"];
        public Uri ModderImage => (Uri)Application.Current.Resources["ThemeImageModder"];
        public Uri ToolsImage => (Uri)Application.Current.Resources["ThemeImageTools"];
        public Uri ThemeImage => (Uri)Application.Current.Resources["ThemeImageTheme"];
        public Uri UpdatesImage => (Uri)Application.Current.Resources["ThemeImageUpdates"];

        public RelayCommand OpenUserCommand { get; }
        public RelayCommand OpenModderCommand { get; }
        public RelayCommand OpenThemeCommand { get; }
        public RelayCommand OpenUpdatesCommand { get; }
        public RelayCommand OpenAboutCommand { get; }
        public RelayCommand OpenLauncherCommand { get; }
        public RelayCommand OpenHasherCommand { get; }
        public RelayCommand OpenRaiderCommand { get; }
        public RelayCommand OpenSwatcherCommand { get; }
        public RelayCommand OpenOptionsCommand { get; }

        public IntroViewModel()
        {
            _interactionService = new UserInteractionService();
            _promptService = new PromptService();
            _windowService = new WindowService();
            _themeService = new ThemeService();

            OpenUserCommand = new RelayCommand(UserInteract);
            OpenModderCommand = new RelayCommand(ModderInteract);
            OpenThemeCommand = new RelayCommand(ChangeTheme);
            OpenUpdatesCommand = new RelayCommand(OpenUpdates);
            OpenAboutCommand = new RelayCommand(OpenAbout);
            OpenLauncherCommand = new RelayCommand(OpenLauncher);
            OpenHasherCommand = new RelayCommand(OpenHasher);
            OpenRaiderCommand = new RelayCommand(OpenRaider);
            OpenSwatcherCommand = new RelayCommand(OpenSwatcher);
            OpenOptionsCommand = new RelayCommand(OpenOptions);
        }

        private void UserInteract()
        {
            if (!_interactionService.TryOpenFile(
                    "Binary/ius End Launcher Files|*.end;*.endlauncher|Binary End Launcher Files|*.end|Binarius End Launcher Files|*.endlauncher|All Files|*.*",
                    "Select End Launcher",
                    out var launcherPath))
            {
                return;
            }

            Launch.Deserialize(launcherPath, out Launch launch);

            if (launch.UsageID != eUsage.User)
            {
                throw new Exception($"Usage type of the endscript is stated to be {launch.Usage}, while should be User");
            }

            if (launch.GameID == GameINT.None)
            {
                throw new Exception($"Invalid stated game type named {launch.Game}");
            }

            if (!_interactionService.TrySelectFolder($"Select Need for Speed: {launch.Game} directory to modify.", out var directory))
            {
                return;
            }

            launch.Directory = directory;
            launch.ThisDir = Path.GetDirectoryName(launcherPath);
            launch.CheckEndscript();
            launch.CheckFiles();
            launch.LoadLinks();

            var baseDir = launch.ThisDir ?? string.Empty;
            var endscript = Path.Combine(baseDir, launch.Endscript);
            var parser = new EndScriptParser(endscript);
            BaseCommand[] commands;

            try
            {
                commands = parser.Read();
            }
            catch (Exception ex)
            {
                var error = $"Error has occured -> File: {parser.CurrentFile}, Line: {parser.CurrentIndex}" +
                            Environment.NewLine + $"Command: [{parser.CurrentLine}]" + Environment.NewLine +
                            $"Error: {ex.GetLowestMessage()}";

                _interactionService.ShowMessage(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var profile = BaseProfile.NewProfile(launch.GameID, launch.Directory);
            var exceptions = profile.Load(launch);

            if (exceptions.Length > 0)
            {
                foreach (var exception in exceptions)
                {
                    _interactionService.ShowMessage(exception, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                _interactionService.ShowMessage("Unable to execute endscript because of the errors.", "Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            EnsureBackups(profile);

            var manager = new EndScriptManager(profile, commands, endscript);

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
                        var options = combobox.Options.Select(option => option.Name).ToArray();
                        combobox.Choice = _promptService.ShowCombo(combobox.Description, options, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                _interactionService.ShowMessage(ex.GetLowestMessage(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _interactionService.ShowMessage("Execution has been interrupted", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var script = Path.GetFileName(launcherPath);

            if (manager.Errors.Any())
            {
                LaunchHelpers.WriteErrorsToLog(manager.Errors, launcherPath);
                _interactionService.ShowMessage(
                    $"Script {script} has been applied, however, {manager.Errors.Count()} errors have been detected. Check EndError.log for more information",
                    "Information",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            else
            {
                _interactionService.ShowMessage(
                    $"Script {script} has been successfully applied",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }

            var save = _interactionService.ShowMessage("Would you like to save files?", "Prompt", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (save == MessageBoxResult.Yes)
            {
                var errors = profile.Save();

                if (errors.Length > 0)
                {
                    foreach (var error in errors)
                    {
                        _interactionService.ShowMessage(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    AskForGameRun(profile);
                }
            }
        }

        private void ModderInteract()
        {
            _windowService.ShowDialog<Views.EditorWindow>();
            RefreshThemeResources();
        }

        private void ChangeTheme()
        {
            _windowService.ShowDialog<Views.ThemeSelectorWindow>();
            RefreshThemeResources();
        }

        private void OpenUpdates()
        {
            ProcessHelpers.OpenBrowser("https://github.com/nlgxzef/Binarius/releases");
        }

        private void OpenAbout()
        {
            _windowService.ShowWindow<Views.AboutWindow>();
        }

        private void OpenLauncher()
        {
            _windowService.ShowWindow<Views.LanMakerWindow>();
        }

        private void OpenHasher()
        {
            _windowService.ShowWindow<Views.HasherWindow>();
        }

        private void OpenRaider()
        {
            _windowService.ShowWindow<Views.RaiderWindow>();
        }

        private void OpenSwatcher()
        {
            _windowService.ShowWindow<Views.SwatcherWindow>();
        }

        private void OpenOptions()
        {
            _windowService.ShowWindow<Views.OptionsWindow>();
        }

        private void EnsureBackups(BaseProfile profile)
        {
            foreach (var sdb in profile)
            {
                var orig = sdb.FullPath;
                var back = $"{orig}.bacc";
                if (!File.Exists(back)) File.Copy(orig, back, true);
            }
        }

        private void AskForGameRun(BaseProfile profile)
        {
            var result = _interactionService.ShowMessage("Do you wish to run the game?", "Prompt", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    ProcessHelpers.LaunchGame(profile.Directory, profile.GameINT);
                }
                catch (Exception e)
                {
                    _interactionService.ShowMessage(e.GetLowestMessage(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RefreshThemeResources()
        {
            _themeService.ApplyThemeResources();
            OnPropertyChanged(nameof(UserImage));
            OnPropertyChanged(nameof(ModderImage));
            OnPropertyChanged(nameof(ToolsImage));
            OnPropertyChanged(nameof(ThemeImage));
            OnPropertyChanged(nameof(UpdatesImage));
        }
    }
}
