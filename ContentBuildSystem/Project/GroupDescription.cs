using System;

namespace ContentBuildSystem.Project;

[Serializable]
public class GroupDescription
{
    public string? Description;
    public string? Path;
    public string? OutputPath;
    public bool Recursive;
    public bool Flatten;
    public RulesetDescription[]? Ruleset;

    public bool IncludeEmptyFileNames;   // i.e. .gitignore, etc
    public bool IncludeEmptyFolderNames; // i.e. .git, etc
    public bool IncludeHiddenAttribute;
}
