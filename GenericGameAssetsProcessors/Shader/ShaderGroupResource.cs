using System;
using System.Collections.Generic;

namespace GenericGameAssetsProcessors.Shader;

[Serializable]
public class ProgramItem
{
    public string? FragmentShader;
    public string? VertexShader;
    public string[]? Tags;
}

[Serializable]
public class ShaderGroupResource : Dictionary<string, ProgramItem> { }
