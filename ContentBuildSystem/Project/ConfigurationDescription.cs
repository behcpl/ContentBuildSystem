using System;
using System.Collections.Generic;

namespace ContentBuildSystem.Project;

[Serializable]
public class ConfigurationDescription
{
    public string? Default;
    public string? SplitChar;
    public string? Format; // i.e. "platform-build-lang"
    public Dictionary<string, string[]?>? Components; // name from Format used as key for list of available options
}