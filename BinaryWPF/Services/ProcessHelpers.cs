using CoreExtensions.Management;

using Endscript.Enums;

using Nikki.Core;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BinaryWPF.Services
{
    public static class ProcessHelpers
    {
        public static void OpenBrowser(string url)
        {
            try
            {
                _ = Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    _ = Process.Start(new ProcessStartInfo("cmd.exe", $"/c start {url}") { CreateNoWindow = true });
                }
                else
                {
                    _ = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                        ? Process.Start("xdg-open", url)
                        : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                            ? Process.Start("open", url)
                            : throw new PlatformNotSupportedException();
                }
            }
        }

        public static void LaunchGame(string path, GameINT game)
        {
            var exe = path + game switch
            {
                GameINT.Carbon => "\\NFSC.EXE",
                GameINT.MostWanted => "\\SPEED.EXE",
                GameINT.Prostreet => "\\NFS.EXE",
                GameINT.Undercover => "\\NFS.EXE",
                GameINT.Underground1 => "\\SPEED.EXE",
                GameINT.Underground2 => "\\SPEED2.EXE",
                _ => throw new Exception($"Game {game} is an invalid game type")
            };

            Process.Start(new ProcessStartInfo(exe) { WorkingDirectory = path });
        }
    }
}
