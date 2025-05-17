using System.IO;
using System.Text.Json;
using ContentBuildSystem.Interfaces;
using ContentBuildSystem.Project;

namespace ContentBuildSystem.Json;

public class ProjectSerializer
{
    private static readonly JsonSerializerOptions _options = new()
    {
        AllowTrailingCommas = true,
        IncludeFields = true,
    };

    public ProjectDescription? Deserialize(string path, IReport? report)
    {
        Stream fs = File.OpenRead(path);
        ProjectDescription? projectDescription = JsonSerializer.Deserialize<ProjectDescription>(fs, _options);
        if (projectDescription == null)
            return null;

        if (projectDescription.Plugins != null)
        {
            foreach (PluginDescription pluginDescription in projectDescription.Plugins)
            {
                pluginDescription.Options = OptionsConverter.Map(pluginDescription.Options);
            }
        }

        return projectDescription;
    }
}
