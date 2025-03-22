using System.Collections.Generic;
using System.IO;
using System.Text;
using GenericAssets.Legacy.Textures;

namespace GenericAssets.Legacy.Atlases;

public class SpriteAtlasBinarySerializer
{
    public void Serialize(SpriteAtlasSource spriteAtlasSource, string outputPath)
    {
        File.Delete(outputPath);
        using FileStream stream = File.OpenWrite(outputPath);
        using BinaryWriter writer = new BinaryWriter(stream);

        WriteHeader(writer);

        TextureBinarySerializer.WriteTexture(writer, spriteAtlasSource.Texture!);

        WriteSprites(writer, spriteAtlasSource.Sprites!);
    }

    private static void WriteHeader(BinaryWriter writer)
    {
        writer.Write((byte)'B');
        writer.Write((byte)'C');
        writer.Write((byte)'A');
        writer.Write((byte)1);
    }

    private static void WriteSprites(BinaryWriter writer, List<SpriteSource> sprites)
    {
        writer.Write((ushort)sprites.Count);

        foreach (SpriteSource spriteSource in sprites)
        {
            writer.Write((ushort)spriteSource.Placement.Left);
            writer.Write((ushort)spriteSource.Placement.Top);
            writer.Write((ushort)spriteSource.Placement.Right);
            writer.Write((ushort)spriteSource.Placement.Bottom);
            writer.Write((ushort)spriteSource.Margin.Left);
            writer.Write((ushort)spriteSource.Margin.Top);
            writer.Write((ushort)spriteSource.Margin.Right);
            writer.Write((ushort)spriteSource.Margin.Bottom);
            writer.Write((ushort)spriteSource.Pivot.X);
            writer.Write((ushort)spriteSource.Pivot.Y);
        }

        foreach (SpriteSource spriteSource in sprites)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(spriteSource.Name!);

            writer.Write((byte)bytes.Length);
            writer.Write(bytes);
        }
    }
}