namespace GenericAssets.Texture;

public class TextureSurface
{
    public int Width;
    public int Height;
    public int RowOffset;
    public byte[]? Data;
}

public class TextureVolume
{
    public int Width;
    public int Height;
    public int Depth;
    public int RowOffset;
    public int SliceOffset;
    public byte[]? Data;
}

public enum TextureAssetType
{
    TEXTURE_2D,
    TEXTURE_2D_ARRAY,
    CUBE_MAP,
    CUBE_MAP_ARRAY,
    TEXTURE_3D
}

public static class TextureFormat
{
    public const uint COMPRESSED = 0x080000;
    public const uint SRGB = 0x040000;
    public const uint SIGNED = 0x020000;
    public const uint FLOAT = 0x010000;
    public const uint BLOCK_SIZE_64 = 0x8000; // as opposed to 128bit

    // compressed block dimensions
    public const uint _4x4 = 0x44; // most common block dimensions
    public const uint _5x4 = 0x54; // ASTC variations
    public const uint _5x5 = 0x55;
    public const uint _6x5 = 0x65;
    public const uint _6x6 = 0x66;
    public const uint _8x4 = 0x84; // PVRTC 2bit mode 
    public const uint _8x5 = 0x85;
    public const uint _8x6 = 0x86;
    public const uint _10x5 = 0xA5;
    public const uint _10x6 = 0xA6;
    public const uint _8x8 = 0x88;
    public const uint _10x8 = 0xA8;
    public const uint _10x10 = 0xAA;
    public const uint _12x10 = 0xCA;
    public const uint _12x12 = 0xCC;

    // compressed block type
    public const uint BC1 = BLOCK_SIZE_64; // DXT1, S3tc
    public const uint BC2 = 0x100; // DXT3
    public const uint BC3 = 0x200; // DXT5
    public const uint BC4 = 0x300 | BLOCK_SIZE_64; // single alpha block
    public const uint BC5 = 0x400; // ATI2n  
    public const uint BC6 = 0x500; // hdr
    public const uint BC7 = 0x600; // high quality RGB
    public const uint ATC = 0x700; // like S3tc, but without patented stuff
    public const uint ETC1 = 0x800 | BLOCK_SIZE_64;
    public const uint ETC2 = 0x900;
    public const uint EAC_R = 0xA00;
    public const uint EAC_RG = 0xB00;
    public const uint PVRTC = 0xC00;
    public const uint PVRTC2 = 0xD00;
    public const uint ASTC = 0xE00;

    // uncompressed bits per pixel
    public const uint BPP_8 = 0;
    public const uint BPP_16 = 1;
    public const uint BPP_32 = 2;
    public const uint BPP_64 = 3;
    public const uint BPP_128 = 4; // 4 x floats

    public const uint R = 0;
    public const uint RG = 1;
    public const uint RGB = 2;
    public const uint RGBA = 3;

    public const uint PACKED_5_6_5 = 1;
    public const uint PACKED_4_4_4_4 = 1;
    public const uint PACKED_5_5_5_1 = 1;
    public const uint PACKED_10_10_10_2 = 1;
    public const uint PACKED_11_11_10 = 1;
    public const uint PACKED_9_9_9_E5 = 1;
}

public class TextureAsset
{
    public TextureAssetType Type;
    public int Format;
}