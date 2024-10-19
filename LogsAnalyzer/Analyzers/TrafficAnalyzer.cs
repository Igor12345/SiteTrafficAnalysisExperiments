using Infrastructure;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.Generic;
using System.Threading.Channels;
using LogsAnalyzer.IOOperations;
using LogsAnalyzer.Lines;

namespace LogsAnalyzer.Analyzers
{
    /// <summary>
    /// User loyalty is determined depending on whether he has ever visited different pages of the site, regardless of the day of the visit.
    /// In this case, it is enough to use the Dictionary<ulong, long> data type.
    /// When a user is first mentioned in a log, his ID and page identifier are entered into the dictionary.When the user is next recorded,
    /// if another page is specified there, the value in the dictionary changes to negative, i.e.at least two different pages were visited.
    /// </summary>
    public class TrafficAnalyzerRegardlessOfTheDay
    {
        private readonly IEnumerable<string> _files;
        private readonly FileReaderFactory _fileReaderFactory;
        private readonly LineParser _parser;
        public TrafficAnalyzerRegardlessOfTheDay(FileReaderFactory fileReaderFactory, IEnumerable<string> files)
        {
            _files = Guard.NotNull(files);
            _fileReaderFactory = Guard.NotNull(fileReaderFactory);
            _parser = new LineParser();
        }

        public async Task<List<ulong>> FindLoyalUsersAsync()
        {
            List<ulong> loyalUsers = new List<ulong>();
            Dictionary<ulong, long> statistic = new Dictionary<ulong, long>();

            foreach (string file in _files)
            {
                await ReadAndProcessFileAsync(file, statistic, loyalUsers);
            }

            return loyalUsers;
        }

        private async Task ReadAndProcessFileAsync(string file, Dictionary<ulong, long> statistic, List<ulong> loyalCustomers)
        {
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
                if (!statistic.TryAdd(customerId, pageId))
                {
                    long prevPage = statistic[customerId];
                    if (prevPage > 0 && prevPage != pageId)
                    {
                        statistic[customerId] = -1;
                        loyalCustomers.Add(customerId);
                    }
                }
            }
        }

        private void HandleError(string errorMessage)
        {
            //todo
            Console.WriteLine(errorMessage);
        }
    }
}
