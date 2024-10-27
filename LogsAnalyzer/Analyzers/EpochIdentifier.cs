namespace LogsAnalyzer.Analyzers;

public class EpochIdentifier
{
    private readonly Epochs _epochBy;
    private readonly DateTime _startFrom;

    public EpochIdentifier(Epochs epochBy, DateTime startFrom)
    {
        _epochBy = epochBy;
        _startFrom = startFrom;
    }

    public int DetermineEpoch(DateTime dateTime) => _epochBy switch
    {
        Epochs.ByMinute => (int)dateTime.Subtract(_startFrom).TotalMinutes,
        Epochs.ByHour => (int)dateTime.Subtract(_startFrom).TotalHours,
        Epochs.ByDay => (int)dateTime.Subtract(_startFrom).TotalDays,
        _ => throw new ArgumentOutOfRangeException()
    };
}