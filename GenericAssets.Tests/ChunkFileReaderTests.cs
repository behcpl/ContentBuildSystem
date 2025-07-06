using System.Text;
using GenericAssets.BinaryChunkFile;

namespace GenericAssets.Tests;

public class ChunkFileReaderTests
{
    private const uint _MAGIC = 0xdeadbeef;
    private const uint _CHUNK_1 = 0xcccc1111;
    private const uint _CHUNK_2 = 0xabcd1234;

    [Test]
    public void Check_magic()
    {
        MemoryStream memory = CreateSampleFile();

        using BinaryReader reader = new BinaryReader(memory);
        ChunkFileReader chunkReader = new ChunkFileReader(reader);

        Assert.DoesNotThrow(() => chunkReader.ReadHeader(_MAGIC));
    }

    [Test]
    public void Throw_on_invalid_magic()
    {
        MemoryStream memory = CreateSampleFile();

        using BinaryReader reader = new BinaryReader(memory);
        ChunkFileReader chunkReader = new ChunkFileReader(reader);

        Assert.Throws<InvalidDataException>(() => chunkReader.ReadHeader(_CHUNK_1));
    }

    [Test]
    public void Throw_on_empty_file()
    {
        MemoryStream memory = new MemoryStream();

        using BinaryReader reader = new BinaryReader(memory);
        ChunkFileReader chunkReader = new ChunkFileReader(reader);

        Assert.Throws<EndOfStreamException>(() => chunkReader.ReadHeader(_MAGIC));
    }

    [Test]
    public void Read_chunks()
    {
        MemoryStream memory = CreateSampleFile();

        using BinaryReader reader = new BinaryReader(memory);
        ChunkFileReader chunkReader = new ChunkFileReader(reader);

        chunkReader.ReadHeader(_MAGIC);

        byte[] data = new byte[16];

        int read = chunkReader.ReadChunk(_CHUNK_1).Read(data);
        Assert.That(read, Is.EqualTo(data.Length));

        read = chunkReader.ReadChunk(_CHUNK_2).Read(data);
        Assert.That(read, Is.EqualTo(data.Length));
    }

    [Test]
    public void Throw_on_invalid_chunk_type()
    {
        MemoryStream memory = CreateSampleFile();

        using BinaryReader reader = new BinaryReader(memory);
        ChunkFileReader chunkReader = new ChunkFileReader(reader);

        chunkReader.ReadHeader(_MAGIC);

        Assert.Throws<InvalidDataException>(() => chunkReader.ReadChunk(_CHUNK_2));
    }

    [Test]
    public void Throw_on_missing_chunk()
    {
        MemoryStream memory = CreateSampleFile();

        using BinaryReader reader = new BinaryReader(memory);
        ChunkFileReader chunkReader = new ChunkFileReader(reader);

        chunkReader.ReadHeader(_MAGIC);

        byte[] data = new byte[16];
        _ = chunkReader.ReadChunk(_CHUNK_1).Read(data);
        _ = chunkReader.ReadChunk(_CHUNK_2).Read(data);

        Assert.Throws<EndOfStreamException>(() => chunkReader.ReadChunk(_CHUNK_2));
    }

    [Test]
    public void Throw_on_read_past_chunk_size()
    {
        MemoryStream memory = CreateSampleFile();

        using BinaryReader reader = new BinaryReader(memory);
        ChunkFileReader chunkReader = new ChunkFileReader(reader);

        chunkReader.ReadHeader(_MAGIC);

        byte[] data = new byte[16];
        BinaryReader subReader = chunkReader.ReadChunk(_CHUNK_1);
        _ = subReader.Read(data);

        Assert.Throws<EndOfStreamException>(() => _ = subReader.Read(data));
    }

    [Test]
    public void Throw_on_read_past_end_of_file()
    {
        MemoryStream memory = CreateSampleFile();

        using BinaryReader reader = new BinaryReader(memory);
        ChunkFileReader chunkReader = new ChunkFileReader(reader);

        chunkReader.ReadHeader(_MAGIC);

        byte[] data = new byte[16];
        _ = chunkReader.ReadChunk(_CHUNK_1).Read(data);

        BinaryReader subReader = chunkReader.ReadChunk(_CHUNK_2);
        _ = subReader.Read(data);

        Assert.Throws<EndOfStreamException>(() => _ = subReader.Read(data));
    }

    [Test]
    public void ReadChunk_skips_unread_data()
    {
        MemoryStream memory = CreateSampleFile();

        using BinaryReader reader = new BinaryReader(memory);
        ChunkFileReader chunkReader = new ChunkFileReader(reader);

        chunkReader.ReadHeader(_MAGIC);

        byte[] data = new byte[8];
        _ = chunkReader.ReadChunk(_CHUNK_1).Read(data);
        _ = chunkReader.ReadChunk(_CHUNK_2).Read(data);
    }

    // file _MAGIC with _CHUNK_1 & _CHUNK_2 each with 16b of data
    private MemoryStream CreateSampleFile()
    {
        MemoryStream memory = new MemoryStream();

        using BinaryWriter writer = new BinaryWriter(memory, Encoding.Default, true);
        ChunkFileWriter chunkWriter = new ChunkFileWriter(writer);

        chunkWriter.WriteHeader(_MAGIC);

        byte[] data = new byte[16];
        chunkWriter.WriteChunk(_CHUNK_1, data);
        chunkWriter.WriteChunk(_CHUNK_2, data);

        memory.Position = 0;
        return memory;
    }
}
