module Matrosov.Codingame.GenomeSequencing

open System

let readString() = Console.ReadLine()
let readInt = readString >> int

let rec permutate list =
    match list with
    | [] -> []
    | a :: [] -> [[a]]
    | a :: tail -> 
        permutate tail
        |> List.collect (fun l ->
            seq {0 .. l.Length}
            |> Seq.map (fun i -> (List.take i l) @ [a] @ (List.skip i l))
            |> List.ofSeq
        )

let concatWithOverlap (s1:string) (s2:string) =
    let checkOverlapBy i =
        seq {i .. -1 .. 1}
        |> Seq.forall (fun j -> s1.[s1.Length-j] = s2.[i-j])

    let overlapBy =
        seq {(min s1.Length s2.Length) .. -1 .. 1}
        |> Seq.tryPick (fun i -> if checkOverlapBy i then Some i else None)
        |> Option.defaultValue 0

    s1 + s2.Substring(overlapBy)

let mutable concatCache : Map<string * string, string> = Map.empty

let concat s1 s2 =
    match concatCache.TryFind (s1, s2) with
    | Some s -> s
    | None ->
        let result = 
            if s1.Contains(s2) then s1
            elif s2.Contains(s1) then s2
            else concatWithOverlap s1 s2
        concatCache <- concatCache |> Map.add (s1, s2) result
        result

List.init (readInt()) (fun _ -> readString())
|> permutate
|> Seq.map (fun p -> p |> List.reduce concat)
|> Seq.minBy (fun s -> s.Length)
|> Seq.length
|> printf "%d"
