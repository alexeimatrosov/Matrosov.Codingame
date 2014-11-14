using System;

namespace Laconic.Codingame.PlatinumRift
{
    class Program
    {
        static void Main(String[] args)
        {
            var inputs = Console.ReadLine().Split(' ');
            var playerCount = int.Parse(inputs[0]); // the amount of players (2 to 4)
            var myId = int.Parse(inputs[1]); // my player ID (0, 1, 2 or 3)
            var zoneCount = int.Parse(inputs[2]); // the amount of zones on the map
            var linkCount = int.Parse(inputs[3]); // the amount of links between all zones
            for (var i = 0; i < zoneCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                var zoneId = int.Parse(inputs[0]); // this zone's ID (between 0 and zoneCount-1)
                var platinumSource = int.Parse(inputs[1]); // the amount of Platinum this zone can provide per game turn
            }
            for (var i = 0; i < linkCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                var zone1 = int.Parse(inputs[0]);
                var zone2 = int.Parse(inputs[1]);
            }

            // game loop
            while (true)
            {
                var platinum = int.Parse(Console.ReadLine()); // my available Platinum
                for (var i = 0; i < zoneCount; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    var zId = int.Parse(inputs[0]); // this zone's ID
                    var ownerId = int.Parse(inputs[1]); // the player who owns this zone (-1 otherwise)
                    var podsP0 = int.Parse(inputs[2]); // player 0's PODs on this zone
                    var podsP1 = int.Parse(inputs[3]); // player 1's PODs on this zone
                    var podsP2 = int.Parse(inputs[4]); // player 2's PODs on this zone (always 0 for a two player game)
                    var podsP3 = int.Parse(inputs[5]); // player 3's PODs on this zone (always 0 for a two or three player game)
                }

                Console.WriteLine("WAIT");
                Console.WriteLine("1 73");
            }
        }
    }
}