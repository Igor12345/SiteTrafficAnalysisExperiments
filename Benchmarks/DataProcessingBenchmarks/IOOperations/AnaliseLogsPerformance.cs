using BenchmarkDotNet.Attributes;
using FileCreator.Lines;
using FileCreator;
using Infrastructure.IOOperations;
using LogsAnalyzer.Analyzers;
using LogsAnalyzer.LogEntries;
using LogsAnalyzer.LogReader;

namespace DataProcessingBenchmarks.IOOperations
{
    [MemoryDiagnoser]
    public class AnaliseLogsPerformance
    {
        private string[]? _sourceFilePaths;
        private string? _currentDirectory;
        private FileReaderFactory? _fileReaderFactory;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _currentDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            if (!Directory.Exists(_currentDirectory))
                Directory.CreateDirectory(_currentDirectory);

            FileCreatorConfiguration config = new FileCreatorConfiguration()
            {
                OutputDirectory = _currentDirectory,
                FileName = "log_##.txt",
                LinesNumber = 1_000_000,
                PagesNumber = 2000,
                CustomersNumber = 50000,
                IdLowBoundary = 10_000,
                LogEveryThsLine = 100_000
            };
            LinesGenerator generator = new LinesGenerator(new LineCreator(config));

            LinesWriterFactory linesWriterFactory = new LinesWriterFactory(config);
            var logCreator = new LogCreator(config, generator, linesWriterFactory);

            _sourceFilePaths =
                [Path.Combine(_currentDirectory, "log_1.txt"), Path.Combine(_currentDirectory, "log_2.txt")];
            foreach (string file in _sourceFilePaths)
            {
                logCreator.CreateFile();
            }
            _fileReaderFactory = new FileReaderFactory();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            if (_sourceFilePaths != null)
            {
                foreach (string filePath in _sourceFilePaths)
                {
                    File.Delete(filePath);
                }
            }
        }

        [GcServer(true)]
        [Benchmark]
        public async Task<int> AnalyseLogAsStrings()
        {
            int s = 0;
            LogEntryParser parser = new LogEntryParser(";");
            ILinesSourceAsync logsReader = new LogAsStringsReader(_fileReaderFactory, _sourceFilePaths, parser);

            // ITrafficAnalyzer trafficAnalyzer = new TrafficAnalyzerDependingOnDay();
            ITrafficAnalyzer trafficAnalyzer = new TrafficAnalyzerRegardlessOfTheDay();
            var loyalUsers = await trafficAnalyzer.FindLoyalUsersAsync(logsReader);
            return loyalUsers.Count;
        }

        [GcServer(true)]
        [Benchmark]
        public async Task<int> AnalyseLogAsBytes()
        {
            int s = 0;
            LogEntryParser parser = new LogEntryParser(";");
            ILinesSourceAsync logsReader = new LogAsBytesReader(_fileReaderFactory, _sourceFilePaths, new LogEntriesExtractor(parser));

            // ITrafficAnalyzer trafficAnalyzer = new TrafficAnalyzerDependingOnDay();
            ITrafficAnalyzer trafficAnalyzer = new TrafficAnalyzerRegardlessOfTheDay();
            var loyalUsers = await trafficAnalyzer.FindLoyalUsersAsync(logsReader);
            return loyalUsers.Count;
        }
    }
}
