using System;
using System.Collections.Generic;
using GenericAssets.Legacy.Textures;

namespace GenericAssets.Legacy.Atlases;

[Serializable]
public class SpriteAtlasMeta
{
    public List<string>? Files;
    public List<string>? Folders;

    public Dictionary<string, SpriteExtras>? SpriteExtras;

    public int Spacing;
    public bool Padding;
    public bool Compress;
    public bool PremultiplyAlpha;
}

[Serializable]
public class SpriteExtras
{
    public Margin Margin;
    public Point2 Pivot;
}
