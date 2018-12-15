using System;
using System.Collections.Generic;
using System.Linq;

namespace Matrosov.Codingame.TelephoneNumbers
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var n = int.Parse(Console.ReadLine());
            var numbers = new List<string>();
            for (var i = 0; i < n; i++)
            {
                var telephone = Console.ReadLine();
                numbers.Add(telephone);
            }

            var nodes = NumbersToNodes(numbers);
            
            Console.WriteLine(nodes.Values.Sum(x => x.GetTotalNumberOfChilds()));
        }

        private static Dictionary<char, Node> NumbersToNodes(List<string> numbers)
        {
            var nodes = new Dictionary<char, Node>();

            foreach (var number in numbers)
            {
                var node = GetOrAddNode(nodes, number[0]);

                for (var i = 1; i < number.Length; i++)
                {
                    var nextNode = node.Links.FirstOrDefault(x => x.Digit == number[i]);
                    if (nextNode == null)
                    {
                        nextNode = new Node(number[i]);
                        node.Links.Add(nextNode);
                    }
                    node = nextNode;
                }
            }

            return nodes;
        }

        private static Node GetOrAddNode(Dictionary<char, Node> nodes, char digit)
        {
            if (nodes.ContainsKey(digit))
            {
                return nodes[digit];
            }

            var node = new Node(digit);
            nodes.Add(digit, node);

            return node;
        }

        private class Node
        {
            private int _totalNumberOfChilds = -1;

            public char Digit { get; private set; }
            public List<Node> Links { get; private set; }

            public Node(char digit)
            {
                Digit = digit;
                Links = new List<Node>();
            }

            public int GetTotalNumberOfChilds()
            {
                if (_totalNumberOfChilds != -1) return _totalNumberOfChilds;

                _totalNumberOfChilds = Links.Sum(x => x.GetTotalNumberOfChilds()) + 1;

                return _totalNumberOfChilds;
            }
        }
    }
}