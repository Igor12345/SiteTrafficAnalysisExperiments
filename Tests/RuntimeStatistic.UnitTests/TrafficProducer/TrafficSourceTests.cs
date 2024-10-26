using System.Threading.Channels;
using Microsoft.Reactive.Testing;
using Moq;
using RuntimeStatistic.TrafficProducer;

namespace RuntimeStatistic.UnitTests.TrafficProducer;

public class TrafficSourceTests
{
    [Test]
    public async Task ShouldGenerateAndSendEventsToChannel()
    {
        Mock<IEventsGenerator<int>> generatorMock = new();
        int[] expectedValues = [123, 432, 7];
        generatorMock.SetupSequence(g => g.Next()).Returns(expectedValues[0]).Returns(expectedValues[1])
            .Returns(expectedValues[2]);
        
        Channel<int> channel = Channel.CreateUnbounded<int>();
        TestScheduler scheduler = new();
        uint interval = 100;

        EventSourceBuilder builder = new();
        var eventSource = builder.Create<int>(config =>
        {
            config.UseGenerator(generatorMock.Object);
            config.GenerateEventEvery(TimeSpan.FromMilliseconds(interval));
            config.UseScheduler(scheduler);
        }).Build();

        TrafficSource<int> trafficSource = new TrafficSource<int>(eventSource, channel);
        IDisposable disposable = trafficSource.StartGeneration();

        var intervalInTicks = TimeSpan.FromMilliseconds(interval).Ticks;
        for (int i = 0; i < expectedValues.Length; i++)
        {
            scheduler.AdvanceBy(intervalInTicks);
        }
        disposable.Dispose();

        List<int> realResults = new List<int>();
        CancellationTokenSource cts =
            new CancellationTokenSource(TimeSpan.FromMilliseconds(interval * expectedValues.Length + interval / 2.0));
        CancellationToken cancellationToken = cts.Token;
        await foreach (int item in channel.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
            realResults.Add(item);
            if (realResults.Count > expectedValues.Length)
                Assert.Fail("Expected count is greater than the number of events");
        }

        Assert.That(realResults.Count, Is.EqualTo(expectedValues.Length));
        CollectionAssert.AreEqual(expectedValues, realResults);
    }
}