namespace RuntimeStatistic.TrafficProducer;

public interface IEventsGenerator<out T>
{
   public T Next();
}
//todo another assembly
internal sealed class SiteVisitsGenerator : IEventsGenerator<(string dateTime, ulong userId, uint pageId)>
{
   private readonly TimeProvider _timeProvider;
   private readonly ulong _userIdMin;
   private readonly uint _pageIdMin;
   private readonly ulong _userIdMax;
   private readonly uint _pagesIdMax;

   public SiteVisitsGenerator(TimeProvider timeProvider, ulong userIdMin, uint usersNumber, uint pageIdMin,
      uint pagesNumber)
   {
      _timeProvider = timeProvider;
      _userIdMin = userIdMin;
      _userIdMax = userIdMin + usersNumber;
      _pageIdMin = pageIdMin;
      _pagesIdMax = pageIdMin + pagesNumber;
   }

   public (string dateTime, ulong userId, uint pageId) Next()
   {
      //todo unsafe
      var userId = (ulong)Random.Shared.NextInt64((long)_userIdMin, (long)_userIdMax);
      var pageId = (uint)Random.Shared.Next((int)_pageIdMin, (int)_pagesIdMax);
      var time = _timeProvider.GetUtcNow();
      return (time.ToString("s"), userId, pageId);
   }
}