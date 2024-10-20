namespace Infrastructure.IOOperations;

public interface ILineReader : IDisposable
{
    int Peek();
    ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken);
}

public class LineReader : ILineReader
{
    private readonly TextReader _reader;

    public LineReader(TextReader reader)
    {
        _reader = Guard.NotNull(reader);
    }

    public int Peek()
    {
        return _reader.Peek();
    }

    public ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken)
    {
        return _reader.ReadLineAsync(cancellationToken);
    }

    public void Dispose()
    {
        _reader.Dispose();
    }
}