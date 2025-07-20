namespace GenericGameAssetsProcessors.ImageImporter;

public struct Rect
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;

    public int Width => Right - Left;
    public int Height => Bottom - Top;

    public bool IsValid => Width > 0 && Height > 0;

    public Rect(int left, int top, int right, int bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }
}
