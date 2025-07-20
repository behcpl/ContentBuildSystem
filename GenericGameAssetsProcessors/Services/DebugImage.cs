using System;
using System.IO;
using ContentBuildSystem.Interfaces;
using GenericGameAssetsProcessors.ImageImporter;
using StbImageWriteSharp;

namespace GenericGameAssetsProcessors.Services;

internal class DebugImage : IDebugImage
{
    public void SaveAsPng(string filepath, ImageData image, IReport? report)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filepath)!);

            using FileStream outFile = File.OpenWrite(filepath);
            ImageWriter writer = new();
            writer.WritePng(image.ConvertToRGBA8888(), image.Width, image.Height, ColorComponents.RedGreenBlueAlpha, outFile);
        }
        catch (Exception e)
        {
            report?.Warning($"DebugImage.SaveAsPng exception: {e.Message}");
        }
    }
}
