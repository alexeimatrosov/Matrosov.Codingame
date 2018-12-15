module Matrosov.Codingame.PowerOfThor

open System

let readString() = Console.In.ReadLine();
let readInt = readString >> int

let (dx, dy) =
    readString().Split [|' '|]
    |> Array.map int
    |> (fun a -> a.[0] - a.[2], a.[1] - a.[3])

let toSignString a negative zero positive =
    match a with
    | x when x < 0 -> negative
    | x when x > 0 -> positive
    | _ -> zero

let rec getDirections dx dy = 
    seq {
        if dx <> 0 || dy <> 0 then
            yield (toSignString dy "N" "" "S") + (toSignString dx "W" "" "E")
            yield! getDirections (dx - sign dx) (dy - sign dy)
    }

for d in getDirections dx dy do
    let remainingTurns = readInt ()
    printfn "%s" d