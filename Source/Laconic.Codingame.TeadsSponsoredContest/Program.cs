using System;
using System.Collections.Generic;
using System.Linq;

namespace Laconic.Codingame.TeadsSponsoredContest
{
    public class Program
    {
        protected internal static Dictionary<int, HashSet<int>> Graph;
        protected internal static Dictionary<int, bool> VisitedNodes;

        public static void Main(string[] args)
        {
            ReadInput();

            VisitedNodes = Graph.Select(x => x.Key).ToDictionary(x => x, _ => false);

            var leaf = Graph.Where(p => p.Value.Count == 1).Select(x => x.Key).First();

            var maxDepth = Visit(leaf, 0);

            Console.WriteLine(maxDepth / 2 + maxDepth % 2);
        }

        private static void ReadInput()
        {
            Graph = new Dictionary<int, HashSet<int>>();

            var n = ReadInt();
            for (var i = 0; i < n; i++)
            {
                var inputs = ReadString().Split(' ');
                var x = int.Parse(inputs[0]);
                var y = int.Parse(inputs[1]);

                GetOrAdd(Graph, x).Add(y);
                GetOrAdd(Graph, y).Add(x);
            }
        }

        private static HashSet<int> GetOrAdd(Dictionary<int, HashSet<int>> dictionary, int key)
        {
            if (dictionary.TryGetValue(key, out var value) == false)
            {
                dictionary.Add(key, value = new HashSet<int>());
            }

            return value;
        }

        private static int Visit(int node, int depth)
        {
            var maxDepth = depth;

            foreach (var adjacentNode in Graph[node])
            {
                if (VisitedNodes[adjacentNode]) continue;

                VisitedNodes[adjacentNode] = true;

                maxDepth = Math.Max(maxDepth, Visit(adjacentNode, depth + 1));
            }

            return maxDepth;
        }

        private static int ReadInt() => int.Parse(ReadString());

        private static string ReadString() => Console.ReadLine();
    }
}
