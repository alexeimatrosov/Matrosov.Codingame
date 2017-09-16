module Laconic.Codingame.AsciiArt

open System

let readString() = Console.In.ReadLine();
let readInt = readString >> int

let letterWidth = readInt()
let letterHeight = readInt()
let text = readString()
let alphabet = List.init letterHeight (fun i -> readString())

let charToIndex c =
    match Char.ToLowerInvariant(c) with
    | c when 'a' <= c && c <= 'z' -> int c - int 'a'
    | _ -> 26

text.ToCharArray()
|> Array.map charToIndex
|> Array.create letterHeight
|> Array.mapi (fun row letterIndexes ->
    letterIndexes
    |> Array.map (fun i -> alphabet.[row].Substring(i * letterWidth, letterWidth))
    |> String.concat "")
|> Array.iter (fun x -> printfn "%s" x)