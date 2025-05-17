using System.Collections.Generic;
using System.IO;
using GenericAssets.Legacy.Textures;

namespace GenericAssets.Legacy.Fonts;

public class FontBinarySerializer
{
    public void Serialize(FontSource fontSource, string outputPath)
    {
        File.Delete(outputPath);
        using FileStream stream = File.OpenWrite(outputPath);
        using BinaryWriter writer = new(stream);

        WriteHeader(writer);

        TextureBinarySerializer.WriteTexture(writer, fontSource.Texture!);

        WriteMetrics(writer, fontSource);
        WriteGlyphs(writer, fontSource.Glyphs!);
        WriteKerningTable(writer, fontSource.KerningTable!);
    }

    private static void WriteHeader(BinaryWriter writer)
    {
        writer.Write((byte)'B');
        writer.Write((byte)'C');
        writer.Write((byte)'F');
        writer.Write((byte)1);
    }

    private static void WriteMetrics(BinaryWriter writer, FontSource fontSource)
    {
        writer.Write((ushort)fontSource.LineHeight);
        writer.Write((ushort)fontSource.BaseOffset);
        writer.Write((ushort)fontSource.SpaceWidth);
        writer.Write((ushort)fontSource.EmWidth);
    }

    private static void WriteGlyphs(BinaryWriter writer, List<FontGlyphSource> glyphs)
    {
        writer.Write((uint)glyphs.Count);

        foreach (FontGlyphSource glyph in glyphs)
        {
            writer.Write((uint)glyph.Id);
            writer.Write((ushort)glyph.X);
            writer.Write((ushort)glyph.Y);
            writer.Write((ushort)glyph.Width);
            writer.Write((ushort)glyph.Height);
            writer.Write((short)glyph.XOffset);
            writer.Write((short)glyph.YOffset);
            writer.Write((short)glyph.XAdvance);
        }
    }

    private static void WriteKerningTable(BinaryWriter writer, List<FontKerningSource> kerningTable)
    {
        writer.Write((uint)kerningTable.Count);

        foreach (FontKerningSource kerning in kerningTable)
        {
            writer.Write((uint)kerning.Id);
            writer.Write((uint)kerning.IdNext);
            writer.Write(kerning.XAdjust);
        }
    }
}
