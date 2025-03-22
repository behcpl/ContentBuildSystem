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

    public void WriteData(uint type, ReadOnlySpan<byte> data)
    {
        
    }
}