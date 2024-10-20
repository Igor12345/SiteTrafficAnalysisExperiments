using BenchmarkDotNet.Attributes;
using ConsoleUI;
using Infrastructure.IOOperations;
using LogsAnalyzer.Analyzers;
using LogsAnalyzer.Lines;

namespace DataProcessingBenchmarks.IOOperations
{
    [MemoryDiagnoser]
    public class AnaliseLogsPerformance
    {
        private string[] _sourceFilePaths;
        private string _currentDirectory;
        private FileReaderFactory _fileReaderFactory;

        [GlobalSetup]
        public void GlobalSetup()
        {
            //todo
            _currentDirectory = "";
            _sourceFilePaths =
                [Path.Combine(_currentDirectory, "Logs/log_1.txt"), Path.Combine(_currentDirectory, "Logs/log_2.txt")];
            _fileReaderFactory = new FileReaderFactory();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
        }

        [GcServer(true)]
        [Benchmark]
        public async Task<int> AnalyseLogs()
        {
            int s = 0;
            LineParser parser = new LineParser(";");
            ILinesSourceAsync logsReader = new LogsReader(_fileReaderFactory, _sourceFilePaths);

            ITrafficAnalyzer trafficAnalyzer = new TrafficAnalyzerDependingOnDay(parser);
            // ITrafficAnalyzer trafficAnalyzer = new TrafficAnalyzerRegardlessOfTheDay(parser, logsReader);
            var loyalUsers = await trafficAnalyzer.FindLoyalUsersAsync(logsReader);
            return loyalUsers.Count;
        }
    }
}
