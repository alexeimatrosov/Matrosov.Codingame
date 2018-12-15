using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Matrosov.Codingame.DontPanicLevel2
{
    public static class Program
    {
        public static void Main()
        {
            var game = new Game();
            Console.Error.WriteLine(game.ToString());

            var actions = game.FindPathActions();
            Console.WriteLine(string.Join("\n", actions.Select(x => x.ToGameAction())));

            while (true) Console.WriteLine("WAIT");
        }
    }

    public class Game
    {
        private static readonly Action[] ElevatorActions = {Action.Elevator, Action.Wait, Action.Wait, Action.Wait};
        private static readonly Action[] BlockActions = {Action.Block, Action.Wait, Action.Wait, Action.Wait};
        private static readonly Action[] MovementActions = {Action.Wait};

        private readonly Cell _generator;
        private readonly Cell _exit;
        private readonly Cell[,] _grid;

        private readonly int _roundsLimit;
        private readonly int _clonesLimit;
        private readonly int _elevatorsLimit;

        private List<PathNode> _traversalQueue;

        private PathNode _current;

        public Game()
        {
            var inputs = Tools.ReadInts();

            _grid = CreateGrid(inputs[0], inputs[1]);

            _roundsLimit = inputs[2];
            _clonesLimit = inputs[5] - 1;
            _elevatorsLimit = inputs[6];
            _exit = _grid[inputs[3], inputs[4]];
            _exit.Feature |= Feature.Exit;

            for (var i = 0; i < inputs[7]; i++)
            {
                var coordinates = Tools.ReadInts();
                _grid[coordinates[0], coordinates[1]].Feature |= Feature.Elevator;
            }

            var cloneInputs = Tools.ReadStrings();
            var cloneFloor = int.Parse(cloneInputs[0]); // floor of the leading clone
            var clonePosition = int.Parse(cloneInputs[1]); // position of the leading clone on its floor
            //var cloneDirection = cloneInputs[2] == "RIGHT" ? Direction.Right : Direction.Left; // direction of the leading clone: LEFT or RIGHT

            _generator = _grid[cloneFloor, clonePosition];
            _generator.Feature |= Feature.Generator;
        }

        private static Cell[,] CreateGrid(int height, int width)
        {
            var field = new Cell[height, width];
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    field[i, j] = new Cell(i, j);
                }
            }

            return field;
        }

        public override string ToString()
        {
            return $"Rounds limit    : {_roundsLimit,3}\n" +
                   $"Clones limit    : {_clonesLimit,3}\n" +
                   $"Elevators limit : {_elevatorsLimit,3}\n";
        }

        public Action[] FindPathActions()
        {
            _current = new PathNode
                       {
                           Cell = _generator,
                           Parent = null,
                           Cost = 1,
                           Rank = 1 + Heuristic(_generator, _exit),
                           ClonesUsed = 0,
                           ElevatorsUsed = 0,
                           Direction = Direction.Right,
                           Actions = Array.Empty<Action>()
                       };
            _traversalQueue = new List<PathNode>();

            while (_current.Cell != _exit)
            {
                if (_current.Cell.Feature.HasFlag(Feature.Elevator))
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

        private void HandleMovementUp() => HandleMovement(_current.Cell.Floor + 1, _current.Cell.Position);

        private void HandleMovementLateral() => HandleMovement(_current.Cell.Floor, _current.Cell.Position + _current.Direction.ToPositionIncrement());

        private void HandleMovement(int y, int x) => HandleNeighbor(y, x, 0, 0, _current.Direction, MovementActions);

        private void HandleBlock() => HandleNeighbor(_current.Cell.Floor, _current.Cell.Position - _current.Direction.ToPositionIncrement(), 1, 0, _current.Direction.Invert(), BlockActions);

        private void HandleElevator()
        {
            if (ShouldTryNewElevator() == false) return;

            HandleNeighbor(_current.Cell.Floor + 1, _current.Cell.Position, 1, 1, _current.Direction, ElevatorActions);
        }

        private bool ShouldTryNewElevator()
        {
            var elevatorRemaining = _elevatorsLimit - _current.ElevatorsUsed;
            for (var i = 1; i <= elevatorRemaining; i++)
            {
                var cell = _grid.GetOrDefault(_current.Cell.Floor + i, _current.Cell.Position);
                if (cell == null) return false;
                if (cell.Feature.HasFlag(Feature.Elevator) || cell.Feature.HasFlag(Feature.Exit)) return true;
            }

            return false;
        }

        private void HandleNeighbor(int y, int x, int clonesUsed, int elevatorsUsed, Direction direction, Action[] actions)
        {
            var neighbor = _grid.GetOrDefault(y, x);
            if (neighbor == null) return;

            var cost = _current.Cost + actions.Length;
            var rank = cost + Heuristic(neighbor, _exit);
            if (rank > _roundsLimit) return;

            var clonesTotalUsed = _current.ClonesUsed + clonesUsed;
            if (clonesTotalUsed > _clonesLimit) return;

            var elevatorsTotalUsed = _current.ElevatorsUsed + elevatorsUsed;
            if (elevatorsTotalUsed > _elevatorsLimit) return;

            if (_current.Parent?.Cell == neighbor) return;

            AddToTraversalQueue(new PathNode
                                {
                                    Cell = neighbor,
                                    Cost = cost,
                                    Rank = rank,
                                    ClonesUsed = clonesTotalUsed,
                                    ElevatorsUsed = elevatorsTotalUsed,
                                    Parent = _current,
                                    Direction = direction,
                                    Actions = actions
                                });
        }

        private void AddToTraversalQueue(PathNode pathNode)
        {
            var i = 0;
            while (i < _traversalQueue.Count && _traversalQueue[i].Rank < pathNode.Rank)
            {
                i += 1;
            }

            _traversalQueue.Insert(i, pathNode);
        }

        private static int Heuristic(Cell from, Cell to)
        {
            var dx = Math.Abs(from.Position - to.Position);
            var dy = Math.Abs(from.Floor - to.Floor);
            return dx + dy;
        }

        private class PathNode
        {
            public Cell Cell { get; set; }
            public PathNode Parent { get; set; }

            public int Cost { get; set; }
            public int Rank { get; set; }

            public int ClonesUsed { get; set; }
            public int ElevatorsUsed { get; set; }

            public Direction Direction { get; set; }
            public Action[] Actions { get; set; }

            public IEnumerable<PathNode> EnumeratePath()
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

    public class Cell
    {
        public int Floor { get; }
        public int Position { get; }
        public Feature Feature { get; set; }

        public Cell(int floor, int position)
        {
            Floor = floor;
            Position = position;
        }
    }

    public enum Direction : byte
    {
        Right = 1,
        Left
    }

    [Flags]
    public enum Feature : byte
    {
        None = 0x00,
        Generator = 0x01,
        Elevator = 0x02,
        Exit = 0x04,
    }

    public enum Action : byte
    {
        Wait = 1,
        Block,
        Elevator
    }

    public static class Tools
    {
        public static int[] ReadInts() => ReadStrings().Select(int.Parse).ToArray();

        public static string[] ReadStrings() => Console.ReadLine().Split(' ');

        public static T GetOrDefault<T>(this T[,] array, int y, int x)
        {
            return y >= 0 && y < array.GetLength(0) && x >= 0 && x < array.GetLength(1)
                ? array[y, x]
                : default(T);
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

        public static int ToPositionIncrement(this Direction direction)
        {
            return direction == Direction.Right ? 1 : -1;
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