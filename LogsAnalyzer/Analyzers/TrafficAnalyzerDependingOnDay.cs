using Infrastructure;
using LogsAnalyzer.Exception;
using LogsAnalyzer.Lines;

namespace LogsAnalyzer.Analyzers
{
    /// <summary>
    /// User loyalty is determined by whether the user has visited different site pages on different days.
    /// </summary>
    public class TrafficAnalyzerDependingOnDay : ITrafficAnalyzer
    {
        // private readonly ILinesSourceAsync _linesSourceAsync;
        private readonly LineParser _parser;
        private int _currentEpoch;
        private int _logRecordsProcessed;
        DateTime _currentDate = DateTime.MinValue.Date;

        public TrafficAnalyzerDependingOnDay(LineParser parser)
        {
            _parser = Guard.NotNull(parser);
        }

        public async Task<List<ulong>> FindLoyalUsersAsync(IAsyncEnumerable<string> linesSourceAsync)
        {
            //An epoch means each separate time segment that should be processed separately from others.
            //In this case, each day is an epoch. If you need to divide users by weeks or vice versa by hours,
            //then the epoch will be a week or an hour, respectively.
            //For simplicity, the epoch is determined by files, but in general, it is determined by timestamps.
            Dictionary<ulong, UserHistory> trafficHistory = new Dictionary<ulong, UserHistory>();
            List<ulong> loyalUsers = new List<ulong>();

            await foreach (string line in linesSourceAsync)
            {
                _logRecordsProcessed++;

                var parsingResult = _parser.Parse(line);
                if (parsingResult.IsError)
                {
                    HandleError(parsingResult.ErrorMessage);
                    continue;
                }

                var (customerId, pageId, dateTime) = parsingResult.Value;
                int currentEpoch = UpdateEpoch(dateTime);

                if (!trafficHistory.TryGetValue(customerId, out _))
                {
                    trafficHistory.Add(customerId, new UserHistory(currentEpoch, true, false, [pageId]));
                }
                else
                {
                    var userHistory = trafficHistory[customerId];
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
                            trafficHistory[customerId] = new UserHistory(currentEpoch, false, true, null);
                            loyalUsers.Add(customerId);
                            continue;
                        }

                        trafficHistory[customerId] = userHistory with { CurrentEpoch = currentEpoch, FirstEpoch = false };
                    }

                }
            }

            //Controversial decision, not for real application
            if (_logRecordsProcessed > 0 && !trafficHistory.Any())
                throw new IncorrectLogRecordsException();
            

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
