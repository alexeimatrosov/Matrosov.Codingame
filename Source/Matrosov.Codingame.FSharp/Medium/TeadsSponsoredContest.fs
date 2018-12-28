module Matrosov.Codingame.FSharp.Medium.TeadsSponsoredContest

open System

let readString() = Console.ReadLine()
let readInt = readString >> int
let readPair() =
    readString().Split([|' '|])
    |> Array.map int
    |> (fun a -> (a.[0], a.[1]))

let edges = Array.init (readInt()) (fun _ -> readPair())

let graph =
    edges
    |> Seq.append (edges |> Seq.map (fun (u,v) -> (v,u)))
    |> Seq.groupBy fst
    |> Seq.map (fun (u, p) -> (u, p |> Seq.map snd |> Set.ofSeq))
    |> Map.ofSeq

let mutable visited : Set<int> = Set.empty

let rec visit node depth =
    seq {
        yield depth
        for i in graph.[node] do
            if not (visited.Contains i)
            then 
                visited <- visited.Add i
                yield visit i (depth+1)
    }
    |> Seq.max

let leaf = graph |> Map.pick (fun k v -> if v.Count = 1 then Some k else None)

let maxDepth = visit leaf 0

printfn "%d" (maxDepth / 2 + maxDepth % 2)