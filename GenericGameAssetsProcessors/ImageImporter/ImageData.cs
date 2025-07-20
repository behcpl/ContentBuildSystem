using System;

namespace GenericGameAssetsProcessors.ImageImporter;

public class ImageData
{
    public int Width;
    public int Height;
    public Color[] Pixels = [ ]; // Width * Height * (RGBA)
}

public static class ImageDataExtensions
{
    // ReSharper disable once InconsistentNaming
    public static byte[] ConvertToRGBA8888(this ImageData image)
    {
        byte[] rgba = new byte[image.Width * image.Height * 4];
        for (int i = 0; i < image.Width * image.Height; i++)
        {
            (byte r, byte g, byte b, byte a) = image.Pixels[i].ToBytes();
            rgba[4 * i + 0] = r;
            rgba[4 * i + 1] = g;
            rgba[4 * i + 2] = b;
            rgba[4 * i + 3] = a;
        }

        return rgba;
    }

    public static void Clear(ImageData image, Color color)
    {
        Array.Fill(image.Pixels, color);
    }

    public static void FillRect(ImageData image, Color color, Rect rect)
    {
        int count = rect.Width;
        int index = rect.Left;
        for (int y = rect.Top; y < rect.Bottom; y++)
        {
            Array.Fill(image.Pixels, color, index, count);
            index += image.Width;
        }
    }

    // TODO: blitting/copying
    public static void Blit(ImageData dest, ImageData src, int posX, int posY)
    {
        
    }

    // TODO: color conversion
    // TODO: ?
}
