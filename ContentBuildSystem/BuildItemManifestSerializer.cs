using System;
using System.Collections.Generic;
using System.IO;

namespace ContentBuildSystem;

public class BuildItemManifestSerializer
{
    public bool Serialize(string path, string projectPath, BuildItemManifest itemManifest)
    {
        using FileStream file = File.OpenWrite(path);
        using TextWriter writer = new StreamWriter(file);

        if (itemManifest.Output != null)
        {
            foreach (string outPath in itemManifest.Output)
            {
                writer.WriteLine($"OUT {Path.GetRelativePath(projectPath, outPath)}");
            }
        }

        if (itemManifest.Dependencies != null)
        {
            foreach (string inPath in itemManifest.Dependencies)
            {
                writer.WriteLine($"IN {Path.GetRelativePath(projectPath, inPath)}");
            }
        }

        if (itemManifest.FolderDependencies != null)
        {
            foreach (FolderDependency inFolder in itemManifest.FolderDependencies)
            {
                string type = inFolder.Recursive ? "INDR" : "IND";

                if (inFolder.Extensions != null && inFolder.Extensions.Length > 0)
                {
                    writer.WriteLine($"{type} {Path.GetRelativePath(projectPath, inFolder.Path!)}|{string.Join('|', inFolder.Extensions)}");
                }
                else
                {
                    writer.WriteLine($"{type} {Path.GetRelativePath(projectPath, inFolder.Path!)}");
                }

                if (inFolder.Dependencies != null)
                {
                    foreach (string filePath in inFolder.Dependencies)
                    {
                        writer.WriteLine($"IN {Path.GetRelativePath(projectPath, filePath)}");
                    }
                }
            }
        }

        return true;
    }

    public bool Deserialize(string path, string projectPath, out BuildItemManifest itemManifest)
    {
        using FileStream file = File.OpenRead(path);
        using TextReader reader = new StreamReader(file);

        List<string> outPaths = new();
        List<string> inPaths = new();

        List<string> folderFiles = new();
        FolderDependency? currentFolder = null;

        List<FolderDependency> folders = new();

        while (true)
        {
            string? line = reader.ReadLine();
            if (line == null)
                break;

            if (line.StartsWith("OUT "))
            {
                outPaths.Add(Path.GetFullPath(line.Substring(4), projectPath));
                continue;
            }

            if (line.StartsWith("IN "))
            {
                if (currentFolder != null)
                {
                    folderFiles.Add(Path.GetFullPath(line.Substring(3), projectPath));
                }
                else
                {
                    inPaths.Add(Path.GetFullPath(line.Substring(3), projectPath));
                }
            }

            if (line.StartsWith("IND ") || line.StartsWith("INDR "))
            {
                if (currentFolder != null)
                {
                    currentFolder.Dependencies = folderFiles.ToArray();
                    folderFiles.Clear();
                }

                bool recursive = line.StartsWith("INDR ");
                string[] elements = line.Substring(recursive ? 5 : 4).Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                currentFolder = new FolderDependency
                {
                    Recursive = recursive,
                    Path = Path.GetFullPath(elements[0], projectPath),
                };
                folders.Add(currentFolder);

                if (elements.Length > 1)
                    currentFolder.Extensions = elements[1..];
            }
        }

        if (currentFolder != null)
        {
            currentFolder.Dependencies = folderFiles.ToArray();
            folderFiles.Clear();
        }

        itemManifest = new BuildItemManifest { Output = outPaths.ToArray(), Dependencies = inPaths.ToArray(), FolderDependencies = folders.ToArray() };
        return true;
    }
}
