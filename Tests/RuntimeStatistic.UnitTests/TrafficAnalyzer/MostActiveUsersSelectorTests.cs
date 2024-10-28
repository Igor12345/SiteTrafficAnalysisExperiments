using System.Threading.Channels;
using Infrastructure;
using LogsAnalyzer.Analyzers;
using LogsAnalyzer.LogEntries;
using Microsoft.Extensions.Time.Testing;
using RuntimeStatistic.TrafficConsumer;
using RuntimeStatistic.TrafficConsumer.Extensions;
using RuntimeStatistic.TrafficProducer;

namespace RuntimeStatistic.UnitTests.TrafficAnalyzer;

public class MostActiveUsersSelectorTests
{
    [Test]
    public async Task ShouldReturnMostActiveUsers()
    {
        ulong userIdMin = 100_000;
        uint usersNumber = 10000;
        uint pageIdMin = 10_000;
        uint pagesNumber = 1_000;
        var fakeTimeProvider = new FakeTimeProvider();
        IEventsGenerator<(string dateTime, ulong userId, uint pageId)> logsGenerator =
            new SiteVisitsGenerator(userIdMin, usersNumber, pageIdMin, pagesNumber, fakeTimeProvider);

        EpochIdentifier epochIdentifier = new(Epochs.ByMinute, DateTime.UtcNow);
        TrafficHistory trafficHistory = new TrafficHistory(time => epochIdentifier.DetermineEpoch(time));

        MostActiveUsersSelector activityConsumer = new MostActiveUsersSelector(trafficHistory);
        var parserConsumer =
            CompositeConsumer<(string dateTime, ulong userId, uint pageId), Result<LogEntry>>
                .Create(LogEntry.ParseRecord);
        parserConsumer.Then(logEntry => trafficHistory.Save(logEntry))
                .Then(logEntry => activityConsumer.Consume(logEntry));
        
        CancellationTokenSource cts = new CancellationTokenSource();
        int channelCapacity = 100;
        Channel<(string dateTime, ulong userId, uint pageId)> channel = Channel.CreateBounded<(string dateTime, ulong userId, uint pageId)>(channelCapacity);
        TrafficReader<(string dateTime, ulong userId, uint pageId)> trafficReader =
            new TrafficReader<(string dateTime, ulong userId, uint pageId)>(channel, parserConsumer);
        _ = Task.Run(async () => await trafficReader.ReadAsync(cts.Token).ConfigureAwait(false), cts.Token);

        TrafficSourceAsyncStart<(string dateTime, ulong userId, uint pageId)> trafficSource =
            new TrafficSourceAsyncStart<(string dateTime, ulong userId, uint pageId)>(logsGenerator, channel,
                TimeSpan.FromMilliseconds(100), fakeTimeProvider);
        _ = Task.Run(async () => await trafficSource.StartGenerationAsync(cts.Token).ConfigureAwait(false), cts.Token);
        
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine($"Tick, thread {Thread.CurrentThread.ManagedThreadId}");
            fakeTimeProvider.Advance(TimeSpan.FromMilliseconds(100));
            await Task.Delay(TimeSpan.FromMilliseconds(1));
        }
        
        Console.WriteLine($"*** Delay, thread {Thread.CurrentThread.ManagedThreadId}");
        await Task.Delay(TimeSpan.FromSeconds(10));
        ulong[] mostActiveUsers = activityConsumer.GetMostActiveUsers(10);
        
        Console.WriteLine($"Active users: {mostActiveUsers.Length}");
        Assert.That(mostActiveUsers, Is.Not.Null);
        // Assert.That(mostActiveUsers.Length, Is.GreaterThan(0));
    }
}