namespace thirdpartynetwork.Application.Helpers;

public class ProgressStream(Stream inner, IProgress<double> progress) : Stream
{
    private long _totalRead = 0;

    public override int Read(byte[] buffer, int offset, int count)
    {
        var read = inner.Read(buffer, offset, count);
        _totalRead += read;
        progress.Report((double)_totalRead / inner.Length);
        return read;
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        var read = await inner.ReadAsync(buffer, offset, count, cancellationToken);
        _totalRead += read;
        progress.Report((double)_totalRead / inner.Length);
        return read;
    }

    // --- Implement other required abstract members ---
    public override bool CanRead => inner.CanRead;
    public override bool CanSeek => inner.CanSeek;
    public override bool CanWrite => inner.CanWrite;
    public override long Length => inner.Length;
    public override long Position { get => inner.Position; set => inner.Position = value; }
    public override void Flush() => inner.Flush();
    public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);
    public override void SetLength(long value) => inner.SetLength(value);
    public override void Write(byte[] buffer, int offset, int count) => inner.Write(buffer, offset, count);
}