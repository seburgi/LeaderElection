using System;

namespace LeaderElection
{
    public interface ITimeProvider
    {
        DateTime Now { get; }
    }
}