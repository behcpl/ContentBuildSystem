using System;
using System.IO;
using StbImageWriteSharp;

namespace GenericAssets.Legacy.Textures;

public class TextureDebugOutput
{
    private readonly string _tempPath;

    public TextureDebugOutput(string tempPath)
    {
        _tempPath = tempPath;
    }

    public void DebugOut(string name, TextureSource textureSource)
    {
        Directory.CreateDirectory(_tempPath);
        using FileStream outFile = File.OpenWrite(Path.Combine(_tempPath, $"{name}.png"));

        ImageWriter writer = new();
        switch (textureSource.Format)
        {
            case TextureFormat.RGBA8888:
                writer.WritePng(textureSource.Data, textureSource.Width, textureSource.Height, ColorComponents.RedGreenBlueAlpha, outFile);
                break;

            case TextureFormat.R8:
                writer.WritePng(textureSource.Data, textureSource.Width, textureSource.Height, ColorComponents.Grey, outFile);
                break;

            default:
                Console.WriteLine($"Unsupported format: {textureSource.Format.ToString()}");
                break;
        }
    }
}
