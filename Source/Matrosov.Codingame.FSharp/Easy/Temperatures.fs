module Matrosov.Codingame.FSharp.Easy.Temperatures

open System

let readString() = Console.In.ReadLine();
let readInt = readString >> int

match readInt() with
| 0 -> 0
| _ -> readString().Split [|' '|]
       |> Seq.map int
       |> Seq.minBy (fun x -> (abs x) * 10000 - sign x)
|> printfn "%d"
