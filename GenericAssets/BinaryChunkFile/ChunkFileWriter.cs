using System;
using System.IO;

namespace GenericAssets.BinaryChunkFile;

public class ChunkFileWriter
{
    private readonly BinaryWriter _writer;

    public ChunkFileWriter(BinaryWriter writer)
    {
        _writer = writer;
    }

    public void WriteCallback(uint type, Action<BinaryWriter> writeCallback)
    {
        _writer.Write(type);

        MemoryStream stream = new MemoryStream(1024*1024);
        BinaryWriter subWriter = new BinaryWriter(stream);

        writeCallback(subWriter);

        stream.Seek(0, SeekOrigin.Begin);
        
        _writer.Write((int)stream.Length);
        stream.CopyTo(_writer.BaseStream);
    }
    
    public void WriteData(uint type, ReadOnlySpan<byte> data)
    {
        _writer.Write(type);
        _writer.Write(data.Length);
        _writer.Write(data);
    }
}