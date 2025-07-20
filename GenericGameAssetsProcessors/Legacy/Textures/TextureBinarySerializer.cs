using System.IO;

namespace GenericGameAssetsProcessors.Legacy.Textures;

public class TextureBinarySerializer
{
    public void Serialize(TextureSource textureSource, string outputPath)
    {
        File.Delete(outputPath);
        using FileStream stream = File.OpenWrite(outputPath);
        using BinaryWriter writer = new(stream);

        WriteHeader(writer);
        WriteTexture(writer, textureSource);
    }

    private static void WriteHeader(BinaryWriter writer)
    {
        writer.Write((byte)'B');
        writer.Write((byte)'C');
        writer.Write((byte)'T');
        writer.Write((byte)1);
    }

    public static void WriteTexture(BinaryWriter writer, TextureSource textureSource)
    {
        byte flags = 0;
        if (!textureSource.IsLinear)
            flags |= 0x01; //GAMMA
        if (textureSource.AlphaMode == AlphaMode.PREMULTIPLIED)
            flags |= 0x02; //ALPHA PREMUL

        writer.Write((ushort)textureSource.Width);
        writer.Write((ushort)textureSource.Height);
        writer.Write((byte)textureSource.Format);
        writer.Write(flags);
        writer.Write((byte)0); // padding
        writer.Write((byte)0); // padding
        writer.Write(textureSource.Data!.Length);
        writer.Write(textureSource.Data!);
    }
}
