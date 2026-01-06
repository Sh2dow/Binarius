using CoreExtensions.Management;

using Endscript.Core;
using Endscript.Enums;
using Endscript.Profiles;

using Nikki.Core;
using Nikki.Reflection.Abstract;
using Nikki.Reflection.Interface;

using System;
using System.Collections.Generic;
using System.IO;

namespace BinaryWPF.Services
{
    public static class EditorHelpers
    {
        public static IReflective? GetReflective(string path, string separator, BaseProfile profile)
        {
            string[] splits = path.Split(separator);

            if (splits.Length is not 3 and not 5)
            {
                return null;
            }

            var db = profile.Find(splits[0]);
            if (db == null)
            {
                return null;
            }

            var manager = db.Database.GetManager(splits[1]);
            if (manager == null)
            {
                return null;
            }

            var collection = manager[manager.IndexOf(splits[2])] as Collectable;

            return splits.Length == 3
                ? collection
                : (IReflective?)collection?.GetSubPart(splits[4], splits[3]);
        }

        public static string GetStatusString(int loadedFiles, long millisecondsToLoad, string path, string addon)
        {
            return $"Files: {loadedFiles} | {addon} Time: {millisecondsToLoad}ms | Real Time: {DateTime.Now:HH:mm:ss} | Script: {GetTruncatedPath(path)}";
        }

        public static string GetSeparatedPath(string fullPath, char separator)
        {
            const string space = " ";
            string line = string.Empty;
            string[] splits = fullPath.Split(separator);

            for (int i = 0; i < splits.Length - 1; ++i)
            {
                string split = splits[i];
                line += split.Contains(' ') ? $"\"{split}\"" + space : split + space;
            }

            string last = splits[^1];
            line += last.Contains(' ') ? $"\"{last}\"" : last;
            return line;
        }

        public static string GetTruncatedPath(string path)
        {
            string[] splits = path.Split(new[] { '/', '\\' });
            return splits.Length > 3 ? Path.Combine("..", splits[^3], splits[^2], splits[^1]) : path;
        }
    }
}
