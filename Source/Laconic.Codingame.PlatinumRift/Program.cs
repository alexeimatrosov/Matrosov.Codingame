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
                var output = gameController.PlayRound();

                Console.WriteLine(MovementsToCommands(output.Movements));
                Console.WriteLine(PurchasesToCommands(output.Purchases));
            }
        }

        private static string MovementsToCommands(ICollection<GameController.Move> movements)
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
        private readonly int[] _distances;
        private readonly Queue<Zone> _queue;

        public GameController(Game game)
        {
            _game = game;
            
            var zoneCount = _game.Zones.Length;
            _colors = new Color[zoneCount];
            _parentIds = new int[zoneCount];
            _distances = new int[zoneCount];
            _queue = new Queue<Zone>();
        }

        public Output PlayRound()
        {
            var movements = Go();
            var purchases = Buy();

            return new Output
                   {
                       Movements = movements,
                       Purchases = purchases,
                   };
        }

        private IList<Move> Go()
        {
            var moves = new List<Move>();

            foreach (var continent in _game.Continents.Where(x => x.IsOwned == false))
            {
                var zonesWithPods = continent.Zones.Where(x => x.IsMine && x.MyPodsCount > 0 && x.MaxOpponentPodsCount == 0);
                var movesUndetermined = zonesWithPods.SelectMany(x => Enumerable.Range(0, x.MyPodsCount).Select(y => new Move {State = MoveState.Undetermined, OriginZone = x, PodsCount = 1}));
                var movesQueue = new Queue<Move>(movesUndetermined);
                var movesDetermined = new List<Move>();

                while (movesQueue.Count > 0)
                {
                    var move = movesQueue.Dequeue();

                    BfsInitialize(move.OriginZone);

                    while (_queue.Count > 0 && move.State != MoveState.Determined)
                    {
                        var currentZone = _queue.Dequeue();
                        var adjacentZones = currentZone.AdjacentZones
                            .Where(x => _colors[x.Id] == Color.White)
                            .OrderByDescending(x => x.PlatinumSource)
                            .ThenByDescending(x => x.LocalPlatinumSource)
                            .ThenByDescending(x => x.IsNeutral)
                            .ThenByDescending(x => x.IsMine && x.MaxOpponentPodsCount > 0)
                            .ThenBy(x => x.MaxOpponentPodsCount)
                            .ThenBy(x => x.AdjacentZones.Length)
                            .ThenByDescending(x => x.AdjacentMyZonesCount);

                        foreach (var discoveredZone in adjacentZones)
                        {
                            BfsDiscover(currentZone, discoveredZone);

                            if (move.State == MoveState.Determined) continue;
                            if (discoveredZone.IsMine && discoveredZone.MaxOpponentPodsCount == 0) continue;

                            var movesToSameDestination = movesDetermined.Where(x => x.DestinationZone == discoveredZone).ToArray();
                            var longerMovesToSameDestination = movesToSameDestination.Where(x => x.Distance > _distances[discoveredZone.Id]).ToArray();

                            if (movesToSameDestination.Length == 0)
                            {
                                BfsSetMove(MoveState.Determined, move, discoveredZone);
                                movesDetermined.Add(move);
                            }
                            else if (longerMovesToSameDestination.Length > 0)
                            {
                                BfsSetMove(MoveState.Determined, move, discoveredZone);
                                movesDetermined.Add(move);

                                foreach (var cancelMove in longerMovesToSameDestination)
                                {
                                    cancelMove.State = MoveState.Undetermined;
                                    movesDetermined.Remove(cancelMove);
                                    movesQueue.Enqueue(cancelMove);
                                }
                            }
                            else if (discoveredZone.MaxOpponentPodsCount > 0 && (movesToSameDestination.Length + discoveredZone.MyPodsCount < discoveredZone.MaxOpponentPodsCount))
                            {
                                BfsSetMove(MoveState.Determined, move, discoveredZone);
                                movesDetermined.Add(move);
                            }
                            else
                            {
                                BfsSetMove(MoveState.Undetermined, move, discoveredZone);
                            }
                        }

                        _colors[currentZone.Id] = Color.Black;
                    }

                    if (move.State == MoveState.Undetermined)
                    {
                        move.State = MoveState.Determined;
                        movesDetermined.Add(move);
                    }
                }

                moves.AddRange(movesDetermined);
            }

            return moves;
        }

        private void BfsSetMove(MoveState state, Move move, Zone destinationZone)
        {
            move.State = state;
            move.NextZone = BfsNextZoneOnRoute(move.OriginZone, destinationZone);
            move.DestinationZone = destinationZone;
            move.Distance = _distances[destinationZone.Id];
        }

        private void BfsInitialize(Zone zone)
        {
            _queue.Clear();
            for (var i = 0; i < _game.Zones.Length; i++)
            {
                _colors[i] = Color.White;
                _distances[i] = -1;
                _parentIds[i] = -1;
            }

            _queue.Enqueue(zone);
            _colors[zone.Id] = Color.Grey;
            _distances[zone.Id] = 0;
            _parentIds[zone.Id] = zone.Id;
        }

        private void BfsDiscover(Zone currentZone, Zone discoveredZone)
        {
            _queue.Enqueue(discoveredZone);
            _colors[discoveredZone.Id] = Color.Grey;
            _distances[discoveredZone.Id] = _distances[currentZone.Id] + 1;
            _parentIds[discoveredZone.Id] = currentZone.Id;
        }

        private Zone BfsNextZoneOnRoute(Zone originZone, Zone destinationZone)
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

            var continents = _game.Continents.Where(x => x.IsOwned == false && x.CanSpawn && (x.OpponentPodsCount > 0 || x.MyPodsCount == 0)).ToArray();
            SetPodsDistribution(continents, podsCount);

            foreach (var continent in continents)
            {
                PlaceOnContinent(continent);
            }

            return continents.SelectMany(x => x.Zones).Where(x => x.PodsToPurchase > 0).Select(x => new Purchase {Zone = x, PodsCount = x.PodsToPurchase}).ToList();
        }

        private static void SetPodsDistribution(IList<Continent> continents, int podsCount)
        {
            if (continents.Count == 0) return;

            SetContinentsPodsDistribution(continents);
            var continentsByDistribution = continents.OrderByDescending(x => x.PodsDistribution).ToList();

            var podsRemainder = podsCount;
            foreach (var continent in continentsByDistribution)
            {
                var podsToPlace = (int)Math.Floor(continent.PodsDistribution * podsCount);
                continent.DistributedPodsCount = podsToPlace;
                podsRemainder -= podsToPlace;
            }

            //Console.Error.WriteLine("podsCount:{0} podsRemainder:{1} continents.Count:{2}", podsCount, podsRemainder, continents.Count);
            //for (var i = 0; i < continentsByDistribution.Count; i++)
            //{
            //    Console.Error.WriteLine("DistributedPodsCount[{0}]: {1}", i, continentsByDistribution[i].DistributedPodsCount);
            //}

            for (var i = 0; i < podsRemainder; i++)
            {
                continentsByDistribution[i].DistributedPodsCount++;
            }
        }

        private static void SetContinentsPodsDistribution(IList<Continent> continents)
        {
            foreach (var continent in continents)
            {
                continent.PodsDistribution = GetContinentDistribution(continent);
            }

            var totalDistribution = continents.Sum(x => x.PodsDistribution);
            foreach (var continent in continents)
            {
                continent.PodsDistribution /= totalDistribution;
            }
        }

        private static double GetContinentDistribution(Continent continent)
        {
            var weightPlatinumSource = continent.PlatinumSource > 0 ? 1.0 - (double)continent.MyPlatinumSource / continent.PlatinumSource : 0.0;
            var weightZonesCount = 1.0 - (double)continent.MyZonesCount / continent.Zones.Length;
            var weightPodsCount = continent.MyPodsCount > 0 ? (double)continent.OpponentPodsCount / continent.MyPodsCount : 1.0;

            //Console.Error.WriteLine("ContinentId:{0} WPS:{1} WZC:{2} WPC:{3}", continent.Id, weightPlatinumSource, weightZonesCount, weightPodsCount);

            return weightPodsCount*(weightPlatinumSource*continent.PlatinumSource + weightZonesCount*continent.Zones.Length);
        }

        private void PlaceOnContinent(Continent continent)
        {
            if (continent.DistributedPodsCount == 0) return;

            var availablePods = continent.DistributedPodsCount;

            var candidateZones = continent.Zones
                .Where(x => x.IsMine == false || x.MaxOpponentPodsCount > x.MyPodsCount)
                .OrderByDescending(x => x.PlatinumSource)
                .ThenByDescending(x => x.LocalPlatinumSource)
                .ThenByDescending(x => x.IsMine);

            foreach (var zone in candidateZones)
            {
                if (zone.IsMine)
                {
                    for (var i = 0; i < zone.MaxOpponentPodsCount - zone.MyPodsCount && availablePods > 0; i++)
                    {
                        availablePods--;
                        zone.PodsToPurchase++;
                    }
                }
                else if (zone.IsNeutral)
                {
                    availablePods--;
                    zone.PodsToPurchase++;
                }
                else
                {
                    BfsInitialize(zone);

                    while (_queue.Count > 0 && availablePods > 0)
                    {
                        var currentZone = _queue.Dequeue();
                        var adjacentZones = currentZone.AdjacentZones
                            .Where(x => _colors[x.Id] == Color.White)
                            .OrderByDescending(x => x.IsNeutral)
                            .ThenByDescending(x => x.IsMine);

                        foreach (var discoveredZone in adjacentZones)
                        {
                            BfsDiscover(currentZone, discoveredZone);

                            if (discoveredZone.CanSpawn == false || availablePods <= 0) continue;

                            availablePods--;
                            discoveredZone.PodsToPurchase++;
                        }

                        _colors[currentZone.Id] = Color.Black;
                    }
                }

                if (availablePods <= 0) break;
            }
        }

        public class Output
        {
            public IList<Move> Movements { get; set; }
            public IList<Purchase> Purchases { get; set; }
        }

        public class Move
        {
            public MoveState State { get; set; }
            public int PodsCount { get; set; }
            public Zone OriginZone { get; set; }
            public Zone NextZone { get; set; }
            public Zone DestinationZone { get; set; }
            public int Distance { get; set; }
        }

        public class Purchase
        {
            public int PodsCount { get; set; }
            public Zone Zone { get; set; }
        }

        public enum MoveState 
        {
            None,
            Undetermined,
            Tentative,
            Determined,
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
        
        public static Game Read()
        {
            var inputs = Console.ReadLine().Split(' ');

            var zones = Zone.ReadMany(int.Parse(inputs[2]), int.Parse(inputs[3]));
            var continents = ZonesToContinents(zones);

            return new Game
                   {
                       PlayersCount = int.Parse(inputs[0]),
                       MyPlayerId = int.Parse(inputs[1]),
                       MyPlatinum = 0,
                       PlatinumSource = continents.Sum(x => x.PlatinumSource),
                       Zones = zones,
                       Continents = continents
                   };
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

                VisitZone(zone, continentZones, colors);
                continents.Add(new Continent(continentZones));
                continentZones.Clear();
            }

            return continents.ToArray();
        }

        private static void VisitZone(Zone zone, ICollection<Zone> continentZones, IList<Color> colors)
        {
            colors[zone.Id] = Color.Grey;

            continentZones.Add(zone);

            foreach (var adjacentZone in zone.AdjacentZones)
            {
                if (colors[adjacentZone.Id] != Color.White) continue;

                VisitZone(adjacentZone, continentZones, colors);
            }

            colors[zone.Id] = Color.Black;
        }

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

            foreach (var z in Zones)
            {
                z.IsNeutral = z.OwnerId == -1;
                z.IsMine = z.OwnerId == MyPlayerId;
                z.IsOpponents = z.OwnerId != -1 && z.OwnerId != MyPlayerId;
                z.CanSpawn = z.IsMine || z.IsNeutral;
                z.MyPodsCount = z.PodsPerPlayerId[MyPlayerId];
                z.OpponentPodsCount = z.PodsPerPlayerId.Where((x, i) => i != MyPlayerId).Sum();
                z.MaxOpponentPodsCount = z.PodsPerPlayerId.Where((x, i) => i != MyPlayerId).Max();
                z.AdjacentMyZonesCount = z.AdjacentZones.Count(x => x.OwnerId == MyPlayerId);

                z.PodsToPurchase = 0;
            }

            foreach (var c in Continents)
            {
                c.IsOwned = IsContinentOwned(c);
                c.CanSpawn = c.Zones.Any(z => z.CanSpawn);
                c.MyPodsCount = c.Zones.Sum(x => x.MyPodsCount);
                c.OpponentPodsCount = c.Zones.Sum(x => x.OpponentPodsCount);
                c.MyPlatinumSource = c.Zones.Where(x => x.IsMine).Sum(x => x.PlatinumSource);
                c.MyZonesCount = c.Zones.Count(x => x.IsMine);

                c.PodsDistribution = 0.0;
                c.DistributedPodsCount = 0;
            }
        }

        private static bool IsContinentOwned(Continent continent)
        {
            var ownerId = continent.Zones[0].OwnerId;
            for (var i = 1; i < continent.Zones.Length; i++)
            {
                if (ownerId != continent.Zones[i].OwnerId) return false;
            }

            return ownerId != -1;
        }
    }

    public class Zone
    {
        public Game Game { get; private set; }

        public int Id { get; private set; }
        public int PlatinumSource { get; private set; }
        public int LocalPlatinumSource { get; private set; }
        public Zone[] AdjacentZones { get; private set; }

        public int OwnerId { get; set; }
        public int[] PodsPerPlayerId { get; private set; }

        public bool IsNeutral { get; set; }
        public bool IsMine { get; set; }
        public bool IsOpponents { get; set; }
        public bool CanSpawn { get; set; }
        public int MyPodsCount { get; set; }
        public int OpponentPodsCount { get; set; }
        public int MaxOpponentPodsCount { get; set; }
        public int AdjacentMyZonesCount { get; set; }

        public int PodsToPurchase { get; set; }

        public static Zone[] ReadMany(int zoneCount, int linkCount)
        {
            var zones = Enumerable.Range(0, zoneCount).Select(x => ReadOne()).ToArray();
            ReadLinks(zones, linkCount);

            return zones;
        }

        private static Zone ReadOne()
        {
            var inputs = Console.ReadLine().Split(' ');

            return new Zone
                   {
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

            foreach (var zone in zones)
            {
                zone.LocalPlatinumSource = zone.PlatinumSource + zone.AdjacentZones.Sum(x => x.PlatinumSource);
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
        public int DistributedPodsCount { get; set; }

        public bool IsOwned { get; set; }
        public bool CanSpawn { get; set; }
        public int MyPodsCount { get; set; }
        public int OpponentPodsCount { get; set; }
        public int MyPlatinumSource { get; set; }
        public int MyZonesCount { get; set; }

        public Continent(IEnumerable<Zone> zones)
        {
            Id = IdCounter++;
            Zones = zones.ToArray();
            PlatinumSource = Zones.Sum(x => x.PlatinumSource);
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