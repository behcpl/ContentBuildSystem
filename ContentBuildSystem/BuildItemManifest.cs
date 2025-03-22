using System;

namespace ContentBuildSystem;

[Serializable]
public class BuildItemManifest
{
    public string[]? Output;
    public string[]? Dependencies;
}