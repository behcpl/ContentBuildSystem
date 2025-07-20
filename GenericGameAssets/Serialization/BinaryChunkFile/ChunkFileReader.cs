using System.Diagnostics.CodeAnalysis;

namespace GenericGameAssets.Serialization.BinaryChunkFile;

public class ChunkFileReader
{
    private readonly BinaryReader _reader;
    private long _nextChunkOffset;

    public ChunkFileReader(BinaryReader reader)
    {
        _reader = reader;
        _nextChunkOffset = 0;
    }

    public void ReadHeader(uint magic)
    {
        if (_reader.ReadUInt32() != magic)
            throw new InvalidDataException("Magic header is wrong");
        
        _nextChunkOffset = _reader.BaseStream.Position;
    }

    public BinaryReader ReadChunk(uint chunkId)
    {
        if (!_reader.BaseStream.CanSeek && _reader.BaseStream.Position != _nextChunkOffset)
            throw new NotSupportedException();
        
        _reader.BaseStream.Seek(_nextChunkOffset, SeekOrigin.Begin);
        
        uint cid = _reader.ReadUInt32();
        if (cid != chunkId)
            throw new InvalidDataException($"Invalid chunk: {cid}, expected: {chunkId}");

        int chunkSize = _reader.ReadInt32();
        _nextChunkOffset = _reader.BaseStream.Position + chunkSize;
        return new BinaryReader(new SubStreamReadOnly(_reader.BaseStream, chunkSize));
    }

    public bool TryReadChunk(out uint chunkId, out int chunkSize, [MaybeNullWhen(false)] out BinaryReader chunkReader)
    {
        if (_reader.BaseStream.Position == _reader.BaseStream.Length)
        {
            chunkId = 0;
            chunkSize = 0;
            chunkReader = null;
            return false;
        }
        
        if (!_reader.BaseStream.CanSeek && _reader.BaseStream.Position != _nextChunkOffset)
            throw new NotSupportedException();

        chunkId = _reader.ReadUInt32();
        chunkSize = _reader.ReadInt32();
        _nextChunkOffset = _reader.BaseStream.Position + chunkSize;
       
        chunkReader = new BinaryReader(new SubStreamReadOnly(_reader.BaseStream, chunkSize));
        return true;
    }
}
