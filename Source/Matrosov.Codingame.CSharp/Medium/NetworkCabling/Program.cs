using System;
using System.Linq;

namespace Matrosov.Codingame.CSharp.Medium.NetworkCabling
{
    [Obsolete("Refactoring required")]
    public class Program
    {
        public static void Main()
        {
            var points = ReadInput();

            var minX = long.MaxValue;
            var maxX = long.MinValue;
            long sumY = 0;
            foreach (var p in points)
            {
                minX = p.X < minX ? p.X : minX;
                maxX = p.X > maxX ? p.X : maxX;
                sumY += p.Y;
            }

            var averageY = sumY/points.Length;
            var meanPoint = points[0];
            foreach (var p in points)
            {
                meanPoint = Math.Abs(p.Y - averageY) < Math.Abs(meanPoint.Y - averageY) ? p : meanPoint;
            }

            var lengthX = maxX - minX;
            var lengthY = points.Sum(x => Math.Abs(x.Y - meanPoint.Y));

            Console.WriteLine(lengthX + lengthY);
        }

        private static Point[] ReadInput()
        {
            var pointsCount = int.Parse(Console.ReadLine());
            var points = new Point[pointsCount];
            for (var i = 0; i < pointsCount; i++)
            {
                var inputs = Console.ReadLine().Split(' ');
                points[i] = new Point(long.Parse(inputs[0]), long.Parse(inputs[1]));
            }

            return points;
        }

        public struct Point
        {
            public long X { get; private set; }
            public long Y { get; private set; }

            public Point(long x, long y)
                : this()
            {
                X = x;
                Y = y;
            }
        }
    }
}