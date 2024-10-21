using LogsAnalyzer.LogEntries;

namespace LogsAnalyzer.Analyzers;

public class TrafficAnalyzerTopKActiveUsers : ITrafficAnalyzer
{
    public Task<List<ulong>> FindLoyalUsersAsync(IAsyncEnumerable<LogEntry> logEntriesSourceAsync)
    {
        throw new NotImplementedException();
    }
}