using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using GenericGameAssetsProcessors.Legacy.Textures;
using StbRectPackSharp;
using StbTrueTypeSharp;

// using YamlDotNet.Serialization;
// using YamlDotNet.Serialization.NamingConventions;

namespace GenericGameAssetsProcessors.Legacy.Fonts;

public class SdfFontGlyph
{
    public int Id;
    public int Width;
    public int Height;
    public int XAdvance;
    public int XOffset;
    public int YOffset;
    public byte[]? Data;
}

public class SdfFontGlyphComparer : IComparer<SdfFontGlyph>
{
    public int Compare(SdfFontGlyph? x, SdfFontGlyph? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (ReferenceEquals(null, y)) return 1;
        if (ReferenceEquals(null, x)) return -1;

        int ret = y.Height.CompareTo(x.Height);
        if (ret != 0)
            return ret;

        return y.Width.CompareTo(x.Width);
    }
}

public class SdfFontGenerator
{
    public FontSource Import(string path)
    {
        string dir = Path.GetDirectoryName(path)!;
        string name = Path.GetFileNameWithoutExtension(path);
        string metaPath = Path.Combine(dir, $"{name}.meta");

        // TODO: use fntgen file
        VectorFontMeta meta = new() { FontSize = 32, Characters = "QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnm1234567890,./;'[]\\`-=~!@#$%^&*()_+{}|:\"<>?" };

        byte[] fontBytes = File.ReadAllBytes(path);
        FontSource font = new();

        Build(font, meta, fontBytes);

        return font;
    }

    private unsafe void Build(FontSource font, VectorFontMeta meta, byte[] fontBytes)
    {
        StbTrueType.stbtt_fontinfo fontInfo = new();
        fixed (byte* ttfPtr = fontBytes)
        {
            if (StbTrueType.stbtt_InitFont(fontInfo, ttfPtr, 0) == 0)
            {
                throw new Exception("Failed to init font.");
            }
        }

        float scaleFactor = StbTrueType.stbtt_ScaleForPixelHeight(fontInfo, meta.FontSize);

        int ascent, descent, lineGap;
        StbTrueType.stbtt_GetFontVMetrics(fontInfo, &ascent, &descent, &lineGap);

        font.LineHeight = (int)Math.Round((ascent - descent) * scaleFactor);
        font.BaseOffset = (int)Math.Round(ascent * scaleFactor);

        font.EmWidth = font.BaseOffset;

        int emGlyph = StbTrueType.stbtt_FindGlyphIndex(fontInfo, 8195);
        if (emGlyph > 0)
        {
            int advance = 0;
            int bearing = 0;
            StbTrueType.stbtt_GetGlyphHMetrics(fontInfo, emGlyph, &advance, &bearing);
            font.EmWidth = (int)Math.Round(advance * scaleFactor);
        }

        font.SpaceWidth = font.EmWidth / 4;

        int spaceGlyph = StbTrueType.stbtt_FindGlyphIndex(fontInfo, 32);
        if (spaceGlyph > 0)
        {
            int advance = 0;
            int bearing = 0;
            StbTrueType.stbtt_GetGlyphHMetrics(fontInfo, spaceGlyph, &advance, &bearing);

            font.SpaceWidth = (int)Math.Round(advance * scaleFactor);
        }

        int defaultPadding = meta.FontSize / 4;
        int totalArea = 0;
        List<SdfFontGlyph> glyphs = new();

        foreach (char cp in meta.Characters!)
        {
            SdfFontGlyph? glyph = RenderSdfGlyph(fontInfo, scaleFactor, cp, meta.SdfRange ?? defaultPadding);
            if (glyph == null)
            {
                Console.WriteLine($"NOT FOUND {(int)cp} ({cp})");
                continue;
            }

            glyphs.Add(glyph);
            totalArea += (glyph.Width + 1) * (glyph.Height + 1); //with padding
        }

        int spacing = 1;
        int widthPow2 = Math.Max(64, (int)Math.Sqrt(totalArea));
        widthPow2 |= widthPow2 >> 1;
        widthPow2 |= widthPow2 >> 2;
        widthPow2 |= widthPow2 >> 4;
        widthPow2 |= widthPow2 >> 8;
        widthPow2 |= widthPow2 >> 16;
        widthPow2++;

        int heightPow2 = widthPow2 * 4;
        Packer packer = new(widthPow2 - spacing, heightPow2 - spacing);

        glyphs.Sort(new SdfFontGlyphComparer());

        foreach (SdfFontGlyph fontGlyph in glyphs)
        {
            packer.PackRect(fontGlyph.Width + spacing, fontGlyph.Height + spacing, fontGlyph);
        }

        int usedHeight = 0;
        foreach (PackerRectangle packRectangle in packer.PackRectangles)
        {
            usedHeight = Math.Max(usedHeight, packRectangle.Y + packRectangle.Height + spacing);
        }

        usedHeight = (usedHeight + 3) & ~0x3;

        byte[] data = new byte[widthPow2 * usedHeight];

        font.Glyphs = new List<FontGlyphSource>();
        font.KerningTable = new List<FontKerningSource>();

        foreach (PackerRectangle packRectangle in packer.PackRectangles)
        {
            SdfFontGlyph fontGlyph = (SdfFontGlyph)packRectangle.Data;

            font.Glyphs.Add(
                new FontGlyphSource
                {
                    Id = fontGlyph.Id,
                    Width = fontGlyph.Width,
                    Height = fontGlyph.Height,
                    XOffset = fontGlyph.XOffset,
                    YOffset = fontGlyph.YOffset + font.BaseOffset,
                    XAdvance = fontGlyph.XAdvance,
                    X = packRectangle.X + spacing,
                    Y = packRectangle.Y + spacing,
                });

            for (int y = 0; y < fontGlyph.Height; y++)
            {
                Array.Copy(fontGlyph.Data!, y * fontGlyph.Width, data, (y + packRectangle.Y + spacing) * widthPow2 + packRectangle.X + spacing, fontGlyph.Width);
            }
        }

        packer.Dispose();

        font.Texture = new TextureSource
        {
            Width = widthPow2,
            Height = usedHeight,
            AlphaMode = AlphaMode.NONE,
            Data = data,
            Format = TextureFormat.R8,
            IsLinear = true,
        };
    }

    private unsafe SdfFontGlyph? RenderSdfGlyph(StbTrueType.stbtt_fontinfo fontInfo, float scaleFactor, int cp, int paddingPixels)
    {
        int glyphId = StbTrueType.stbtt_FindGlyphIndex(fontInfo, cp);
        if (glyphId == 0)
            return null;

        int advance = 0;
        int bearing = 0;
        StbTrueType.stbtt_GetGlyphHMetrics(fontInfo, glyphId, &advance, &bearing);

        int w, h, xoff, yoff;
        byte* sdf = StbTrueType.stbtt_GetGlyphSDF(fontInfo, scaleFactor, glyphId, paddingPixels, 128, 128.0f / paddingPixels, &w, &h, &xoff, &yoff);

        SdfFontGlyph glyph = new()
        {
            Id = cp,
            Width = w,
            Height = h,
            Data = new byte[w * h],
            XOffset = xoff,
            YOffset = yoff,
            XAdvance = (int)Math.Round(advance * scaleFactor),
        };

        fixed (byte* ptr = &glyph.Data[0])
        {
            Unsafe.CopyBlock(ptr, sdf, (uint)(w * h));
        }

        StbTrueType.stbtt_FreeSDF(sdf, null);

        return glyph;
    }
}
