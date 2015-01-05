using System;
using System.Collections.Generic;
using System.Linq;

namespace Laconic.Codingame.SkynetFinaleLevel2
{
    public class Program
    {
        static int _numberOfNodes;
        static int _numberOfLinks;
        static int _numberOfExits;

        static int[] _exits;
        static int[][] _matrix;

        static Color[] _colors;
        static int[] _distances;
        static int[] _parents;
        static readonly Queue<int> Queue = new Queue<int>();

        public static void Main(String[] args)
        {
            ReadInput();

            while (true)
            {
                var agentNodeIndex = int.Parse(Console.ReadLine());

                var minDistance = int.MaxValue;
                var minDistanceFromNodeIndex = 0;
                var minDistanceToNodeIndex = 0;
                var minDistanceToMaxNeighborExitsCount = int.MaxValue;
                var maxNeighborExitsCount = int.MinValue;
                var maxNeighborExitsCountFromNodeIndex = 0;
                var maxNeighborExitsCountToNodeIndex = 0;
                foreach (var exitNodeIndex in _exits)
                {
                    RunBfs(exitNodeIndex);

                    var distance = _distances[agentNodeIndex];
                    if (distance == -1) continue;

                    var nextNodeIndex = GetNextNodeIndexOnRoute(exitNodeIndex, agentNodeIndex);
                    var neighborExitsCount = GetNeighborExitsCount(nextNodeIndex);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        minDistanceFromNodeIndex = nextNodeIndex;
                        minDistanceToNodeIndex = exitNodeIndex;
                    }

                    if (maxNeighborExitsCount < neighborExitsCount || (maxNeighborExitsCount == neighborExitsCount && distance < minDistanceToMaxNeighborExitsCount))
                    {
                        minDistanceToMaxNeighborExitsCount = distance;
                        maxNeighborExitsCount = neighborExitsCount;
                        maxNeighborExitsCountFromNodeIndex = nextNodeIndex;
                        maxNeighborExitsCountToNodeIndex = exitNodeIndex;
                    }
                }

                int fromNodeIndex;
                int toNodeIndex;
                if (minDistance > 1)
                {
                    fromNodeIndex = maxNeighborExitsCountFromNodeIndex;
                    toNodeIndex = maxNeighborExitsCountToNodeIndex;
                }
                else
                {
                    fromNodeIndex = minDistanceFromNodeIndex;
                    toNodeIndex = minDistanceToNodeIndex;
                }

                _matrix[fromNodeIndex][toNodeIndex] = 0;
                _matrix[toNodeIndex][fromNodeIndex] = 0;

                Console.WriteLine("{0} {1}", fromNodeIndex, toNodeIndex);
            }
        }

        private static int GetNextNodeIndexOnRoute(int fromNodeIndex, int toNodeIndex)
        {
            var nextNodeIndex = toNodeIndex;
            while (_parents[nextNodeIndex] != fromNodeIndex)
            {
                nextNodeIndex = _parents[nextNodeIndex];
            }

            return nextNodeIndex;
        }

        private static int GetNeighborExitsCount(int fromNodeIndex)
        {
            var result = 0;
            for (var i = 0; i < _matrix[fromNodeIndex].Length; i++)
            {
                if (_matrix[fromNodeIndex][i] != 0 && _exits.Contains(i))
                {
                    result++;
                }
            }

            return result;
        }

        private static void RunBfs(int node)
        {
            ResetBfs();

            _colors[node] = Color.Grey;
            _distances[node] = 0;
            _parents[node] = 0;

            Queue.Enqueue(node);

            while (Queue.Count > 0)
            {
                var currentNode = Queue.Dequeue();

                for (var i = 0; i < _numberOfNodes; i++)
                {
                    if (_matrix[currentNode][i] != 1 || _colors[i] != Color.White) continue;

                    _colors[i] = Color.Grey;
                    _distances[i] = _distances[currentNode] + 1;
                    _parents[i] = currentNode;
                    Queue.Enqueue(i);
                }

                _colors[currentNode] = Color.Black;
            }
        }

        private static void ResetBfs()
        {
            Queue.Clear();

            for (int i = 0; i < _colors.Length; i++)
            {
                _colors[i] = Color.White;
                _distances[i] = -1;
                _parents[i] = -1;
            }
        }

        private static void ReadInput()
        {
            var inputs = Console.ReadLine().Split(' ');
            _numberOfNodes = int.Parse(inputs[0]); // the total number of nodes in the level, including the gateways
            _numberOfLinks = int.Parse(inputs[1]); // the number of links
            _numberOfExits = int.Parse(inputs[2]); // the number of exit gateways

            _colors = new Color[_numberOfNodes];
            _distances = new int[_numberOfNodes];
            _parents = new int[_numberOfNodes];
            _matrix = new int[_numberOfNodes][];
            for (var i = 0; i < _numberOfNodes; i++)
            {
                _matrix[i] = new int[_numberOfNodes];
            }

            for (var i = 0; i < _numberOfLinks; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                var N1 = int.Parse(inputs[0]); // N1 and N2 defines a link between these nodes
                var N2 = int.Parse(inputs[1]);
                _matrix[N1][N2] = 1;
                _matrix[N2][N1] = 1;
            }

            _exits = new int[_numberOfExits];
            for (var i = 0; i < _numberOfExits; i++)
            {
                _exits[i] = int.Parse(Console.ReadLine());
            }
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