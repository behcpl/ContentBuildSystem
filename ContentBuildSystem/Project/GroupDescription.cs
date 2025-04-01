using System;

namespace ContentBuildSystem.Project;

[Serializable]
public class GroupDescription
{
    public string? Description;
    public string? Path;
    public bool Recursive;
    public RulesetDescription[]? Ruleset;
    // TODO: flatten settings here?
}