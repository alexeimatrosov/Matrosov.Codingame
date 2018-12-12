module Laconic.Codingame.RollerCoaster

open System

let readString() = Console.ReadLine()
let readInt = readString >> int

let token = readString().Split(' ') |> Array.map int
let placesCount = token.[0]
let ridesCount = token.[1]
let queue = Array.init token.[2] (fun _ -> readInt())

let makeRideStartingAt queueIndex =
    let rec makeRideAux placesLeft (earnedThisRide:int64) queueIndex groupsTaken =
        let groupSize = queue.[queueIndex]
        match placesLeft < groupSize || groupsTaken = queue.Length with
        | true -> (queueIndex, earnedThisRide)
        | false -> makeRideAux (placesLeft - groupSize) (earnedThisRide + int64 groupSize) ((queueIndex + 1) % queue.Length) (groupsTaken + 1)

    makeRideAux placesCount 0L queueIndex 0

let ridesStartingAtCache = Array.init queue.Length makeRideStartingAt

let rec calculateEarnings ridesLeft (earningSoFar:int64) queueIndex =
    match ridesLeft with
    | 0 -> earningSoFar
    | _ -> 
        let (nextQueueIndex, earnedThisRide) = ridesStartingAtCache.[queueIndex]
        calculateEarnings (ridesLeft - 1) (earningSoFar + earnedThisRide) nextQueueIndex

let answer = calculateEarnings ridesCount 0L 0

printfn "%d" answer