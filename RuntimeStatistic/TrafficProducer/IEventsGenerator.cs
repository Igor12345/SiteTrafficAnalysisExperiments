namespace RuntimeStatistic.TrafficProducer;

public interface IEventsGenerator<out T>
{
   public T Next();
}
//todo another assembly
public sealed class SiteVisitsGenerator : IEventsGenerator<(string dateTime, UserId userId, PageId pageId)>
{
   private readonly TimeProvider _timeProvider;
   private readonly UserId _userIdMin;
   private readonly PageId _pageIdMin;
   private readonly UserId _userIdMax;
   private readonly PageId _pagesIdMax;

   public SiteVisitsGenerator(UserId userIdMin, uint usersNumber, PageId pageIdMin,
      uint pagesNumber, TimeProvider? timeProvider = null)
   {
      _timeProvider = timeProvider ?? TimeProvider.System;
      _userIdMin = userIdMin;
      _userIdMax = userIdMin + usersNumber;
      _pageIdMin = pageIdMin;
      _pagesIdMax = pageIdMin + pagesNumber;
   }

   public (string dateTime, UserId userId, PageId pageId) Next()
   {
      //todo unsafe
      var userId = (UserId)Random.Shared.NextInt64((long)_userIdMin, (long)_userIdMax);
      var pageId = (PageId)Random.Shared.Next((int)_pageIdMin, (int)_pagesIdMax);
      var time = _timeProvider.GetUtcNow();
      return (time.ToString("s"), userId, pageId);
   }
}