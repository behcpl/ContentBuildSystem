namespace GenericAssets.Texture;

public class ImageData
{
    public int Width;
    public int Height;
    // some format

    public byte[] RawData;
}

public class ImageData2D
{
    public int Width;
    public int Height;

    public byte[] Data8;
    public ushort[] Data16;
    public float[] Data32f;
}