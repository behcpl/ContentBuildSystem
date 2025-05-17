using System.Collections.Generic;
using GenericAssets.Legacy.Textures;

namespace GenericAssets.Legacy.Fonts;

public class FontGlyphSource
{
    public int Id;
    public int X, Y, Width, Height;
    public int XOffset, YOffset, XAdvance;
}

public class FontKerningSource
{
    public int Id;
    public int IdNext;
    public int XAdjust;
}

public class FontSource
{
    public TextureSource? Texture;
    public List<FontGlyphSource>? Glyphs;
    public List<FontKerningSource>? KerningTable;

    public int LineHeight;
    public int BaseOffset;
    public int SpaceWidth;
    public int EmWidth;
}
