using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    static int nbFloors;
    static int width;
    static int nbRounds;
    static int exitFloor;
    static int exitPos;
    static int nbTotalClones;
    static int nbAdditionalElevators;
    static int nbElevators;

    static Point GeneratorPoint;
    static Point ExitPoint;
    static Point[][] Points;
    static int AdditionalElevatorsPlaced = 0;

    static void Main(String[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        nbFloors = int.Parse(inputs[0]); // number of floors
        width = int.Parse(inputs[1]); // width of the area
        nbRounds = int.Parse(inputs[2]); // maximum number of rounds
        exitFloor = int.Parse(inputs[3]); // floor on which the exit is found
        exitPos = int.Parse(inputs[4]); // position of the exit on its floor
        nbTotalClones = int.Parse(inputs[5]); // number of generated clones
        nbAdditionalElevators = int.Parse(inputs[6]); // ignore (always zero)
        nbElevators = int.Parse(inputs[7]); // number of elevators
        Points = new Point[nbFloors][];
        for (int i = 0; i < nbFloors; i++)
        {
            Points[i] = new Point[width];
        }
        for (int i = 0; i < nbElevators; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int elevatorFloor = int.Parse(inputs[0]); // floor on which this elevator is found
            int elevatorPos = int.Parse(inputs[1]); // position of the elevator on its floor

            Points[elevatorFloor][elevatorPos] = new Point
            {
                Type = PointType.Elevator,
                Floor = elevatorFloor,
                Position = elevatorPos,
                ShouldBeBlocked = false,
                IsBlocked = false,
                DoesExist = true,
                IsExitReachable = true,
            };
        }

        Console.Error.WriteLine("Rounds:{0} Exit:({1},{2}) Clones:{3} AddElevators:{4}", nbRounds, exitFloor, exitPos, nbTotalClones, nbAdditionalElevators);

        ExitPoint = new Point
        {
            Type = PointType.Exit,
            Floor = exitFloor,
            Position = exitPos,
            ShouldBeBlocked = false,
            IsBlocked = false,
            DoesExist = true,
            IsExitReachable = true,
        };
        Points[exitFloor][exitPos] = ExitPoint;

        // game loop
        var isFirstCycle = true;
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            int cloneFloor = int.Parse(inputs[0]); // floor of the leading clone
            int clonePos = int.Parse(inputs[1]); // position of the leading clone on its floor
            //String direction = inputs[2]; // direction of the leading clone: LEFT or RIGHT

            if (isFirstCycle)
            {
                GeneratorPoint = new Point
                {
                    Type = PointType.Generator,
                    Floor = cloneFloor,
                    Position = clonePos,
                    ShouldBeBlocked = false,
                    IsBlocked = false,
                    DoesExist = true,
                    IsExitReachable = true,
                };
                Points[cloneFloor][clonePos] = GeneratorPoint;

                ArrangePoints();
            }

            var action = GetCloneAction(cloneFloor, clonePos);

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            isFirstCycle = false;
            Console.WriteLine(action); // action: WAIT or BLOCK
        }
    }

    static string GetCloneAction(int cloneFloor, int clonePos)
    {
        if (cloneFloor == -1 || clonePos == -1) return "WAIT";

        var point = Points[cloneFloor][clonePos];
        if (point != null && point.Type == PointType.Generator && point.ShouldBeBlocked && point.IsBlocked == false)
        {
            point.IsBlocked = true;
            return "BLOCK";
        }
        else if (point != null && point.Type == PointType.Elevator && point.DoesExist == false)
        {
            point.DoesExist = true;
            return "ELEVATOR";
        }

        var lowerPoint = cloneFloor > 0 ? Points[cloneFloor - 1][clonePos] : null;
        if (lowerPoint != null && lowerPoint.Type == PointType.Elevator && lowerPoint.ShouldBeBlocked && lowerPoint.IsBlocked == false)
        {
            lowerPoint.IsBlocked = true;
            return "BLOCK";
        }

        return "WAIT";
    }

    static void ArrangePoints()
    {
        var path = GetPath();

        var direction = "RIGHT";
        for (int i = 0; i < path.Count - 1; i++)
        {
            if (direction == "RIGHT" && path[i].Position > path[i + 1].Position)
            {
                path[i].ShouldBeBlocked = true;
                direction = "LEFT";
            }
            else if (direction == "LEFT" && path[i].Position < path[i + 1].Position)
            {
                path[i].ShouldBeBlocked = true;
                direction = "RIGHT";
            }
        }

        ////
        Console.Error.WriteLine("AdditionalElevatorsPlaced:{0}", AdditionalElevatorsPlaced);
        foreach (var point in path)
        {
            Console.Error.WriteLine(point);
        }
    }

    static List<Point> GetPath()
    {
        ValidateElevatorsExitReachability();

        // while(nbAdditionalElevators > AdditionalElevatorsPlaced)
        // {
        //     Point candidatePoint = null;
        //     var maxDifference = 0;

        //     var upperPoint = ExitPoint;
        //     for(int i = ExitPoint.Floor-1; i >= 1; i--)
        //     {
        //         var point = GetClosestExitReachableElevator(i, upperPoint.Position);
        //         var lowerPoint = GetClosestExitReachableElevator(i-1, point.Position);

        //         var uld = Math.Abs(upperPoint.Position - lowerPoint.Position);
        //         var upd = Math.Abs(upperPoint.Position - point.Position);
        //         var difference = upd - uld;
        //         if(uld < upd && difference > maxDifference)
        //         {
        //             maxDifference = difference;
        //             candidatePoint = Points[point.Floor][upperPoint.Position];
        //         }

        //         if(i == 3)
        //         {
        //             Console.Error.WriteLine(upperPoint);
        //             Console.Error.WriteLine(point);
        //             Console.Error.WriteLine(lowerPoint);
        //             Console.Error.WriteLine(difference);
        //         }

        //         upperPoint = point;
        //     }

        //     //if(candidatePoint != null)
        //     //{
        //         AddElevator(candidatePoint.Floor, candidatePoint.Position);
        //     //}

        //     ValidateElevatorsExitReachability();
        // }

        var path = new List<Point>();

        path.Add(ExitPoint);
        for (int i = ExitPoint.Floor - 1; i >= 0; i--)
        {
            var previousPoint = path.Last();
            var elevator = GetClosestExitReachableElevator(i, previousPoint.Position);
            path.Add(elevator);
        }
        path.Add(GeneratorPoint);
        path.Reverse();

        return path;
    }

    static void ValidateElevatorsExitReachability()
    {
        foreach (var point in Points[ExitPoint.Floor].Where(x => x != null && x.Type == PointType.Elevator))
        {
            point.IsExitReachable = false;
        }

        var upperReachablePoint = ExitPoint;
        for (int i = ExitPoint.Floor - 1; i >= 0; i--)
        {
            foreach (var point in Points[i].Where(x => x != null))
            {
                ValidateElevatorExitReachability(point);
                // if(point.Floor == 9)
                // {
                //     Console.Error.WriteLine(point);
                // }
            }

            var exitReachableElevator = GetClosestExitReachableElevator(i, upperReachablePoint.Position);
            if (exitReachableElevator == null)
            {
                var newElevator = AddElevator(i, upperReachablePoint.Position);
                upperReachablePoint = newElevator;
            }
            else
            {
                upperReachablePoint = exitReachableElevator;
            }
        }
    }

    static void ValidateElevatorExitReachability(Point point)
    {
        if (point.Type != PointType.Elevator) return;

        var upperPoints = Points[point.Floor + 1];
        var upperPoint = upperPoints[point.Position];
        if (upperPoint != null)
        {
            point.IsExitReachable = upperPoint.IsExitReachable;
            return;
        }

        // if(point.Floor == 9 && point.Position == 17)
        // foreach(var p in upperPoints.Where(x => x != null))
        // {
        //     Console.Error.WriteLine(p);
        // }

        var upperRightPoint = upperPoints.Where(x => x != null && x.Position > point.Position).FirstOrDefault();
        var upperLeftPoint = upperPoints.Where(x => x != null && x.Position < point.Position).LastOrDefault();

        // if(point.Floor == 9 && point.Position == 17)
        // {
        //     Console.Error.WriteLine(upperRightPoint);
        //     Console.Error.WriteLine(upperLeftPoint);
        // }

        if ((upperRightPoint != null && upperRightPoint.IsExitReachable) || (upperLeftPoint != null && upperLeftPoint.IsExitReachable))
        {
            point.IsExitReachable = true;
            return;
        }

        point.IsExitReachable = false;
    }

    static Point GetClosestExitReachableElevator(int floor, int position)
    {
        return Points[floor].Where(x => x != null && x.Type == PointType.Elevator && x.IsExitReachable).OrderBy(x => Math.Abs(x.Position - position)).FirstOrDefault();
    }

    static Point AddElevator(int floor, int position)
    {
        var point = new Point
        {
            Type = PointType.Elevator,
            Floor = floor,
            Position = position,
            ShouldBeBlocked = false,
            IsBlocked = false,
            DoesExist = false,
            IsExitReachable = true,
        };
        Points[floor][position] = point;

        AdditionalElevatorsPlaced++;

        return point;
    }

    class Point
    {
        public PointType Type { get; set; }
        public int Floor { get; set; }
        public int Position { get; set; }
        public bool ShouldBeBlocked { get; set; }
        public bool IsBlocked { get; set; }
        public bool DoesExist { get; set; }
        public bool IsExitReachable { get; set; }
        public int Distance { get; set; }

        public override string ToString()
        {
            return string.Format("{0} ({1},{2}) ToBlock:{3} Exist:{4} Reachable:{5}", Type, Floor, Position, ShouldBeBlocked, DoesExist, IsExitReachable);
        }
    }

    enum PointType
    {
        None,
        Generator,
        Elevator,
        Exit,
    }
}
