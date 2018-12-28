using System;
using System.Collections.Generic;
using System.Linq;

namespace Matrosov.Codingame.CSharp.Medium.MarsLanderEpisode2
{
    public class Player
    {
        private const double G = -3.711;

        public static void Main(string[] args)
        {
            var points = new List<Point>();

            var N = int.Parse(Console.ReadLine()); // the number of points used to draw the surface of Mars.
            for (var i = 0; i < N; i++)
            {
                var inputs = Console.ReadLine().Split(' ');
                var LAND_X = int.Parse(inputs[0]); // X coordinate of a surface point. (0 to 6999)
                var LAND_Y = int.Parse(inputs[1]); // Y coordinate of a surface point. By linking all the points together in a sequential fashion, you form the surface of Mars.
                points.Add(new Point(LAND_X, LAND_Y));
            }

            var land = new Land(points);
            var controller = new Controller(land);

            // game loop
            while (true)
            {
                var inputs = Console.ReadLine().Split(' ');
                var X = int.Parse(inputs[0]);
                var Y = int.Parse(inputs[1]);
                var HS = int.Parse(inputs[2]); // the horizontal speed (in m/s), can be negative.
                var VS = int.Parse(inputs[3]); // the vertical speed (in m/s), can be negative.
                var F = int.Parse(inputs[4]); // the quantity of remaining fuel in liters.
                var R = int.Parse(inputs[5]); // the rotation angle in degrees (-90 to 90).
                var P = int.Parse(inputs[6]); // the thrust power (0 to 4).

                var output = controller.NextOutput(new Point(X, Y), HS, VS, F, R, P);

                Console.WriteLine("{0} {1}", (int) Math.Round(output.Rotation), output.Power); // R P. R is the desired rotation angle. P is the desired thrust power.
            }
        }

        private class Controller
        {
            private const double G = -3.711;
            private const double MaxPower = 4.0;
            private const int TravelHorizontalSpeedLimit = 50;
            private const int HorizontalSpeedLimit = 20;
            private const int VerticalSpeedLimit = 40;
            private static readonly double VerticalCompensationAngle = Math.Acos(Math.Abs(G) / MaxPower) * 180 / Math.PI;

            private readonly Land _land;

            public Controller(Land land)
            {
                _land = land;
            }

            public Output NextOutput(Point position, int horizontalSpeed, int verticalSpeed, int fuel, int rotation, int power)
            {
                var relativePosition = _land.GetRelativePosition(position);
                var relativePositionSign = _land.GetRelativePositionSign(position);

                var newRotation = 0.0;
                int newPower;

                if (relativePosition == RelativePosition.Left)
                {
                    if (horizontalSpeed > TravelHorizontalSpeedLimit)
                    {
                        newRotation = VerticalCompensationAngle;
                        newPower = 4;
                    }
                    else if (HorizontalSpeedLimit <= horizontalSpeed && horizontalSpeed <= TravelHorizontalSpeedLimit)
                    {
                        newRotation = verticalSpeed < 0 ? 0.0 : -VerticalCompensationAngle;
                        newPower = 4;
                    }
                    else if (0 <= horizontalSpeed && horizontalSpeed < HorizontalSpeedLimit)
                    {
                        newRotation = -VerticalCompensationAngle;
                        newPower = 4;
                    }
                    else
                    {
                        newRotation = -2 * VerticalCompensationAngle;
                        newPower = 4;
                    }
                }
                else if (relativePosition == RelativePosition.Right)
                {
                    horizontalSpeed = -horizontalSpeed;

                    if (horizontalSpeed > TravelHorizontalSpeedLimit)
                    {
                        newRotation = -VerticalCompensationAngle;
                        newPower = 4;
                    }
                    else if (HorizontalSpeedLimit <= horizontalSpeed && horizontalSpeed <= TravelHorizontalSpeedLimit)
                    {
                        newRotation = verticalSpeed < 0 ? 0.0 : VerticalCompensationAngle;
                        newPower = 4;
                    }
                    else if (0 <= horizontalSpeed && horizontalSpeed < HorizontalSpeedLimit)
                    {
                        newRotation = VerticalCompensationAngle;
                        newPower = 4;
                    }
                    else
                    {
                        newRotation = 2 * VerticalCompensationAngle;
                        newPower = 4;
                    }
                }
                else
                {
                    if (_land.GetHeightAboveFlatGround(position) < VerticalSpeedLimit * 2)
                    {
                        newRotation = 0.0;
                        newPower = Math.Abs(verticalSpeed) < VerticalSpeedLimit ? 3 : 4;
                    }
                    else if (Math.Abs(horizontalSpeed) > HorizontalSpeedLimit)
                    {
                        newRotation = 2 * Math.Sign(horizontalSpeed) * VerticalCompensationAngle;
                        newPower = 4;
                    }
                    else
                    {
                        newRotation = Math.Sign(horizontalSpeed) * VerticalCompensationAngle;
                        newPower = Math.Abs(verticalSpeed) < VerticalSpeedLimit ? 3 : 4;
                    }
                }

                return new Output
                {
                    Rotation = newRotation,
                    Power = newPower,
                };
            }
        }

        private class Land
        {
            private readonly List<Point> _points;
            private Point _flatGroundStart;
            private Point _flatGroundEnd;

            public int FlatY => _flatGroundStart.Y;

            public Land(IEnumerable<Point> points)
            {
                _points = points.ToList();
                FindFlatGround();
            }

            private void FindFlatGround()
            {
                for (var i = 0; i < _points.Count - 1; i++)
                {
                    if (_points[i].Y == _points[i + 1].Y)
                    {
                        _flatGroundStart = _points[i];
                        _flatGroundEnd = _points[i + 1];
                        break;
                    }
                }
            }

            public int GetHeightAboveFlatGround(Point point)
            {
                return point.Y - _flatGroundStart.Y;
            }

            public RelativePosition GetRelativePosition(Point point)
            {
                if (point.X < _flatGroundStart.X) return RelativePosition.Left;
                if (point.X > _flatGroundEnd.X) return RelativePosition.Right;

                return RelativePosition.Above;
            }

            public int GetRelativePositionSign(Point point)
            {
                if (point.X < _flatGroundStart.X) return -1;
                if (point.X > _flatGroundEnd.X) return 1;

                return 0;
            }

            public bool IsAboveFlatGround(Point point)
            {
                return _flatGroundStart.X <= point.X && point.X <= _flatGroundEnd.X;
            }
        }

        private enum RelativePosition
        {
            None = 0,
            Left = 1,
            Above = 2,
            Right = 3,
        }

        private class Output
        {
            public double Rotation { get; set; }
            public int Power { get; set; }
        }

        private struct Point
        {
            public int X { get; }
            public int Y { get; }

            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }
        }
    }
}
