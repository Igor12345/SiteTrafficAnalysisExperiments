using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading.Channels;
using Infrastructure;
using LogsAnalyzer.Analyzers;
using LogsAnalyzer.LogEntries;
using Microsoft.Extensions.Time.Testing;
using Microsoft.Reactive.Testing;
using RuntimeStatistic.TrafficConsumer;
using RuntimeStatistic.TrafficConsumer.Extensions;
using RuntimeStatistic.TrafficProducer;

using PageId = uint;
using UserId = ulong;

namespace RuntimeStatistic.UnitTests.TrafficAnalyzer;

public class MostActiveUsersSelectorTests
{
    //only for experiments
    [Test]
    public async Task ShouldReturnMostActiveUsers()
    {
        UserId userIdMin = 100_000;
        uint usersNumber = 10000;
        PageId pageIdMin = 10_000;
        uint pagesNumber = 1_000;
        var fakeTimeProvider = new FakeTimeProvider();
        var zero1 = GC.CollectionCount(0);
        var first1 = GC.CollectionCount(1);
        var second1 = GC.CollectionCount(2);
        var total1 = GC.GetTotalMemory(false) / 1024 / 1024;
        var allocated1 = GC.GetTotalAllocatedBytes() / 1024 / 1024;

        IEventsGenerator<(string dateTime, UserId userId, PageId pageId)> logsGenerator =
            new SiteVisitsGenerator(userIdMin, usersNumber, pageIdMin, pagesNumber, fakeTimeProvider);

        EpochIdentifier epochIdentifier = new(Epochs.ByMinute, DateTime.UtcNow);
        TrafficHistory trafficHistory = new TrafficHistory(time => epochIdentifier.DetermineEpoch(time));

        MostActiveUsersSelector activityConsumer = new MostActiveUsersSelector(trafficHistory, 1000);
        var parserConsumer =
            CompositeConsumer<(string dateTime, UserId userId, PageId pageId), Result<LogEntry>>
                .Create(LogEntry.ParseRecord);
        parserConsumer.Then(logEntry => trafficHistory.Save(logEntry))
            .Then(logEntry => activityConsumer.Consume(logEntry));

        CancellationTokenSource cts = new CancellationTokenSource();
        int channelCapacity = 1000;
        Channel<(string dateTime, UserId userId, PageId pageId)> channel =
            Channel.CreateBounded<(string dateTime, UserId userId, PageId pageId)>(channelCapacity);
        TrafficReader<(string dateTime, UserId userId, PageId pageId)> trafficReader =
            new TrafficReader<(string dateTime, UserId userId, PageId pageId)>(channel, parserConsumer);
        _ = Task.Run(async () => await trafficReader.ReadAsync(cts.Token), cts.Token);

        var interval = TimeSpan.FromMilliseconds(1);
        TrafficSourceAsyncStart<(string dateTime, UserId userId, PageId pageId)> trafficSource =
            new TrafficSourceAsyncStart<(string dateTime, UserId userId, PageId pageId)>(logsGenerator, channel,
                interval, fakeTimeProvider);
        _ = Task.Run(async () => await trafficSource.StartGenerationAsync(cts.Token), cts.Token);

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
        var total2 = GC.GetTotalMemory(false) / 1024 / 1024;
        var allocated2 = GC.GetTotalAllocatedBytes() / 1024 / 1024;
        Console.WriteLine(
            $"GC.CollectionCount 0 = {zero2 - zero1}, 1 = {first2 - first1}, 2 = {second2 - second1}; Total = {total1}-{total2}; Allocated = {allocated1} - {allocated2}");

        Console.WriteLine($"Active users: {mostActiveUsers.Length}");
        Assert.That(mostActiveUsers, Is.Not.Null);
        Assert.That(mostActiveUsers.Length, Is.GreaterThan(0));
    }

    //only for experiments
    [Test]
    public async Task ShouldReturnMostActiveUsersRxApproach()
    {
        var zero1 = GC.CollectionCount(0);
        var first1 = GC.CollectionCount(1);
        var second1 = GC.CollectionCount(2);
        var total1 = GC.GetTotalMemory(false) / 1024 / 1024;
        var allocated1 = GC.GetTotalAllocatedBytes() / 1024 / 1024;

        UserId userIdMin = 100_000;
        uint usersNumber = 10_000;
        PageId pageIdMin = 10_000;
        uint pagesNumber = 1_000;
        var fakeTimeProvider = new FakeTimeProvider();

        IEventsGenerator<(string dateTime, UserId userId, PageId pageId)> logsGenerator =
            new SiteVisitsGenerator(userIdMin, usersNumber, pageIdMin, pagesNumber, fakeTimeProvider);

        EpochIdentifier epochIdentifier = new(Epochs.ByMinute, DateTime.UtcNow);
        TrafficHistory trafficHistory = new TrafficHistory(time => epochIdentifier.DetermineEpoch(time));

        MostActiveUsersSelector activityConsumer = new MostActiveUsersSelector(trafficHistory, (int)usersNumber);
        var parserConsumer =
            CompositeConsumer<(string dateTime, UserId userId, PageId pageId), Result<LogEntry>>
                .Create(LogEntry.ParseRecord);
        parserConsumer.Then(logEntry => trafficHistory.Save(logEntry))
            .Then(logEntry => activityConsumer.Consume(logEntry));

        CancellationTokenSource cts = new CancellationTokenSource();
        int channelCapacity = 1000;
        Channel<(string dateTime, UserId userId, PageId pageId)> channel =
            Channel.CreateBounded<(string dateTime, UserId userId, PageId pageId)>(channelCapacity);
        TrafficReader<(string dateTime, UserId userId, PageId pageId)> trafficReader =
            new TrafficReader<(string dateTime, UserId userId, PageId pageId)>(channel, parserConsumer);
        _ = Task.Run(async () => await trafficReader.ReadAsync(cts.Token).ConfigureAwait(false), cts.Token);

        var interval = TimeSpan.FromMilliseconds(1);

        TestScheduler scheduler = new();

        EventSourceBuilder builder = new();
        var eventSource = builder.Create<(string dateTime, UserId userId, PageId pageId)>(config =>
        {
            config.UseGenerator(logsGenerator);
            config.GenerateEventEvery(interval);
            config.UseScheduler(scheduler);
            config.ExecuteOnEveryEvent(LogEvent);
        }).Build();

        Assert.That(eventSource, Is.Not.Null);

        HashSet<UserId> uniqueUsers = new();
        var observable = eventSource.Run();
        using var subscr = observable
            .Select(LogEntry.ParseRecord)
            .Select(trafficHistory.Save)
            .Select(activityConsumer.Consume)
            .Subscribe(e => SaveToStorage(e, uniqueUsers),
                e => HandleError(e, uniqueUsers));
        CancellationTokenSource ctsInternal = CancellationTokenSource.CreateLinkedTokenSource(cts.Token);
        ctsInternal.Token.UnsafeRegister((s, _) => (s as IDisposable)?.Dispose(), subscr);

        var intervalInTicks = interval.Ticks;
        for (int i = 0; i < 100_000; i++)
        {
            scheduler.AdvanceBy(intervalInTicks);
        }

        await Task.Delay(TimeSpan.FromSeconds(1));
        UserId[] mostActiveUsers = activityConsumer.GetMostActiveUsers(100);

        var zero2 = GC.CollectionCount(0);
        var first2 = GC.CollectionCount(1);
        var second2 = GC.CollectionCount(2);
        var total2 = GC.GetTotalMemory(false) / 1024 / 1024;
        var allocated2 = GC.GetTotalAllocatedBytes() / 1024 / 1024;
        Console.WriteLine(
            $"GC.CollectionCount 0 = {zero2 - zero1}, 1 = {first2 - first1}, 2 = {second2 - second1}; Total = ({total1} : {total2}) Mb; Allocated = ({allocated1} : {allocated2}) Mb");

        Console.WriteLine($"Active users: {mostActiveUsers.Length}");
        Assert.That(mostActiveUsers, Is.Not.Null);
        Assert.That(mostActiveUsers.Length, Is.GreaterThan(0));
    }

    private void SaveToStorage(Result<LogEntry> result, HashSet<UserId> uniqueUsers)
    {
        uniqueUsers.Add(result.Value.CustomerId);
    }

    private void HandleError(Exception exc, HashSet<UserId> uniqueUsers)
    {
        Console.WriteLine($"<<->> Error: {exc}");
        Console.WriteLine($"<<->> Error, there are {uniqueUsers.Count} users");
    }

    private void LogEvent((string dateTime, UserId userId, PageId pageId) value)
    {
        // Console.WriteLine($"-->> Event value {value}, Thread {Thread.CurrentThread.ManagedThreadId}");
    }
}