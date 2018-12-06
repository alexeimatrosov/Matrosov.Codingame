using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Laconic.Codingame.DontPanicLevel2
{
    public static class Program
    {
        public static void Main()
        {
            var game = Game.Read();
            Console.Error.WriteLine(game.ToString());

            var i = 0;
            var actionsQueue = new Queue<Action>();
            while (true)
            {
                var inputs = Tools.ReadStrings();
                var cloneFloor = int.Parse(inputs[0]); // floor of the leading clone
                var clonePosition = int.Parse(inputs[1]); // position of the leading clone on its floor
                //var cloneDirection = inputs[2] == "RIGHT" ? Direction.Right : Direction.Left; // direction of the leading clone: LEFT or RIGHT

                if (i == 0)
                {
                    game.SetGenerator(cloneFloor, clonePosition);

                    foreach (var a in game.FindPathActions())
                    {
                        actionsQueue.Enqueue(a);
                    }
                }

                var action = actionsQueue.Count > 0 ? actionsQueue.Dequeue() : Action.Wait;

                Console.WriteLine(action.ToGameAction());
                i++;
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

        public Action[] FindPathActions()
        {
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
            return $"Rounds limit    : {_roundsLimit,3}\n" +
                   $"Clones limit    : {_clonesLimit,3}\n" +
                   $"Elevators limit : {_additionalElevatorsLimit,3}\n";
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

        private List<PathInfo> _traversalQueue;

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
                           Point = _start,
                           Parent = null,
                           Cost = 1,
                           Heuristic = Heuristic(_start, _end),
                           BlocksUsed = 0,
                           ElevatorsUsed = 0,
                           Direction = Direction.Right,
                           Actions = Array.Empty<Action>()
                       };
            _traversalQueue = new List<PathInfo>();

            while (_current.Point != _end)
            {
                if (_current.Point.Feature.HasFlag(Feature.Elevator))
                {
                    HandleMovementUp();
                }
                else
                {
                    HandleMovementLateral();
                    HandleElevator();
                }

                HandleBlock();

                _current = _traversalQueue[0];
                _traversalQueue.RemoveAt(0);
            }

            return _current.EnumeratePath().SelectMany(x => x.Actions.Reverse()).Reverse().ToArray();
        }

        private void HandleMovementUp()
        {
            HandleMovement(_current.Point.Floor + 1, _current.Point.Position);
        }

        private void HandleMovementLateral()
        {
            var positionIncrement = _current.Direction == Direction.Right ? 1 : -1;
            HandleMovement(_current.Point.Floor, _current.Point.Position + positionIncrement);
        }

        private void HandleMovement(int y, int x)
        {
            HandleNeighbor(y, x, 0, 0, _current.Direction, MovementActions);
        }

        private void HandleBlock()
        {
            var positionIncrement = _current.Direction == Direction.Right ? -1 : 1;
            HandleNeighbor(_current.Point.Floor, _current.Point.Position + positionIncrement, 1, 0, _current.Direction.Invert(), BlockActions);
        }

        private void HandleElevator()
        {
            if (ShouldTryNewElevator() == false) return;

            HandleNeighbor(_current.Point.Floor + 1, _current.Point.Position, 0, 1, _current.Direction, ElevatorActions);
        }

        private bool ShouldTryNewElevator()
        {
            var elevatorRemaining = _elevatorsLimit - _current.ElevatorsUsed;
            for (var i = 1; i <= elevatorRemaining; i++)
            {
                var point = _field.GetOrDefault(_current.Point.Floor + i, _current.Point.Position);
                if (point == null) return false;
                if (point.Feature.HasFlag(Feature.Elevator) || point.Feature.HasFlag(Feature.Exit)) return true;
            }
            return false;
        }

        private void HandleNeighbor(int y, int x, int blocksIncrement, int elevatorsIncrement, Direction direction, Action[] actions)
        {
            var neighbor = _field.GetOrDefault(y, x);
            if (neighbor == null) return;

            var cost = _current.Cost + actions.Length;
            if (cost > _costLimit) return;

            var blocksUsed = _current.BlocksUsed + blocksIncrement;
            if (blocksUsed > _blocksLimit) return;

            var elevatorsUsed = _current.ElevatorsUsed + elevatorsIncrement;
            if (elevatorsUsed > _elevatorsLimit) return;

            if (_current.Parent?.Point == neighbor) return;

            AddToTraversalQueue(new PathInfo
                         {
                             Point = neighbor,
                             Cost = cost,
                             Heuristic = Heuristic(neighbor, _end),
                             BlocksUsed = blocksUsed,
                             ElevatorsUsed = elevatorsUsed,
                             Parent = _current,
                             Direction = direction,
                             Actions = actions
                         });
        }

        private void AddToTraversalQueue(PathInfo pathInfo)
        {
            var i = 0;
            while (i < _traversalQueue.Count && _traversalQueue[i].Rank < pathInfo.Rank)
            {
                i += 1;
            }

            _traversalQueue.Insert(i, pathInfo);
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
            public PathInfo Parent { get; set; }

            public int Cost { get; set; }
            public int Heuristic { get; set; }
            public int Rank => Cost + Heuristic;

            public int BlocksUsed { get; set; }
            public int ElevatorsUsed { get; set; }

            public Direction Direction { get; set; }
            public Action[] Actions { get; set; }

            public IEnumerable<PathInfo> EnumeratePath()
            {
                yield return this;
                var parent = Parent;
                while (parent != null)
                {
                    yield return parent;
                    parent = parent.Parent;
                }
            }
        }
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

        public override string ToString()
        {
            return $"({Floor},{Position})";
        }
    }

    public enum Direction : byte
    {
        None,
        Right,
        Left
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

        public static Direction Invert(this Direction direction)
        {
            switch (direction)
            {
                case Direction.Right: return Direction.Left;
                case Direction.Left: return Direction.Right;
                default: throw new InvalidEnumArgumentException();
            }
        }

        public static string ToGameAction(this Action action)
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
}