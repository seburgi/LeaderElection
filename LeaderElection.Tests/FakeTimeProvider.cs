using System;

namespace LeaderElection.Tests
{
    public class FakeTimeProvider : ITimeProvider
    {
        public FakeTimeProvider()
        {
            Now = DateTime.Now;
        }

        public DateTime Now { get; set; }

        public void AddSeconds(int seconds)
        {
            Now = Now.AddSeconds(seconds);
        }
    }
}