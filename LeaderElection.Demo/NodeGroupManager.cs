using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace LeaderElection.Demo
{
    public class NodeGroupManager : AbstractNodeGroupManager
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        public NodeGroupManager(TimeSpan timeToLive, ITimeProvider timeProvider = null) : base(timeToLive, timeProvider) {}

        public override NodeGroup GetNodeGroup(string name, string version)
        {
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                return GetNodeGroupImpl(name, version, con);
            }
        }

        public override NodeGroup GetOrUpdateNodeGroup(string name, string version, string leaderNode, DateTime leaderUntil)
        {
            var newLeaderUntil = GetNewLeaderUntil();

            if (newLeaderUntil <= leaderUntil)
            {
                throw new ArgumentException("Argument oldLeaderUntil must be in the past.", "leaderUntil");
            }

            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();

                if (leaderUntil != null)
                {
                    bool updated = con.Execute(
                        @"UPDATE NodeGroups SET LeaderNode = @leaderNode, LeaderUntil = @newLeaderUntil
                              WHERE Name = @name AND Version = @version AND
                              LeaderUntil = @oldLeaderUntil AND
                              LeaderUntil < @newLeaderUntil AND
                              (LeaderNode = @leaderNode OR LeaderUntil <= GETDATE())",
                        new {name, version, leaderNode, oldLeaderUntil = leaderUntil, newLeaderUntil}) > 0;

                    if (updated)
                    {
                        return new NodeGroup(name, version, leaderNode, newLeaderUntil);
                    }
                }

                NodeGroup nodeGroup = GetNodeGroupImpl(name, version, con);

                while (nodeGroup == null)
                {
                    // new Group
                    bool inserted = con.Execute(
                        @"INSERT INTO NodeGroups (Name, Version, LeaderNode, LeaderUntil)
                              VALUES(@name, @version, @leaderNode, @newLeaderUntil)",
                        new {name, version, leaderNode, newLeaderUntil}) > 0;

                    nodeGroup = inserted ?
                        new NodeGroup(name, version, leaderNode, newLeaderUntil) :
                        GetNodeGroupImpl(name, version, con);
                }

                return nodeGroup;
            }
        }

        private static NodeGroup GetNodeGroupImpl(string name, string version, SqlConnection con)
        {
            return con.Query<NodeGroup>(
                @"SELECT * FROM NodeGroups WHERE Name = @name AND Version = @version",
                new {name, version})
                .SingleOrDefault();
        }
    }
}