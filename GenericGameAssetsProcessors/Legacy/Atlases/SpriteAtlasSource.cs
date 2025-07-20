using System.Collections.Generic;
using GenericGameAssetsProcessors.Legacy.Textures;

namespace GenericGameAssetsProcessors.Legacy.Atlases;

public class SpriteSource
{
    public string? Name;
    public Margin Placement; //it's not the same as margin - this is absolute values not relative offset
    public Margin Margin;
    public Point2 Pivot;
}

public class SpriteAtlasSource
{
    public TextureSource? Texture;
    public List<SpriteSource>? Sprites;
}
