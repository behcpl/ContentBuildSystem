using System.Collections.Generic;

namespace GenericGameAssetsProcessors.Shader;

public class ShaderGroupProcessorSettings
{
    public static readonly ShaderGroupProcessorSettings Default = new()
    {
        GlobalDefines = null,
    };

    public Dictionary<string, int?>? GlobalDefines;
}
