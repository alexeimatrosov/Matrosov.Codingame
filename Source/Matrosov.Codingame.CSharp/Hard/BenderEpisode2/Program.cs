using System;
using System.Linq;

namespace Matrosov.Codingame.CSharp.Hard.BenderEpisode2
{
    public class Program
    {
        public static void Main()
        {
            var input = Input.Read();
            var pathFinder = new FullSearchMoneyCollector(input);
            var maxAmountOfMoney = pathFinder.FindMaxAmountOfMoney();
            
            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            Console.WriteLine(maxAmountOfMoney);
        }

        private interface IMoneyCollector
        {
            int FindMaxAmountOfMoney();
        }

        private class FullSearchMoneyCollector : IMoneyCollector
        {
            private readonly Input _input;
            private readonly int[] _cache;

            public FullSearchMoneyCollector(Input input)
            {
                _input = input;
                _cache = new int[input.N + 1];
                for (var i = 0; i < input.N + 1; i++)
                {
                    _cache[i] = -1;
                }
            }

            public int FindMaxAmountOfMoney()
            {
                return FindMaxAmountOfMoneyFromNode(0);
            }

            private int FindMaxAmountOfMoneyFromNode(int node)
            {
                if (_cache[node] != -1)
                {
                    return _cache[node];
                }

                var maxMoney = _input.Adjacents[node].Length == 0
                    ? 0
                    : _input.Adjacents[node].Select(x => _input.Money[node] + FindMaxAmountOfMoneyFromNode(x)).Max();

                _cache[node] = maxMoney;

                return maxMoney;
            }
        }

        //private class DijkstraMoneyCollector : IMoneyCollector
        //{
        //    private readonly Input _input;
        //    private readonly int[] _distance;
        //    private readonly int[] _tentativeDistance;
        //    private readonly int[] _parents;
        //    private readonly HashSet<int> _unvisited;

        //    public DijkstraMoneyCollector(Input input)
        //    {
        //        _input = input;

        //        _unvisited = new HashSet<int>(Enumerable.Range(0, _input.N + 1));
        //        _distance = _input.Money.Select(x => -x).ToArray();
        //        //_distance = _input.Money.Select(x => x == 0 ? 0 : 1000000 / x).ToArray();
        //        //_distance = _input.Money.Select(x => x == 0 ? 0 : 1000000.0 / (double)x).ToArray();

        //        _parents = new int[_input.N + 1];
        //        _tentativeDistance = new int[_input.N + 1];
        //        for (var i = 0; i < _input.N + 1; i++)
        //        {
        //            _parents[i] = -1;
        //            _tentativeDistance[i] = int.MaxValue;
        //        }

        //        _tentativeDistance[0] = _distance[0];
        //    }

        //    public int FindMaxAmountOfMoney()
        //    {
        //        var node = 0;
        //        while (true)
        //        {
        //            foreach (var nextNode in _input.Adjacents[node])
        //            {
        //                EvaluateNeighborTentativeDistance(node, nextNode);
        //            }

        //            _unvisited.Remove(node);

        //            if (node == _input.N) break;

        //            var nextIndex = FindNextSmallestUnvisited();
        //            //if (nextIndex == -1) break;

        //            node = nextIndex;
        //        }

        //        return GetReversePath().Sum(x => _input.Money[x]);
        //    }

        //    private void EvaluateNeighborTentativeDistance(int fromNode, int toNode)
        //    {
        //        var tentativeDistance = _tentativeDistance[fromNode] + _distance[toNode];

        //        if (tentativeDistance < _tentativeDistance[toNode])
        //        {
        //            _tentativeDistance[toNode] = tentativeDistance;
        //            _parents[toNode] = fromNode;
        //        }
        //    }

        //    private int FindNextSmallestUnvisited()
        //    {
        //        var nextNode = -1;
        //        var minTentativeDistance = int.MaxValue;

        //        foreach (var node in _unvisited)
        //        {
        //            if (_tentativeDistance[node] < minTentativeDistance)
        //            {
        //                minTentativeDistance = _tentativeDistance[node];
        //                nextNode = node;
        //            }
        //        }

        //        return nextNode;
        //    }

        //    private IEnumerable<int> GetReversePath()
        //    {
        //        var node = _input.N;

        //        while (node != 0)
        //        {
        //            yield return node;

        //            node = _parents[node];
        //        }

        //        yield return node;
        //    }
        //}

        private class Input
        {
            public int N { get; private set; }
            public int[] Money { get; private set; }
            public int[][] Adjacents { get; private set; }

            public static Input Read()
            {
                var n = int.Parse(Console.ReadLine());

                var input = new Input
                            {
                                N = n,
                                Money = new int[n + 1],
                                Adjacents = new int[n + 1][],
                            };

                for (var i = 0; i < n; i++)
                {
                    var inputs = Console.ReadLine().Split(' ');
                    var node = int.Parse(inputs[0]);
                    input.Money[node] = int.Parse(inputs[1]);
                    input.Adjacents[node] = new[]
                                            {
                                                inputs[2] == "E" ? n : int.Parse(inputs[2]),
                                                inputs[3] == "E" ? n : int.Parse(inputs[3])
                                            };
                }
                input.Money[n] = 0;
                input.Adjacents[n] = new int[0];

                return input;
            }
        }
    }
}