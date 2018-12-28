using System;
using System.Collections.Generic;
using System.Linq;

namespace Matrosov.Codingame.CSharp.Medium.DwarfsOnGiants
{
    class Program
    {
        static readonly Dictionary<int, Node> Nodes = new Dictionary<int, Node>();

        static void Main()
        {
            var n = int.Parse(Console.ReadLine());
            for (var i = 0; i < n; i++)
            {
                var inputs = Console.ReadLine().Split(' ').Select(int.Parse).ToArray();
                AddLink(inputs[0], inputs[1]);
            }

            var length = Nodes.Values.Max(x => x.GetLongestSuccession());

            Console.WriteLine(length);
        }

        private static void AddLink(int fromId, int toId)
        {
            var fromNode = GetOrAddNode(fromId);
            var toNode = GetOrAddNode(toId);

            fromNode.Links.Add(toNode);
        }

        private static Node GetOrAddNode(int id)
        {
            if (Nodes.ContainsKey(id))
            {
                return Nodes[id];
            }

            var node = new Node(id);
            Nodes.Add(id, node);

            return node;
        }

        private class Node
        {
            private int _longestSuccession;

            public int Id { get; private set; }
            public List<Node> Links { get; private set; }

            public Node(int id)
            {
                Id = id;
                Links = new List<Node>();
            }

            public int GetLongestSuccession()
            {
                if (_longestSuccession != 0) return _longestSuccession;

                _longestSuccession = Links.Count == 0 ? 1 : Links.Max(x => x.GetLongestSuccession()) + 1;

                return _longestSuccession;
            }
        }
    }
}