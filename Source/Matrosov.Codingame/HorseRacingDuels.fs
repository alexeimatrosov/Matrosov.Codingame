module Matrosov.Codingame.HorseRacingDuels

open System

let readString() = Console.In.ReadLine();
let readInt = readString >> int

let n = readInt()
let p =
    [1..n]
    |> List.map (fun _ -> readInt())
    |> List.sort

let answer = 
    p
    |> Seq.take (p.Length-1)
    |> Seq.map2 (fun a b -> abs (a - b)) (p.Tail)
    |> Seq.min

printfn "%d" answer