using CoreExtensions.IO;

using Endscript.Core;
using Endscript.Helpers;

using System;
using System.Collections.Generic;
using System.IO;

namespace BinaryWPF.Services
{
    public static class LaunchHelpers
    {
        public static void FixLaunchDirectory(Launch launch, string filename)
        {
            string directory = Path.GetDirectoryName(filename) ?? string.Empty;

            try
            {
                if (Path.IsPathRooted(launch.Directory))
                {
                    return;
                }

                string maybePath = Path.GetFullPath(Path.Combine(directory, launch.Directory));

                if (Directory.Exists(maybePath))
                {
                    launch.Directory = maybePath;
                }
            }
            catch
            {
            }
        }

        public static void WriteErrorsToLog(IEnumerable<EndError> errors, string filename)
        {
            using var logger = new Logger("EndError.log", $"Endscript : {filename}", true);

            foreach (var error in errors)
            {
                logger.WriteLine($"File: {error.Filename}, Line: {error.Index}");
                logger.WriteLine($"Command: [{error.Line}]");
                logger.WriteLine($"Error: {error.Error}");
                logger.WriteLine(string.Empty);
            }
        }
    }
}
