using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Laconic.Codingame.DontPanicLevel2
{
    public static class Program
    {
        public static void Main()
        {
            var game = Game.Read();
            Console.Error.WriteLine(game.ToString());

            var i = 0;
            Queue<Action> actionsQueue = null;
            while (true)
            {
                var inputs = Tools.ReadStrings();
                var cloneFloor = int.Parse(inputs[0]); // floor of the leading clone
                var clonePosition = int.Parse(inputs[1]); // position of the leading clone on its floor
                var cloneDirection = inputs[2] == "RIGHT" ? Direction.Right : Direction.Left; // direction of the leading clone: LEFT or RIGHT

                if (i == 0)
                {
                    game.SetGenerator(cloneFloor, clonePosition);
                    var actions = game.FindPathActions();
                    //foreach (var a in actions)
                    //{
                        //Console.Error.WriteLine(a);
                    //}
                    actionsQueue = new Queue<Action>(actions);
                }

                var action = actionsQueue.Count > 0 ? actionsQueue.Dequeue() : Action.Wait;

                Console.WriteLine(ActionToString(action)); // action: WAIT or BLOCK or ELEVATOR
                i++;
            }
        }

        public static string ActionToString(Action action)
        {
            switch (action)
            {
                case Action.Wait: return "WAIT";
                case Action.Block: return "BLOCK";
                case Action.Elevator: return "ELEVATOR";
                default: throw new InvalidEnumArgumentException();
            }
        }
    }

    public class Game
    {
        private Point _generator;
        private Point _exit;
        private Point[,] _field;

        private int _roundsLimit;
        private int _clonesLimit;
        private int _additionalElevatorsLimit;

        private Game()
        {
        }

        public Action[] FindPathActions(/*int cloneFloor, int clonePosition, Direction cloneDirection*/)
        {
            //if (cloneFloor == -1 || clonePosition == -1) return Action.Wait;

            return new PathFinding(_field, _generator, _exit, _roundsLimit, _clonesLimit-1, _additionalElevatorsLimit).FindPathActions();
        }

        public static Game Read()
        {
            var inputs = Tools.ReadInts();

            var field = CreateField(inputs[0], inputs[1]);

            var game = new Game
                       {
                           _roundsLimit = inputs[2],
                           _clonesLimit = inputs[5],
                           _additionalElevatorsLimit = inputs[6],
                           _field = field,
                           _exit = field[inputs[3], inputs[4]]
                       };

            game._exit.Feature |= Feature.Exit;

            for (var i = 0; i < inputs[7]; i++)
            {
                var coordinates = Tools.ReadInts();
                game._field[coordinates[0], coordinates[1]].Feature |= Feature.Elevator;
            }

            return game;
        }

        private static Point[,] CreateField(int height, int width)
        {
            var field = new Point[height, width];
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    field[i, j] = new Point(i, j);
                }
            }

            return field;
        }

        public void SetGenerator(int floor, int position)
        {
            _generator = _field[floor, position];
            _generator.Feature |= Feature.Generator;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Rounds limit    : {_roundsLimit,3}");
            sb.AppendLine($"Clones limit    : {_clonesLimit,3}");
            sb.AppendLine($"Elevators limit : {_additionalElevatorsLimit,3}");
            return sb.ToString();
        }
    }

    public class PathFinding
    {
        private static readonly Action[] ElevatorActions = {Action.Elevator, Action.Wait, Action.Wait, Action.Wait};
        private static readonly Action[] BlockActions = {Action.Block, Action.Wait, Action.Wait, Action.Wait};
        private static readonly Action[] MovementActions = {Action.Wait};

        private readonly Point[,] _field;
        private readonly Point _start;
        private readonly Point _end;
        private readonly int _costLimit;
        private readonly int _blocksLimit;
        private readonly int _elevatorsLimit;

        private List<PathInfo> _openSet;

        private PathInfo _current;

        public PathFinding(Point[,] field, Point start, Point end, int costLimit, int blocksLimit, int elevatorsLimit)
        {
            _field = field;
            _start = start;
            _end = end;
            _costLimit = costLimit;
            _blocksLimit = blocksLimit;
            _elevatorsLimit = elevatorsLimit;
        }

        public Action[] FindPathActions()
        {
            _current = new PathInfo
                       {
                           Cost = 1,
                           Heuristic = Heuristic(_start, _end),
                           Point = _start,
                           BlocksUsed = 0,
                           ElevatorsUsed = 0,
                           ParentPathInfo = null,
                           Direction = Direction.Right,
                           Actions = Array.Empty<Action>()
                       };
            _openSet = new List<PathInfo>();

            while (_current.Point != _end)
            {
                //Console.Error.WriteLine($"OpenSetLength, before: {_openSet.Count}");

                var currentPoint = _current.Point;
                var positionIncrement = _current.Direction == Direction.Right ? 1 : -1;

                if (currentPoint.Feature.HasFlag(Feature.Elevator))
                {
                    HandleMovement(currentPoint.Floor + 1, currentPoint.Position);
                }
                else
                {
                    HandleMovement(currentPoint.Floor, currentPoint.Position + positionIncrement);
                    HandleElevator(currentPoint.Floor + 1, currentPoint.Position);
                }

                HandleBlock(currentPoint.Floor, currentPoint.Position - positionIncrement);

                //Console.Error.WriteLine($"OpenSetLength,  after: {_openSet.Count}");

                _current = _openSet[0];
                _openSet.RemoveAt(0);
            }

            return TraversePathActions(_current);
        }

        private static Action[] TraversePathActions(PathInfo pathInfo)
        {
            return pathInfo.EnumeratePath().SelectMany(x => x.Actions.Reverse()).Reverse().ToArray();
        }

        private void HandleElevator(int y, int x)
        {
            HandleNeighbor(y, x, 0, 1, _current.Direction, ElevatorActions);
        }

        private void HandleBlock(int y, int x)
        {
            HandleNeighbor(y, x, 1, 0, InvertDirection(_current.Direction), BlockActions);
        }

        private void HandleMovement(int y, int x)
        {
            HandleNeighbor(y, x, 0, 0, _current.Direction, MovementActions);
        }

        private void HandleNeighbor(int y, int x, int blocksIncrement, int elevatorsIncrement, Direction direction, Action[] actions)
        {
            //Console.Error.WriteLine($"Handling neighbor: {y}, {x}");

            var neighbor = _field.GetOrDefault(y, x);
            if (neighbor == null)
            {
                //Console.Error.WriteLine($"Handling neighbor: {y}, {x} - Null");
                return;
            }

            var cost = _current.Cost + actions.Length;
            if (cost > _costLimit) return;

            var blocksUsed = _current.BlocksUsed + blocksIncrement;
            if (blocksUsed > _blocksLimit) return;

            var elevatorsUsed = _current.ElevatorsUsed + elevatorsIncrement;
            if (elevatorsUsed > _elevatorsLimit) return;

            if (_current.ParentPathInfo?.Point == neighbor)
            {
                //Console.Error.WriteLine($"Handling neighbor: {y}, {x} - In the path already");
                return;
            }

            var pathInfo = new PathInfo
                           {
                               Point = neighbor,
                               Cost = cost,
                               Heuristic = Heuristic(neighbor, _end),
                               BlocksUsed = blocksUsed,
                               ElevatorsUsed = elevatorsUsed,
                               ParentPathInfo = _current,
                               Direction = direction,
                               Actions = actions
                           };
            var i = 0;
            while (i < _openSet.Count && _openSet[i].Rank < pathInfo.Rank)
            {
                i += 1;
            }
            _openSet.Insert(i, pathInfo);
            //Console.Error.WriteLine($"Handling neighbor: {y}, {x} - Added to Open Set");
        }

        private static Direction InvertDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.Right: return Direction.Left;
                case Direction.Left: return Direction.Right;
                default: throw new InvalidEnumArgumentException();
            }
        }

        private static int Heuristic(Point from, Point to)
        {
            var dx = Math.Abs(from.Position - to.Position);
            var dy = Math.Abs(from.Floor - to.Floor);
            return dx + dy;
        }

        private class PathInfo
        {
            public Point Point { get; set; }

            public int Cost { get; set; }
            public int Heuristic { get; set; }
            public int Rank => Cost + Heuristic;

            public int BlocksUsed { get; set; }
            public int ElevatorsUsed { get; set; }

            public PathInfo ParentPathInfo { get; set; }
            public Direction Direction { get; set; }
            public Action[] Actions { get; set; }

            public IEnumerable<PathInfo> EnumeratePath()
            {
                yield return this;
                var parent = ParentPathInfo;
                while (parent != null)
                {
                    yield return parent;
                    parent = parent.ParentPathInfo;
                }
            }
        }
    }

    public enum Direction
    {
        None,
        Right,
        Left
    }

    public class Point
    {
        public int Floor { get; }
        public int Position { get; }

        public Feature Feature { get; set; }

        public Point(int floor, int position)
        {
            Floor = floor;
            Position = position;
        }
    }

    [Flags]
    public enum Feature : byte
    {
        None = 0x00,
        Generator = 0x01,
        Elevator = 0x02,
        Exit = 0x04,
        Blocked = 0x08
    }

    public enum Action : byte
    {
        None,
        Wait,
        Block,
        Elevator
    }

    public static class Tools
    {
        public static int[] ReadInts()
        {
            return ReadStrings().Select(int.Parse).ToArray();
        }

        public static string[] ReadStrings()
        {
            return Console.ReadLine().Split(' ');
        }

        public static T GetOrDefault<T>(this T[,] array, int y, int x)
        {
            return y < 0 || y >= array.GetLength(0) || x < 0 || x >= array.GetLength(1)
                ? default(T)
                : array[y, x];
        }
    }
}