using JetBrains.Annotations;

namespace GenericAssets.Legacy.Textures;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public struct Margin
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;

    public static Margin Join(Margin first, Margin second)
    {
        return new Margin
        {
            Left = first.Left + second.Left,
            Top = first.Top + second.Top,
            Right = first.Right + second.Right,
            Bottom = first.Bottom + second.Bottom,
        };
    }

    public static Margin MakeUniform(int value)
    {
        return new Margin
        {
            Left = value,
            Top = value,
            Right = value,
            Bottom = value,
        };
    }

    public static Margin Offset(Margin first, Margin second)
    {
        return new Margin
        {
            Left = first.Left + second.Left,
            Top = first.Top + second.Top,
            Right = first.Right - second.Right,
            Bottom = first.Bottom - second.Bottom,
        };
    }
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public struct Point2
{
    public int X;
    public int Y;
}
