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
                sb.AppendFormat("{0} {1} {2} ", m.PodsCount, m.OriginZone.Id, m.NextZone.Id);
            }

            return sb.ToString(0, sb.Length-1);
        }

        private static string PurchasesToCommands(ICollection<GameController.Purchase> purchases)
        {
            if (purchases.Count == 0) return "WAIT";

            var sb = new StringBuilder();
            foreach (var m in purchases)
            {
                sb.AppendFormat("{0} {1} ", m.PodsCount, m.Zone.Id);
            }

            return sb.ToString(0, sb.Length - 1);
        }
    }

    public class GameController
    {
        //private static readonly Random Entropy = new Random();
        private const int PodCost = 20;

        private readonly Game _game;

        private readonly Color[] _colors;
        private readonly int[] _parentIds;
        private readonly Queue<Zone> _queue;

        public GameController(Game game)
        {
            _game = game;
            
            var zoneCount = _game.Zones.Length;
            _colors = new Color[zoneCount];
            _parentIds = new int[zoneCount];
            _queue = new Queue<Zone>();
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

            foreach (var continent in _game.Continents.Where(x => x.IsOwned == false))
            {
                foreach (var zone in continent.Zones.Where(x => x.IsMine && x.MyPodsCount > 0 && x.OpponentPodsCount == 0))
                {
                    var availablePods = zone.MyPodsCount;

                    RunBfs(zone, z => z.AdjacentZones.OrderBy(x => x.IsMine).ThenByDescending(x => x.PlatinumSource).ThenBy(x => x.IsNeutral),
                        (originZone, discoveredZone) =>
                        {
                            if (discoveredZone.IsMine || availablePods <= 0) return;

                            availablePods--;
                            movements.Add(new Movement
                                          {
                                              PodsCount = 1,
                                              OriginZone = originZone,
                                              NextZone = GetNextZoneOnRoute(originZone, discoveredZone),
                                              DestinationZone = discoveredZone,
                                          });
                        }, () => availablePods <= 0);
                }
            }

            return movements;
        }

        private void RunBfs(Zone originZone, Func<Zone, IEnumerable<Zone>> adjacentZoneSelector, Action<Zone, Zone> zoneDiscoveredAction, Func<bool> stopCondition)
        {
            _queue.Clear();
            for (var i = 0; i < _game.Zones.Length; i++)
            {
                _colors[i] = Color.White;
                _parentIds[i] = -1;
            }

            _queue.Enqueue(originZone);
            _colors[originZone.Id] = Color.Grey;
            _parentIds[originZone.Id] = originZone.Id;

            while (_queue.Count > 0 && stopCondition() == false)
            {
                var currentZone = _queue.Dequeue();

                foreach (var discoveredZone in adjacentZoneSelector(currentZone).Where(x => _colors[x.Id] == Color.White))
                {
                    _queue.Enqueue(discoveredZone);
                    _colors[discoveredZone.Id] = Color.Grey;
                    _parentIds[discoveredZone.Id] = currentZone.Id;

                    zoneDiscoveredAction(originZone, discoveredZone);
                }

                _colors[currentZone.Id] = Color.Black;
            }
        }

        private Zone GetNextZoneOnRoute(Zone originZone, Zone destinationZone)
        {
            var resultId = destinationZone.Id;

            while (_parentIds[resultId] != originZone.Id)
            {
                resultId = _parentIds[resultId];
            }

            return _game.Zones[resultId];
        }

        private IList<Purchase> Buy()
        {
            var podsCount = _game.MyPlatinum / PodCost;
            if (podsCount == 0) return new List<Purchase>();

            var continents = _game.Continents.Where(x => x.IsOwned == false && x.Zones.Any(y => y.IsMine || y.IsNeutral)).ToArray();

            return DistributePods(continents, podsCount).SelectMany(x => PlaceOnContinent(x.Key, x.Value)).ToList();
        }

        private static IEnumerable<KeyValuePair<Continent, int>> DistributePods(Continent[] continents, int podsCount)
        {
            CalculateDistribution(continents, podsCount);

            var podsToPlace = continents.ToDictionary(x => x, x => 0);
            while (podsCount > 0)
            {
                var continent = continents.MaxBy(x => x.PodsDistribution);
                podsToPlace[continent] += 1;
                continent.PodsDistribution -= 1.0;
                podsCount--;
            }

            return podsToPlace;
        }

        private static void CalculateDistribution(Continent[] conqueringContinents, int podsCount)
        {
            foreach (var continent in conqueringContinents)
            {
                continent.PodsDistribution += GetContinentDistribution(conqueringContinents, continent, podsCount);
            }
        }

        private static double GetContinentDistribution(IEnumerable<Continent> conqueringContinents, Continent continent, int podsCount)
        {
            return podsCount*((double) continent.Zones.Length/conqueringContinents.Sum(x => x.Zones.Length));
        }

        private IEnumerable<Purchase> PlaceOnContinent(Continent continent, int podsCount)
        {
            if (podsCount == 0) return Enumerable.Empty<Purchase>();

            var placedPods = new Dictionary<Zone, int>();

            var availablePods = podsCount;
            foreach (var zone in continent.Zones.Where(x => x.IsMine == false).OrderByDescending(x => x.PlatinumSource))
            {
                if (zone.IsNeutral)
                {
                    availablePods--;
                    placedPods.AddOrModify(zone, 1, x => x + 1);
                }
                else
                {
                    RunBfs(zone, z => z.AdjacentZones.OrderByDescending(x => x.IsNeutral).ThenByDescending(x => x.IsMine),
                        (originZone, discoveredZone) =>
                        {
                            if (discoveredZone.CanSpawn == false || availablePods <= 0) return;

                            availablePods--;
                            placedPods.AddOrModify(discoveredZone, 1, x => x + 1);
                        }, () => availablePods <= 0);
                }

                if (availablePods <= 0) break;
            }

            return placedPods.Select(x => new Purchase {Zone = x.Key, PodsCount = x.Value});
        }

        public class Output
        {
            public IList<Movement> Movements { get; set; }
            public IList<Purchase> Purchases { get; set; }
        }

        public class Movement
        {
            public int PodsCount { get; set; }
            public Zone OriginZone { get; set; }
            public Zone NextZone { get; set; }
            public Zone DestinationZone { get; set; }
        }

        public class Purchase
        {
            public int PodsCount { get; set; }
            public Zone Zone { get; set; }
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

        public bool IsOpponents
        {
            get { return OwnerId != -1 && OwnerId != Game.MyPlayerId; }
        }

        public int MyPodsCount
        {
            get { return PodsPerPlayerId[Game.MyPlayerId]; }
        }

        public int OpponentPodsCount
        {
            get { return PodsPerPlayerId.Where((x, i) => i != Game.MyPlayerId).Sum(); }
        }

        public bool CanSpawn
        {
            get { return IsMine || IsNeutral; }
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
        public static int IdCounter = 0;

        public int Id { get; private set; }
        public Zone[] Zones { get; private set; }
        public int PlatinumSource { get; private set; }
        public double PodsDistribution { get; set; }

        public Continent(IEnumerable<Zone> zones)
        {
            Id = IdCounter++;
            Zones = zones.ToArray();
            PlatinumSource = Zones.Sum(x => x.PlatinumSource);
            PodsDistribution = 0.0;
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

        public int MyPodsCount
        {
            get { return Zones.Sum(x => x.MyPodsCount); }
        }

        public int OpponentPodsCount
        {
            get { return Zones.Sum(x => x.OpponentPodsCount); }
        }

        public bool CanSpawn
        {
            get { return Zones.Any(x => x.IsMine || x.IsNeutral); }
        }
    }

    public enum Color
    {
        None,
        White,
        Grey,
        Black
    }

    public static class DictionaryExtentions
    {
        public static void AddOrModify<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue initialValue, Func<TValue, TValue> modifyValueFunc)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = modifyValueFunc(dictionary[key]);
            }
            else
            {
                dictionary[key] = initialValue;
            }
        }
    }

    public static class EnumerableExtentions
    {
        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
        {
            return source.MaxBy(selector, Comparer<TKey>.Default);
        }

        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (selector == null) throw new ArgumentNullException("selector");
            if (comparer == null) throw new ArgumentNullException("comparer");
            using (var sourceIterator = source.GetEnumerator())
            {
                if (!sourceIterator.MoveNext())
                {
                    throw new InvalidOperationException("Sequence contains no elements");
                }
                var max = sourceIterator.Current;
                var maxKey = selector(max);
                while (sourceIterator.MoveNext())
                {
                    var candidate = sourceIterator.Current;
                    var candidateProjected = selector(candidate);
                    if (comparer.Compare(candidateProjected, maxKey) > 0)
                    {
                        max = candidate;
                        maxKey = candidateProjected;
                    }
                }
                return max;
            }
        }
    }
}