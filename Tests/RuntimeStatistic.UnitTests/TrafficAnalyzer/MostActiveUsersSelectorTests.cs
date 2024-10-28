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
        var zero1 = GC.CollectionCount(0);
        var first1 = GC.CollectionCount(1);
        var second1 = GC.CollectionCount(2);
        var total1 = GC.GetTotalMemory(false)/1024/1024;
        var allocated1 = GC.GetTotalAllocatedBytes()/1024/1024;
        
        IEventsGenerator<(string dateTime, ulong userId, uint pageId)> logsGenerator =
            new SiteVisitsGenerator(userIdMin, usersNumber, pageIdMin, pagesNumber, fakeTimeProvider);

        EpochIdentifier epochIdentifier = new(Epochs.ByMinute, DateTime.UtcNow);
        TrafficHistory trafficHistory = new TrafficHistory(time => epochIdentifier.DetermineEpoch(time));

        MostActiveUsersSelector activityConsumer = new MostActiveUsersSelector(trafficHistory, 1000);
        var parserConsumer =
            CompositeConsumer<(string dateTime, ulong userId, uint pageId), Result<LogEntry>>
                .Create(LogEntry.ParseRecord);
        parserConsumer.Then(logEntry => trafficHistory.Save(logEntry))
                .Then(logEntry => activityConsumer.Consume(logEntry));
        
        CancellationTokenSource cts = new CancellationTokenSource();
        int channelCapacity = 1000;
        Channel<(string dateTime, ulong userId, uint pageId)> channel = Channel.CreateBounded<(string dateTime, ulong userId, uint pageId)>(channelCapacity);
        TrafficReader<(string dateTime, ulong userId, uint pageId)> trafficReader =
            new TrafficReader<(string dateTime, ulong userId, uint pageId)>(channel, parserConsumer);
        _ = Task.Run(async () => await trafficReader.ReadAsync(cts.Token).ConfigureAwait(false), cts.Token);

        var interval = TimeSpan.FromMilliseconds(1);
        TrafficSourceAsyncStart<(string dateTime, ulong userId, uint pageId)> trafficSource =
            new TrafficSourceAsyncStart<(string dateTime, ulong userId, uint pageId)>(logsGenerator, channel,
                interval, fakeTimeProvider);
        _ = Task.Run(async () => await trafficSource.StartGenerationAsync(cts.Token).ConfigureAwait(false), cts.Token);
        
        for (int i = 0; i < 1000; i++)
        {
            fakeTimeProvider.Advance(interval);
            await Task.Delay(TimeSpan.FromMilliseconds(1));
        }
        
        await Task.Delay(TimeSpan.FromSeconds(1));
        ulong[] mostActiveUsers = activityConsumer.GetMostActiveUsers(100);
        
        var zero2 = GC.CollectionCount(0);
        var first2 = GC.CollectionCount(1);
        var second2 = GC.CollectionCount(2);
        var total2 = GC.GetTotalMemory(false)/1024/1024;
        var allocated2 = GC.GetTotalAllocatedBytes()/1024/1024;
        Console.WriteLine(
            $"GC.CollectionCount 0 = {zero2 - zero1}, 1 = {first2 - first1}, 2 = {second2 - second1}; Total = {total1}-{total2}; Allocated = {allocated1} - {allocated2}");
        
        Console.WriteLine($"Active users: {mostActiveUsers.Length}");
        Assert.That(mostActiveUsers, Is.Not.Null);
        Assert.That(mostActiveUsers.Length, Is.GreaterThan(0));
    }
}