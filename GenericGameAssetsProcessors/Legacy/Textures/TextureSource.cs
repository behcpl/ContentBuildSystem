namespace GenericGameAssetsProcessors.Legacy.Textures;

public enum AlphaMode
{
    NONE,
    STRAIGHT,
    PREMULTIPLIED,
}

public enum TextureFormat
{
    UNKNOWN = 0,
    RGBA8888 = 1,
    RG88 = 2,
    R8 = 3,
    DXT1 = 64,
    DXT5 = 65,
}

public class TextureSource
{
    public int Width;
    public int Height;
    public TextureFormat Format;
    public byte[]? Data;

    public Margin Margin;
    public Margin SourceOffset;
    public Point2 Pivot;
    public bool IsLinear;
    public AlphaMode AlphaMode;

    public static int Stride(TextureFormat format, int width)
    {
        return format switch
        {
            TextureFormat.UNKNOWN  => 0,
            TextureFormat.RGBA8888 => 4 * width,
            TextureFormat.RG88     => 2 * width,
            TextureFormat.R8       => width,
            TextureFormat.DXT1     => width / 2,
            TextureFormat.DXT5     => width,
            _                      => 0,
        };
    }
}
