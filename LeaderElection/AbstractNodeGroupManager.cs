using System;

namespace LeaderElection
{
    public abstract class AbstractNodeGroupManager
    {
        protected readonly ITimeProvider TimeProvider;
        private readonly TimeSpan _timeToLive;

        protected AbstractNodeGroupManager(TimeSpan timeToLive, ITimeProvider timeProvider = null)
        {
            _timeToLive = timeToLive;
            TimeProvider = timeProvider ?? new RealTimeProvider();
        }

        public abstract NodeGroup GetNodeGroup(string name, string version);

        public abstract NodeGroup GetOrUpdateNodeGroup(string name, string version, string leaderNode, DateTime leaderUntil);
S
        protected DateTime GetNewLeaderUntil()
        {
            return TimeProvider.Now.Add(_timeToLive);
        }
    }
}