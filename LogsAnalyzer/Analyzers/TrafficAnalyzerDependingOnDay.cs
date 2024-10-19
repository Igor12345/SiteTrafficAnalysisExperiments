using Infrastructure;
using LogsAnalyzer.IOOperations;
using LogsAnalyzer.Lines;

namespace LogsAnalyzer.Analyzers
{
    /// <summary>
    /// User loyalty is determined by whether the user has visited different site pages on different days.
    /// </summary>
    public class TrafficAnalyzerDependingOnDay
    {
        private readonly IEnumerable<string> _files;
        private readonly FileReaderFactory _fileReaderFactory;
        private readonly LineParser _parser;
        private int _currentEpoch;

        public TrafficAnalyzerDependingOnDay(FileReaderFactory fileReaderFactory, IEnumerable<string> files)
        {
            _files = Guard.NotNull(files);
            _fileReaderFactory = Guard.NotNull(fileReaderFactory);
            _parser = new LineParser();
        }

        public async Task<List<ulong>> FindLoyalUsersAsync()
        {
            List<ulong> loyalUsers = new List<ulong>();
            //An epoch means each separate time segment that should be processed separately from others.
            //In this case, each day is an epoch. If you need to divide users by weeks or vice versa by hours,
            //then the epoch will be a week or an hour, respectively.
            //For simplicity, the epoch is determined by files, but in general, it is determined by timestamps.
            Dictionary<ulong, UserHistory> statistic = new Dictionary<ulong, UserHistory>();

            foreach (string file in _files)
            {
                await ReadAndProcessFileAsync(file, statistic, loyalUsers);
            }

            return loyalUsers;
        }

        private async Task ReadAndProcessFileAsync(string file,
            Dictionary<ulong, UserHistory> statistic, List<ulong> loyalCustomers)
        {
            int currentEpoch = UpdateEpoch();

            using var fileReader = _fileReaderFactory.Create(file);

            while (fileReader.Peek() > -1)
            {
                var line = await fileReader.ReadLineAsync();
                if (line == null)
                    break;

                var parsingResult = _parser.Parse(line);
                if (parsingResult.IsError)
                {
                    HandleError(parsingResult.ErrorMessage);
                    continue;
                }

                var (customerId, pageId) = parsingResult.Value;

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
                            loyalCustomers.Add(customerId);
                            continue;
                        }

                        statistic[customerId] = userHistory with { CurrentEpoch = currentEpoch, FirstEpoch = false };
                    }

                }
            }
        }

        private int UpdateEpoch()
        {
            return ++_currentEpoch;
        }

        private void HandleError(string errorMessage)
        {
            //todo
            Console.WriteLine(errorMessage);
        }

        private record struct UserHistory(int CurrentEpoch, bool FirstEpoch, bool Processed, HashSet<uint>? Pages);
    }
}
