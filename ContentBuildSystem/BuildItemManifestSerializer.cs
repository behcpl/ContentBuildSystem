using System.Collections.Generic;
using System.IO;

namespace ContentBuildSystem;

public class BuildItemManifestSerializer
{
    public bool Serialize(string path, BuildItemManifest itemManifest)
    {
        using FileStream file = File.OpenWrite(path);
        using TextWriter writer = new StreamWriter(file);

        if (itemManifest.Output != null)
        {
            foreach (string outPath in itemManifest.Output)
            {
                writer.WriteLine($"OUT {outPath}");
            }
        }

        if (itemManifest.Dependencies != null)
        {
            foreach (string inPath in itemManifest.Dependencies)
            {
                writer.WriteLine($"IN {inPath}");
            }
        }

        return true;
    }

    public bool Deserialize(string path, out BuildItemManifest itemManifest)
    {
        using FileStream file = File.OpenRead(path);
        using TextReader reader = new StreamReader(file);

        List<string> outPaths = new List<string>();
        List<string> inPaths = new List<string>();

        while (true)
        {
            string? line = reader.ReadLine();
            if (line == null)
                break;

            if (line.StartsWith("OUT "))
                outPaths.Add(line.Substring(4));
            if (line.StartsWith("IN "))
                inPaths.Add(line.Substring(3));
        }


        itemManifest = new BuildItemManifest { Output = outPaths.ToArray(), Dependencies = inPaths.ToArray() };
        return true;
    }
}