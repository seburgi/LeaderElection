using System;
using NUnit.Framework;

namespace LeaderElection.Tests
{
    [TestFixture]
    public abstract class NodeGroupManagerTests
    {
        protected FakeTimeProvider Time;
        protected AbstractNodeGroupManager NodeGroupManager;

        private const string GROUPNAME = "GROUP1";
        private const string GROUPVERSION = "1.0.0.0";

        private const string NODE1 = "NODE1";
        private const string NODE2 = "NODE2";
        private const string NODE3 = "NODE3";

        public class NoNodeTests : NodeGroupManagerTests
        {
            [SetUp]
            public void Setup()
            {
                Time = new FakeTimeProvider();
                NodeGroupManager = new InProcessNodeGroupManager(TimeSpan.FromMinutes(1), Time);

                _node1 = NodeGroupManager.GetNodeGroup(GROUPNAME, GROUPVERSION);
            }

            private NodeGroup _node1;

            [Test]
            public void Can_start_election_process()
            {
                Assert.IsNotNull(_node1);
                Assert.AreEqual(GROUPNAME, _node1.Name);
                Assert.AreEqual(GROUPVERSION, _node1.Version);
            }

            [Test]
            public void Has_no_leader()
            {
                Assert.IsNullOrEmpty(_node1.LeaderNode);
                Assert.AreEqual(DateTime.MinValue, _node1.LeaderUntil);
            }
        }

        public class SingleNodeTests : NodeGroupManagerTests
        {
            [SetUp]
            public void Setup()
            {
                Time = new FakeTimeProvider();
                NodeGroupManager = new InProcessNodeGroupManager(TimeSpan.FromMinutes(1), Time);

                NodeGroup nodeGroup = NodeGroupManager.GetNodeGroup(GROUPNAME, GROUPVERSION);
                _nodeGroup = NodeGroupManager.GetOrUpdateNodeGroup(GROUPNAME, GROUPVERSION, NODE1, nodeGroup.LeaderUntil);
            }

            private readonly DateTime _leaderUntil = DateTime.Now.Add(TimeSpan.FromMinutes(1));
            private NodeGroup _nodeGroup;

            [Test]
            public void Can_become_leader_if_no_other_nodes_exist()
            {
                Assert.AreEqual(NODE1, _nodeGroup.LeaderNode);
                Assert.AreEqual(_leaderUntil, _nodeGroup.LeaderUntil);
            }
        }

        public class MultiNodeTests : NodeGroupManagerTests
        {
            [SetUp]
            public void Setup()
            {
                Time = new FakeTimeProvider();
                NodeGroupManager = new InProcessNodeGroupManager(TimeSpan.FromSeconds(10), Time);
            }

            private readonly DateTime _leaderUntil = DateTime.Now.AddSeconds(10);

            [Test]
            public void Only_one_node_can_become_leader()
            {
                NodeGroup nodeGroup = NodeGroupManager.GetNodeGroup(GROUPNAME, GROUPVERSION);
                NodeGroupManager.GetOrUpdateNodeGroup(GROUPNAME, GROUPVERSION, NODE1, nodeGroup.LeaderUntil);

                nodeGroup = NodeGroupManager.GetNodeGroup(GROUPNAME, GROUPVERSION);
                nodeGroup = NodeGroupManager.GetOrUpdateNodeGroup(GROUPNAME, GROUPVERSION, NODE2, nodeGroup.LeaderUntil);
                
                Assert.AreEqual(NODE1, nodeGroup.LeaderNode);
                Assert.AreEqual(_leaderUntil, nodeGroup.LeaderUntil);
            }

            [Test]
            public void Node2_can_become_leader_when_Node1_times_out()
            {
                NodeGroup nodeGroup = NodeGroupManager.GetNodeGroup(GROUPNAME, GROUPVERSION);
                NodeGroupManager.GetOrUpdateNodeGroup(GROUPNAME, GROUPVERSION, NODE1, nodeGroup.LeaderUntil);

                nodeGroup = NodeGroupManager.GetNodeGroup(GROUPNAME, GROUPVERSION);
                nodeGroup = NodeGroupManager.GetOrUpdateNodeGroup(GROUPNAME, GROUPVERSION, NODE2, nodeGroup.LeaderUntil);

                Time.Now = _leaderUntil.AddSeconds(1);
                nodeGroup = NodeGroupManager.GetOrUpdateNodeGroup(GROUPNAME, GROUPVERSION, NODE2, nodeGroup.LeaderUntil);

                Assert.AreEqual(NODE1, nodeGroup.LeaderNode);
                Assert.AreEqual(_leaderUntil, nodeGroup.LeaderUntil);
            }

            [Test]
            public void Node3_can_become_leader_when_Node1_times_out()
            {
                Time = new FakeTimeProvider();
                NodeGroupManager = new InProcessNodeGroupManager(TimeSpan.FromMinutes(1), Time);

                ClientPing(NODE1);
                var nodeGroup = NodeGroupManager.GetNodeGroup(GROUPNAME, GROUPVERSION);
                Assert.AreEqual(NODE1, nodeGroup.LeaderNode);
                Assert.AreEqual(Time.Now.AddSeconds(10), nodeGroup.LeaderUntil);
                
                Time.AddSeconds(9);

                nodeGroup = NodeGroupManager.GetNodeGroup(GROUPNAME, GROUPVERSION);
                Assert.AreEqual(NODE1, nodeGroup.LeaderNode);
                Assert.AreEqual(Time.Now.AddSeconds(1), nodeGroup.LeaderUntil);

                ClientPing(NODE1);
                nodeGroup = NodeGroupManager.GetNodeGroup(GROUPNAME, GROUPVERSION);
                Assert.AreEqual(NODE1, nodeGroup.LeaderNode);
                Assert.AreEqual(Time.Now.AddSeconds(10), nodeGroup.LeaderUntil);

                Time.AddSeconds(9);

                ClientPing(NODE2);
                nodeGroup = NodeGroupManager.GetNodeGroup(GROUPNAME, GROUPVERSION);
                Assert.AreEqual(NODE1, nodeGroup.LeaderNode);
                Assert.AreEqual(Time.Now.AddSeconds(1), nodeGroup.LeaderUntil);

                Time.AddSeconds(2);

                ClientPing(NODE2);
                nodeGroup = NodeGroupManager.GetNodeGroup(GROUPNAME, GROUPVERSION);
                Assert.AreEqual(NODE2, nodeGroup.LeaderNode);
                Assert.AreEqual(Time.Now.AddSeconds(10), nodeGroup.LeaderUntil);

                Time.AddSeconds(10);

                ClientPing(NODE2);
                nodeGroup = NodeGroupManager.GetNodeGroup(GROUPNAME, GROUPVERSION);
                Assert.AreEqual(NODE2, nodeGroup.LeaderNode);
                Assert.AreEqual(Time.Now.AddSeconds(10), nodeGroup.LeaderUntil);

                Time.AddSeconds(10);

                ClientPing(NODE1);
                nodeGroup = NodeGroupManager.GetNodeGroup(GROUPNAME, GROUPVERSION);
                Assert.AreEqual(NODE1, nodeGroup.LeaderNode);
                Assert.AreEqual(Time.Now.AddSeconds(10), nodeGroup.LeaderUntil);
            }

            private void ClientPing(string nodeName)
            {
                var nodeGroup = NodeGroupManager.GetNodeGroup(GROUPNAME, GROUPVERSION);
                NodeGroupManager.GetOrUpdateNodeGroup(GROUPNAME, GROUPVERSION, nodeName, nodeGroup.LeaderUntil);
            }
        }
    }

}