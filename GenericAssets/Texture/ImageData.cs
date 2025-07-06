using System;
using GenericAssets.Legacy;

namespace GenericAssets.Texture;


public class ImageData
{
    public int Width;
    public int Height;
    public Color[] Pixels; // Width * Height * (RGBA)
}

public static class ImageDataExtensions
{ 
    public static void Clear(ImageData image, Color color)
    {
        Array.Fill(image.Pixels, color);
    }

    public static void FillRect()
    {
        
    }
    
    // TODO: blitting/copying
    public static void Blit()
    {
        
    }
    
    // TODO: color conversion
    // TODO: normal operators
}
