﻿using Infrastructure.DataStructures;
using LogsAnalyzer.LogEntries;

namespace LogsAnalyzer.Analyzers
{
    /// <summary>
    /// User loyalty is determined by whether the user has visited different site pages on different days.
    /// </summary>
    public class TrafficAnalyzerDependingOnDay : ITrafficAnalyzer
    {
        private const int poolChunkSize = 500;
        private int _currentEpoch;
        DateTime _currentDate = DateTime.MinValue.Date;
        
        public async Task<List<ulong>> FindLoyalUsersAsync(IAsyncEnumerable<LogEntry> logEntriesSourceAsync)
        {
            //An epoch means each separate time segment that should be processed separately from others.
            //In this case, each day is an epoch. If you need to divide users by weeks or vice versa by hours,
            //then the epoch will be a week or an hour, respectively.
            //For simplicity, the epoch is determined by files, but in general, it is determined by timestamps.
            Dictionary<ulong, UserHistory> trafficHistory = new Dictionary<ulong, UserHistory>();
            ExpandableStorage<ulong> loyalUsers = new ExpandableStorage<ulong>(poolChunkSize); //i.e. in fact 512

            await foreach (LogEntry logEntry in logEntriesSourceAsync)
            {
                int currentEpoch = UpdateEpoch(logEntry.DateTime);

                if (!trafficHistory.TryGetValue(logEntry.CustomerId, out _))
                {
                    trafficHistory.Add(logEntry.CustomerId, new UserHistory(currentEpoch, true, false, [logEntry.PageId]));
                }
                else
                {
                    var userHistory = trafficHistory[logEntry.CustomerId];
                    if (userHistory.Processed)
                        continue;
                    if (userHistory.FirstEpoch && userHistory.CurrentEpoch == currentEpoch)
                    {
                        userHistory.Pages!.Add(logEntry.PageId);
                    }
                    else
                    {
                        if (!userHistory.Pages!.Contains(logEntry.PageId))
                        {
                            trafficHistory[logEntry.CustomerId] = new UserHistory(currentEpoch, false, true, null);
                            loyalUsers.Add(logEntry.CustomerId);
                            continue;
                        }

                        trafficHistory[logEntry.CustomerId] = userHistory with { CurrentEpoch = currentEpoch, FirstEpoch = false };
                    }
                }
            }

            int loyalUsersNumber = loyalUsers.Count;
            ulong[] loyalUsersResult = new ulong[loyalUsersNumber];
            loyalUsers.CopyTo(loyalUsersResult, loyalUsersNumber);

            //todo update interface
            return [..loyalUsersResult];
        }
        
        private int UpdateEpoch(DateTime dateTime)
        {
            if (dateTime.Date > _currentDate)
            {
                _currentDate = dateTime.Date;
                return ++_currentEpoch;
            }
            return _currentEpoch;
        }

        private record struct UserHistory(int CurrentEpoch, bool FirstEpoch, bool Processed, HashSet<uint>? Pages);
    }
}
