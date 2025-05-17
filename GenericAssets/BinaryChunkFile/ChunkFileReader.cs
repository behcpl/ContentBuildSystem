using System.IO;

namespace GenericAssets.BinaryChunkFile;

public class ChunkFileReader
{
    private readonly BinaryReader _reader;

    // private long _nextChunkOffset;
    
    public ChunkFileReader(BinaryReader reader)
    {
        _reader = reader;
    }

    public bool ValidateHeader(char magic1, char magic2, char magic3, char magic4)
    {
        try
        {
            return _reader.ReadByte() == magic1 && _reader.ReadByte() == magic2 && _reader.ReadByte() == magic3 && _reader.ReadByte() == magic4;
        }
        catch
        {
            return false;
        }
    }

    
    
    public bool TryReadChunkHeader(out ChunkHeader header)
    {
        header = default;
        return false;
    }
    
    public void SkipChunk()
    {
        
    }
}