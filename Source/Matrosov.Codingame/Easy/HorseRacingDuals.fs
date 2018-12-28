module Matrosov.Codingame.Easy.HorseRacingDuals

open System

let readString() = Console.ReadLine();
let readInt = readString >> int

Seq.init (readInt()) (fun _ -> readInt())
|> Seq.sort
|> Seq.fold (fun (m, p) x -> abs (p - x) |> min m, x) (Int32.MaxValue, 0)
|> fst
|> printfn "%d"
