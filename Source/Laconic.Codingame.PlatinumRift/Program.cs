using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Laconic.Codingame.PlatinumRift
{
    public class Program
    {
        static void Main()
        {
            var game = Game.Read();

            // game loop
            while (true)
            {
                game.ReadRound();

                Console.WriteLine("WAIT");
                Console.WriteLine("1 73");
            }
        }
    }

    public class Game
    {
        public int PlayersCount { get; set; }
        public int MyPlayerId { get; set; }
        public int MyPlatinum { get; set; }
        public Zone[] Zones { get; set; }

        public void ReadRound()
        {
            MyPlatinum = int.Parse(Console.ReadLine()); // my available Platinum

            for (var i = 0; i < Zones.Length; i++)
            {
                var inputs = Console.ReadLine().Split(' ');
                var id = int.Parse(inputs[0]); // this zone's ID

                var zone = Zones[id];
                zone.OwnerId = int.Parse(inputs[1]);
                zone.Pods[0] = int.Parse(inputs[2]);
                zone.Pods[1] = int.Parse(inputs[3]);
                zone.Pods[2] = int.Parse(inputs[4]);
                zone.Pods[3] = int.Parse(inputs[5]);
            }
        }

        public static Game Read()
        {
            var inputs = Console.ReadLine().Split(' ');
            var zoneCount = int.Parse(inputs[2]); // the amount of zones on the map
            var linkCount = int.Parse(inputs[3]); // the amount of links between all zones
            
            var zones = Enumerable.Range(0, zoneCount).Select(Zone.Read).ToArray();
            Zone.ReadLinks(zones, linkCount);

            return new Game
                   {
                       PlayersCount = int.Parse(inputs[0]),
                       MyPlayerId = int.Parse(inputs[1]),
                       Zones = zones
                   };
        }
    }

    public class Zone
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public int PlatinumSource { get; set; }
        public IList<int> AdjacentZoneIds { get; set; }
        public int[] Pods { get; set; }

        public static Zone Read(int id)
        {
            var inputs = Console.ReadLine().Split(' ');

            return new Zone
                   {
                       Id = int.Parse(inputs[0]),
                       OwnerId = -1,
                       PlatinumSource = int.Parse(inputs[1]),
                       AdjacentZoneIds = new List<int>(),
                       Pods = new int[4],
                   };
        }

        public static void ReadLinks(Zone[] zones, int linkCount)
        {
            for (var i = 0; i < linkCount; i++)
            {
                var inputs = Console.ReadLine().Split(' ');
                var zone1 = int.Parse(inputs[0]);
                var zone2 = int.Parse(inputs[1]);

                zones[zone1].AdjacentZoneIds.Add(zone2);
                zones[zone2].AdjacentZoneIds.Add(zone1);
            }
        }
    }
}