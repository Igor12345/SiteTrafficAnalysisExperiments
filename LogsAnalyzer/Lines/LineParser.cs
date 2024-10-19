using Infrastructure;

namespace LogsAnalyzer.Lines;

public class LineParser
{
    private readonly string _delimiter;

    public LineParser(string delimiter)
    {
        _delimiter = Guard.NotNullOrEmpty(delimiter);
    }

    public Result<(ulong customerId, uint pageId)> Parse(string line)
    {
        var parts = line.Split(_delimiter);
        if (parts.Length != 3)
            return Result<(ulong, uint)>.Error($"Invalid line: {line}");

        if (!ulong.TryParse(parts[1], out var customerId))
            return Result<(ulong, uint)>.Error($"Invalid customer id: {parts[1]}");

        if (!uint.TryParse(parts[2], out var pageId))
            return Result<(ulong, uint)>.Error($"Invalid page id: {parts[2]}");

        return Result<(ulong, uint)>.Ok((customerId, pageId));
    }
}