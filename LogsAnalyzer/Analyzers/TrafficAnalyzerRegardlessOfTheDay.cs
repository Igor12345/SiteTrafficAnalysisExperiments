using LogsAnalyzer.LogEntries;

namespace LogsAnalyzer.Analyzers
{
    /// <summary>
    /// User loyalty is determined depending on whether he has ever visited different pages of the site, regardless of the day of the visit.
    /// In this case, it is enough to use the Dictionary<ulong, long> data type.
    /// When a user is first mentioned in a log, his ID and page identifier are entered into the dictionary.When the user is next recorded,
    /// if another page is specified there, the value in the dictionary changes to negative, i.e.at least two different pages were visited.
    /// </summary>
    public class TrafficAnalyzerRegardlessOfTheDay : ITrafficAnalyzer
    {
        public async Task<List<ulong>> FindLoyalUsersAsync(IAsyncEnumerable<LogEntry> logEntriesSourceAsync)
        {
            Dictionary<ulong, long> statistic = new Dictionary<ulong, long>();
            List<ulong> loyalUsers = new List<ulong>();

            await foreach (LogEntry logEntry in logEntriesSourceAsync)
            {
                if (!statistic.TryAdd(logEntry.CustomerId, logEntry.PageId))
                {
                    long prevPage = statistic[logEntry.CustomerId];
                    if (prevPage > 0 && prevPage != logEntry.PageId)
                    {
                        statistic[logEntry.CustomerId] = -1;
                        loyalUsers.Add(logEntry.CustomerId);
                    }
                }
            }

            return loyalUsers;
        }

        private void HandleError(string errorMessage)
        {
            //todo
            Console.WriteLine(errorMessage);
        }
    }
}
