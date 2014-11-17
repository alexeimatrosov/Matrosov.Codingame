using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Laconic.Codingame.PlatinumRift
{
    public class Program
    {
        public static void Main()
        {
            var game = Game.Read();
            var gameController = new GameController(game);

            while (true)
            {
                game.ReadRound();
                var output = gameController.CalculateRound();

                Console.Error.WriteLine("ContinentsCount:{0} PlatinumSource:{1}", game.Continents.Length, game.PlatinumSource);
                foreach (var continent in game.Continents)
                {
                    Console.Error.WriteLine("Length:{0} IsNeutral:{1} IsOwned:{2} PlatinumSource:{3}", continent.Zones.Length, continent.IsNeutral, continent.IsOwned, continent.PlatinumSource);
                }

                Console.WriteLine(MovementsToCommands(output.Movements));
                Console.WriteLine(PurchasesToCommands(output.Purchases));
            }
        }

        private static string MovementsToCommands(ICollection<GameController.Movement> movements)
        {
            if (movements.Count == 0) return "WAIT";

            var sb = new StringBuilder();
            foreach (var m in movements)
            {
                sb.AppendFormat("{0} {1} {2} ", m.PodsCount, m.OriginZoneId, m.DestinationZoneId);
            }

            return sb.ToString(0, sb.Length-1);
        }

        private static string PurchasesToCommands(ICollection<GameController.Purchase> purchases)
        {
            if (purchases.Count == 0) return "WAIT";

            var sb = new StringBuilder();
            foreach (var m in purchases)
            {
                sb.AppendFormat("{0} {1} ", m.PodsCount, m.ZoneId);
            }

            return sb.ToString(0, sb.Length - 1);
        }
    }

    public class GameController
    {
        private static Random _entropy = new Random();

        private readonly Game _game;

        public GameController(Game game)
        {
            _game = game;
        }

        public Output CalculateRound()
        {
            var movements = Move();
            var purchases = Buy();

            return new Output
                   {
                       Movements = movements,
                       Purchases = purchases,
                   };
        }

        private IList<Movement> Move()
        {
            var movements = new List<Movement>();

            foreach (var continent in _game.Continents)
            {
                foreach (var zone in continent.Zones.Where(x => x.IsMine && x.MyPodsCount > 0))
                {
                    var notMineZones = zone.AdjacentZones.Where(x => x.IsMine == false).OrderByDescending(x => x.IsNeutral).ToList();

                    for (var i = 0; i < zone.MyPodsCount; i++)
                    {
                        var destinationZone = notMineZones.Count > 0 ? notMineZones.First() : zone.AdjacentZones[_entropy.Next(zone.AdjacentZones.Length)];

                        movements.Add(new Movement
                                      {
                                          PodsCount = 1,
                                          OriginZoneId = zone.Id,
                                          DestinationZoneId = destinationZone.Id,
                                      });
                    }
                }
            }

            return movements;
        }

        private IList<Purchase> Buy()
        {
            return new[]
                   {
                       new Purchase {PodsCount = 1, ZoneId = 73}
                   };
        }

        public class Output
        {
            public IList<Movement> Movements { get; set; }
            public IList<Purchase> Purchases { get; set; }
        }

        public class Movement
        {
            public int PodsCount { get; set; }
            public int OriginZoneId { get; set; }
            public int DestinationZoneId { get; set; }
        }

        public class Purchase
        {
            public int PodsCount { get; set; }
            public int ZoneId { get; set; }
        }
    }

    public class Game
    {
        public int PlayersCount { get; private set; }
        public int MyPlayerId { get; private set; }
        public int MyPlatinum { get; set; }
        public int PlatinumSource { get; private set; }
        public Zone[] Zones { get; private set; }
        public Continent[] Continents { get; private set; }

        public void ReadRound()
        {
            MyPlatinum = int.Parse(Console.ReadLine());

            for (var i = 0; i < Zones.Length; i++)
            {
                var inputs = Console.ReadLine().Split(' ');
                var id = int.Parse(inputs[0]);

                var zone = Zones[id];
                zone.OwnerId = int.Parse(inputs[1]);
                zone.PodsPerPlayerId[0] = int.Parse(inputs[2]);
                zone.PodsPerPlayerId[1] = int.Parse(inputs[3]);
                zone.PodsPerPlayerId[2] = int.Parse(inputs[4]);
                zone.PodsPerPlayerId[3] = int.Parse(inputs[5]);
            }
        }

        public static Game Read()
        {
            var inputs = Console.ReadLine().Split(' ');

            var game = new Game
                       {
                           PlayersCount = int.Parse(inputs[0]),
                           MyPlayerId = int.Parse(inputs[1]),
                           MyPlatinum = 0
                       };

            var zones = Zone.ReadMany(game, int.Parse(inputs[2]), int.Parse(inputs[3]));
            var continents = ZonesToContinents(zones);

            game.PlatinumSource = continents.Sum(x => x.PlatinumSource);
            game.Zones = zones;
            game.Continents = continents;

            return game;
        }

        private static Continent[] ZonesToContinents(ICollection<Zone> zones)
        {
            var colors = new Color[zones.Count];
            for (var i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.White;
            }

            var continents = new List<Continent>();
            var continentZones = new List<Zone>();

            foreach (var zone in zones)
            {
                if (colors[zone.Id] != Color.White) continue;

                VisitZone(continentZones, colors, zone);
                continents.Add(new Continent(continentZones));
                continentZones.Clear();
            }

            return continents.ToArray();
        }

        private static void VisitZone(ICollection<Zone> continentZones, IList<Color> colors, Zone zone)
        {
            colors[zone.Id] = Color.Grey;

            continentZones.Add(zone);

            foreach (var adjacentZone in zone.AdjacentZones)
            {
                if (colors[adjacentZone.Id] != Color.White) continue;

                VisitZone(continentZones, colors, adjacentZone);
            }

            colors[zone.Id] = Color.Black;
        }
    }

    public class Zone
    {
        public Game Game { get; private set; }

        public int Id { get; private set; }
        public int PlatinumSource { get; private set; }
        public int[] PodsPerPlayerId { get; private set; }
        public Zone[] AdjacentZones { get; private set; }

        public int OwnerId { get; set; }

        public bool IsNeutral
        {
            get { return OwnerId == -1; }
        }

        public bool IsMine
        {
            get { return OwnerId == Game.MyPlayerId; }
        }

        public int MyPodsCount
        {
            get { return PodsPerPlayerId[Game.MyPlayerId]; }
        }

        public static Zone[] ReadMany(Game game, int zoneCount, int linkCount)
        {
            var zones = Enumerable.Range(0, zoneCount).Select(x => ReadOne(game, x)).ToArray();
            ReadLinks(zones, linkCount);

            return zones;
        }

        private static Zone ReadOne(Game game, int id)
        {
            var inputs = Console.ReadLine().Split(' ');

            return new Zone
                   {
                       Game = game,
                       OwnerId = -1,
                       Id = int.Parse(inputs[0]),
                       PlatinumSource = int.Parse(inputs[1]),
                       PodsPerPlayerId = new int[4],
                   };
        }

        private static void ReadLinks(ICollection<Zone> zones, int linkCount)
        {
            var adjacentZoneIds = new List<int>[zones.Count];

            for (var i = 0; i < adjacentZoneIds.Length; i++)
            {
                adjacentZoneIds[i] = new List<int>();
            }

            for (var i = 0; i < linkCount; i++)
            {
                var inputs = Console.ReadLine().Split(' ');
                var zone1 = int.Parse(inputs[0]);
                var zone2 = int.Parse(inputs[1]);

                adjacentZoneIds[zone1].Add(zone2);
                adjacentZoneIds[zone2].Add(zone1);
            }

            foreach (var zone in zones)
            {
                zone.AdjacentZones = zones.Join(adjacentZoneIds[zone.Id], x => x.Id, y => y, (x, y) => x).ToArray();
            }
        }
    }

    public class Continent
    {
        public Zone[] Zones { get; private set; }
        public int PlatinumSource { get; private set; }

        public Continent(IEnumerable<Zone> zones)
        {
            Zones = zones.ToArray();
            PlatinumSource = Zones.Sum(x => x.PlatinumSource);
        }

        public bool IsNeutral
        {
            get { return Zones.All(x => x.IsNeutral); }
        }

        public bool IsOwned
        {
            get
            {
                var ownerId = Zones[0].OwnerId;
                for (var i = 1; i < Zones.Length; i++)
                {
                    if (ownerId != Zones[i].OwnerId) return false;
                }
                
                return ownerId != -1;
            }
        }
    }

    public enum Color
    {
        None,
        White,
        Grey,
        Black
    }
}