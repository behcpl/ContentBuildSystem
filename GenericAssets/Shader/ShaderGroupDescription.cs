using System;
using System.Collections.Generic;

namespace GenericAssets.Shader;

[Serializable]
public class ShaderDesc
{
    public string? Source;
    public Dictionary<string, int?>? Defines;
}

[Serializable]
public class ProgramDesc
{
    public ShaderDesc? FragmentShader;
    public ShaderDesc? VertexShader;
    public string[]? Tags;
}

[Serializable]
public class ShaderGroupDescription
{
    public Dictionary<string, ProgramDesc?>? Programs;
}
