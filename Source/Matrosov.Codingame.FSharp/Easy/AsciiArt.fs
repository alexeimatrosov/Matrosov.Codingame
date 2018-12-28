module Matrosov.Codingame.FSharp.Easy.AsciiArt

open System

let readString() = Console.ReadLine();
let readInt = readString >> int

let letterWidth = readInt()
let letterHeight = readInt()

let textIndices =
    readString()
    |> Seq.map Char.ToLowerInvariant
    |> Seq.map (fun c -> if 'a' <= c && c <= 'z' then int c - int 'a' else 26)
    |> Array.ofSeq

let alphabet = Array.init letterHeight (fun _ -> readString())

seq {0..letterHeight-1}
|> Seq.map (fun r ->
    textIndices
    |> Seq.map (fun c -> alphabet.[r].Substring(c * letterWidth, letterWidth))
    |> String.concat "")
|> Seq.iter (printfn "%s")