module Matrosov.Codingame.Hard.BenderEpisode3

open System

let readString() = Console.ReadLine()
let readInt = readString >> int
let readPair() = readString().Split [|' '|] |> Array.map float |> (fun a -> (a.[0], a.[1]))

let log2 x = log x / log 2.0

let data = Array.init (readInt()) (fun _ -> readPair())

let calculateVariance (_, f) =
    let coefficient =
        data
        |> Seq.map (fun (n, t) -> t / f n)
        |> Seq.min

    data
    |> Seq.map (fun (n, t) -> t - coefficient * (f n))
    |> Seq.map (fun x -> x * x)
    |> Seq.average

[|
    ("O(1)", fun _ -> 1.0)
    ("O(log n)", fun n -> log2 n)
    ("O(n)", fun n -> n)
    ("O(n log n)", fun n -> n * log2 n)
    ("O(n^2)", fun n -> n * n)
    ("O(n^2 log n)", fun n -> n * n * log2 n)
    ("O(n^3)", fun n -> n * n * n)
    ("O(2^n)", fun n -> Math.Pow(2.0, n))
|]
|> Seq.minBy calculateVariance
|> fst
|> printf "%s"
