using System;

namespace Laconic.Codingame.NetworkCabling
{
    public class Program
    {
        public static void Main()
        {
            var points = ReadInput();

            long lengthY = 0;
            var minX = points[0].X;
            var maxX = points[0].X;
            for (var i = 1; i < points.Length; i++)
            {
                minX = points[i].X < minX ? points[i].X : minX;
                maxX = points[i].X > maxX ? points[i].X : maxX;
                //lengthY += Math.Abs(points[i].Y - points[i-1].Y);
            }

            Console.WriteLine(maxX - minX + lengthY);
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