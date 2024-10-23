using System.Threading.Channels;

namespace RuntimeStatistic.TrafficPipeline
{
   public sealed class PipelineFactory
   {
      public Channel<T> Create<T>(int bound)
      {
         return Channel.CreateBounded<T>(bound);
      }
   }
}
