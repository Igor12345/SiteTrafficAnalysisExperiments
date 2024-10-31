using Infrastructure;

namespace LogsAnalyzer.LogEntries;

public interface ILogEntryParser
{
    Result<LogEntry> Parse(ReadOnlyMemory<byte> lineAsSpan);
    Result<LogEntry> Parse(string line);
    Result<LogEntry> ParseShort(string line);
}