using System.Diagnostics;

namespace GenericAssets.Texture;

public class TextureAsset
{
    public TextureAssetType Type;
    public TextureFormat Format;
    public int Width;
    public int Height;
    public int Count; // or depth in case of TEXTURE_3D
    public int MipMaps;

    public TextureSurface[]? Surfaces; // [Count (* 6 for cubemaps) * MipMaps]
    public TextureVolume[]? Volumes;   // [MipMaps]
}

public enum TextureAssetType
{
    TEXTURE_2D,
    TEXTURE_2D_ARRAY,
    CUBE_MAP,
    CUBE_MAP_ARRAY,
    TEXTURE_3D,
}

public class TextureSurface
{
    public int Index;
    public int MipLevel;
    public int RowOffset;
    public byte[]? Data;
}

public class TextureVolume
{
    public int Index;
    public int MipLevel;
    public int RowOffset;
    public int SliceOffset;
    public byte[]? Data;
}

// ReSharper disable InconsistentNaming
public readonly struct TextureFormat
{
    public const uint CHANNEL_R = 1;
    public const uint CHANNEL_RG = 2;
    public const uint CHANNEL_RGBA = 4;

    public const uint TYPE_1B_UNORM = 1 | 0x10;
    public const uint TYPE_1B_SNORM = 2 | 0x10;
    public const uint TYPE_1B_INT = 3 | 0x10;
    public const uint TYPE_1B_UINT = 4 | 0x10;
    public const uint TYPE_1B_UNORM_GAMMA = 5 | 0x10;
    public const uint TYPE_2B_UNORM = 1 | 0x20;
    public const uint TYPE_2B_SNORM = 2 | 0x20;
    public const uint TYPE_2B_INT = 3 | 0x20;
    public const uint TYPE_2B_UINT = 4 | 0x20;
    public const uint TYPE_2B_FLOAT = 5 | 0x20;
    public const uint TYPE_4B_UNORM = 1 | 0x40;
    public const uint TYPE_4B_SNORM = 2 | 0x40;
    public const uint TYPE_4B_INT = 3 | 0x40;
    public const uint TYPE_4B_UINT = 4 | 0x40;
    public const uint TYPE_4B_FLOAT = 5 | 0x40;

    public const uint PACKED_5_6_5 = 1;
    public const uint PACKED_4_4_4_4 = 2;
    public const uint PACKED_5_5_5_1 = 3;
    public const uint PACKED_10_10_10_2 = 4;
    public const uint PACKED_11_11_10 = 5;
    public const uint PACKED_9_9_9_E5 = 6;

    public const uint BLOCK_4x4 = 0x44; // most common block dimensions
    public const uint BLOCK_5x4 = 0x54; // ASTC variations
    public const uint BLOCK_5x5 = 0x55;
    public const uint BLOCK_6x5 = 0x65;
    public const uint BLOCK_6x6 = 0x66;
    public const uint BLOCK_8x5 = 0x85;
    public const uint BLOCK_8x6 = 0x86;
    public const uint BLOCK_10x5 = 0xA5;
    public const uint BLOCK_10x6 = 0xA6;
    public const uint BLOCK_8x8 = 0x88;
    public const uint BLOCK_10x8 = 0xA8;
    public const uint BLOCK_10x10 = 0xAA;
    public const uint BLOCK_12x10 = 0xCA;
    public const uint BLOCK_12x12 = 0xCC;
    // public const uint BLOCK_8x4 = 0x84; // PVRTC 2bit mode 

    public const uint METHOD_BC1 = 1;              // DXT1, S3tc
    public const uint METHOD_BC1_GAMMA = 2;        // DXT1, S3tc
    public const uint METHOD_BC2 = 3;              // DXT3
    public const uint METHOD_BC2_GAMMA = 4;        // DXT3
    public const uint METHOD_BC3 = 5;              // DXT5
    public const uint METHOD_BC3_GAMMA = 6;        // DXT5
    public const uint METHOD_BC4 = 7;              // single alpha block from DXT5
    public const uint METHOD_BC5 = 8;              // ATI2n (2x alpha block)
    public const uint METHOD_BC6 = 9;              // hdr
    public const uint METHOD_BC7 = 10;             // high quality RGB
    public const uint METHOD_BC7_GAMMA = 11;       // high quality RGB
    public const uint METHOD_ETC2_RGB = 17;        // required by GLES 3.0
    public const uint METHOD_ETC2_RGB_GAMMA = 18;  // required by GLES 3.0
    public const uint METHOD_ETC2_RGBA = 19;       // required by GLES 3.0
    public const uint METHOD_ETC2_RGBA_GAMMA = 20; // required by GLES 3.0
    public const uint METHOD_EAC_R = 21;
    public const uint METHOD_EAC_RG = 22;
    public const uint METHOD_ASTC = 23; // modern GLES de-facto standard, not supported by OpenGL bindings
    public const uint METHOD_ASTC_GAMMA = 24;

    // old formats, not relevant
    // public const uint METHOD_PVRTC_2BIT = ?;
    // public const uint METHOD_PVRTC_4BIT = ?;
    // public const uint METHOD_PVRTC2_2BIT = ?;
    // public const uint METHOD_PVRTC2_4BIT = ?;
    // public const uint METHOD_ATC = ?; // like S3tc, but without patented stuff
    // public const uint METHOD_ETC1 = ?; //no alpha support

    private const uint _MASK = 0xC0000000;
    private const uint _UNCOMPRESSED = 0x40000000;
    private const uint _COMPRESSED = 0x80000000;
    private const uint _PACKED = 0xC0000000;

    private const int _METHOD_OFFSET = 8;
    private const int _SIZE_OFFSET = 16;
    private const int _CHANNEL_OFFSET = 8;

    private const uint _SIZE_MASK = 0xFF;
    private const uint _METHOD_MASK = 0xFF;
    private const uint _BLOCK_MASK = 0xFF;
    private const uint _TYPE_MASK = 0xFF;
    private const uint _CHANNEL_MASK = 0x0F;

    internal readonly uint Value;

    internal TextureFormat(uint value)
    {
        Value = value;
    }

    public bool IsCompressed => (Value & _MASK) == _COMPRESSED;
    public uint Size => (Value >> _SIZE_OFFSET) & _SIZE_MASK; // per block for compressed or per pixel for uncompressed/packed
    public int BlockWidth => IsCompressed ? (int)((Value >> 4) & 0xF) : 0;
    public int BlockHeight => IsCompressed ? (int)(Value & 0xF) : 0;


    // TODO: get BPP
    // TODO: get bytes per block
    // TODO: get block dims

    public static TextureFormat Make(uint formatBits)
    {
        // TODO: validate
        return new TextureFormat(formatBits);
    }

    public static TextureFormat MakeUncompressed(uint channelsCount, uint channelType)
    {
        Debug.Assert(channelsCount is CHANNEL_R or CHANNEL_RG or CHANNEL_RGBA);
        Debug.Assert(channelType is >= TYPE_1B_UNORM and <= TYPE_1B_UNORM_GAMMA or >= TYPE_2B_UNORM and <= TYPE_2B_FLOAT or >= TYPE_4B_UNORM and <= TYPE_4B_FLOAT);

        uint channelSize = (channelType & 0xF0) >> 4;
        uint size = channelsCount * channelSize;

        return new TextureFormat(_UNCOMPRESSED | (channelsCount << _CHANNEL_OFFSET) | channelType | (size << _SIZE_OFFSET));
    }

    public static TextureFormat MakeCompressed(uint block, uint method)
    {
        uint size = method switch
        {
            METHOD_BC1 or METHOD_BC1_GAMMA or METHOD_BC4 or METHOD_ETC2_RGB or METHOD_ETC2_RGB_GAMMA or METHOD_EAC_R => 8,
            _                                                                                                        => 16,
        };

        return new TextureFormat(_COMPRESSED | block | (method << _METHOD_OFFSET) | (size << _SIZE_OFFSET));
    }

    public static TextureFormat MakePacked(uint packedFormat)
    {
        uint size = packedFormat switch
        {
            PACKED_4_4_4_4 or PACKED_5_5_5_1 or PACKED_5_6_5 => 2,
            _                                                => 4,
        };

        return new TextureFormat(_PACKED | packedFormat | (size << _SIZE_OFFSET));
    }
}
