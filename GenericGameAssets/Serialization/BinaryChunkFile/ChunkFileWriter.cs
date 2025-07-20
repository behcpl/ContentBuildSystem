namespace GenericGameAssets.Serialization.BinaryChunkFile;

public class ChunkFileWriter
{
    private readonly BinaryWriter _writer;
    private readonly MemoryStream _stream = new(1024 * 1024);

    public ChunkFileWriter(BinaryWriter writer)
    {
        _writer = writer;
    }

    public void WriteHeader(uint magic)
    {
        _writer.Write(magic);
    }

    public void WriteChunk(uint type, Action<BinaryWriter> writeCallback)
    {
        _writer.Write(type);

        BinaryWriter subWriter = new(_stream);

        writeCallback(subWriter);

        _stream.Seek(0, SeekOrigin.Begin);

        _writer.Write((int)_stream.Length);
        _stream.CopyTo(_writer.BaseStream);

        _stream.Position = 0;
        _stream.SetLength(0);
    }

    public void WriteChunk(uint type, ReadOnlySpan<byte> data)
    {
        _writer.Write(type);
        _writer.Write(data.Length);
        _writer.Write(data);
    }
}
