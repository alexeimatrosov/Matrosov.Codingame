module Matrosov.Codingame.FSharp.Hard.PowerOfThorEpisode2

open System

type Action =
    | Strike
    | Move of int * int

let readString() = Console.ReadLine()
let readIntPair() = readString().Split(' ') |> Array.map int |> (fun a -> (a.[0], a.[1]))

let pi = Math.PI
let width = 40
let height = 18

let sqr x = x * x

let distance (x1, y1) (x2, y2) =
    (x2 - x1 |> sqr) + (y2 - y1 |> sqr)
    |> float
    |> sqrt

let toDxy angle =
    let a = (abs angle) % pi
    let dx = if (pi / 8.0) < a && a < (pi - pi / 8.0) then sign angle else 0
    let dy = if a < (pi / 2.0 - pi / 8.0) then 1 elif (pi / 2.0 + pi / 8.0) < a then -1 else 0
    (dx, dy)

let addDxy (x, y) (dx, dy) = (x + dx, y + dy)

let enumerateDirections angle =
    seq {
        yield angle
        yield angle - pi * 0.25
        yield angle + pi * 0.25
        yield angle - pi * 0.50
        yield angle + pi * 0.50
        yield angle - pi * 0.75
        yield angle + pi * 0.75
        yield angle - pi
    }

let isWithinReach reach (x, y) (tx, ty) =
    abs <| x - tx <= reach && abs <| y - ty <= reach

let rec play thor =
    seq {
        let (thorX, thorY) = thor
        let (hammerStrikesLeft, giantsLeft) = readIntPair()
        let giants = Array.init giantsLeft (fun _ -> readIntPair())

        let giantsUnderStrike =
            giants
            |> Array.filter (isWithinReach 4 thor)

        let action =
            if giantsUnderStrike.Length = giants.Length
            then Strike
            else
                let farestGiants =
                    giants
                    |> Seq.sortByDescending (distance thor)
                    |> Seq.truncate 3
                    |> Array.ofSeq

                let (centerX, centerY) =
                    farestGiants
                    |> Array.fold (fun (sx, sy) (x, y) -> (sx + float x, sy + float y)) (0.0, 0.0)
                    |> (fun (sx, sy) -> (sx / float farestGiants.Length, sy / float farestGiants.Length))

                let dxyOption =
                    atan2 (centerX - float thorX) (centerY - float thorY)
                    |> enumerateDirections 
                    |> Seq.tryPick (fun angle ->
                        let dxy = angle |> toDxy 
                        let (x', y') = addDxy thor dxy
                        if 0 <= x' && x' < width && 0 <= y' && y' < height && giantsUnderStrike |> Seq.forall (isWithinReach 1 (x', y') >> not)
                        then Some dxy
                        else None)

                match dxyOption with
                | None -> Strike
                | Some dxy -> Move dxy

        yield action
        yield! play (match action with
                     | Strike -> thor
                     | Move (dx, dy) -> addDxy thor (dx, dy))
    }

let yMoves = [|"N"; ""; "S"|]
let xMoves = [|"W"; ""; "E"|]

readIntPair()
|> play
|> Seq.map (fun action ->
    match action with
    | Strike -> "STRIKE"
    | Move (0, 0) -> "WAIT"
    | Move (dx, dy) -> yMoves.[dy+1] + xMoves.[dx+1])
|> Seq.iter (printfn "%s")
