module Matrosov.Codingame.Easy.PowerOfThor

open System

let toSignValue negative zero positive a =
    if a > 0 then positive
    elif a < 0 then negative
    else zero

let rec getDirections dx dy = 
    seq {
        if dx <> 0 || dy <> 0 then
            yield (dy |> toSignValue "N" "" "S") + (dx |> toSignValue "W" "" "E")
            yield! getDirections (dx - sign dx) (dy - sign dy)
    }

Console.ReadLine().Split [|' '|]
|> Array.map int
|> (fun a -> getDirections (a.[0] - a.[2]) (a.[1] - a.[3]))
|> Seq.iter (printfn "%s")