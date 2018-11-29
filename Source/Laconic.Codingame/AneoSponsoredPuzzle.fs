module Laconic.Codingame.AneoSponsoredPuzzle

open System
open FSharp.Data.UnitSystems.SI.UnitSymbols

[<Measure>] type kph

type TrafficLight = {
    Distance : float<m>
    Duration : float<s>
}

let toMetersPerSecond (x:float<kph>) = x * 1000.0<m> / 3600.0<s> / 1.0<kph>
let toKilometersPerHour (x:float<m/s>) = x * 3600.0<s> / 1000.0<m> * 1.0<kph>

let getTime (distance:float<m>) (speed:float<kph>) = distance / (speed |> toMetersPerSecond)
let getSpeed (distance:float<m>) (time:float<s>) = (distance / time) |> toKilometersPerHour

let readString() = Console.In.ReadLine();
let readInt = readString >> int

let readTrafficLight() =
    readString().Split [|' '|]
    |> Array.map float
    |> (fun a -> {Distance = a.[0] * 1.0<m>; Duration = a.[1] * 1.0<s>})

let speedLimit = readInt()
let lightCount = readInt()

let trafficLights = Array.init lightCount (fun _ -> readTrafficLight())

let isGreen (speed:float<kph>) trafficLight =
    let time = getTime trafficLight.Distance speed
    let timeInt = Math.Round(time |> float, 12) |> int
    let durationInt = trafficLight.Duration |> int
    timeInt % (durationInt * 2) < durationInt

let checkSpeed (speed:float<kph>) =
    trafficLights
    |> Seq.forall (fun x -> isGreen speed x)

let speed =
    seq { for s in speedLimit .. -1 .. 1 do if checkSpeed ((float s) * 1.0<kph>) then yield s }
    |> Seq.head

printfn "%d" speed