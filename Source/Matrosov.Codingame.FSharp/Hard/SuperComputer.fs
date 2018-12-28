module Matrosov.Codingame.FSharp.Hard.SuperComputer

open System

let readString() = Console.ReadLine()
let readInt = readString >> int
let readTask() = readString().Split(' ') |> Array.map int |> (fun a -> (a.[0], a.[0] + a.[1] - 1))

Seq.init (readInt()) (fun _ -> readTask())
|> Seq.sortBy snd
|> Seq.fold (fun result (start, finish) -> if start > (result |> List.head) then finish :: result else result) [0]
|> Seq.tail
|> Seq.length
|> printfn "%d"
