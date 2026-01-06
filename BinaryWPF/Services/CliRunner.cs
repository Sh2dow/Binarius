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

namespace BinaryWPF.Services
{
    public sealed class CliRunner
    {
        private BaseProfile? _profile;

        public void Run(string[] args)
        {
            _ = args[0].ToLowerInvariant() switch
            {
                "modder" => eUsage.Modder,
                "user" => eUsage.User,
                _ => throw new ArgumentException("Invalid argument: {args[0]} - \"user\" or \"modder\" expected."),
            };

            if (args.Length < 2)
            {
                throw new ArgumentException("Expected argument missing: VERSN1 path missing");
            }

            if (args.Length < 3)
            {
                throw new ArgumentException("Expected argument missing: VERSN2 path missing");
            }

            var cli = new CliRunner();
            cli.LoadProfile(args[1]);
            cli.ImportEndscript(args[2]);
            cli.Save();
        }

        public void LoadProfile(string path)
        {
            Launch.Deserialize(path, out var launch);
            launch.ThisDir = Path.GetDirectoryName(path);

            LaunchHelpers.FixLaunchDirectory(launch, path);

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

            _profile = BaseProfile.NewProfile(launch.GameID, launch.Directory);

            var watch = Stopwatch.StartNew();
            string[] exceptions = _profile.Load(launch);
            watch.Stop();

            PrintExceptions(exceptions);
            Console.WriteLine($"Completed in {watch.Elapsed.TotalSeconds} seconds");
        }

        public void ImportEndscript(string path)
        {
            if (_profile == null)
            {
                throw new InvalidOperationException("Profile is not loaded.");
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

                Console.WriteLine(error);
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
                        Console.WriteLine(infobox.Description);
                    }
                    else if (command is CheckboxCommand checkbox)
                    {
                        Console.WriteLine(checkbox.Description);
                        Console.WriteLine("Select one [yes, no]: ");
                        string? result = Console.ReadLine();

                        checkbox.Choice = GetCheckboxOptionChosen(result ?? string.Empty);
                    }
                    else if (command is ComboboxCommand combobox)
                    {
                        Console.WriteLine(combobox.Description);
                        Console.WriteLine($"Select one [{GetInlinedOptions(combobox)}]: ");
                        string? result = Console.ReadLine();

                        combobox.Choice = GetComboboxOptionChosen(combobox, result ?? string.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.GetLowestMessage());
                return;
            }

            string script = Path.GetFileName(path);

            if (manager.Errors.Any())
            {
                LaunchHelpers.WriteErrorsToLog(manager.Errors, path);
                Console.WriteLine($"Script {script} has been applied, however, {manager.Errors.Count()} errors " +
                    "have been detected. Check EndError.log for more information.");
            }
            else
            {
                Console.WriteLine($"Script {script} has been successfully applied.");
            }

            static string GetInlinedOptions(ComboboxCommand command)
            {
                string result = string.Empty;

                for (int i = 0; i < command.Options.Length - 1; ++i)
                {
                    result += command.Options[i].Name + ", ";
                }

                return result + command.Options[^1].Name;
            }

            static int GetCheckboxOptionChosen(string strOption)
            {
                return string.Compare(strOption, "YES", StringComparison.OrdinalIgnoreCase) == 0
                    ? 1
                    : string.Compare(strOption, "NO", StringComparison.OrdinalIgnoreCase) == 0
                        ? 0
                        : throw new Exception("Argument passed is invalid, terminating execution...");
            }

            static int GetComboboxOptionChosen(ComboboxCommand command, string strOption)
            {
                for (int i = 0; i < command.Options.Length; ++i)
                {
                    if (string.Compare(strOption, command.Options[i].Name, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return i;
                    }
                }

                throw new Exception("Argument passed is invalid, terminating execution...");
            }
        }

        public void Save()
        {
            if (_profile == null)
            {
                throw new InvalidOperationException("Profile is not loaded.");
            }

            Console.WriteLine("Saving... Please wait...");

            var watch = Stopwatch.StartNew();
            string[] exceptions = _profile.Save();
            watch.Stop();

            PrintExceptions(exceptions);
            Console.WriteLine($"Complete in {watch.Elapsed.TotalSeconds} seconds.");
        }

        private static void PrintExceptions(string[] exceptions)
        {
            string print = string.Empty;

            foreach (string exception in exceptions)
            {
                print += $"Exception: {exception}\n";
            }

            Console.WriteLine(print);
        }
    }
}
