using System;

namespace GenericGameAssetsProcessors.ImageImporter;

public struct Color
{
    public static readonly Color White = new(1.0f, 1.0f, 1.0f, 1.0f);
    public static readonly Color Black = new(0.0f, 0.0f, 0.0f, 1.0f);
    public static readonly Color Transparent = new(0.0f, 0.0f, 0.0f, 0.0f);
    public static readonly Color Red = new(1.0f, 0.0f, 0.0f, 1.0f);
    public static readonly Color Green = new(0.0f, 1.0f, 0.0f, 1.0f);
    public static readonly Color Blue = new(0.0f, 0.0f, 1.0f, 1.0f);
    public static readonly Color Yellow = new(1.0f, 1.0f, 0.0f, 1.0f);
    public static readonly Color Cyan = new(0.0f, 1.0f, 1.0f, 1.0f);
    public static readonly Color Magenta = new(1.0f, 0.0f, 1.0f, 1.0f);

    public float R;
    public float G;
    public float B;
    public float A;

    public Color(float r, float g, float b, float a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public static Color FromPackedColor(uint packed)
    {
        byte r = unchecked((byte)packed);
        byte g = unchecked((byte)(packed >> 8));
        byte b = unchecked((byte)(packed >> 16));
        byte a = unchecked((byte)(packed >> 24));
        return FromBytes(r, g, b, a);
    }

    public static Color FromBytes(byte r, byte g, byte b, byte a)
    {
        const float SCALE = 1.0f / 255.0f;
        return new Color(r * SCALE, g * SCALE, b * SCALE, a * SCALE);
    }

    public uint ToPackedColor()
    {
        uint r = unchecked((uint)MathF.Round(Math.Clamp(R, 0.0f, 1.0f) * 255));
        uint g = unchecked((uint)MathF.Round(Math.Clamp(G, 0.0f, 1.0f) * 255));
        uint b = unchecked((uint)MathF.Round(Math.Clamp(B, 0.0f, 1.0f) * 255));
        uint a = unchecked((uint)MathF.Round(Math.Clamp(A, 0.0f, 1.0f) * 255));
        return r | (g << 8) | (b << 16) | (a << 24);
    }

    public (byte r, byte g, byte b, byte a) ToBytes()
    {
        byte r = unchecked((byte)MathF.Round(Math.Clamp(R, 0.0f, 1.0f) * 255));
        byte g = unchecked((byte)MathF.Round(Math.Clamp(G, 0.0f, 1.0f) * 255));
        byte b = unchecked((byte)MathF.Round(Math.Clamp(B, 0.0f, 1.0f) * 255));
        byte a = unchecked((byte)MathF.Round(Math.Clamp(A, 0.0f, 1.0f) * 255));
        return (r, g, b, a);
    }

    public Color ToLinearSpace()
    {
        return new Color(GammaToLinear(R), GammaToLinear(G), GammaToLinear(B), A);
    }

    public Color ToGammaSpace()
    {
        return new Color(LinearToGamma(R), LinearToGamma(G), LinearToGamma(B), A);
    }

    public Color ToAlphaPremultiplied()
    {
        return new Color(R * A, G * A, B * A, A);
    }

    private static float GammaToLinear(float v)
    {
        return v > 0.04045f ? MathF.Pow((v + 0.055f) / 1.055f, 2.2f) : v / 12.92f;
    }

    private static float LinearToGamma(float v)
    {
        return v > 0.0031308f ? 1.055f * MathF.Pow(v, 1 / 2.2f) - 0.055f : v * 12.92f;
    }
}
