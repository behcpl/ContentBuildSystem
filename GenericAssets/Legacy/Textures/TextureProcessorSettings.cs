using System;

namespace GenericAssets.Legacy.Textures;

[Serializable]
public class TextureProcessorSettings
{
    public bool PremultipliedAlpha;
    public bool LinearSpace;

    // transform
    public bool PremultiplyAlpha;
    public bool Compress;
    public bool AddFrame;
}