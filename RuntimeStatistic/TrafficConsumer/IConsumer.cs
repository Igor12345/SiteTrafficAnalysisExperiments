namespace RuntimeStatistic.TrafficConsumer;

public interface IConsumer<in T>
{
   void Consume(T item);
}