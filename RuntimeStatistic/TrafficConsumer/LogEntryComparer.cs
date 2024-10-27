using LogsAnalyzer.LogEntries;

namespace RuntimeStatistic.TrafficConsumer;

internal class LogEntryComparer : IComparer<LogEntry>
{
    private readonly TrafficHistory _history;

    public LogEntryComparer(TrafficHistory history)
    {
        _history = history ?? throw new ArgumentNullException(nameof(history));
    }
   
    public int Compare(LogEntry x, LogEntry y)
    {
        uint uniqueVisitsX = _history.GetUniqueVisits(x.CustomerId);
        uint uniqueVisitsY = _history.GetUniqueVisits(y.CustomerId);
        return Comparer<uint>.Default.Compare(uniqueVisitsX, uniqueVisitsY);
    }
}