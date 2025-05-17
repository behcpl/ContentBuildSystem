using System;
using System.Collections.Generic;

namespace ContentBuildSystem.Project;

[Serializable]
public class RulesetDescription
{
    public string? Path;
    public RulesetConfiguration? Configuration;
}

[Serializable]
public class RulesetConfiguration
{
    public Dictionary<string, string[]?>? Whitelist;
    public Dictionary<string, string[]?>? Blacklist;
}
