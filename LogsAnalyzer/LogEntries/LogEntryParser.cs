using Infrastructure;

namespace LogsAnalyzer.LogEntries;

public class LogEntryParser
{
    private readonly string _delimiter;

    public LogEntryParser(string delimiter)
    {
        _delimiter = Guard.NotNullOrEmpty(delimiter);
    }

    public Result<(ulong customerId, uint pageId)> ParseShort(string line)
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

    public Result<(ulong customerId, uint pageId, DateTime dateTime)> Parse(string line)
    {
        var parts = line.Split(_delimiter);
        if (parts.Length != 3)
            return Result<(ulong, uint, DateTime)>.Error($"Invalid line: {line}");

        if (!DateTime.TryParse(parts[0], out var dateTime))
            return Result<(ulong, uint, DateTime)>.Error($"Invalid time mark: {parts[0]}");

        if (!ulong.TryParse(parts[1], out var customerId))
            return Result<(ulong, uint, DateTime)>.Error($"Invalid customer id: {parts[1]}");

        if (!uint.TryParse(parts[2], out var pageId))
            return Result<(ulong, uint, DateTime)>.Error($"Invalid page id: {parts[2]}");

        return Result<(ulong, uint, DateTime)>.Ok((customerId, pageId, dateTime));
    }
}