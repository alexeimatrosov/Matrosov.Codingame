module Matrosov.Codingame.FSharp.Easy.Temperatures

open System

let readString() = Console.In.ReadLine();
let readInt = readString >> int

let result =
    match readInt() with
    | 0 -> 0
    | _ -> readString().Split [|' '|]
           |> Array.map int
           |> Array.minBy (fun x -> (abs x) * 10000 - sign x)

printfn "%d" result