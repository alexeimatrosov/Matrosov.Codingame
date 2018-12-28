using System;
using System.Collections.Generic;
using System.Linq;

namespace Matrosov.Codingame.CSharp.Contests.GameOfDrones
{
    public class Program
    {
        static void Main()
        {
            var game = Game.Initialize();
            var controller = new GameController(game);

            while (true)
            {
                game.ReadRound();
                var points = controller.EvaluateRound();

                WriteRoundOutput(points);
            }
        }

        static void WriteRoundOutput(IEnumerable<Point> points)
        {
            foreach (var point in points)
            {
                Console.WriteLine("{0} {1}", point.X, point.Y); // output a destination point to be reached by one of your drones. The first line corresponds to the first of your drones that you were provided as input, the next to the second, etc.
            }
        }

        private class GameController
        {
            private readonly Game _game;
            private GameState _gameState;
            private readonly Drone[] _enemyDrones;
            private readonly Player _myPlayer;
            private readonly Drone[] _myDrones;
            private readonly DroneController[] _myDroneControllers;

            public GameController(Game game)
            {
                _game = game;
                _gameState = GameState.Started;
                _enemyDrones = _game.Players.Where(x => x.Id != _game.PlayerId).SelectMany(x => x.Drones).ToArray();
                _myPlayer = _game.Players.First(x => x.Id == _game.PlayerId);
                _myDrones = _myPlayer.Drones;
                _myDroneControllers = _myDrones.Select(x => new DroneController(x)).ToArray();
            }

            public IEnumerable<Point> EvaluateRound()
            {
                if (_gameState == GameState.Started)
                {
                    foreach (var zone in _game.Zones)
                    {
                        var droneController = _myDroneControllers.Where(x => x.State == DroneState.Idle).OrderBy(x => x.Drone.Point.DistanceTo(zone.Point)).FirstOrDefault();
                        if (droneController == null) continue;

                        droneController.SendToZone(zone);
                    }

                    foreach (var droneController in _myDroneControllers.Where(x => x.State == DroneState.Idle))
                    {
                        var zone = _game.Zones.OrderBy(x => x.Point.DistanceTo(droneController.Drone.Point)).First();
                        droneController.SendToZone(zone);
                    }

                    _gameState = GameState.Ongoing;
                }

                return _myDroneControllers.Select(x => x.CommandPoint);
            }
        }

        private enum GameState
        {
            None,
            Started,
            Ongoing
        }

        private class DroneController
        {
            public Drone Drone { get; private set; }
            public DroneState State { get; private set; }
            public Point CommandPoint { get; private set; }

            public DroneController(Drone drone)
            {
                Drone = drone;
                State = DroneState.Idle;
                CommandPoint = Drone.Point;
            }

            public void SendToZone(Zone zone)
            {
                State = DroneState.GoingToZone;
                CommandPoint = zone.Point;
            }
        }

        private enum DroneState
        {
            None,
            Idle,
            GoingToZone,
            Defending,
        }

        private class Game
        {
            public int NumberOfPlayers { get; private set; }
            public int PlayerId { get; private set; }
            public int NumberOfZones { get; private set; }
            public int NumberOfDrones { get; private set; }

            public Zone[] Zones { get; private set; }
            public Player[] Players { get; private set; }

            public static Game Initialize()
            {
                var inputs = Console.ReadLine().Split(' ');
                var game = new Game
                           {
                               NumberOfPlayers = int.Parse(inputs[0]), // number of players in the game (2 to 4 players)
                               PlayerId = int.Parse(inputs[1]), // ID of your player (0, 1, 2, or 3)
                               NumberOfDrones = int.Parse(inputs[2]), // number of drones in each team (3 to 11)
                               NumberOfZones = int.Parse(inputs[3]), // number of zones on the map (4 to 8)
                           };


                var zones = new Zone[game.NumberOfZones];
                for (var i = 0; i < zones.Length; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    var x = int.Parse(inputs[0]);
                    var y = int.Parse(inputs[1]);

                    zones[i] = new Zone
                                {
                                    Point = new Point(x, y),
                                    ControlPlayerId = -1,
                                };
                }
                game.Zones = zones;

                var players = new Player[game.NumberOfPlayers];
                for (var i = 0; i < players.Length; i++)
                {
                    var drones = new Drone[game.NumberOfDrones];
                    for (var j = 0; j < drones.Length; j++)
                    {
                        drones[j] = new Drone
                                    {
                                        Id = i*game.NumberOfDrones + j,
                                        PlayerId = i,
                                        Point = new Point(),
                                    };
                    }
                    players[i] = new Player
                                 {
                                     Id = i,
                                     Drones = drones
                                 };
                }
                game.Players = players;

                return game;
            }

            public void ReadRound()
            {
                for (var i = 0; i < NumberOfZones; i++)
                {
                    Zones[i].ControlPlayerId = int.Parse(Console.ReadLine());
                }

                for (var i = 0; i < NumberOfPlayers; i++)
                {
                    for (var j = 0; j < NumberOfDrones; j++)
                    {
                        var inputs = Console.ReadLine().Split(' ');
                        Players[i].Drones[j].Point = new Point(int.Parse(inputs[0]), int.Parse(inputs[1]));
                    }
                }
            }
        }

        private class Player
        {
            public int Id { get; set; }
            public Drone[] Drones { get; set; }
        }

        private class Drone
        {
            public int Id { get; set; }
            public int PlayerId { get; set; }
            public Point Point { get; set; }
        }

        private class Zone
        {
            public Point Point { get; set; }
            public int ControlPlayerId { get; set; }
        }

        private struct Point
        {
            public int X { get; private set; }
            public int Y { get; private set; }

            public Point(int x, int y) : this()
            {
                X = x;
                Y = y;
            }

            public double DistanceTo(Point point)
            {
                return Math.Sqrt(Math.Pow(X - point.X, 2) + Math.Pow(Y - point.Y, 2));
            }
        }
    }
}