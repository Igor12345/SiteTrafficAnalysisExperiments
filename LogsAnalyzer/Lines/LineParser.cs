using Infrastructure;

namespace LogsAnalyzer.Lines;

public class LineParser
{
    public Result<(ulong customerId, uint pageId)> Parse(string line)
    {
        var parts = line.Split(";");
        if (parts.Length != 3)
            return Result<(ulong, uint)>.Error($"Invalid line: {line}");

        if (!ulong.TryParse(parts[1], out var customerId))
            return Result<(ulong, uint)>.Error($"Invalid customer id: {parts[1]}");

        if (!uint.TryParse(parts[2], out var pageId))
            return Result<(ulong, uint)>.Error($"Invalid page id: {parts[2]}");

        return Result<(ulong, uint)>.Ok((customerId, pageId));
    }
}