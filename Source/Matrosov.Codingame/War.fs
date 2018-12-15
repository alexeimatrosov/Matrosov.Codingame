module Matrosov.Codingame.War

open System

type Value =
    | Two = 2
    | Three = 3
    | Four = 4
    | Five = 5
    | Six = 6
    | Seven = 7
    | Eight = 8
    | Nine = 9
    | Ten = 10
    | Jack = 11
    | Queen = 12
    | King = 13
    | Ace = 14

type Suit =
    | Diamonds = 'D'
    | Hearts = 'H'
    | Clubs = 'C'
    | Spades = 'S'

type Card = {
    Value : Value
    Suit : Suit
}

let readString() = Console.ReadLine()
let readInt = readString >> int

let readCard() =
    let s = readString()
    {
        Value = match s.[..s.Length-2] with
                | "J" -> Value.Jack
                | "Q" -> Value.Queen
                | "K" -> Value.King
                | "A" -> Value.Ace
                | v -> v |> int |> enum<Value>;
        Suit = s |> Seq.last |> LanguagePrimitives.EnumOfValue
    }

let readDeck() = List.init (readInt()) (fun _ -> readCard())

let skipOrEmpty count (list:'a list) = if list.Length < count then [] else list |> List.skip count

let rec fight deck1 deck2 warDeck1 warDeck2 =
    match deck1 with
    | [] -> None
    | card1 :: tail1 ->
        match deck2 with
        | [] -> None
        | card2 :: tail2 ->
            match (int card1.Value) - (int card2.Value) with
            | 0 -> fight (tail1 |> skipOrEmpty 3) (tail2 |> skipOrEmpty 3) (warDeck1 @ [card1] @ (tail1 |> List.truncate 3)) (warDeck2 @ [card2] @ (tail2 |> List.truncate 3))
            | x when x > 0 -> Some (tail1 @ warDeck1 @ [card1] @ warDeck2 @ [card2], tail2)
            | x when x < 0 -> Some (tail1, tail2 @ warDeck1 @ [card1] @ warDeck2 @ [card2])

let rec round deck1 deck2 count =
    match deck1 with
    | [] -> (2, count)
    | _ -> 
        match deck2 with
        | [] -> (1, count)
        | _ ->
            match (fight deck1 deck2 [] []) with
            | Some (d1, d2) -> round d1 d2 (count+1)
            | _ -> (0, count)
 
let deck1 = readDeck()
let deck2 = readDeck()

match (round deck1 deck2 0) with
| (0, _) -> printfn "PAT"
| (winner, roundsCount) -> printfn "%d %d" winner roundsCount
