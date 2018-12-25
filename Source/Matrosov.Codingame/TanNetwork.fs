module Matrosov.Codingame.TanNetwork

open System
open System.Collections.Generic

type Stop = {
    Id : string
    Name : string
    Latitude : float
    Longitude : float
}

type Route = {
    From : Stop
    To : Stop
    Distance : float
}

let distance a b =
    let x = (b.Longitude - a.Longitude) * (cos ((a.Latitude + b.Latitude) / 2.0))
    let y = b.Latitude - a.Latitude
    sqrt (x * x + y * y) * 6371.0

let toRadians degrees = degrees * Math.PI / 180.0

let readString() = Console.ReadLine()
let readInt = readString >> int
let readStop () =
    let a = readString().Split [|','|]
    {
        Id = a.[0]
        Name = a.[1].Trim [|'"'|]
        Latitude = a.[3] |> float |> toRadians
        Longitude = a.[4] |> float |> toRadians
    }

let startStopId = readString()
let endStopId = readString()

let stopsMap =
    Seq.init (readInt()) (fun _ -> readStop())
    |> Seq.map (fun s -> (s.Id, s))
    |> Map.ofSeq

let readRoute() =
    let a = readString().Split [|' '|]
    let x = stopsMap.[a.[0]]
    let y = stopsMap.[a.[1]]
    {
        From = x
        To = y
        Distance = distance x y
    }

let routesMap =
    List.init (readInt()) (fun _ -> readRoute())
    |> List.groupBy (fun r -> r.From)
    |> Map.ofList

let findPath startStop endStop =
    let visited = HashSet<Stop>()
    let pathMap = Dictionary<Stop, Stop * float>()
    pathMap.Add(startStop, (startStop, 0.0))

    let pickLeg() =
        let legs =
            pathMap
            |> Seq.map (fun x -> (x.Key, snd x.Value))
            |> Seq.filter (fun (stop, _) -> not <| visited.Contains(stop))
            |> List.ofSeq
        match legs with
        | [] -> None
        | _ -> Some (legs |> Seq.minBy snd)

    let rec visit() =
        match pickLeg() with
        | Some (stop, distanceToThis) when stop <> endStop ->
            visited.Add(stop) |> ignore
            routesMap.[stop]
            |> Seq.filter (fun route -> not <| visited.Contains(route.To))
            |> Seq.iter (fun route -> 
                let distanceThroughThis = distanceToThis + route.Distance
                let isFound = pathMap.ContainsKey(route.To)
                if (not isFound) || (snd pathMap.[route.To]) > distanceThroughThis then
                    pathMap.[route.To] <- (stop, distanceThroughThis)
            )
            visit()
        | _ -> ()

    let rec traversePathBack startStop endStop =
        seq {
            yield endStop
            if endStop <> startStop then yield! traversePathBack startStop (fst pathMap.[endStop])
        }
    
    visit()

    if pathMap.ContainsKey(endStop)
    then Some (traversePathBack startStop endStop |> Seq.rev |> List.ofSeq)
    else None

let path = findPath stopsMap.[startStopId] stopsMap.[endStopId]

match path with
| None -> printfn "IMPOSSIBLE"
| Some stops ->
    stops
    |> Seq.map (fun stop -> stop.Name)
    |> Seq.iter (printfn "%s")
