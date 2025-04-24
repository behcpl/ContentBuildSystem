using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ContentBuildSystem.Interfaces;

public static class DirectoryHelper
{
    public static string[] EnumerateFiles(string path, bool recursive, IReadOnlyList<string>? extensions)
    {
        string[] files = Directory.GetFiles(path, "*", new EnumerationOptions { RecurseSubdirectories = recursive, IgnoreInaccessible = true, AttributesToSkip = FileAttributes.Hidden });

        if (extensions != null && extensions.Count > 0)
        {
            List<string> filteredFiles = new(files.Length);
            foreach (string filePath in files)
            {
                string ext = Path.GetExtension(filePath).ToLowerInvariant().TrimStart('.');
                if (extensions.Contains(ext))
                    filteredFiles.Add(filePath);
            }

            files = filteredFiles.ToArray();
        }

        return files;
    }
}