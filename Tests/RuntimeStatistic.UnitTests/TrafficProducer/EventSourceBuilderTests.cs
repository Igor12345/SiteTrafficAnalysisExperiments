using Microsoft.Reactive.Testing;
using Moq;
using RuntimeStatistic.TrafficProducer;

namespace RuntimeStatistic.UnitTests.TrafficProducer;

public class EventSourceBuilderTests
{
    [Test]
    public void ShouldConfigureAndBuildEventSource()
    {
        List<int> events = [];
        HashSet<int> events2 = [];

        Mock<IEventsGenerator<int>> generatorMock = new();
        int[] expectedValues = [123, 432, 7];
        generatorMock.SetupSequence(g => g.Next()).Returns(expectedValues[0]).Returns(expectedValues[1])
            .Returns(expectedValues[2]);

        TestScheduler scheduler = new();
        uint interval = 100;

        EventSourceBuilder builder = new();
        var eventSource = builder.Create<int>(config =>
        {
            config.UseGenerator(generatorMock.Object);
            config.GenerateEventEvery(TimeSpan.FromMilliseconds(interval));
            config.UseScheduler(scheduler);
            config.ExecuteOnEveryEvent(Console.WriteLine);
            config.ExecuteOnEveryEvent(e => events.Add(e));
            config.ExecuteOnEveryEvent(e => events2.Add(e));
        }).Build();

        Assert.That(eventSource, Is.Not.Null);

        var observable = eventSource.Run();
        using (observable.Subscribe(Console.WriteLine))
        {
            var intervalInTicks = TimeSpan.FromMilliseconds(interval).Ticks;
            for (int i = 0; i < expectedValues.Length; i++)
            {
                scheduler.AdvanceBy(intervalInTicks);
            }
        }

        generatorMock.Verify(g => g.Next(), Times.Exactly(expectedValues.Length));
        Assert.That(events.Count, Is.EqualTo(expectedValues.Length));
        Assert.That(events2.Count, Is.EqualTo(expectedValues.Length));
    }
}