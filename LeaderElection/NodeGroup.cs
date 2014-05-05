using System;

namespace LeaderElection
{
    public class NodeGroup
    {
        public NodeGroup(string name, string version, string leaderNode, DateTime leaderUntil)
        {
            LeaderNode = leaderNode;
            LeaderUntil = leaderUntil;
            Name = name;
            Version = version;
        }

        public string LeaderNode { get; private set; }

        public DateTime LeaderUntil { get; private set; }
        public string Name { get; private set; }
        public string Version { get; private set; }
    }
}