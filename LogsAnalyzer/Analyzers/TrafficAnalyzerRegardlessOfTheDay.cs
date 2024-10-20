using Infrastructure;
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
        private readonly LogEntryParser _parser;

        public TrafficAnalyzerRegardlessOfTheDay(LogEntryParser parser)
        {
            _parser = Guard.NotNull(parser);
        }

        public async Task<List<ulong>> FindLoyalUsersAsync(IAsyncEnumerable<string> linesSourceAsync)
        {
            Dictionary<ulong, long> statistic = new Dictionary<ulong, long>();
            List<ulong> loyalUsers = new List<ulong>();

            await foreach (string line in linesSourceAsync)
            {
                var parsingResult = _parser.ParseShort(line);
                if (parsingResult.IsError)
                {
                    HandleError(parsingResult.ErrorMessage);
                    continue;
                }

                var (customerId, pageId) = parsingResult.Value;
                if (!statistic.TryAdd(customerId, pageId))
                {
                    long prevPage = statistic[customerId];
                    if (prevPage > 0 && prevPage != pageId)
                    {
                        statistic[customerId] = -1;
                        loyalUsers.Add(customerId);
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
