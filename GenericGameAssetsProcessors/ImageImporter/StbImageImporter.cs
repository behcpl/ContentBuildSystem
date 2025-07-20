using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using ContentBuildSystem.Interfaces;
using StbImageSharp;

namespace GenericGameAssetsProcessors.ImageImporter;

public class StbImageImporter : IImporter<ImageData>
{
    public static string[] SupportedExtensions = [ "jpeg", "jpg", "png", "tga", "bmp", "psd", "gif", "hdr", "pic", "pnm", "ppm", "pgm" ];

    public bool TryImport(string path, [MaybeNullWhen(false)] out ImageData result, IReport? report)
    {
        try
        {
            using FileStream stream = File.OpenRead(path);
            ImageResult image = ImageResult.FromStream(stream);

            result = new ImageData { Width = image.Width, Height = image.Height, Pixels = new Color[image.Width * image.Height] };

            switch (image.Comp)
            {
                case ColorComponents.Grey:
                    for (int i = 0; i < image.Width * image.Height; i++)
                    {
                        byte v = image.Data[i];
                        result.Pixels[i] = Color.FromBytes(v, v, v, 255);
                    }

                    break;

                case ColorComponents.GreyAlpha:
                    for (int i = 0; i < image.Width * image.Height; i++)
                    {
                        byte v = image.Data[2 * i];
                        byte a = image.Data[2 * i + 1];
                        result.Pixels[i] = Color.FromBytes(v, v, v, a);
                    }

                    break;

                case ColorComponents.RedGreenBlue:
                    for (int i = 0; i < image.Width * image.Height; i++)
                    {
                        byte r = image.Data[3 * i];
                        byte g = image.Data[3 * i + 1];
                        byte b = image.Data[3 * i + 2];
                        result.Pixels[i] = Color.FromBytes(r, g, b, 255);
                    }

                    break;

                case ColorComponents.RedGreenBlueAlpha:
                    for (int i = 0; i < image.Width * image.Height; i++)
                    {
                        byte r = image.Data[4 * i];
                        byte g = image.Data[4 * i + 1];
                        byte b = image.Data[4 * i + 2];
                        byte a = image.Data[4 * i + 3];
                        result.Pixels[i] = Color.FromBytes(r, g, b, a);
                    }

                    break;

                case ColorComponents.Default:
                default:
                    throw new Exception($"Unknown ColorComponents value: {image.Comp}");
            }

            return true;
        }
        catch (Exception e)
        {
            result = null;
            report?.Exception(e);
            return false;
        }
    }
}
