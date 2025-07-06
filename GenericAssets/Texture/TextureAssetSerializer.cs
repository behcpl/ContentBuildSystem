using System.IO;
using GenericAssets.BinaryChunkFile;

namespace GenericAssets.Texture;

public class TextureAssetSerializer
{
    private const uint _FILE_MAGIC = 'b' + ('t' << 8) + ('x' << 16) + ('1' << 24);
    private const uint _CHUNK_HEADER_ID = 't' + ('x' << 8) + ('h' << 16) + ('1' << 24);
    private const uint _CHUNK_BUFFER_ID = 'b' + ('u' << 8) + ('f' << 16) + ('1' << 24);

    public void Write(BinaryWriter writer, TextureAsset asset)
    {
        ChunkFileWriter chunkFileWriter = new ChunkFileWriter(writer);
        chunkFileWriter.WriteHeader(_FILE_MAGIC);

        chunkFileWriter.WriteChunk(_CHUNK_HEADER_ID, w => WriteHeader(w, asset));

        if (asset.Buffers != null)
        {
            foreach (TextureBuffer buffer in asset.Buffers)
            {
                chunkFileWriter.WriteChunk(_CHUNK_BUFFER_ID, w => WriteBuffer(w, buffer));
            }
        }
    }

    public TextureAsset Read(BinaryReader reader)
    {
        ChunkFileReader chunkFileReader = new ChunkFileReader(reader);
        chunkFileReader.ReadHeader(_FILE_MAGIC);

        TextureAsset textureAsset = ReadHeaderChunk(chunkFileReader.ReadChunk(_CHUNK_HEADER_ID));

        int surfaceCount = textureAsset.BuffersCount();
        textureAsset.Buffers = new TextureBuffer[surfaceCount];

        for (int i = 0; i < surfaceCount; i++)
        {
            textureAsset.Buffers[i] = ReadBufferChunk(chunkFileReader.ReadChunk(_CHUNK_BUFFER_ID));
        }

        return textureAsset;
    }

    private static TextureAsset ReadHeaderChunk(BinaryReader reader)
    {
        return new TextureAsset
        {
            Type = (TextureAssetType)reader.ReadInt32(),
            Format = TextureFormat.Make(reader.ReadUInt32()),
            Width = reader.ReadInt16(),
            Height = reader.ReadInt16(),
            Count = reader.ReadInt16(),
            MipMaps = reader.ReadInt16(),
        };
    }

    private static TextureBuffer ReadBufferChunk(BinaryReader reader)
    {
        int size = (int)reader.BaseStream.Length;

        TextureBuffer buffer = new TextureBuffer
        {
            Index = reader.ReadInt16(),
            MipLevel = reader.ReadInt16(),
            RowOffset = reader.ReadInt16(),
            SliceOffset = reader.ReadInt16(),
            Data = new byte[size - sizeof(short) * 4],
        };

        int read = reader.Read(buffer.Data, 0, buffer.Data.Length);
        if (read != buffer.Data.Length)
            throw new InvalidDataException("Not enough surface data");

        return buffer;
    }

    private static void WriteHeader(BinaryWriter writer, TextureAsset asset)
    {
        writer.Write((int)asset.Type);
        writer.Write(asset.Format.Value);
        writer.Write((short)asset.Width);
        writer.Write((short)asset.Height);
        writer.Write((short)asset.Count);
        writer.Write((short)asset.MipMaps);
    }

    private static void WriteBuffer(BinaryWriter writer, TextureBuffer buffer)
    {
        writer.Write((short)buffer.Index);
        writer.Write((short)buffer.MipLevel);
        writer.Write((short)buffer.RowOffset);
        writer.Write((short)buffer.SliceOffset);

        // NOTE: Data.Length is preserved as ChunkSize - 4 x sizeof(short)
        if (buffer.Data != null)
            writer.Write(buffer.Data);
    }
}
