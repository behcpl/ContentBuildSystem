using System;

namespace ContentBuildSystem;

[Serializable]
public class BuildItemManifest
{
    public string[]? Output;
    public string[]? Dependencies;
    public FolderDependency[]? FolderDependencies;
}

[Serializable]
public class FolderDependency
{
    public bool Recursive;
    public string? Path;
    public string[]? Extensions;
    public string[]? Dependencies;
}