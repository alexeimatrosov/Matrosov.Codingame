module Matrosov.Codingame.Medium.War

open System

let readString() = Console.ReadLine()
let readInt = readString >> int

let readCard() =
    match readString().[0] with
    | '1' -> 10
    | 'J' -> 11
    | 'Q' -> 12
    | 'K' -> 13
    | 'A' -> 14
    | c -> int c - int '0'

let readDeck() = List.init (readInt()) (fun _ -> readCard())

let skipOrEmpty count (list:'a list) = if list.Length < count then [] else list |> List.skip count

let rec round deck1 deck2 count =
    let rec fight deck1 deck2 warDeck1 warDeck2 =
        match (deck1, deck2) with
        | ([], _)
        | (_, []) -> None
        | (c1 :: t1, c2 :: t2) ->
            match c1 - c2 with
            | 0 -> fight (t1 |> skipOrEmpty 3) (t2 |> skipOrEmpty 3) (warDeck1 @ [c1] @ (t1 |> List.truncate 3)) (warDeck2 @ [c2] @ (t2 |> List.truncate 3))
            | x when x > 0 -> Some (t1 @ warDeck1 @ [c1] @ warDeck2 @ [c2], t2)
            | x when x < 0 -> Some (t1, t2 @ warDeck1 @ [c1] @ warDeck2 @ [c2])

    match (deck1, deck2) with
    | (_, []) -> (1, count)
    | ([], _) -> (2, count)
    | (d1, d2) -> 
        match (fight d1 d2 [] []) with
        | Some (d1, d2) -> round d1 d2 (count+1)
        | _ -> (0, count)
 
let deck1 = readDeck()
let deck2 = readDeck()

match (round deck1 deck2 0) with
| (0, _) -> printfn "PAT"
| (winner, roundsCount) -> printfn "%d %d" winner roundsCount
