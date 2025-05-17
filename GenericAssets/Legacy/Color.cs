using System;

namespace GenericAssets.Legacy;

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
        const float scale = 1.0f / 255.0f;
        return new Color(r * scale, g * scale, b * scale, a * scale);
    }

    public uint ToPackedColor()
    {
        uint r = unchecked((uint)Math.Round(Math.Clamp(R, 0.0f, 1.0f) * 255));
        uint g = unchecked((uint)Math.Round(Math.Clamp(G, 0.0f, 1.0f) * 255));
        uint b = unchecked((uint)Math.Round(Math.Clamp(B, 0.0f, 1.0f) * 255));
        uint a = unchecked((uint)Math.Round(Math.Clamp(A, 0.0f, 1.0f) * 255));
        return r | (g << 8) | (b << 16) | (a << 24);
    }

    public (byte r, byte g, byte b, byte a) ToBytes()
    {
        byte r = unchecked((byte)Math.Round(Math.Clamp(R, 0.0f, 1.0f) * 255));
        byte g = unchecked((byte)Math.Round(Math.Clamp(G, 0.0f, 1.0f) * 255));
        byte b = unchecked((byte)Math.Round(Math.Clamp(B, 0.0f, 1.0f) * 255));
        byte a = unchecked((byte)Math.Round(Math.Clamp(A, 0.0f, 1.0f) * 255));
        return (r, g, b, a);
    }

    public Color ToLinearSpace()
    {
        return new Color(G2L(R), G2L(G), G2L(B), A);
    }

    public Color ToGammaSpace()
    {
        return new Color(L2G(R), L2G(G), L2G(B), A);
    }

    public Color ToAlphaPremultiplied()
    {
        return new Color(R * A, G * A, B * A, A);
    }

    private static float G2L(float v)
    {
        return v > 0.04045f ? (float)Math.Pow((v + 0.055) / 1.055, 2.2) : v / 12.92f;
    }

    private static float L2G(float v)
    {
        return v > 0.0031308f ? 1.055f * (float)Math.Pow(v, 1 / 2.2) - 0.055f : v * 12.92f;
    }
}
