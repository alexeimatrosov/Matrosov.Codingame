using System;
using System.Collections.Generic;
using System.Linq;

namespace Matrosov.Codingame.CSharp.Hard.SkynetRevolutionEpisode2
{
    [Obsolete("Refactoring required")]
    public class Program
    {
        public static void Main(String[] args)
        {
            var graph = Graph.ReadInput();

            while (true)
            {
                var agentNode = int.Parse(Console.ReadLine());
                var link = graph.GetLinkToSevere(agentNode);

                Console.WriteLine("{0} {1}", link.Item1, link.Item2);
            }
        }
    }

    public class Graph
    {
        private int[] Exits { get; set; }

        readonly int[][] _matrix;
        readonly Color[] _colors;
        readonly int[] _distances;
        readonly int[] _parents;
        readonly int[] _pathTransitNodesCount;
        readonly Queue<int> _queue = new Queue<int>();

        private Graph(int nodesCount)
        {
            _matrix = new int[nodesCount][];
            for (var i = 0; i < nodesCount; i++)
            {
                _matrix[i] = new int[nodesCount];
            }

            _colors = new Color[nodesCount];
            _distances = new int[nodesCount];
            _parents = new int[nodesCount];
            _pathTransitNodesCount = new int[nodesCount];
        }

        public Tuple<int, int> GetLinkToSevere(int agentNode)
        {
            int fromNode;
            int toNode;

            RunBfs(agentNode);

            var reachableExits = Exits.Where(x => _distances[x] != -1).ToArray();
            var closestExitNode = reachableExits.OrderBy(x => _distances[x]).First();
            if (_distances[closestExitNode] == 1)
            {
                fromNode = _parents[closestExitNode];
                toNode = closestExitNode;
            }
            else
            {
                RunBfs(GetNextNodeIndexOnRoute(agentNode, closestExitNode));

                var exitNodeAtRisk = reachableExits
                    .OrderByDescending(x => GetNeighborExitsCount(_parents[x]))
                    .ThenBy(x => _pathTransitNodesCount[x])
                    .ThenBy(x => _distances[x])
                    .First();

                fromNode = _parents[exitNodeAtRisk];
                toNode = exitNodeAtRisk;
            }

            RemoveLink(fromNode, toNode);

            return new Tuple<int, int>(fromNode, toNode);
        }

        private void RunBfs(int node)
        {
            ResetBfs();

            _colors[node] = Color.Grey;
            _distances[node] = 0;
            _parents[node] = 0;
            _pathTransitNodesCount[node] = GetNeighborExitsCount(node) > 0 ? 0 : 1;

            _queue.Enqueue(node);

            while (_queue.Count > 0)
            {
                var currentNode = _queue.Dequeue();

                for (var i = 0; i < _colors.Length; i++)
                {
                    if (_matrix[currentNode][i] != 1 || _colors[i] != Color.White) continue;

                    _colors[i] = Color.Grey;
                    _distances[i] = _distances[currentNode] + 1;
                    _parents[i] = currentNode;
                    _pathTransitNodesCount[i] = _pathTransitNodesCount[currentNode] + (GetNeighborExitsCount(i) > 0 ? 0 : 1);
                    _queue.Enqueue(i);
                }

                _colors[currentNode] = Color.Black;
            }
        }

        private void ResetBfs()
        {
            _queue.Clear();

            for (var i = 0; i < _colors.Length; i++)
            {
                _colors[i] = Color.White;
                _distances[i] = -1;
                _parents[i] = -1;
                _pathTransitNodesCount[i] = -1;
            }
        }

        private int GetNeighborExitsCount(int fromNodeIndex)
        {
            var result = 0;
            for (var i = 0; i < _matrix[fromNodeIndex].Length; i++)
            {
                if (_matrix[fromNodeIndex][i] != 0 && Exits.Contains(i))
                {
                    result++;
                }
            }

            return result;
        }

        private int GetNextNodeIndexOnRoute(int fromNodeIndex, int toNodeIndex)
        {
            var nextNodeIndex = toNodeIndex;
            while (_parents[nextNodeIndex] != fromNodeIndex)
            {
                nextNodeIndex = _parents[nextNodeIndex];
            }

            return nextNodeIndex;
        }

        public static Graph ReadInput()
        {
            var inputs = Console.ReadLine().Split(' ');
            var nodesCount = int.Parse(inputs[0]); // the total number of nodes in the level, including the gateways
            var linksCount = int.Parse(inputs[1]); // the number of links
            var exitsCount = int.Parse(inputs[2]); // the number of exit gateways

            var graph = new Graph(nodesCount);
            for (var i = 0; i < linksCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                graph.AddLink(int.Parse(inputs[0]), int.Parse(inputs[1]));
            }

            var exits = new int[exitsCount];
            for (var i = 0; i < exitsCount; i++)
            {
                exits[i] = int.Parse(Console.ReadLine());
            }
            graph.Exits = exits;

            return graph;
        }

        private void AddLink(int fromNode, int toNode)
        {
            _matrix[fromNode][toNode] = 1;
            _matrix[toNode][fromNode] = 1;
        }

        private void RemoveLink(int fromNode, int toNode)
        {
            _matrix[fromNode][toNode] = 0;
            _matrix[toNode][fromNode] = 0;
        }

        enum Color
        {
            None,
            White,
            Grey,
            Black
        }
    }
}