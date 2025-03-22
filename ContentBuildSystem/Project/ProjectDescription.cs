using System;
using System.Collections.Generic;

namespace ContentBuildSystem.Project;

[Serializable]
public class ProjectDescription
{
    public List<PluginDescription>? Plugins;
    public List<GroupDescription>? ItemGroups;
}