using Infrastructure;
using LogsAnalyzer.Lines;

namespace LogsAnalyzer.Analyzers
{
    /// <summary>
    /// User loyalty is determined by whether the user has visited different site pages on different days.
    /// </summary>
    public class TrafficAnalyzerDependingOnDay
    {
        private readonly ILinesSourceAsync _linesSourceAsync;
        private readonly LineParser _parser;
        private int _currentEpoch;
        DateTime _currentDate = DateTime.MinValue.Date;

        public TrafficAnalyzerDependingOnDay(LineParser parser, ILinesSourceAsync linesSourceAsync)
        {
            _parser = Guard.NotNull(parser);
            _linesSourceAsync = Guard.NotNull(linesSourceAsync);
        }

        public async Task<List<ulong>> FindLoyalUsersAsync()
        {
            //An epoch means each separate time segment that should be processed separately from others.
            //In this case, each day is an epoch. If you need to divide users by weeks or vice versa by hours,
            //then the epoch will be a week or an hour, respectively.
            //For simplicity, the epoch is determined by files, but in general, it is determined by timestamps.
            Dictionary<ulong, UserHistory> statistic = new Dictionary<ulong, UserHistory>();
            List<ulong> loyalUsers = new List<ulong>();

            await foreach (string line in _linesSourceAsync)
            {
                var parsingResult = _parser.Parse(line);
                if (parsingResult.IsError)
                {
                    HandleError(parsingResult.ErrorMessage);
                    continue;
                }

                var (customerId, pageId, dateTime) = parsingResult.Value;
                int currentEpoch = UpdateEpoch(dateTime);

                if (!statistic.TryGetValue(customerId, out _))
                {
                    statistic.Add(customerId, new UserHistory(currentEpoch, true, false, [pageId]));
                }
                else
                {
                    var userHistory = statistic[customerId];
                    if (userHistory.Processed)
                        continue;
                    if (userHistory.FirstEpoch && userHistory.CurrentEpoch == currentEpoch)
                    {
                        userHistory.Pages!.Add(pageId);
                    }
                    else
                    {
                        if (!userHistory.Pages!.Contains(pageId))
                        {
                            statistic[customerId] = new UserHistory(currentEpoch, false, true, null);
                            loyalUsers.Add(customerId);
                            continue;
                        }

                        statistic[customerId] = userHistory with { CurrentEpoch = currentEpoch, FirstEpoch = false };
                    }

                }
            }

            return loyalUsers;
        }
        
        private int UpdateEpoch(DateTime dateTime)
        {
            if (dateTime.Date > _currentDate)
            {
                _currentDate = dateTime.Date;
                return ++_currentEpoch;
            }
            return _currentEpoch;
        }

        private void HandleError(string errorMessage)
        {
            //todo
            Console.WriteLine(errorMessage);
        }

        private record struct UserHistory(int CurrentEpoch, bool FirstEpoch, bool Processed, HashSet<uint>? Pages);
    }
}
