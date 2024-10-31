using Infrastructure;
using Infrastructure.IOOperations;
using LogsAnalyzer.Analyzers;
using LogsAnalyzer.LogEntries;
using LogsAnalyzer.LogReader;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ConsoleUI;

internal class LogsAnalyzerService : IHostedService
{
    private readonly ILogger<LogsAnalyzerService> _logger;
    private readonly IResultsSaver _resultsSaver;
    private readonly LogReaderConfiguration _configuration;
    private readonly FileReaderFactory _fileReaderFactory;

    public LogsAnalyzerService(LogReaderConfiguration configuration, IResultsSaver resultsSaver, ILogger<LogsAnalyzerService> logger)
    {
        _configuration = Guard.NotNull(configuration);
        _resultsSaver = Guard.NotNull(resultsSaver);
        _fileReaderFactory = new FileReaderFactory();
        _logger = Guard.NotNull(logger);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Result<string[]> readFilesResult = GetLogFiles();

        string[] files;
        
        switch (readFilesResult)
        {
            case Failure<string[]> failure:
                ProcessErrors(failure.ErrorMessage);
                return;
            case Success<string[]> success:
                files = success.Value;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(files));
        }
        
        _logger.LogInformation($"{files.Length} logs will be analyzed.");

        ILogEntryParser parser = new LogEntryParser(_configuration.LineDelimiter);
        ILinesSourceAsync logsReader = new LogAsStringsReader(_fileReaderFactory, files, parser);
        ILinesSourceAsync logAsBytesReader = new LogAsBytesReader(_fileReaderFactory, files, new LogEntriesExtractor(parser), _logger);
        
        ITrafficAnalyzer trafficAnalyzer = new TrafficAnalyzerDependingOnDay();
        // ITrafficAnalyzer trafficAnalyzer = new TrafficAnalyzerRegardlessOfTheDay();

        var loyalUsers = await trafficAnalyzer.FindLoyalUsersAsync(logAsBytesReader);
        await SaveResultAsync(loyalUsers);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("******* Final ********");
        return Task.CompletedTask;
    }

    private void ProcessErrors(string errorMessage)
    {
        _logger.LogError(errorMessage);
    }

    private Result<string[]> GetLogFiles()
    {
        if(!Directory.Exists(_configuration.LogsFolder))
            return Result<string[]>.Error($"The folder {_configuration.LogsFolder} does not exist.");

        return Result<string[]>.Ok(Directory.EnumerateFiles(_configuration.LogsFolder).ToArray());
    }

    private async Task SaveResultAsync(List<ulong> loyalUsers)
    {
        await _resultsSaver.SaveUserIdsAsync(loyalUsers);
    }
}