using System;
using System.Collections.Concurrent;

namespace LeaderElection
{
    public class InProcessNodeGroupManager : AbstractNodeGroupManager
    {
        private readonly ConcurrentDictionary<string, NodeGroup> _nodeGroups = new ConcurrentDictionary<string, NodeGroup>();

        public InProcessNodeGroupManager(TimeSpan timeToLive, ITimeProvider timeProvider) : base(timeToLive, timeProvider) {}

        public override NodeGroup GetNodeGroup(string name, string version)
        {
            NodeGroup nodeGroup = _nodeGroups.GetOrAdd(name, (key) => new NodeGroup(name, version, null, DateTime.MinValue));

            return new NodeGroup(nodeGroup.Name, nodeGroup.Version, nodeGroup.LeaderNode, nodeGroup.LeaderUntil);
        }

        public override NodeGroup GetOrUpdateNodeGroup(string name, string version, string leaderNode, DateTime leaderUntil)
        {
            var newLeaderUntil = GetNewLeaderUntil();

            NodeGroup nodeGroup = GetNodeGroup(name, version);
            
            if (nodeGroup.LeaderUntil == leaderUntil && 
                nodeGroup.LeaderUntil < newLeaderUntil && 
                (nodeGroup.LeaderNode == leaderNode  || nodeGroup.LeaderUntil <= TimeProvider.Now))
            {
                nodeGroup = new NodeGroup(name, version, leaderNode, newLeaderUntil);
                _nodeGroups[nodeGroup.Name] = nodeGroup;
            }

            return nodeGroup;
        }
    }
}