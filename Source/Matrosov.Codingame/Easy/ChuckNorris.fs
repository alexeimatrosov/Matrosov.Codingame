module Matrosov.Codingame.Easy.ChuckNorris

open System

let byteToBits (b:byte) = [|for i in 6..-1..0 -> (b >>> i) &&& 1uy|]

Console.In.ReadLine()
|> Seq.collect (byte >> byteToBits)
|> Seq.fold (fun s x ->
    match s with
    | (b, c) :: t when b = x -> (b, c+1) :: t
    | _ -> (x, 1) :: s) []
|> Seq.rev
|> Seq.map (fun (b, c) -> (if b = 0uy then "00" else "0") + " " + (String.replicate c "0"))
|> String.concat " "
|> printf "%s"
