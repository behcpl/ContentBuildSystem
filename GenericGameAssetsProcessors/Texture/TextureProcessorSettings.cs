using System;

namespace GenericGameAssetsProcessors.Texture;

[Serializable]
public class TextureProcessorSettings
{
    public static readonly TextureProcessorSettings Default = new()
    {
        Transformations = null,
    };    
    
    public object[]? Transformations;
    public object Conversion;
}
