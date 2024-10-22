namespace FileCreator.Lines;

internal sealed class LinesWriter : IDisposable
{
    private readonly FileStream _fileStream;

    public LinesWriter(string filePath)
    {
        _fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, false);
    }

    public void Write(Span<byte> buffer)
    {
        //let's propagate the exception. It looks like there is nothing that we can do about this.
        _fileStream.Write(buffer);
    }

    public void Flush()
    {
        _fileStream.Flush();
    }

    public void Dispose()
    {
        _fileStream.Dispose();
    }
}