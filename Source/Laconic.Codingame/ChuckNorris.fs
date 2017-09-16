module Laconic.Codingame.ChuckNorris

open System

let message = Console.In.ReadLine()

let byteToBits (b:byte) =
    [for i in 6..-1..0 -> (b >>> i) &&& 1uy]

let rec countSequencesRecursive (list:'T list) counted =
    match list with
    | value :: tail ->
        let (lastValue, lastCount) = List.head counted
        if value = lastValue then
            countSequencesRecursive tail ((lastValue, lastCount+1) :: (List.tail counted))
        else
            countSequencesRecursive tail ((value, 1) :: counted)
    | [] -> counted

let countSequences list =
    countSequencesRecursive list [(list.[0], 0)]
    |> List.rev

let answer =
    message.ToCharArray()
    |> Array.map byte
    |> List.ofArray
    |> List.collect byteToBits
    |> countSequences
    |> List.map (fun (bit, count) -> sprintf "%s %s" (if bit = 0uy then "00" else "0") (String.replicate count "0"))
    |> String.concat " "

printf "%s" answer