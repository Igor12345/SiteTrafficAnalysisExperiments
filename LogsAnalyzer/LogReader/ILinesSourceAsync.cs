using Infrastructure;

namespace LogsAnalyzer.LogReader;

public interface ILinesSourceAsync : IAsyncEnumerable<LogEntry>
{
}