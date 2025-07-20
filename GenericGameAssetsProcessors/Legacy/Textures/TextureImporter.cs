using System;
using System.IO;
using ContentBuildSystem.Interfaces;
using GenericGameAssetsProcessors.ImageImporter;
using StbDxtSharp;
using StbImageSharp;

namespace GenericGameAssetsProcessors.Legacy.Textures;

public class TextureImporter
{
    private readonly bool _canCompress;

    public TextureImporter(bool canCompress = true)
    {
        _canCompress = canCompress;
    }

    public TextureSource Import(string path, TextureProcessorSettings settings, IReport? report)
    {
        using FileStream stream = File.OpenRead(path);
        ImageResult image = ImageResult.FromStream(stream);

        bool hasAlpha = false;
        TextureFormat format = TextureFormat.UNKNOWN;
        byte[]? data = null;
        switch (image.SourceComp)
        {
            case ColorComponents.Grey: // R8
                format = TextureFormat.R8;
                data = image.Data;
                break;

            case ColorComponents.GreyAlpha: // not directly supported, expand to RGBA8888
                format = TextureFormat.RGBA8888;
                data = ExpandGrayAlpha(image.Data);
                hasAlpha = HasAlpha(data);
                break;

            case ColorComponents.RedGreenBlue: // not directly supported, expand to RGBX8888
                format = TextureFormat.RGBA8888;
                data = ExpandRgb(image.Data);
                break;

            case ColorComponents.RedGreenBlueAlpha: // RGBA8888
                format = TextureFormat.RGBA8888;
                data = image.Data;
                hasAlpha = HasAlpha(data);
                break;
        }

        if (hasAlpha && format == TextureFormat.RGBA8888 && settings.PremultiplyAlpha && !settings.PremultipliedAlpha)
        {
            if (settings.LinearSpace)
                PremulAlphaLinear(data!);
            else
                PremulAlphaGamma(data!);
        }

        Margin offset = new();
        int width = image.Width;
        int height = image.Height;
        if (format == TextureFormat.RGBA8888 && settings.AddFrame)
        {
            data = AddFrameMirror(data!, image.Width, image.Height);
            width += 2;
            height += 2;
            offset = Margin.MakeUniform(1);
        }

        if (settings.Compress && (image.Width % 4 != 0 || image.Height % 4 != 0))
        {
            report?.Warning($"Cannot compress, dimensions ({image.Width}x{image.Height}) are not divisible by 4!");
            settings.Compress = false;
        }

        if (_canCompress && settings.Compress && format == TextureFormat.RGBA8888)
        {
            if (hasAlpha)
            {
                format = TextureFormat.DXT5;
                data = StbDxt.CompressDxt5(width, height, data!, CompressionMode.Dithered | CompressionMode.HighQuality)!;
            }
            else
            {
                format = TextureFormat.DXT1;
                data = StbDxt.CompressDxt1(width, height, data!, CompressionMode.Dithered | CompressionMode.HighQuality)!;
            }
        }

        return new TextureSource
        {
            Width = width,
            Height = height,
            Format = format,
            Data = data,
            AlphaMode = hasAlpha ? AlphaMode.NONE : settings.PremultiplyAlpha || settings.PremultipliedAlpha ? AlphaMode.PREMULTIPLIED : AlphaMode.STRAIGHT,
            IsLinear = settings.LinearSpace,
            // Margin = settings.Margin,
            // Pivot = settings.Pivot,
            SourceOffset = offset,
        };
    }

    private bool HasAlpha(byte[] data)
    {
        for (int i = 0; i < data.Length; i += 4)
        {
            if (data[i + 3] != 255)
                return true;
        }

        return false;
    }

    private byte[] ExpandGrayAlpha(byte[] source)
    {
        byte[] dest = new byte[source.Length * 2];

        int d = 0;
        for (int i = 0; i < source.Length; i += 2)
        {
            dest[d] = source[i];
            dest[d + 1] = source[i];
            dest[d + 2] = source[i];
            dest[d + 3] = source[i + 1];

            d += 4;
        }

        return dest;
    }

    private byte[] ExpandRgb(byte[] source)
    {
        byte[] dest = new byte[source.Length * 4 / 3];

        int d = 0;
        for (int i = 0; i < source.Length; i += 3)
        {
            dest[d] = source[i];
            dest[d + 1] = source[i + 1];
            dest[d + 2] = source[i + 2];
            dest[d + 3] = 0xFF;

            d += 4;
        }

        return dest;
    }

    private void PremulAlphaLinear(byte[] data)
    {
        for (int c = 0; c < data.Length; c += 4)
        {
            Color tmp = Color.FromBytes(data[c], data[c + 1], data[c + 2], data[c + 3]).ToAlphaPremultiplied();
            (data[c], data[c + 1], data[c + 2], data[c + 3]) = tmp.ToBytes();
        }
    }

    private void PremulAlphaGamma(byte[] data)
    {
        for (int c = 0; c < data.Length; c += 4)
        {
            Color tmp = Color.FromBytes(data[c], data[c + 1], data[c + 2], data[c + 3]).ToLinearSpace().ToAlphaPremultiplied();
            (data[c], data[c + 1], data[c + 2], data[c + 3]) = tmp.ToGammaSpace().ToBytes();
        }
    }

    private byte[] AddFrameMirror(byte[] src, int width, int height)
    {
        int newWidth = width + 2;
        int newHeight = height + 2;

        byte[] dest = new byte[newWidth * newHeight * 4];

        for (int y = 0; y < height; y++)
        {
            Array.Copy(src, y * width * 4, dest, ((y + 1) * newWidth + 1) * 4, width * 4);

            dest[(y + 1) * newWidth * 4 + 0] = src[y * width * 4 + 0];
            dest[(y + 1) * newWidth * 4 + 1] = src[y * width * 4 + 1];
            dest[(y + 1) * newWidth * 4 + 2] = src[y * width * 4 + 2];
            dest[(y + 1) * newWidth * 4 + 3] = src[y * width * 4 + 3];

            dest[((y + 1) * newWidth + width + 1) * 4 + 0] = src[(y * width + width - 1) * 4 + 0];
            dest[((y + 1) * newWidth + width + 1) * 4 + 1] = src[(y * width + width - 1) * 4 + 1];
            dest[((y + 1) * newWidth + width + 1) * 4 + 2] = src[(y * width + width - 1) * 4 + 2];
            dest[((y + 1) * newWidth + width + 1) * 4 + 3] = src[(y * width + width - 1) * 4 + 3];
        }

        Array.Copy(src, 0, dest, 4, width * 4);
        Array.Copy(src, (height - 1) * width * 4, dest, ((height + 1) * newWidth + 1) * 4, width * 4);

        dest[0] = src[0];
        dest[1] = src[1];
        dest[2] = src[2];
        dest[3] = src[3];

        dest[(newWidth - 1) * 4 + 0] = src[(width - 1) * 4 + 0];
        dest[(newWidth - 1) * 4 + 1] = src[(width - 1) * 4 + 1];
        dest[(newWidth - 1) * 4 + 2] = src[(width - 1) * 4 + 2];
        dest[(newWidth - 1) * 4 + 3] = src[(width - 1) * 4 + 3];

        dest[(newHeight - 1) * newWidth * 4 + 0] = src[(height - 1) * width * 4 + 0];
        dest[(newHeight - 1) * newWidth * 4 + 1] = src[(height - 1) * width * 4 + 1];
        dest[(newHeight - 1) * newWidth * 4 + 2] = src[(height - 1) * width * 4 + 2];
        dest[(newHeight - 1) * newWidth * 4 + 3] = src[(height - 1) * width * 4 + 3];

        dest[((newHeight - 1) * newWidth + newWidth - 1) * 4 + 0] = src[((height - 1) * width + width - 1) * 4 + 0];
        dest[((newHeight - 1) * newWidth + newWidth - 1) * 4 + 1] = src[((height - 1) * width + width - 1) * 4 + 1];
        dest[((newHeight - 1) * newWidth + newWidth - 1) * 4 + 2] = src[((height - 1) * width + width - 1) * 4 + 2];
        dest[((newHeight - 1) * newWidth + newWidth - 1) * 4 + 3] = src[((height - 1) * width + width - 1) * 4 + 3];

        return dest;
    }
}
