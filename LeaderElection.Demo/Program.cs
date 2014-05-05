using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeaderElection.Demo
{
    class Program
    {
        private const string _version = "1.0.0.0";

        static void Main(string[] args)
        {
            Console.WriteLine("Please enter group name: G");
            //var groupName = Console.ReadLine();
            var groupName = "G";

            Console.Write("Please enter node name: ");
            var nodeName = Console.ReadLine();

            var nodeManager = new NodeGroupManager(TimeSpan.FromSeconds(5));
            var nodeGroup = nodeManager.GetNodeGroup(groupName, _version);

            while (true)
            {
                nodeGroup = nodeManager.GetOrUpdateNodeGroup(groupName, _version, nodeName, nodeGroup.LeaderUntil);
                Console.WriteLine("Leader: " +nodeGroup.LeaderNode + " " + (nodeGroup.LeaderUntil - DateTime.Now) + "sec");
                Thread.Sleep(1000);
            }
        }
    }
}
