using System;
using System.Collections.Generic;

namespace ContentBuildSystem.Project;

[Serializable]
public class PluginDescription
{
    public string? Path;
    public string? Namespace;
    public Dictionary<string, object>? Options;
}