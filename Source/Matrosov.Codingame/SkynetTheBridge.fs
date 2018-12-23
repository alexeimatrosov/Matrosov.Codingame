module Matrosov.Codingame.SkynetTheBridge

open System

let readString() = Console.ReadLine();
let readInt = readString >> int
let readBike() = readString().Split [|' '|] |> Array.map int |> (fun a -> a.[0], a.[1], a.[2] > 0)

let bikesCount = readInt()
let bikesToSurvive = readInt()
let lanes = Array.init 4 (fun _ -> readString())
let laneLength = lanes.[0].Length

let actions = [|"SPEED"; "WAIT"; "JUMP"; "SLOW"; "UP"; "DOWN"|]

let isHoleHit count x y =
    lanes.[y]
    |> Seq.skip (x + 1)
    |> Seq.truncate count
    |> Seq.forall (fun c -> c = '.')

let move speed (x, y, _) = (x + speed, y, isHoleHit speed x y)
let moveUp speed (x, y, _) = (x + speed, y - 1, (isHoleHit (speed - 1) x y) && (isHoleHit speed x (y - 1)))
let moveDown speed (x, y, _) = (x + speed, y + 1, (isHoleHit (speed - 1) x y) && (isHoleHit speed x (y + 1)))
let jump speed (x, y, _) = (x + speed, y, (x + speed) >= laneLength || lanes.[y].[x + speed] = '.')

let calculateTurns speed bikes =
    let rec aux speed bikes turns =
        let act f =
            bikes
            |> Seq.map f
            |> Seq.filter (fun (_, _, a) -> a)
            |> Array.ofSeq

        let tryAction action =
            match action with
            | "SPEED" -> aux (speed + 1) (move (speed + 1) |> act) (action :: turns)
            | "WAIT" -> if speed > 0 then aux speed (move speed |> act) (action :: turns) else None
            | "JUMP" -> if speed > 0 then aux speed (jump speed |> act) (action :: turns) else None
            | "SLOW" -> if speed > 1 then aux (speed - 1) (move (speed - 1) |> act) (action :: turns) else None
            | "UP" -> if speed > 0 && (bikes |> Seq.forall (fun (_, y, _) -> y > 0)) then aux speed (moveUp speed |> act) (action :: turns) else None
            | "DOWN" -> if speed > 0 && (bikes |> Seq.forall (fun (_, y, _) -> y < 3)) then aux speed (moveDown speed |> act) (action :: turns) else None
            | _ -> None

        if bikes |> Array.length < bikesToSurvive then None
        elif bikes |> Seq.filter (fun (x, _, _) -> x >= laneLength) |> Seq.length >= bikesToSurvive then Some turns
        else actions |> Array.tryPick tryAction

    aux speed bikes []
    |> Option.get

let speed = readInt()
let bikes = Array.init bikesCount (fun _ -> readBike())
    
calculateTurns speed bikes
|> Seq.rev
|> Seq.iter (printfn "%s")

printfn "WAIT"
