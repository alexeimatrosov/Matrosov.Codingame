module Laconic.Codingame.AneoSponsoredPuzzle

open System

type TrafficLight = {
    Distance : float;
    Duration : float
}

let readString() = Console.In.ReadLine();
let readInt = readString >> int

let readTrafficLight() =
    readString().Split [|' '|]
    |> Array.map float
    |> (fun a -> {Distance = a.[0]; Duration = a.[1]})

let toMetersPerSecond x = x * 1000.0 / 60.0 / 60.0
//let toKilometersPerHours x = x * 60.0 * 60.0 / 1000.0

let isGreen speed trafficLight =
    let t = trafficLight.Distance / (speed |> toMetersPerSecond)
    (t / trafficLight.Duration) % 2.0 < 1.0

let speedLimit = readInt()
let lightCount = readInt()

let trafficLights = 
    seq { for _ in 1 .. lightCount do yield readTrafficLight() }
    |> Seq.toArray

let checkSpeed speed =
    trafficLights
    |> Seq.map (isGreen (speed |> float))
    |> Seq.map (fun b -> if b then 1 else 0)
    |> Seq.iter (eprintf "%d ")

    eprintfn ""

    trafficLights
    |> Seq.forall (isGreen (speed |> float))

let speed =
    seq { for i in speedLimit .. -1 .. 1 do
            eprintfn "%d" i
            yield (i, checkSpeed i)
    }
    |> Seq.filter snd
    |> Seq.head
    |> fst
    
printfn "%d" speed