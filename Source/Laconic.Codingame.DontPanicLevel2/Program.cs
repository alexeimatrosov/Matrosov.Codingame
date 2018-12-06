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

            var i = 0;
            var action = Action.None;
            Queue<Action> actionsQueue = null;
            while (true)
            {
                var inputs = Tools.ReadStrings();
                var cloneFloor = int.Parse(inputs[0]); // floor of the leading clone
                var clonePosition = int.Parse(inputs[1]); // position of the leading clone on its floor
                //String direction = inputs[2]; // direction of the leading clone: LEFT or RIGHT

                if (i == 0)
                {
                    game.SetGenerator(cloneFloor, clonePosition);
                    var actions = game.FindPathActions();
                    foreach (var a in actions)
                    {
                        Console.Error.WriteLine(a);
                    }
                    actionsQueue = new Queue<Action>(actions);
                }

                game.Round(action);

                Console.Error.WriteLine(game.ToString());

                //action = i == 2 ? Action.Elevator : Action.Wait;
                //action = i == 1 ? Action.Elevator : action;
                //action = i == 3 ? Action.Elevator : action;
                //action = i == 5 ? Action.Block : action;

                action = actionsQueue.Dequeue();

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

        private int _round;
        private int _roundsLimit;

        private int _clonesGenerated;
        private int _clonesLimit;

        private int _additionalElevatorsUsed;
        private int _additionalElevatorsLimit;

        private readonly List<Clone> _clones = new List<Clone>();

        private Game()
        {
        }

        public void Round(Action action)
        {
            _round++;

            //PlayAction(action);
            //MoveClones();
            //GenerateClone();
        }

        //private void PlayAction(Action action)
        //{
        //    var leadingClone = _clones.FirstOrDefault();
        //    if (leadingClone == null) return;

        //    if (action == Action.Elevator)
        //    {
        //        if (_additionalElevatorsUsed == _additionalElevatorsLimit) throw new GameOverException("You try to build an elevator and there are no more elevators.");

        //        _additionalElevatorsUsed++;
        //        leadingClone.Location.Feature |= Feature.Elevator;
        //        RemoveClone(leadingClone);
        //    }
        //    else if (action == Action.Block)
        //    {
        //        leadingClone.Location.Feature |= Feature.Blocked;
        //        RemoveClone(leadingClone);
        //    }
        //}

        //private void MoveClones()
        //{
        //    foreach (var clone in _clones.ToArray())
        //    {
        //        MoveClone(clone);
        //    }
        //}

        //private void MoveClone(Clone clone)
        //{
        //    if (clone.Location.Feature.HasFlag(Feature.Elevator))
        //    {
        //        var newLocation = _field.GetOrDefault(clone.Location.Floor + 1, clone.Location.Position);
        //        if (newLocation == null)
        //        {
        //            RemoveClone(clone);
        //            return;
        //        }

        //        clone.Location = newLocation;
        //        InvertDirectionIfBlocked(clone);
        //    }
        //    else
        //    {
        //        var newLocation = _field.GetOrDefault(clone.Location.Floor, clone.Location.Position + (clone.Direction == Direction.Right ? 1 : -1));
        //        if (newLocation == null)
        //        {
        //            RemoveClone(clone);
        //            return;
        //        }

        //        if (newLocation.Feature.HasFlag(Feature.Blocked))
        //        {
        //            InvertDirectionIfBlocked(clone, newLocation);
        //            newLocation = _field.GetOrDefault(clone.Location.Floor, clone.Location.Position + (clone.Direction == Direction.Right ? 1 : -1));
        //        }

        //        if (newLocation == null)
        //        {
        //            RemoveClone(clone);
        //            return;
        //        }

        //        if (newLocation.Feature.HasFlag(Feature.Blocked))
        //        {
        //            InvertDirectionIfBlocked(clone, newLocation);
        //        }
        //        else
        //        {
        //            clone.Location = newLocation;
        //        }
        //    }
        //}

        //private void GenerateClone()
        //{
        //    if (_round % 3 != 1 || _clonesGenerated == _clonesLimit) return;

        //    var clone = new Clone
        //                {
        //                    Direction = Direction.Right,
        //                    Location = _generator
        //                };
        //    InvertDirectionIfBlocked(clone);
        //    _clones.Add(clone);

        //    _clonesGenerated++;

        //    if (_clones.Count == 0 && _clonesGenerated == _clonesLimit) throw new GameOverException("No more clones");
        //}

        //private void RemoveClone(Clone clone)
        //{
        //    _clones.Remove(clone);
        //}

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
            var sb = new StringBuilder();

            sb.AppendLine($"Round            : {_round,3} / {_roundsLimit,3}");
            sb.AppendLine($"Clones generated : {_clonesGenerated,3} / {_clonesLimit,3}");
            sb.AppendLine($"Elevators added  : {_additionalElevatorsUsed,3} / {_additionalElevatorsLimit,3}");

            var fieldChars = _field.Map(PointToChar);

            foreach (var clone in _clones)
            {
                fieldChars[clone.Location.Floor, clone.Location.Position] = clone.Direction == Direction.Right ? '>' : '<';
            }

            for (var i = fieldChars.GetLength(0) - 1; i >= 0; i--)
            {
                for (var j = 0; j < fieldChars.GetLength(1); j++)
                {
                    sb.Append(fieldChars[i, j]);
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static char PointToChar(Point point)
        {
            if (point.Feature.HasFlag(Feature.Blocked)) return '!';
            if (point.Feature.HasFlag(Feature.Exit)) return 'E';
            if (point.Feature.HasFlag(Feature.Elevator)) return 'I';
            if (point.Feature.HasFlag(Feature.Generator)) return 'G';
            return '.';
        }

        //private static void InvertDirectionIfBlocked(Clone clone)
        //{
        //    InvertDirectionIfBlocked(clone, clone.Location);
        //}

        //private static void InvertDirectionIfBlocked(Clone clone, Point point)
        //{
        //    if (point.Feature.HasFlag(Feature.Blocked) == false) return;

        //    clone.Direction = InvertDirection(clone.Direction);
        //}
    }

    public class PathFinding
    {
        private readonly Point[,] _field;
        private readonly Point _start;
        private readonly Point _end;
        private readonly int _costLimit;
        private readonly int _blocksLimit;
        private readonly int _elevatorsLimit;

        private Dictionary<Point, PathInfo> _openSet;
        private Dictionary<Point, PathInfo> _closedSet;
        private Point _current;
        private PathInfo _currentPathInfo;

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
            _current = _start;
            _currentPathInfo = new PathInfo
                               {
                                   Cost = 1,
                                   Heuristic = Heuristic(_start, _end),
                                   BlocksUsed = 0,
                                   ElevatorsUsed = 0,
                                   ParentPoint = null,
                                   Direction = Direction.Right,
                                   Actions = new[] {Action.Wait}
                               };

            _openSet = new Dictionary<Point, PathInfo>
                       {
                           {_current, _currentPathInfo}
                       };
            _closedSet = new Dictionary<Point, PathInfo>();

            while (_current != _end)
            {
                _openSet.Remove(_current);
                _closedSet.Add(_current, _currentPathInfo);

                var positionIncrement = _currentPathInfo.Direction == Direction.Right ? 1 : -1;

                if (_current.Feature.HasFlag(Feature.Elevator))
                {
                    HandleMovement(_current.Floor + 1, _current.Position);
                }
                else
                {
                    HandleElevator(_current.Floor + 1, _current.Position);
                    HandleMovement(_current.Floor, _current.Position + positionIncrement);
                }

                HandleBlock(_current.Floor, _current.Position - positionIncrement);

                var p = _openSet.OrderBy(x => x.Value.Rank).First();
                _current = p.Key;
                _currentPathInfo = p.Value;
            }

            return TraversePathActions();
        }

        private Action[] TraversePathActions()
        {
            var result = new List<Action>
                         {
                             Action.Wait,
                         };
            var parent = _current;
            var parentPathInfo = _currentPathInfo;
            while (parent != _start)
            {
                result.AddRange(parentPathInfo.Actions.Reverse());
                parent = parentPathInfo.ParentPoint;
                parentPathInfo = _closedSet[parent];
            }

            result.Reverse();
            return result.ToArray();
        }

        private void HandleElevator(int y, int x)
        {
            HandleNeighbor(y, x, 1000000, 0, 1, _currentPathInfo.Direction, new[] {Action.Elevator, Action.Wait, Action.Wait, Action.Wait});
        }

        private void HandleBlock(int y, int x)
        {
            HandleNeighbor(y, x, 1000, 1, 0, InvertDirection(_currentPathInfo.Direction), new[] {Action.Block, Action.Wait, Action.Wait, Action.Wait});
        }

        private void HandleMovement(int y, int x)
        {
            HandleNeighbor(y, x, 1, 0, 0, _currentPathInfo.Direction, new[] {Action.Wait});
        }

        private void HandleNeighbor(int y, int x, int costIncrement, int blocksIncrement, int elevatorsIncrement, Direction direction, Action[] actions)
        {
            var neighbor = _field.GetOrDefault(y, x);
            if (neighbor == null) return;

            var cost = _currentPathInfo.Cost + costIncrement;
            var blocksUsed = _currentPathInfo.BlocksUsed + blocksIncrement;
            var elevatorsUsed = _currentPathInfo.ElevatorsUsed + elevatorsIncrement;

            //if (cost > _costLimit || blocksUsed > _blocksLimit || elevatorsUsed > _elevatorsLimit)
            //{
            //    //var parent = _current;
            //    //var parentPathInfo = _currentPathInfo;
            //    //while (parent != _start)
            //    //{
            //    //    parentPathInfo.Cost = 2000000000;
            //    //    parent = parentPathInfo.ParentPoint;
            //    //    if (_closedSet.TryGetValue(parent, out var pathInfo))
            //    //    {
            //    //        parentPathInfo = pathInfo;
            //    //    }
            //    //    else
            //    //    {
            //    //        parentPathInfo = _openSet[parent];
            //    //    }
            //    //}
            //    return;
            //}

            if (_openSet.TryGetValue(neighbor, out var openPathInfo) && (/*elevatorsUsed < openPathInfo.ElevatorsUsed || blocksUsed < openPathInfo.BlocksUsed ||*/ cost < openPathInfo.Cost))
            {
                _openSet.Remove(neighbor);
            }

            if (_closedSet.TryGetValue(neighbor, out var closedPathInfo) && (/*elevatorsUsed < closedPathInfo.ElevatorsUsed || blocksUsed < closedPathInfo.BlocksUsed ||*/ cost < closedPathInfo.Cost))
            {
                _closedSet.Remove(neighbor);
            }

            if (_openSet.ContainsKey(neighbor) == false && _closedSet.ContainsKey(neighbor) == false)
            {
                _openSet.Add(neighbor, new PathInfo
                                       {
                                           Cost = cost,
                                           Heuristic = Heuristic(neighbor, _end),
                                           BlocksUsed = blocksUsed,
                                           ElevatorsUsed = elevatorsUsed,
                                           ParentPoint = _current,
                                           Direction = direction,
                                           Actions = actions
                                       });
            }
        }

        private static int Heuristic(Point from, Point to)
        {
            var dx = Math.Abs(from.Position - to.Position);
            var dy = Math.Abs(from.Floor - to.Floor);
            return dx + dy;
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

        private class PathInfo
        {
            public int Cost { get; set; }
            public int Heuristic { get; set; }
            public int Rank => Heuristic + Cost;

            public int BlocksUsed { get; set; }
            public int ElevatorsUsed { get; set; }

            public Point ParentPoint { get; set; }
            public Direction Direction { get; set; }
            public Action[] Actions { get; set; }
        }
    }

    public class Clone
    {
        public Direction Direction { get; set; }

        public Point Location { get; set; }
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

        public static T2[,] Map<T1, T2>(this T1[,] array, Func<T1, T2> mapping)
        {
            var result = new T2[array.GetLength(0), array.GetLength(1)];

            for (var i = 0; i < result.GetLength(0); i++)
            {
                for (var j = 0; j < result.GetLength(1); j++)
                {
                    result[i, j] = mapping(array[i, j]);
                }
            }

            return result;
        }
    }
}