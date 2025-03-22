using System;

namespace ContentBuildSystem.Project;

[Serializable]
public class GroupDescription
{
    public string? Description;
    public string? Path;
    public bool Recursive;
    public string[]? Ruleset;
    // TODO: flatten settings here?
}