using Infrastructure;

namespace LogsAnalyzer.Analyzers;

public interface ITrafficAnalyzer
{
    Task<List<ulong>> FindLoyalUsersAsync(IAsyncEnumerable<LogEntry> logEntriesSourceAsync);
}