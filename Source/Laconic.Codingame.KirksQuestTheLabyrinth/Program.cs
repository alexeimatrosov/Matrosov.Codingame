using System;
using System.Collections.Generic;
using System.Linq;

namespace Laconic.Codingame.KirksQuestTheLabyrinth
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var game = Game.ReadInput();

            while (true)
            {
                var direction = game.GetRoundDirection();

                Console.WriteLine(direction.ToString().ToUpper());
            }
        }
    }

    public class Game
    {
        private readonly int _alarmCountdown;
        private readonly Maze _maze;
        private GameState _state = GameState.Exploration;

        private Game(int rowsCount, int columnsCount, int alarmCountdown)
        {
            _alarmCountdown = alarmCountdown;
            _maze = new Maze(rowsCount, columnsCount);
        }

        public static Game ReadInput()
        {
            var inputs = Console.ReadLine().Split(' ');
            var rowsCount = int.Parse(inputs[0]);
            var columnsCount = int.Parse(inputs[1]);
            var alarmCountdown = int.Parse(inputs[2]);

            return new Game(rowsCount, columnsCount, alarmCountdown);
        }

        public Step GetRoundDirection()
        {
            var kirkPoint = ReadRound();

            if (_state == GameState.Exploration && _maze.IsControlRoomLocated && _maze.FindPathTo(Cell.ControlRoom, kirkPoint).Count > 0 && _maze.FindPathTo(Cell.StartingPosition, _maze.ControlRoomPoint).Count <= _alarmCountdown)
            {
                _state = GameState.GoingToControlRoom;
            }
            else if (_state == GameState.GoingToControlRoom && _maze.IsOn(Cell.ControlRoom, kirkPoint))
            {
                _state = GameState.Escape;
            }

            if (_state == GameState.Exploration)
            {
                return _maze.FindExplorationPath(kirkPoint).Dequeue();
            }

            if (_state == GameState.GoingToControlRoom)
            {
                return _maze.FindPathTo(Cell.ControlRoom, kirkPoint).Dequeue();
            }

            if (_state == GameState.Escape)
            {
                return _maze.FindPathTo(Cell.StartingPosition, kirkPoint).Dequeue();
            }

            return Step.None;
        }

        private Point ReadRound()
        {
            var inputs = Console.ReadLine().Split(' ');
            var kirksY = int.Parse(inputs[0]);
            var kirksX = int.Parse(inputs[1]);

            _maze.ReadRound();

            return new Point(kirksX, kirksY);
        }
    }

    public enum GameState
    {
        None,
        Exploration,
        GoingToControlRoom,
        Escape,
    }

    public class Maze
    {
        private readonly Dictionary<char, Cell> _charToCell
            = new Dictionary<char, Cell>
                  {
                      {'?', Cell.Unknown},
                      {'.', Cell.Space},
                      {'#', Cell.Wall},
                      {'T', Cell.StartingPosition},
                      {'C', Cell.ControlRoom},
                  };

        private readonly Cell[] _explorationPathViaCells = {Cell.Space, Cell.StartingPosition, Cell.Unknown};
        private readonly Cell[] _exploredPathViaCells = {Cell.Space, Cell.StartingPosition, Cell.ControlRoom};

        private readonly int _rowsCount;
        private readonly int _columnsCount;
        private readonly Cell[,] _cells;
        private readonly Color[,] _colors;
        private readonly Point[,] _parents;
        private readonly Queue<Point> _pointsQueue;

        public bool IsControlRoomLocated { get; private set; }
        public Point ControlRoomPoint { get; private set; }

        public Maze(int rowsCount, int columnsCount)
        {
            _rowsCount = rowsCount;
            _columnsCount = columnsCount;
            _cells = new Cell[_rowsCount, _columnsCount];
            _colors = new Color[_rowsCount, _columnsCount];
            _parents = new Point[_rowsCount, _columnsCount];
            _pointsQueue = new Queue<Point>();

            IsControlRoomLocated = false;
            ControlRoomPoint = new Point();
        }

        public void ReadRound()
        {
            for (var i = 0; i < _rowsCount; i++)
            {
                var row = Console.ReadLine();
                for (var j = 0; j < _columnsCount; j++)
                {
                    var cell = _charToCell[row[j]];
                    if (cell == Cell.ControlRoom)
                    {
                        IsControlRoomLocated = true;
                        ControlRoomPoint = new Point(j, i);
                    }
                    _cells[i, j] = cell;
                }
            }
        }

        public bool IsOn(Cell cell, Point point)
        {
            return _cells[point.Y, point.X] == cell;
        }

        public Path FindExplorationPath(Point fromPoint)
        {
            var path = FindPathViaTo(_explorationPathViaCells, Cell.Unknown, fromPoint);
            path.TrimLast();

            return path;
        }

        public Path FindPathTo(Cell cell, Point fromPoint)
        {
            return FindPathViaTo(_exploredPathViaCells, cell, fromPoint);
        }

        private Path FindPathViaTo(Cell[] viaCells, Cell cell, Point fromPoint)
        {
            var toPoint = RunPathFinding(viaCells, cell, fromPoint);
            var pathPoints = GetPathPoints(fromPoint, toPoint);

            return new Path(pathPoints);
        }

        private Point RunPathFinding(Cell[] viaCells, Cell cell, Point fromPoint)
        {
            ResetPathFinding();

            _pointsQueue.Enqueue(fromPoint);
            _colors[fromPoint.Y, fromPoint.X] = Color.Grey;
            _parents[fromPoint.Y, fromPoint.X] = new Point();

            while (_pointsQueue.Count > 0)
            {
                var point = _pointsQueue.Dequeue();
                var x = point.X;
                var y = point.Y;

                if (_cells[y, x] == cell)
                {
                    return point;
                }

                EnqueuePoint(viaCells, new Point(x, y - 1), point);
                EnqueuePoint(viaCells, new Point(x + 1, y), point);
                EnqueuePoint(viaCells, new Point(x, y + 1), point);
                EnqueuePoint(viaCells, new Point(x - 1, y), point);

                _colors[y, x] = Color.Black;
            }

            return fromPoint;
        }

        private void ResetPathFinding()
        {
            _pointsQueue.Clear();

            for (var i = 0; i < _rowsCount; i++)
            {
                for (var j = 0; j < _columnsCount; j++)
                {
                    _colors[i, j] = Color.White;
                    _parents[i, j] = new Point();
                }
            }
        }

        private void EnqueuePoint(Cell[] viaCells, Point point, Point parent)
        {
            var x = point.X;
            var y = point.Y;

            if (x < 0 || x >= _columnsCount || y < 0 || y >= _rowsCount) return;

            if (_colors[y, x] == Color.White && viaCells.Contains(_cells[y, x]))
            {
                _pointsQueue.Enqueue(point);
                _colors[y, x] = Color.Grey;
                _parents[y, x] = parent;
            }
        }

        private IList<Point> GetPathPoints(Point @from, Point to)
        {
            var result = new List<Point>();

            var parent = to;
            while (parent.Equals(@from) == false)
            {
                result.Add(parent);
                parent = _parents[parent.Y, parent.X];
            }
            result.Add(@from);
            result.Reverse();

            return result;
        }
    }

    public struct Point
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        public Point(int x, int y)
            : this()
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return string.Format("[{0},{1}]", X, Y);
        }
    }

    public class Path : List<Step>
    {
        public Path(IList<Point> pathPoints)
        {
            var previousPoint = pathPoints[0];
            for (var i = 1; i < pathPoints.Count; i++)
            {
                var nextPoint = pathPoints[i];
                Add(GetStepBetween(previousPoint, nextPoint));
                previousPoint = nextPoint;
            }
        }

        private static Step GetStepBetween(Point a, Point b)
        {
            var xDiff = b.X - a.X;
            if (xDiff == 1) return Step.Right;
            if (xDiff == -1) return Step.Left;

            var yDiff = b.Y - a.Y;
            if (yDiff == 1) return Step.Down;
            if (yDiff == -1) return Step.Up;

            return Step.None;
        }

        public Step Dequeue()
        {
            var step = this[0];
            RemoveAt(0);

            return step;
        }

        public void TrimLast()
        {
            RemoveAt(Count-1);
        }
    }

    public enum Color
    {
        None,
        White,
        Grey,
        Black,
    }

    public enum Cell
    {
        None,
        Unknown,
        Space,
        Wall,
        StartingPosition,
        ControlRoom,
    }

    public enum Step
    {
        None,
        Up,
        Right,
        Down,
        Left
    }
}