using System.Text;
using Infrastructure.ByteOperations;

namespace FileCreator.Lines;

internal interface ILineCreator
{
    int WriteLine(Span<byte> buffer);
    void UpdateDate();
}

internal class LineCreator : ILineCreator
{
    private readonly FileCreatorConfiguration _config;
    private readonly byte[] _eol;
    private byte[] _endlessTime;
    private int _endlessTimeLength;
    private readonly int _eolLength;
    private readonly byte[] _delimiter;
    private readonly int _delimiterLength;
    private DateTime _currentDateTime;

    public const string Delimiter = ";";

    public LineCreator(FileCreatorConfiguration config)
    {
        _config = config;

        _eol = Encoding.UTF8.GetBytes(Environment.NewLine);
        _eolLength = _eol.Length;
        _delimiter = Encoding.UTF8.GetBytes(Delimiter);
        _delimiterLength = _delimiter.Length;
        _currentDateTime = DateTime.UtcNow.AddDays(-7);
        
        UpdateDate();
    }

    public int WriteLine(Span<byte> buffer)
    {
        int position = WriteTime(buffer);
        position = WriteDelimiter(buffer, position);
        position = WriteId(buffer, position, _config.IdLowBoundary, _config.CustomersMaxId);
        position = WriteDelimiter(buffer, position);
        position = WriteId(buffer, position, _config.IdLowBoundary, _config.PagesMaxId);
        position = WriteEol(buffer, position);

        return position;
    }

    public void UpdateDate()
    {
        _currentDateTime = _currentDateTime.AddDays(1);
        _endlessTime = Encoding.UTF8.GetBytes(_currentDateTime.ToString("s"));
        _endlessTimeLength = _endlessTime.Length;
    }

    private int WriteTime(Span<byte> buffer)
    {
        _endlessTime.CopyTo(buffer);
        return _endlessTimeLength;
    }

    private int WriteId(Span<byte> buffer, int position, long min, long max)
    {
        ulong nextNumber = (ulong)Random.Shared.NextInt64(min, max);
        int length = LongToBytesConverter.WriteULongToBytes(nextNumber, buffer[position..]);
        return position + length;
    }

    private int WriteDelimiter(Span<byte> buffer, int position)
    {
        _delimiter.CopyTo(buffer[position..]);
        return position + _delimiterLength;
    }

    private int WriteEol(Span<byte> buffer, int position)
    {
        _eol.CopyTo(buffer[position..]);
        return position + _eolLength;
    }
}