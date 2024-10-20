using Infrastructure;
using Infrastructure.IOOperations;
using LogsAnalyzer.Analyzers;
using LogsAnalyzer.Lines;
using Microsoft.Extensions.Hosting;

namespace ConsoleUI;

internal class LogsAnalyzerService : IHostedService
{
    private readonly IResultsSaver _resultsSaver;
    private readonly LogReaderConfiguration _configuration;
    private readonly FileReaderFactory _fileReaderFactory;

    public LogsAnalyzerService(LogReaderConfiguration configuration, IResultsSaver resultsSaver)
    {
        _configuration = Guard.NotNull(configuration);
        _resultsSaver = Guard.NotNull(resultsSaver);
        _fileReaderFactory = new FileReaderFactory();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Result<string[]> files = GetLogFiles();
        if (files.IsError)
        {
            ProcessErrors(files.ErrorMessage);
            return;
        }

        LineParser parser = new LineParser(_configuration.LineDelimiter);
        ILinesSourceAsync logsReader = new LogsReader(_fileReaderFactory, files.Value);

        ITrafficAnalyzer trafficAnalyzer = new TrafficAnalyzerDependingOnDay(parser);
        // ITrafficAnalyzer trafficAnalyzer = new TrafficAnalyzerRegardlessOfTheDay(parser);
        var loyalUsers = await trafficAnalyzer.FindLoyalUsersAsync(logsReader);
        await SaveResultAsync(loyalUsers);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("******* Final ********");
        return Task.CompletedTask;
    }

    private void ProcessErrors(string errorMessage)
    {
        Console.WriteLine("Something went wrong");
        Console.WriteLine(errorMessage);
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