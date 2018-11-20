module Laconic.Codingame.AneoSponsoredPuzzle

open System
open FSharp.Data.UnitSystems.SI.UnitSymbols

[<Measure>] type kph

let toMetersPerSecond (x:float<kph>) = x * 1000.0<m> / 3600.0<s> / 1.0<kph>
let toKilometersPerHour (x:float<m/s>) = x * 3600.0<s> / 1000.0<m> * 1.0<kph>

let floor (x:float<_>) = x |> float |> floor |> LanguagePrimitives.FloatWithMeasure
let ceil (x:float<_>) = x |> float |> ceil |> LanguagePrimitives.FloatWithMeasure
let round (x:float<_>) = x |> float |> round |> LanguagePrimitives.FloatWithMeasure

let getTime (distance:float<m>) (speed:float<kph>) = distance / (speed |> toMetersPerSecond)
let getSpeed (distance:float<m>) (time:float<s>) = (distance / time) |> toKilometersPerHour

type TrafficLight = {
    Distance : float<m>;
    Duration : float<s>
}

let readString() = Console.In.ReadLine();
let readInt = readString >> int
let readFloat = readString >> float

let readTrafficLight() =
    readString().Split [|' '|]
    |> Array.map float
    |> (fun a -> {Distance = a.[0] * 1.0<m>; Duration = a.[1] * 1.0<s>})

let speedLimit = readFloat() * 1.0<kph>
let lightCount = readInt()

let trafficLights = 
    seq { for _ in 1 .. lightCount do yield readTrafficLight() }
    |> Seq.toList

//let rec findMaxSpeed (maxSpeedLimit:float<kph>) (minSpeedLimit:float<kph>) trafficLights =
//    match trafficLights with
//    | trafficLight :: others ->
//        let fastestTime = getTime trafficLight.Distance maxSpeedLimit
//        let slowestTime = getTime trafficLight.Distance minSpeedLimit

//        let firstPeriodIndex = fastestTime / (trafficLight.Duration * 2.0) |> int
//        let lastPeriodIndex = slowestTime / (trafficLight.Duration * 2.0) |> int

//        seq {
//            for i in firstPeriodIndex .. (lastPeriodIndex + 1) do 
//                let greenStartTime = trafficLight.Duration * 2.0 * (float i)
//                let redStartTime = greenStartTime + trafficLight.Duration
                
//                let periodFastestTime = max greenStartTime fastestTime
//                let periodSlowestTime = min redStartTime slowestTime
                
//                let periodMaxSpeedLimit = getSpeed trafficLight.Distance periodFastestTime //|> floor
//                let periodMinSpeedLimit = getSpeed trafficLight.Distance periodSlowestTime //|> ceil

//                //let periodFastestTimeTruncated = getTime trafficLight.Distance periodMaxSpeedLimit
//                //let periodSlowestTimeTruncated = getTime trafficLight.Distance periodMinSpeedLimit
                
//                yield
//                    if periodFastestTime >= periodSlowestTime
//                    then None
//                    else (findMaxSpeed periodMaxSpeedLimit periodMinSpeedLimit others)
//            }
//        |> Seq.tryFind (fun x -> x <> None)
//        |> Option.defaultValue None
//    | [] -> Some maxSpeedLimit

//let speedOption = findMaxSpeed speedLimit 1.0<kph> trafficLights
//let speed = speedOption |> Option.defaultValue 0.0<kph>

let isGreen (speed:float<kph>) trafficLight =
    let time = getTime trafficLight.Distance speed
    let timeInt = Math.Round(time |> float, 12) |> int
    eprintfn "  isGreen (%A, %A): %A, %A" trafficLight.Distance trafficLight.Duration time timeInt
    //(((time % (trafficLight.Duration * 2.0)) |> round |> int) % (trafficLight.Duration |> int)) < (trafficLight.Duration |> int)
    let durationInt = trafficLight.Duration |> int
    (timeInt % (durationInt * 2)) < durationInt

let checkSpeed (speed:float<kph>) =
    eprintfn "Checking speed: %A kph" speed
    trafficLights
    |> Seq.forall (fun x -> isGreen speed x)

let speed =
    seq { for s in (speedLimit |> int) .. -1 .. 1 do yield (s, checkSpeed ((float s) * 1.0<kph>)) }
    |> Seq.filter (fun x -> x |> snd = true)
    |> Seq.head
    |> fst

eprintfn "Result: %A kph" speed
//printfn "%d" (speed |> int)
printfn "%d" speed