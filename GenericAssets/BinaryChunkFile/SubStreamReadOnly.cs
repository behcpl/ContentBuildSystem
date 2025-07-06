using System;
using System.IO;

namespace GenericAssets.BinaryChunkFile;

internal class SubStreamReadOnly : Stream
{
    private readonly Stream _baseStream;
    private readonly long _length;
    private readonly long _offset;

    public SubStreamReadOnly(Stream baseStream, long length)
    {
        _baseStream = baseStream;
        _length = length;
        _offset = baseStream.Position;
    }

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => false;

    public override long Length => _length;

    public override long Position
    {
        get => _baseStream.Position - _offset;
        set
        {
            if (value < 0 || value > _length)
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
            _baseStream.Position = value + _offset;
        }
    }

    public override void Flush()
    {
        // NOOP
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (count < 0 || count + _baseStream.Position - _offset > _length)
            throw new EndOfStreamException();

        return _baseStream.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        long ret;
        switch (origin)
        {
            case SeekOrigin.Begin:
                ret = _baseStream.Seek(offset + _offset, SeekOrigin.Begin);
                break;

            case SeekOrigin.Current:
                ret = _baseStream.Seek(offset, SeekOrigin.Current);
                break;

            case SeekOrigin.End:
                ret = _baseStream.Seek(offset + _offset + _length, SeekOrigin.Begin);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
        }

        return ret - _offset;
    }

    public override void SetLength(long value)
    {
        throw new System.NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new System.NotImplementedException();
    }
}
